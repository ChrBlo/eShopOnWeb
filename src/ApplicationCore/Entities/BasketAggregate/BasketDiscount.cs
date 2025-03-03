using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

public class BasketDiscount : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DiscountId { get; set; }
    public string DiscountCode { get; set; }
    public double DiscountPercentage { get; set; }
    public bool DiscountIsUsed { get; set; }

    public BasketDiscount() { }

    public BasketDiscount(int id, string discountCode, double discountPercentage, bool discountIsUsed)
    {
        DiscountId = id;
        DiscountCode = discountCode;
        DiscountPercentage = discountPercentage;
        DiscountIsUsed = discountIsUsed;
    }

    public void SetDiscountIsUsed(int discountId)
    {
        DiscountIsUsed = true;
    }

    public bool IsValid()
    {
        if (DiscountIsUsed)
        {
            return false;
        }
        return true;
    }
}