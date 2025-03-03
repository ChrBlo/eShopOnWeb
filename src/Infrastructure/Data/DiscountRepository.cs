using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public class DiscountRepository : IDiscountRepository
{
    private readonly CatalogContext _dbContext;

    public DiscountRepository(CatalogContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDiscount?> GetDiscountCodeByIdAsync(string code)
    {
        return await _dbContext.Discounts
            .FirstOrDefaultAsync(d => d.DiscountCode == code && !d.DiscountIsUsed);
    }

    public async Task<bool> CheckIfDiscountIsValidAsync(string code)
    {
        return await _dbContext.Discounts
            .AnyAsync(d => d.DiscountCode == code && !d.DiscountIsUsed);
    }

    public async Task SetDiscountAsUsedAsync(int discountId)
    {
        var discount = await _dbContext.Discounts.FindAsync(discountId);
        if (discount != null)
        {
            discount.SetDiscountIsUsed(discountId);
            await _dbContext.SaveChangesAsync();
        }
    }
}