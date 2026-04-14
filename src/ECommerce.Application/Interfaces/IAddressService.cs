using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.address;

namespace ECommerce.Application.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId);
        Task<AddressDto> CreateAsync(int userId, AddressCreateDto createDto);
        Task<AddressDto> UpdateAsync(int userId, int addressId, AddressUpdateDto updateDto);
        Task<bool> DeleteAsync(int userId, int addressId);
    }
}