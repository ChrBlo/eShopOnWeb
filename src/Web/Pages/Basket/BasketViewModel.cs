using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

public class BasketViewModel
{
    public int Id { get; set; }
    public List<BasketItemViewModel> Items { get; set; } = new List<BasketItemViewModel>();
    public string? BuyerId { get; set; }
    public BasketDiscount? BasketDiscount { get; set; }

    public decimal Total()
    {
        return Math.Round(Items.Sum(x => x.UnitPrice * x.Quantity), 2);
    }

    public decimal DiscountAmount()
    {
        if (BasketDiscount == null || !BasketDiscount.IsValid())
        {
            return 0;
        }

        var total = Total();

        decimal discountPercentageDecimal = Convert.ToDecimal(BasketDiscount.DiscountPercentage);
        return Math.Round(total * (discountPercentageDecimal / 100), 2);
    }

    public bool HasValidDiscount()
    {
        return BasketDiscount != null && BasketDiscount.IsValid();
    }

    public decimal TotalAfterDiscount()
    {
        var total = Total();
        var discount = DiscountAmount();

        return Math.Max(total - discount, 0);
    }
}
