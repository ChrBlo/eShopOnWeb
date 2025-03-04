using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.Web.Controllers;

public class StatisticsController : Controller
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<CatalogItem> _catalogItemRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsController(
        IRepository<Order> orderRepository,
        IRepository<CatalogItem> catalogItemRepository,
        UserManager<ApplicationUser> userManager)
    {
        _orderRepository = orderRepository;
        _catalogItemRepository = catalogItemRepository;
        _userManager = userManager;
    }

    public async Task<IActionResult> GetStatistics()
    {
        var orders = await _orderRepository.ListAsync();

        decimal totalOrderValue = 0;
        foreach (var order in orders)
        {
            // Use OrderWithItemsByIdSpec to get the complete order with items
            var spec = new OrderWithItemsByIdSpec(order.Id);
            var completeOrder = await _orderRepository.FirstOrDefaultAsync(spec);

            if (completeOrder != null)
            {
                totalOrderValue += completeOrder.Total();
            }
        }

        var products = await _catalogItemRepository.ListAsync();

        var userCount = _userManager.Users.Count();

        // Create view model with statistics
        var viewModel = new StatisticsViewModel
        {
            TotalOrders = orders.Count,
            TotalOrderValue = totalOrderValue,
            TotalProducts = products.Count,
            TotalUsers = userCount
        };

        return View("~/Views/Statistics/GetStatistics.cshtml", viewModel);
    }
}

public class StatisticsViewModel
{
    public int TotalOrders { get; set; }
    public decimal TotalOrderValue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
}
