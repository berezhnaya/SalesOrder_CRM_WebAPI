using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models.IG.Marketing
{
    public class MarketingStockDetailModel
    {
        /// <summary>
        /// Id акции
        /// </summary> 
        public Guid StockId { get; set; }
        /// <summary>
        /// Название акции
        /// </summary> 
        public string Name { get; set; }
        /// <summary>
        /// Подробное описание акции
        /// </summary> 
        public string DetailDescription { get; set; }
        /// <summary>
        /// Комплект продуктов
        /// </summary> 
        public bool IsSet { get; set; }
        /// <summary>
        /// Список продуктов
        /// </summary> 
        public List<ProductModel> ProductList { get; set; }
    }
}