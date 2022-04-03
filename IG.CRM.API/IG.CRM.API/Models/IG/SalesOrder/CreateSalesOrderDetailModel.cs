using System;

namespace IG.CRM.API.Models.IG
{
    public class CreateSalesOrderDetailModel
    {
        /// <summary>
        /// Код продукта
        /// </summary> 
        public string ProductCode { get; set; }
        /// <summary>
        /// Кол-во продукта
        /// </summary> 
        public decimal? Quantity { get; set; }
        /// <summary>
        /// Код ед. изм
        /// </summary> 
        public Guid? ItemUomId { get; set; }
        /// <summary>
        /// Id акции
        /// </summary> 
        public Guid? StockId { get; set; }
    }
}