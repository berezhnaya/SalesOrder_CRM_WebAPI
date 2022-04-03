namespace IG.CRM.API.Models
{
    public class ProductRecommendPriceModel
    {
        public string ProductCode { get; set; }       
        public double RecommendedPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string ItemUomCode { get; set; }
    }
}