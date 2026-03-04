namespace OrderService.Services.Internal
{
    public static class OrderCalculationHelpers
    {
        /// <summary>
        /// Distribute a total amount proportionally across items based on their taxable value.
        /// Ensures sum of distributed amounts equals totalAmount.
        /// </summary>
        public static List<decimal> DistributeAmountProportionally(
            List<decimal> itemValues,
            decimal totalAmount)
        {
            var distributed = new List<decimal>();
            decimal sum = itemValues.Sum();

            if (sum == 0)
            {
                // If all items are zero, split equally
                decimal equalShare = Math.Round(totalAmount / itemValues.Count, 2);
                for (int i = 0; i < itemValues.Count; i++)
                    distributed.Add(equalShare);

                // Adjust last item to match exactly
                decimal diff = totalAmount - distributed.Sum();
                distributed[^1] += diff;

                return distributed;
            }

            decimal runningTotal = 0;
            for (int i = 0; i < itemValues.Count; i++)
            {
                decimal share = Math.Round(totalAmount * (itemValues[i] / sum), 2);
                distributed.Add(share);
                runningTotal += share;
            }

            // Adjust last item to ensure total matches exactly
            decimal adjustment = totalAmount - distributed.Sum();
            distributed[^1] += adjustment;

            return distributed;
        }
    }
}
