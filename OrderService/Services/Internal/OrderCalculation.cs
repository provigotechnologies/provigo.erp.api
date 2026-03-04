using ProviGo.Common.Models;

public static class OrderCalculation
{
    public static void BuildAndCalculate(
        List<(int ProductId, decimal Quantity, decimal UnitPrice)> rawItems,
        List<Discount> discountsFromDb,
        List<Tax> taxesFromDb,
        List<Charge> chargesFromDb,
        out List<OrderItem> orderItems,
        out List<OrderDiscount> orderDiscounts,
        out List<OrderTax> orderTaxes,
        out List<OrderCharge> orderCharges,
        out decimal subTotal,
        out decimal discountTotal,
        out decimal taxTotal,
        out decimal chargeTotal,
        out decimal grandTotal)
    {
        // 1️⃣ Build order items
        var tempOrderItems = rawItems.Select(r => new OrderItem
        {
            ProductId = r.ProductId,
            Quantity = r.Quantity,
            UnitPrice = r.UnitPrice,
            TotalPrice = r.Quantity * r.UnitPrice
        }).ToList();

        decimal tempSubTotal = tempOrderItems.Sum(i => i.TotalPrice);

        // 2️⃣ Taxes (applied on subtotal)
        var tempOrderTaxes = taxesFromDb.Select(t => new OrderTax
        {
            TaxId = t.TaxId,
            TaxAmount = tempSubTotal * t.Rate / 100
        }).ToList();
        decimal tempTaxTotal = tempOrderTaxes.Sum(t => t.TaxAmount);

        // 3️⃣ Charges
        var tempOrderCharges = chargesFromDb.Select(c => new OrderCharge
        {
            ChargeId = c.ChargeId,
            ChargeAmount = c.Value
        }).ToList();
        decimal tempChargeTotal = tempOrderCharges.Sum(c => c.ChargeAmount);

        // 4️⃣ Discounts 
        var tempOrderDiscounts = discountsFromDb.Select(d => new OrderDiscount
        {
            DiscountId = d.DiscountId,
            DiscountAmount = tempSubTotal * d.Value / 100m
        }).ToList();
        decimal tempDiscountTotal = tempOrderDiscounts.Sum(d => d.DiscountAmount);

        // 5️⃣ Grand Total
        decimal tempGrandTotal =
            (tempSubTotal + tempTaxTotal + tempChargeTotal) - tempDiscountTotal;

        // Assign outputs
        orderItems = tempOrderItems;
        orderDiscounts = tempOrderDiscounts;
        orderTaxes = tempOrderTaxes;
        orderCharges = tempOrderCharges;
        subTotal = tempSubTotal;
        discountTotal = tempDiscountTotal;
        taxTotal = tempTaxTotal;
        chargeTotal = tempChargeTotal;
        grandTotal = tempGrandTotal;
    }
}