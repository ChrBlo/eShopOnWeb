using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IBasketService _basketService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOrderService _orderService;
    private string? _username = null;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly IAppLogger<CheckoutModel> _logger;
    private readonly IDiscountRepository _discountRepository;
    private readonly IRepository<ApplicationCore.Entities.BasketAggregate.Basket> _basketRepository;


    public CheckoutModel(IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        SignInManager<ApplicationUser> signInManager,
        IOrderService orderService,
        IAppLogger<CheckoutModel> logger,
        IDiscountRepository discountRepository,
        IRepository<ApplicationCore.Entities.BasketAggregate.Basket> basketRepository)
    {
        _basketService = basketService;
        _signInManager = signInManager;
        _orderService = orderService;
        _basketViewModelService = basketViewModelService;
        _logger = logger;
        _discountRepository = discountRepository;
        _basketRepository = basketRepository;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPost(IEnumerable<BasketItemViewModel> items)
    {
        try
        {
            await SetBasketModelAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
            await _basketService.SetQuantities(BasketModel.Id, updateModel);
            await _orderService.CreateOrderAsync(BasketModel.Id, new Address("123 Main St.", "Kent", "OH", "United States", "44240"));
            await _basketService.DeleteBasketAsync(BasketModel.Id);
        }
        catch (EmptyBasketOnCheckoutException emptyBasketOnCheckoutException)
        {
            //Redirect to Empty Basket page
            _logger.LogWarning(emptyBasketOnCheckoutException.Message);
            return RedirectToPage("/Basket/Index");
        }

        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        Guard.Against.Null(User?.Identity?.Name, nameof(User.Identity.Name));
        if (_signInManager.IsSignedIn(HttpContext.User))
        {
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(User.Identity.Name);
        }
        else
        {
            GetOrSetBasketCookieAndUserName();
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(_username!);
        }
    }

    private void GetOrSetBasketCookieAndUserName()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            _username = Request.Cookies[Constants.BASKET_COOKIENAME];
        }
        if (_username != null) return;

        _username = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions();
        cookieOptions.Expires = DateTime.Today.AddYears(10);
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, _username, cookieOptions);
    }

    public async Task<IActionResult> OnPostValidateDiscountCodeAsync(string discountCode)
    {
        _logger.LogInformation($"Validating discount code: {discountCode}");

        await SetBasketModelAsync();

        _logger.LogInformation($"Basket ID: {BasketModel.Id}, Item Count: {BasketModel.Items.Count}");

        if (string.IsNullOrEmpty(discountCode))
        {
            _logger.LogWarning("Empty discount code provided");
            ModelState.AddModelError("discountCode", "Please enter a discount code");
            return Page();
        }

        _logger.LogInformation($"Checking if discount code '{discountCode}' is valid");
        var isValid = await _discountRepository.CheckIfDiscountIsValidAsync(discountCode);

        if (!isValid)
        {
            _logger.LogWarning($"Invalid discount code: '{discountCode}'");
            ModelState.AddModelError("discountCode", "Invalid discount code");
            return Page();
        }

        _logger.LogInformation($"Discount code '{discountCode}' is valid, retrieving details");
        var discount = await _discountRepository.GetDiscountCodeByIdAsync(discountCode);
        _logger.LogInformation($"Got discount: ID={discount?.DiscountId}, Value={discount?.DiscountPercentage}%");

        _logger.LogInformation($"Getting basket with ID: {BasketModel.Id}");
        var basket = await _basketRepository.GetByIdAsync(BasketModel.Id);

        if (basket == null)
        {
            _logger.LogInformation($"Could not find basket with ID: {BasketModel.Id}");
            ModelState.AddModelError("", "There was a problem with your basket. Please try again.");
            return Page();
        }

        _logger.LogInformation("Applying discount to basket");
        basket.ApplyDiscount(discount);
        await _basketRepository.UpdateAsync(basket);
        _logger.LogInformation("Discount applied and basket updated");

        // Refresh the basket model
        await SetBasketModelAsync();
        _logger.LogInformation("Basket model refreshed, returning page");

        return Page();
    }
}
