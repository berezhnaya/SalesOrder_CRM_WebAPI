namespace IG.CRM.API.CRM.Enums.IG
{
    public enum ShippingMethodCode
    {
        /// <summary>
        /// Самовывоз
        /// </summary> 
        NoDelivery = 1,
        /// <summary>
        ///Перевозчик
        /// </summary> 
        DeliveryService = 2,
        /// <summary>
        /// Наемный транспорт
        /// </summary> 
        HiredTransport = 3,
        /// <summary>
        /// Собственный транспорт
        /// </summary> 
        OwnTransport = 4
    }
}