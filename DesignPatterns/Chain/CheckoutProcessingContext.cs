using System.Collections.Generic;

namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CheckoutProcessingContext
    {
        public CheckoutChainRequest Request { get; set; } = new();

        // Giá hiện tại lấy từ DB, không tin giá từ client/session
        public Dictionary<int, decimal> CurrentPrices { get; set; } = new();

        // Tồn kho hiện tại lấy từ DB
        public Dictionary<int, int> CurrentStocks { get; set; } = new();

        public decimal CalculatedTotal { get; set; }
        public int? CreatedOrderId { get; set; }
    }
}