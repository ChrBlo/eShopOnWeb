using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IDiscountRepository
{
    Task<BasketDiscount?> GetDiscountCodeByIdAsync(string code);
    Task<bool> CheckIfDiscountIsValidAsync(string code);
    Task SetDiscountAsUsedAsync(int discountId);
}

