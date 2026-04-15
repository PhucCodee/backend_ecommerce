using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.DTOs.address;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class AddressService(
        IUserAddressRepository userAddressRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IAddressService
    {
        private readonly IUserAddressRepository _userAddressRepository = userAddressRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId)
        {
            var addresses = await _userAddressRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<AddressDto> CreateAsync(int userId, AddressCreateDto createDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            var existing = (await _userAddressRepository.GetByUserIdAsync(userId)).ToList();
            var isFirstAddress = !existing.Any();

            var makeDefaultShipping = createDto.IsDefaultShipping || isFirstAddress;
            var makeDefaultBilling = createDto.IsDefaultBilling || isFirstAddress;

            if (makeDefaultShipping)
                await _userAddressRepository.ClearDefaultShippingAsync(userId);

            if (makeDefaultBilling)
                await _userAddressRepository.ClearDefaultBillingAsync(userId);

            var address = UserAddress.CreateDefault(
                user: user,
                type: createDto.Type,
                label: createDto.Label.Trim(),
                recipientName: createDto.RecipientName.Trim(),
                phone: createDto.Phone.Trim(),
                addressLine1: createDto.AddressLine1.Trim(),
                city: createDto.City.Trim(),
                stateProvince: createDto.StateProvince.Trim(),
                postalCode: createDto.PostalCode.Trim(),
                country: createDto.Country.Trim(),
                addressLine2: string.IsNullOrWhiteSpace(createDto.AddressLine2) ? null : createDto.AddressLine2.Trim(),
                isDefaultShipping: makeDefaultShipping,
                isDefaultBilling: makeDefaultBilling);

            await _userAddressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAsync(int userId, int addressId, AddressUpdateDto updateDto)
        {
            var address = await _userAddressRepository.GetByIdAndUserIdAsync(addressId, userId)
                ?? throw new NotFoundException("Address not found");

            if (updateDto.IsDefaultShipping == true)
                await _userAddressRepository.ClearDefaultShippingAsync(userId, addressId);

            if (updateDto.IsDefaultBilling == true)
                await _userAddressRepository.ClearDefaultBillingAsync(userId, addressId);

            if (updateDto.Type.HasValue) address.Type = updateDto.Type.Value;
            if (!string.IsNullOrWhiteSpace(updateDto.Label)) address.Label = updateDto.Label.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.RecipientName)) address.RecipientName = updateDto.RecipientName.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.Phone)) address.Phone = updateDto.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.AddressLine1)) address.AddressLine1 = updateDto.AddressLine1.Trim();
            if (updateDto.AddressLine2 != null) address.AddressLine2 = string.IsNullOrWhiteSpace(updateDto.AddressLine2) ? null : updateDto.AddressLine2.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.City)) address.City = updateDto.City.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.StateProvince)) address.StateProvince = updateDto.StateProvince.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.PostalCode)) address.PostalCode = updateDto.PostalCode.Trim();
            if (!string.IsNullOrWhiteSpace(updateDto.Country)) address.Country = updateDto.Country.Trim();

            if (updateDto.IsDefaultShipping.HasValue) address.IsDefaultShipping = updateDto.IsDefaultShipping.Value;
            if (updateDto.IsDefaultBilling.HasValue) address.IsDefaultBilling = updateDto.IsDefaultBilling.Value;

            address.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AddressDto>(address);
        }

        public async Task<bool> DeleteAsync(int userId, int addressId)
        {
            var address = await _userAddressRepository.GetByIdAndUserIdAsync(addressId, userId)
                ?? throw new NotFoundException("Address not found");

            var wasDefaultShipping = address.IsDefaultShipping;
            var wasDefaultBilling = address.IsDefaultBilling;

            await _userAddressRepository.DeleteAsync(address);
            await _unitOfWork.SaveChangesAsync();

            var fallback = await _userAddressRepository.GetFirstByUserIdAsync(userId);
            if (fallback != null)
            {
                var shouldSave = false;

                if (wasDefaultShipping && !fallback.IsDefaultShipping)
                {
                    fallback.IsDefaultShipping = true;
                    shouldSave = true;
                }

                if (wasDefaultBilling && !fallback.IsDefaultBilling)
                {
                    fallback.IsDefaultBilling = true;
                    shouldSave = true;
                }

                if (shouldSave)
                    await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }
    }
}