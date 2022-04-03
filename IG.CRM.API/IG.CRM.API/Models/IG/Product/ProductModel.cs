namespace IG.CRM.API.Models.IG
{
    public class ProductModel
    {
        public string ProductCode { get; set; }
        public double Price { get; set; }
        public double RecommendedPrice { get; set; }
        public string CurrencyCode { get; set; }
        public string ItemUomCode { get; set; }
    }
}