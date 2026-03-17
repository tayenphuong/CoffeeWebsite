using System.Text.Json.Serialization;

namespace WebBanNuocMVC.DesignPatterns.Strategy
{
    [JsonDerivedType(typeof(PercentageDiscount), typeDiscriminator: "percentage")]
    [JsonDerivedType(typeof(FixedAmountDiscount), typeDiscriminator: "fixed")]
    [JsonDerivedType(typeof(NoDiscount), typeDiscriminator: "none")]
    public interface IDiscountStrategy
    {
        decimal CalculateDiscount(decimal totalAmount);
        string GetDescription();
    }
}
