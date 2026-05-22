using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IProductSkuRepository : IRepository<ProductSku>
    {
        Task<ProductSku?> GetByIdWithDetailsAsync(int skuId);
        /// <summary>
        /// True if the SKU is referenced by an order that is still active
        /// (created / confirmed / processing / shipped).
        /// </summary>
        Task<bool> HasActiveOrdersAsync(int skuId);
        Task<IEnumerable<ProductSku>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductSku>> GetByProductIdWithDetailsAsync(int productId);
        Task<ProductSku?> GetBySkuCodeAsync(string skuCode);
        Task<(IEnumerable<ProductSku> Skus, int TotalCount)> GetByProductIdPagedAsync(
            int productId,
            int pageNumber,
            int pageSize
        );
        Task<(IEnumerable<ProductSku> Skus, int TotalCount)> GetBySellerPagedAsync(
            int sellerId,
            int pageNumber,
            int pageSize
        );
    }
}
