using System;

namespace IG.CRM.API.Models.IG
{
    public class ProductPriceModel
    {
        public string ProductCode { get; set; } = "";
        public Guid? ItemUomId { get; set; }
        public decimal ItemUomCoefficient { get; set; } = 0;
        public decimal DefaultUomCoefficient { get; set; } = 0;
        public string DefaultUomName { get; set; } = "";
        public Guid? CurrencyId { get; set; }
        public double Price { get; set; }      
    }
}


                                        