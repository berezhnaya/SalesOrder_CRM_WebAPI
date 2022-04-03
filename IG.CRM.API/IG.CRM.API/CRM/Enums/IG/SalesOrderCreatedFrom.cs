namespace IG.CRM.API.CRM.Enums.IG
{
    public enum SalesOrderCreatedFrom
    {
        /// <summary>
        /// Создан из CRM
        /// </summary>
        CRM = 1,
        /// <summary>
        /// Из моб. заказа
        /// </summary>
        MobileForm = 2,
        /// <summary>
        /// Через API
        /// </summary>
        API = 3,
        /// <summary>
        /// Из телеграм-бота
        /// </summary>
        TelegramBot = 4,
        /// <summary>
        /// Импортирован из EDIN
        /// </summary>
        Edin = 5
    }
}