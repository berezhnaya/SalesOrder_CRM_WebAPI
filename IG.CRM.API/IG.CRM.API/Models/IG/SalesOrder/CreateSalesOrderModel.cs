using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.Models
{
    public class CreateSalesOrderModel
    {
        /// <summary>
        /// Код клиента для отгрузки
        /// </summary> 
        public Guid? ShipmentAccountId { get; set; }

        /// <summary>
        /// Код клиента для выставления счета
        /// </summary> 
        public Guid? PaymentAccountId { get; set; }

        /// <summary>
        /// Требуемая дата отгрузки
        /// </summary> 
        public string DeliveryDate { get; set; }

        /// <summary>
        /// Способ доставки
        /// </summary> 
        public int? DeliveryType { get; set; }

        /// <summary>
        /// Способ оплаты
        /// </summary> 
        public int? AccountingType { get; set; }

        /// <summary>
        /// Условие оплаты
        /// </summary> 
        public int? PaymentType { get; set; }

        /// <summary>
        /// Код валюты
        /// </summary> 
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Примечание к заказу
        /// </summary> 
        public string NoteOrder { get; set; }

        /// <summary>
        /// Внешний код заказа
        /// </summary> 
        public string ExternalCode { get; set; }

        /// <summary>
        /// Адрес доставки
        /// </summary> 
        public Guid? AccountDeliveryAddressId { get; set; }

        /// <summary>
        /// Список продуктов
        /// </summary> 
        public List<CreateSalesOrderDetailModel> Products { get; set; }
    }
}