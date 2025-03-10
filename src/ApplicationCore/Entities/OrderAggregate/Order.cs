﻿using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

public class Order : BaseEntity, IAggregateRoot
{
#pragma warning disable CS8618 // Required by Entity Framework
    private Order() { }

    public Order(string buyerId, Address shipToAddress, List<OrderItem> items)
    {
        Guard.Against.NullOrEmpty(buyerId, nameof(buyerId));

        BuyerId = buyerId;
        ShipToAddress = shipToAddress;
        _orderItems = items;
    }

    public string Status { get; private set; } = "Pending";
    public string BuyerId { get; private set; }
    public DateTimeOffset OrderDate { get; private set; } = DateTimeOffset.Now;
    public Address ShipToAddress { get; private set; }
    public BasketDiscount BasketDiscount { get; private set; }

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method Order.AddOrderItem() which includes behavior.
    private readonly List<OrderItem> _orderItems = new List<OrderItem>();

    // Using List<>.AsReadOnly() 
    // This will create a read only wrapper around the private list so is protected against "external updates".
    // It's much cheaper than .ToList() because it will not have to copy all items in a new collection. (Just one heap alloc for the wrapper instance)
    //https://msdn.microsoft.com/en-us/library/e78dcd75(v=vs.110).aspx 
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public void ApplyDiscount(BasketDiscount discount)
    {
        Guard.Against.Null(discount, nameof(discount));
        BasketDiscount = discount;
    }

    public decimal Total()
    {
        var total = 0m;

        foreach (var item in _orderItems)
        {
            total += item.UnitPrice * item.Units;
        }

        return total;
    }

    public decimal TotalAfterDiscount()
    {
        var discountedTotal = Total();

        if (BasketDiscount != null && BasketDiscount.IsValid())
        {
            var discountAmount = discountedTotal * Convert.ToDecimal(BasketDiscount.DiscountPercentage) / 100m;
            discountedTotal -= discountAmount;
        }

        return discountedTotal;
    }

    public decimal OrderDiscount()
    {
        var total = Total();

        if (BasketDiscount != null && BasketDiscount.IsValid())
        {
            var discountAmount = total * (Convert.ToDecimal(BasketDiscount.DiscountPercentage) / 100m);
            return discountAmount;
        }
        else
        {
            return 0;
        }
    }

    public void SetToOutForDelivery()
    {
        Status = "Out for Delivery";
    }

    public void SetToDelivered()
    {
        Status = "Delivered";
    }
}
