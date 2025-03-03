using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.Web.ViewModels;

public class OrderViewModel
{
    // private const string DEFAULT_STATUS = "Pending";

    public int OrderNumber { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public decimal Total { get; set; }
    public decimal OrderDiscount { get; set; }
    public decimal TotalAfterDiscount { get; set; }
    public string Status { get; set; } = "Pending";
    public Address? ShippingAddress { get; set; }
}
