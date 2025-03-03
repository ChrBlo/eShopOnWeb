using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IDiscountRepository _discountRepository;


    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer,
        IDiscountRepository discountRepository)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _discountRepository = discountRepository;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItems = await _itemRepository.ListAsync(new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray()));

        var itemsOrdered = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, itemsOrdered);

        Random rand = new Random();
        int randomNumber = rand.Next(1, 11);

        if (randomNumber > 0 && randomNumber <= 3)
        {
            order.SetToDelivered();
        }
        if (randomNumber > 7 && randomNumber <= 10)
        {
            order.SetToOutForDelivery();
        }

        if (basket.BasketDiscount != null && basket.BasketDiscount.IsValid())
        {
            order.ApplyDiscount(basket.BasketDiscount);

            // Mark the discount as used
            await _discountRepository.SetDiscountAsUsedAsync(basket.BasketDiscount.DiscountId);
        }

        await _orderRepository.AddAsync(order);
    }
}
