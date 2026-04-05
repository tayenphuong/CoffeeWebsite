namespace WebBanNuocMVC.DesignPatterns.Chain
{
    public class CheckoutChainResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CreatedOrderId { get; set; }
        public decimal TotalAmount { get; set; }

        public static CheckoutChainResult Fail(string message)
        {
            return new CheckoutChainResult
            {
                Succeeded = false,
                Message = message
            };
        }

        public static CheckoutChainResult Success(string message, int? createdOrderId = null, decimal totalAmount = 0)
        {
            return new CheckoutChainResult
            {
                Succeeded = true,
                Message = message,
                CreatedOrderId = createdOrderId,
                TotalAmount = totalAmount
            };
        }
    }
}