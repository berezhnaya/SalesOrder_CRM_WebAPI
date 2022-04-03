using System;
using System.Collections.Generic;

namespace IG.CRM.API.Models.IG.Marketing
{
    public class MarketingStockModel
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Названия
        /// </summary> 
        public string Name { get; set; }
        /// <summary>
        /// Описание
        /// </summary> 
        public string Description { get; set; }
        /// <summary>
        /// Изображение
        /// </summary> 
        public List<string> Images { get; set; }
        /// <summary>
        /// Комплект
        /// </summary> 
        public bool IsSet { get; set; }
    }
}