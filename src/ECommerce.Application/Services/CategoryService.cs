using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<CategoryDto> GetByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> GetBySlugAsync(string slug)
        {
            var category = await _categoryRepository.GetBySlugAsync(slug)
                ?? throw new NotFoundException("Category not found");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<PagedResult<CategoryDto>> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var (categories, totalCount) = await _categoryRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return PagedResult<CategoryDto>.Create(
                categoryDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<PagedResult<CategoryDto>> GetCoreCategoriesPagedAsync(PaginationParams paginationParams)
        {
            var (categories, totalCount) = await _categoryRepository.GetCoreCategoriesPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return PagedResult<CategoryDto>.Create(
                categoryDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentId)
        {
            var categories = await _categoryRepository.GetChildCategoriesAsync(parentId);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto createDto)
        {
            // Validate parent category if provided
            if (createDto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(createDto.ParentCategoryId.Value)
                    ?? throw new BadRequestException("Parent category not found");
            }

            var slug = GenerateSlug(createDto.Name);

            // Check if slug already exists
            var existingCategory = await _categoryRepository.GetBySlugAsync(slug);
            if (existingCategory != null)
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var category = Category.CreateDefault(
                name: createDto.Name,
                slug: slug,
                parentCategoryId: createDto.ParentCategoryId,
                description: createDto.Description,
                imageUrl: createDto.ImageUrl,
                displayOrder: createDto.DisplayOrder,
                isCore: createDto.IsCore,
                isActive: createDto.IsActive);

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateAsync(int categoryId, CategoryUpdateDto updateDto)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            // Validate parent category if provided
            if (updateDto.ParentCategoryId.HasValue)
            {
                if (updateDto.ParentCategoryId.Value == categoryId)
                    throw new BadRequestException("Category cannot be its own parent");

                var parentCategory = await _categoryRepository.GetByIdAsync(updateDto.ParentCategoryId.Value)
                    ?? throw new BadRequestException("Parent category not found");
            }

            ApplyUpdates(category, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found");

            if (category.IsDeleted())
                throw new BadRequestException("Category is already deleted");

            // Check if category has active children
            var children = await _categoryRepository.GetChildCategoriesAsync(categoryId);
            if (children.Any())
                throw new BadRequestException("Cannot delete category with active child categories");

            // Check if category has any products
            if (await _categoryRepository.HasProductsAsync(categoryId))
                throw new BadRequestException("Cannot delete category with products");

            // Soft delete
            category.SoftDelete();
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static void ApplyUpdates(Category category, CategoryUpdateDto updateDto)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            {
                category.CategoryName = updateDto.Name;
                category.Slug = GenerateSlug(updateDto.Name);
            }

            if (updateDto.ParentCategoryId.HasValue)
                category.ParentCategoryId = updateDto.ParentCategoryId.Value == 0 ? null : updateDto.ParentCategoryId;

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

        private static string GenerateSlug(string name)
        {
            var slug = new string([.. name.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')]);
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }
            return slug.Trim('-');
        }
    }
}