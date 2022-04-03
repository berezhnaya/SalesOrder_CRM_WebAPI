using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Helpers
{
    public class ConverterPrice
    {
        public static double Calculate(ProductPriceModel productPriceLevel, double defaultCurrencyRate, double priceCurrencyRate)
        {
            var price = 0.0;

            if (productPriceLevel.DefaultUomCoefficient == productPriceLevel.ItemUomCoefficient)
            {
                price = productPriceLevel.Price;
            }
            //ИСКЛЮЧЕНИЕ ДЛЯ ПЕРЕВОДА В КГ 
            else if (productPriceLevel.DefaultUomName == "КГ" && productPriceLevel.DefaultUomCoefficient > 1)
            {
                price = productPriceLevel.Price / Convert.ToDouble(productPriceLevel.ItemUomCoefficient) * Convert.ToDouble(productPriceLevel.DefaultUomCoefficient);
            }
            else if (productPriceLevel.DefaultUomCoefficient != 1)
            {
                price = productPriceLevel.Price * Convert.ToDouble(productPriceLevel.DefaultUomCoefficient);
            }
            else
            {
                price = productPriceLevel.Price / Convert.ToDouble(productPriceLevel.ItemUomCoefficient);
            }
            if (price != 0)
            {
                price = Math.Round((price * (double)priceCurrencyRate) / (double)defaultCurrencyRate, 2);
            }
            return price;
        }
    }
}