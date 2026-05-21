using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Application.Services
{
    public class CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper
    ) : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<CategoryDto> GetByIdAsync(int categoryId)
        {
            var category =
                await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> GetBySlugAsync(string slug)
        {
            var category =
                await _categoryRepository.GetBySlugAsync(slug)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<PagedResult<CategoryDto>> GetAllPagedAsync(
            PaginationParams paginationParams,
            bool includeInactive = false
        )
        {
            var (categories, totalCount) = await _categoryRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize,
                includeInactive
            );

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return PagedResult<CategoryDto>.Create(
                categoryDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount
            );
        }

        public async Task<PagedResult<CategoryDto>> GetCoreCategoriesPagedAsync(
            PaginationParams paginationParams
        )
        {
            var (categories, totalCount) = await _categoryRepository.GetCoreCategoriesPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return PagedResult<CategoryDto>.Create(
                categoryDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount
            );
        }

        public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentId)
        {
            var categories = await _categoryRepository.GetChildCategoriesAsync(parentId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto createDto)
        {
            if (createDto.ParentCategoryId.HasValue)
            {
                var parentCategory =
                    await _categoryRepository.GetByIdAsync(createDto.ParentCategoryId.Value)
                    ?? throw new BadRequestException("Parent category not found");
            }

            var baseSlug = SlugHelper.GenerateSlug(createDto.Name);
            var slug = await SlugHelper.EnsureUniqueAsync(
                baseSlug,
                async s => await _categoryRepository.GetBySlugAsync(s) != null
            );

            var category = Category.CreateDefault(
                name: createDto.Name,
                slug: slug,
                parentCategoryId: createDto.ParentCategoryId,
                description: createDto.Description,
                imageUrl: createDto.ImageUrl,
                displayOrder: createDto.DisplayOrder,
                isCore: createDto.IsCore,
                isActive: createDto.IsActive
            );

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateAsync(int categoryId, CategoryUpdateDto updateDto)
        {
            var category =
                await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            // Validate parent category if provided
            if (updateDto.ParentCategoryId.HasValue)
            {
                if (updateDto.ParentCategoryId.Value == categoryId)
                    throw new BadRequestException("Category cannot be its own parent");

                var parentCategory =
                    await _categoryRepository.GetByIdAsync(updateDto.ParentCategoryId.Value)
                    ?? throw new BadRequestException("Parent category not found");
            }

            ApplyUpdates(category, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var category =
                await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            if (category.IsDeleted())
                throw new BadRequestException("Category is already deleted");

            var children = await _categoryRepository.GetChildCategoriesAsync(categoryId);
            if (children.Any())
                throw new BadRequestException(
                    "Cannot delete category with active child categories"
                );

            if (await _categoryRepository.HasProductsAsync(categoryId))
                throw new BadRequestException("Cannot delete category with products");

            // Hard delete: the row is permanently removed from the DB.
            // Pre-checks above guarantee no active products or child
            // categories reference this row, so the only path that can
            // still trip the product_categories / categories foreign keys
            // is a soft-deleted product (preserved for order history) that
            // still carries this category. Convert that to a 409 so the
            // admin gets a clear message instead of a generic 500.
            await _categoryRepository.DeleteAsync(categoryId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
            {
                throw new ConflictException(
                    "Cannot delete this category because it is still referenced by removed products kept for order history. "
                        + "Reassign or permanently remove those products first."
                );
            }

            return true;
        }

        private static bool IsForeignKeyViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pg
                && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation;
        }

        private static void ApplyUpdates(Category category, CategoryUpdateDto updateDto)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            {
                category.CategoryName = updateDto.Name;
                category.Slug = SlugHelper.GenerateSlug(updateDto.Name);
            }

            if (updateDto.ParentCategoryId.HasValue)
                category.ParentCategoryId =
                    updateDto.ParentCategoryId.Value == 0 ? null : updateDto.ParentCategoryId;

            if (updateDto.Description != null)
                category.Description = updateDto.Description;

            if (updateDto.ImageUrl != null)
                category.ImageUrl = updateDto.ImageUrl;

            if (updateDto.DisplayOrder.HasValue)
                category.DisplayOrder = updateDto.DisplayOrder.Value;

            if (updateDto.IsCore.HasValue)
                category.IsCore = updateDto.IsCore.Value;

            if (updateDto.IsActive.HasValue)
                category.IsActive = updateDto.IsActive.Value;

            category.UpdatedAt = DateTime.UtcNow;
        }
    }
}
