using System;

namespace Kesco.App.Web.V4.ExchangeRate
{
    /// <summary>
    ///     Класс для хранения параметров контрола PeriodTimePicker
    /// </summary>
    public struct PeriodTimePickerParameters
    {
        public string ParamPeriod;
        public string ParamDateFrom;
        public string ParamDateTo;
        public DateTime DefaultDateFrom;
        public DateTime DefaultDateTo;

        /// <summary>
        ///     Инициализирует новый экземпляр класса PeriodTimePicker
        /// </summary>
        /// <param name="paramPeriod">Название параметра Тип периода</param>
        /// <param name="paramDateFrom">Название параметра Дата от</param>
        /// <param name="paramDateTo">Название параметра Дата по</param>
        /// <param name="defaultDateFrom">Значение по умолчанию Дата от</param>
        /// <param name="defaultDateTo">Значение по умолчанию Дата по</param>
        public PeriodTimePickerParameters(string paramPeriod, string paramDateFrom, string paramDateTo,
            DateTime defaultDateFrom, DateTime defaultDateTo)
        {
            ParamPeriod = paramPeriod;
            ParamDateFrom = paramDateFrom;
            ParamDateTo = paramDateTo;
            DefaultDateFrom = defaultDateFrom;
            DefaultDateTo = defaultDateTo;
        }
    }
}