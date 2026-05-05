using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.coupon;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class CouponService(
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : ICouponService
    {
        private readonly ICouponRepository _couponRepository = couponRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedResult<CouponDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var (coupons, totalCount) = await _couponRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var dtos = _mapper.Map<IEnumerable<CouponDto>>(coupons);

            return PagedResult<CouponDto>.Create(
                dtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<CouponDto> CreateAsync(CouponCreateDto createDto)
        {
            var code = NormalizeCode(createDto.Code);

            var existing = await _couponRepository.GetByCodeAsync(code);
            if (existing != null)
                throw new BadRequestException("Coupon code already exists");

            ValidateDateRange(createDto.ValidFrom, createDto.ValidUntil);
            ValidateDiscountRules(createDto.DiscountType, createDto.DiscountValue);

            var coupon = new Coupon
            {
                Code = code,
                Description = createDto.Description,
                DiscountType = createDto.DiscountType,
                DiscountValue = createDto.DiscountType == DiscountType.free_shipping ? 0 : createDto.DiscountValue,
                MinOrderAmount = createDto.MinOrderAmount,
                UsageLimit = createDto.UsageLimit,
                ValidFrom = createDto.ValidFrom,
                ValidUntil = createDto.ValidUntil,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _couponRepository.AddAsync(coupon);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> UpdateAsync(int couponId, CouponUpdateDto updateDto)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId)
                ?? throw new NotFoundException("Coupon not found");

            if (!string.IsNullOrWhiteSpace(updateDto.Code))
            {
                var newCode = NormalizeCode(updateDto.Code);
                if (!string.Equals(coupon.Code, newCode, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await _couponRepository.GetByCodeAsync(newCode);
                    if (existing != null)
                        throw new BadRequestException("Coupon code already exists");

                    coupon.Code = newCode;
                }
            }

            var nextType = updateDto.DiscountType ?? coupon.DiscountType;
            var nextValue = updateDto.DiscountValue ?? coupon.DiscountValue;

            ValidateDateRange(updateDto.ValidFrom ?? coupon.ValidFrom, updateDto.ValidUntil ?? coupon.ValidUntil);
            ValidateDiscountRules(nextType, nextValue);

            if (updateDto.Description != null)
                coupon.Description = updateDto.Description;

            coupon.DiscountType = nextType;
            coupon.DiscountValue = nextType == DiscountType.free_shipping ? 0 : nextValue;

            if (updateDto.MinOrderAmount.HasValue)
                coupon.MinOrderAmount = updateDto.MinOrderAmount.Value;

            if (updateDto.UsageLimit.HasValue)
                coupon.UsageLimit = updateDto.UsageLimit.Value;

            if (updateDto.ValidFrom.HasValue || updateDto.ValidFrom == null)
                coupon.ValidFrom = updateDto.ValidFrom;

            if (updateDto.ValidUntil.HasValue || updateDto.ValidUntil == null)
                coupon.ValidUntil = updateDto.ValidUntil;

            if (updateDto.IsActive.HasValue)
                coupon.IsActive = updateDto.IsActive.Value;

            coupon.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<bool> DeleteAsync(int couponId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId)
                ?? throw new NotFoundException("Coupon not found");

            var usageCount = await _couponRepository.CountUsageAsync(couponId);
            if (usageCount > 0)
                throw new BadRequestException("Cannot delete coupon that has already been used");

            await _couponRepository.DeleteAsync(coupon.CouponId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<CouponDto> GetByCodeAsync(string code, decimal? subtotal = null)
        {
            var normalized = NormalizeCode(code);

            var coupon = await _couponRepository.GetByCodeAsync(normalized)
                ?? throw new NotFoundException("Coupon not found");

            if (!coupon.IsActive)
                throw new BadRequestException("Coupon is inactive");

            var now = DateTime.UtcNow;
            if (coupon.ValidFrom.HasValue && coupon.ValidFrom.Value > now)
                throw new BadRequestException("Coupon is not valid yet");

            if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < now)
                throw new BadRequestException("Coupon has expired");

            if (coupon.UsageLimit.HasValue)
            {
                var usageCount = await _couponRepository.CountUsageAsync(coupon.CouponId);
                if (usageCount >= coupon.UsageLimit.Value)
                    throw new BadRequestException("Coupon usage limit reached");
            }

            if (subtotal.HasValue && coupon.MinOrderAmount.HasValue &&
                subtotal.Value < coupon.MinOrderAmount.Value)
                throw new BadRequestException("Order amount too low for this coupon");

            return _mapper.Map<CouponDto>(coupon);
        }

        private static string NormalizeCode(string code)
        {
            var normalized = code.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new BadRequestException("Coupon code is required");
            return normalized;
        }

        private static void ValidateDateRange(DateTime? validFrom, DateTime? validUntil)
        {
            if (validFrom.HasValue && validUntil.HasValue && validFrom.Value > validUntil.Value)
                throw new BadRequestException("ValidFrom cannot be later than ValidUntil");
        }

        private static void ValidateDiscountRules(DiscountType type, decimal value)
        {
            switch (type)
            {
                case DiscountType.percentage:
                    if (value <= 0 || value > 20)
                        throw new BadRequestException("Percentage coupon must be greater than 0 and at most 20%");
                    break;

                case DiscountType.fixed_amount:
                    if (value <= 0)
                        throw new BadRequestException("Fixed amount coupon must be greater than 0");
                    break;

                case DiscountType.free_shipping:
                    if (value != 0)
                        throw new BadRequestException("Free shipping coupon must have discount value = 0");
                    break;

                default:
                    throw new BadRequestException("Invalid discount type");
            }
        }
    }
}