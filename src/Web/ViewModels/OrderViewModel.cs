using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.Web.ViewModels;

public class OrderViewModel
{
    public int OrderNumber { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public decimal Total { get; set; }
    public decimal OrderDiscount { get; set; }
    public decimal TotalAfterDiscount { get; set; }
    public string Status { get; set; } = "Pending";
    public Address? ShippingAddress { get; set; }
}
