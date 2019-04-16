using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Web.DBSelect.V4;

namespace Kesco.App.Web.V4.ExchangeRate
{
    /// <summary>
    ///     Форма Курсы валют
    /// </summary>
    public partial class Search : EntityPage
    {
        private const string CheckBoxAllHtmlId = "currencyAllCheckBox";
        private const string CheckBoxAllAvgHtmlId = "currencyAllAvgCheckBox";
        private const string IndeterminateCssClass = "indeterminate";
        private const string CheckBoxCssClass = "currencyValueCheckBox";
        private const string CheckBoxAvgCssClass = "currencyValueAvgCheckBox";
        private const string CurrenciesChangedHandlerName = "CurrenciesChanged";
        private const string CurrenciesAvgChangedHandlerName = "CurrenciesAvgChanged";

        private readonly string _currencyTop = ConfigurationManager.AppSettings["CURRENCY_TOP"];
        private readonly string _currencyNot = ConfigurationManager.AppSettings["CURRENCY_NOT"];

        private Lib.Web.Settings.Parameters.AppParamsManager _paramsManager;
        private Dictionary<int, Currency> _currencies = new Dictionary<int, Currency>();
        private string _idsCheckedCurrencies = string.Empty;
        private string _idsCheckedCurrenciesAvg = string.Empty;
        private int _rateId;
        private bool _allowEdit;


        /// <summary>
        ///     Ссылка на справку
        /// </summary>
        public override string HelpUrl { get; set; } = "hlp/help.htm?id=1";

        /// <summary>
        ///     Сохранить параметры пользователя
        /// </summary>
        public override void SaveParameters()
        {
            var parameters = new Dictionary<string, string>()
            {
                { "ExRatePeriod", periodRate.ValuePeriod },
                { "ExRatePeriodAvg", periodRateAvg.ValuePeriod },
                { "ExRatePeriodCross", periodRateCross.ValuePeriod },
                { "ExRateDateFrom", periodRate.ValueDateFrom.HasValue ? periodRate.ValueFromODBC : string.Empty},
                { "ExRateDateFromAvg", periodRateAvg.ValueDateFrom.HasValue ? periodRateAvg.ValueFromODBC : string.Empty},
                { "ExRateDateFromCross", periodRateCross.ValueDateFrom.HasValue ? periodRateCross.ValueFromODBC : string.Empty},
                { "ExRateDateTo", periodRate.ValueDateTo.HasValue ? periodRate.ValueToODBC : string.Empty},
                { "ExRateDateToAvg", periodRateAvg.ValueDateTo.HasValue ? periodRateAvg.ValueToODBC : string.Empty},
                { "ExRateDateToCross", periodRateCross.ValueDateTo.HasValue ? periodRateCross.ValueToODBC : string.Empty},
                { "ExRateCurrency", _idsCheckedCurrencies },
                { "ExRateCurrencyAvg", _idsCheckedCurrenciesAvg },
                { "ExRateCurrencyCrsSrc", dbsCurrencyCrossSource.Value },
                { "ExRateCurrencyCrsTgt", dbsCurrencyCrossTarget.Value }
            };

            foreach (var key in parameters.Keys)
            {
                _paramsManager.SetDbParameterValue(key, parameters[key]);
            }

            _paramsManager.SaveParams();
        }

        /// <summary>
        ///     Инициализирует новый экземпляр класса Search
        /// </summary>
        public Search()
        {
            LogoImage = "logo_exrate.gif";
        }

        /// <summary>
        ///     Обработчик события преинициализации страницы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Параметр события</param>
        protected void Page_PreInit(object sender, EventArgs e)
        {

        }

        /// <summary>
        ///     Обработчик события загрузки страницы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Параметр события</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            var employee = new Employee(true);

            // редактирование разрешено для роли "Ответственный за курсы валют"
            _allowEdit = employee.HasRole(14);
            
            IsRememberWindowProperties = true;
            WindowParameters = new WindowParameters("ExRateWndLeft", "ExRateWndTop", "ExRateWndWidth", "ExRateWndHeight");
            IsSilverLight = false;

            var _params = new StringCollection {
                "ExRateDateFrom", "ExRateDateTo", "ExRatePeriod",
                "ExRateDateFromAvg", "ExRateDateToAvg", "ExRatePeriodAvg",
                "ExRateDateFromCross", "ExRateDateToCross", "ExRatePeriodCross",
                "ExRateCurrency", "ExRateCurrencyAvg", "ExRateCurrencyCrsSrc", "ExRateCurrencyCrsTgt"
            };

            _paramsManager = new Lib.Web.Settings.Parameters.AppParamsManager(ClId, _params);

            //JS.Write("var msgCurrencyNotSelected = '{0}';", Resx.GetString("ExRate_msgCurrencyNotSelected"));

            _currencies = Currency.GetAllCurrencies()
                .Where(x => !_currencyNot.Split(',').Contains(x.Key.ToString()))
                .OrderBy(x => x.Value.Name)
                .ToDictionary(x => x.Key, x => x.Value);

            _idsCheckedCurrencies = _paramsManager.GetDbParameterValue("ExRateCurrency") ?? _currencyTop;
            _idsCheckedCurrenciesAvg = _paramsManager.GetDbParameterValue("ExRateCurrencyAvg") ?? _currencyTop;

            InitTabRate();
            InitTabRateAvg();
            InitTabRateCross();

            var form = Request.QueryString["form"];
            switch (form)
            {
                case "avg":
                    JS.Write(@"tabActivate(1);$('#tabs2').focus();");
                    break;
                case "cross":
                    JS.Write(@"tabActivate(2);$('#tabs3').focus();");
                    break;
                default:
                    JS.Write(@"$('#tabs1').focus();");
                    break;
            }

            JS.Write("checkBoxCssClass='{0}';checkBoxAvgCssClass='{1}';", CheckBoxCssClass, CheckBoxAvgCssClass);
        }

        /// <summary>
        /// Инициализация вкладки "Курсы валют"
        /// </summary>
        private void InitTabRate()
        {
            btnAddRate.Visible = _allowEdit;
            btnAddRate.Text = Resx.GetString("ExRate_btnAddPosition") + "&nbsp;(Ins)";
            btnAddRate.OnClick = "cmd('cmd','RateAdd');";
            periodRate.Changed += delegate { SetReadOnlyGrid("divGridRate"); };
            btnFilterRates.Text = Resx.GetString("ExRate_lblSelect");
            btnFilterRates.OnClick = "btnFilterRates_Click()";

            InitComboBoxRate();

            var pickerParams = new PeriodTimePickerParameters
            {
                ParamPeriod = "ExRatePeriod",
                ParamDateFrom = "ExRateDateFrom",
                DefaultDateFrom = DateTime.Now,
                ParamDateTo = "ExRateDateTo",
                DefaultDateTo = DateTime.Now
            };

            InitFilterPeriod(periodRate, pickerParams);
            InitGridRate();
            LoadDataGridRate();
        }

        /// <summary>
        ///  Инициализация вкладки "Средние курсы"
        /// </summary>
        private void InitTabRateAvg()
        {
            periodRateAvg.Changed += delegate { SetReadOnlyGrid("divGridRateAvg"); };
            btnFilterAvgRates.Text = Resx.GetString("ExRate_lblSelect");
            btnFilterAvgRates.OnClick = "btnFilterAvgRates_Click()";

            var pickerParamsAvg = new PeriodTimePickerParameters
            {
                ParamPeriod = "ExRatePeriodAvg",
                ParamDateFrom = "ExRateDateFromAvg",
                DefaultDateFrom = DateTime.Now.AddMonths(-1),
                ParamDateTo = "ExRateDateToAvg",
                DefaultDateTo = DateTime.Now
            };

            InitFilterPeriod(periodRateAvg, pickerParamsAvg);
            InitGridRateAvg();
            //LoadDataGridRateAvg();
        }

        /// <summary>
        ///  Инициализация вкладки "Кросс-курсы"
        /// </summary>
        private void InitTabRateCross()
        {
            periodRateCross.Changed += delegate { SetReadOnlyGrid("divGridRateCross"); };
            btnFilterCrossRates.Text = Resx.GetString("ExRate_lblSelect");
            btnFilterCrossRates.OnClick = "btnFilterCrossRates_Click()";

            InitComboBoxRateCrossSource();
            InitComboBoxRateCrossTarget();

            var pickerParamsCross = new PeriodTimePickerParameters
            {
                ParamPeriod = "ExRatePeriodCross",
                ParamDateFrom = "ExRateDateFromCross",
                DefaultDateFrom = DateTime.Now.AddDays(-10),
                ParamDateTo = "ExRateDateToCross",
                DefaultDateTo = DateTime.Now
            };

            InitFilterPeriod(periodRateCross, pickerParamsCross);
            InitGridRateCross();
            //LoadDataGridRateCross();

            dbsCurrencyCrossSource.BeforeSearch += DbsCurrencyCrossSource_BeforeSearch;
            dbsCurrencyCrossTarget.BeforeSearch += DbsCurrencyCrossTarget_BeforeSearch;

            btnInverseCurrencies.OnClick += "btnInverseCurrencies_Click();";
            btnInverseCurrencies.Title = Resx.GetString("ExRate_lblSwapCurrencies");

            btnInverseCurrencies.IconJQueryUI = ButtonIconsEnum.Swap;
        }

        private void DbsCurrencyCrossSource_BeforeSearch(object sender)
        {
            DbsFilterItems(dbsCurrencyCrossSource, dbsCurrencyCrossTarget.ValueInt);
            SetReadOnlyGrid("divGridRateCross");
        }

        private void DbsCurrencyCrossTarget_BeforeSearch(object sender)
        {
            DbsFilterItems(dbsCurrencyCrossTarget, dbsCurrencyCrossSource.ValueInt);
            SetReadOnlyGrid("divGridRateCross");
        }

        private void DbsFilterItems(DBSCurrency control, int? itemId)
        {
            control.Filter.CurrencyId.Clear();
            control.Filter.CurrencyId.Add(_currencyNot);
            if (itemId.HasValue) control.Filter.CurrencyId.Add(itemId.ToString());
            control.Filter.CurrencyId.HowSearch = SearchIds.NotIn;
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Название команды</param>
        /// <param name="param">Параметры</param>
        /// 
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            List<string> validList;
            switch (cmd)
            {
                case "FilterRates":
                    _idsCheckedCurrencies = param["Ids"];
                    LoadDataGridRate();
                    JS.Write("Wait.render(false);");
                    break;
                case "FilterRatesAvg":
                    _idsCheckedCurrenciesAvg = param["Ids"];
                    LoadDataGridRateAvg();
                    JS.Write("Wait.render(false);");
                    break;
                case "FilterRatesCross":
                    LoadDataGridRateCross();
                    JS.Write("Wait.render(false);");
                    break;
                case "RateAdd":
                    _rateId = 0;
                    dbsCurrencyEdit.ValueInt = null;
                    dpRateDate.Value = DateTime.Now.ToString();
                    numRateValue.Value = "";
                    numRateUnits.Value = "1";
                    JS.Write("rateEdit_dialogShow('{0}','{1}','{2}','{3}');", Resx.GetString("ExRate_lblAddingRate"), Resx.GetString("cmdSave"), Resx.GetString("cmdCancel"), cmd);
                    break;
                case "RateEdit":
                    _rateId = int.Parse(param["RateId"]);
                    var sqlParams = new Dictionary<string, object> { { "@id", _rateId } };
                    var dt = DBManager.GetData(SQLQueries.SELECT_КурсВалюты, Config.DS_person, CommandType.Text, sqlParams);
                    if (dt.Rows.Count > 0)
                    {
                        dbsCurrencyEdit.ValueInt = (int)dt.Rows[0]["КодВалюты"];
                        dpRateDate.Value = dt.Rows[0]["ДатаКурса"].ToString();
                        numRateValue.Value = dt.Rows[0]["Курс"].ToString();
                        numRateUnits.Value = dt.Rows[0]["Единиц"].ToString();
                    }
                    JS.Write("rateEdit_dialogShow('{0}','{1}','{2}','{3}');", Resx.GetString("ExRate_lblEditingRate"), Resx.GetString("cmdSave"), Resx.GetString("cmdCancel"), cmd);
                    break;
                case "RateDelete":
                    if (!param["RateId"].IsNullEmptyOrZero())
                    {
                        var rateId = Convert.ToInt32(param["RateId"]);
                        DBManager.ExecuteNonQuery(SQLQueries.DELETE_КурсВалют, rateId, CommandType.Text, Config.DS_person);
                        LoadDataGridRate();
                    }
                    break;
                case "SetRate":
                    if (ValidateDataRate(out validList))
                    {
                        SaveDataRate();
                    }
                    else
                    {
                        RenderErrors(validList, "<br/> " + Resx.GetString("_Msg_НеСохраняется"));
                    }
                    break;
                case "CurrenciesChanged":
                    _idsCheckedCurrencies = param["Ids"];
                    SetReadOnlyGrid("divGridRate");
                    break;
                case "CurrenciesAvgChanged":
                    _idsCheckedCurrenciesAvg = param["Ids"];
                    SetReadOnlyGrid("divGridRateAvg");
                    break;
                case "InverseCurrencies":
                    string oldValueSource = dbsCurrencyCrossSource.Value;
                    string oldValueTarget = dbsCurrencyCrossTarget.Value;
                    string newValueSource = param["targetId"];
                    string newValueTarget = param["sourceId"];
                    dbsCurrencyCrossSource.Value = newValueSource;
                    dbsCurrencyCrossTarget.Value = newValueTarget;
                    dbsCurrencyCrossSource.OnValueChanged(new ValueChangedEventArgs(newValueSource, oldValueSource));
                    dbsCurrencyCrossTarget.OnValueChanged(new ValueChangedEventArgs(newValueTarget, oldValueTarget));
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        /// <summary>
        ///     Отрисовка верхней панели меню
        /// </summary>
        /// <returns>Строка, получаемая из StringWriter</returns>
        protected string RenderDocumentHeader()
        {
            using (var w = new StringWriter())
            {
                try
                {
                    ClearMenuButtons();
                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(Resx.GetString("ExRate_errFailedGenerateButtons") + ": " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        /// <summary>
        ///     Сформировать сообщение об ошибках
        /// </summary>
        protected void RenderErrors(List<string> li, string text = null)
        {
            using (var w = new StringWriter())
            {
                foreach (var l in li)
                    w.Write("<div style='white-space: nowrap;'>{0}</div>", l);

                ShowMessage(w + text, Resx.GetString("errIncorrectlyFilledField"), MessageStatus.Error, "", 500, 200);
            }
        }

        /// <summary>
        /// Отрисовка фильтра Список валют
        /// </summary>
        protected string RenderCheckBoxListCurrency(int tabNum)
        {
            using (var w = new StringWriter())
            {
                try
                {
                    switch (tabNum)
                    {
                        case 1:
                            WriteMarkupTableCurrency(w, CheckBoxCssClass, CurrenciesChangedHandlerName, CheckBoxAllHtmlId, _idsCheckedCurrencies);
                            break;
                        case 2:
                            WriteMarkupTableCurrency(w, CheckBoxAvgCssClass, CurrenciesAvgChangedHandlerName, CheckBoxAllAvgHtmlId, _idsCheckedCurrenciesAvg);
                            break;
                    }
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(Resx.GetString("ExRate_errFailedGenerateCurrencyList") + ": " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        /// <summary>
        ///     Записать разметку фильтра Список валют
        /// </summary>
        /// <param name="w">Строка, получаемая из StringWriter</param>
        /// <param name="checkBoxCssClass">Класс стилей для CheckBox</param>
        /// <param name="handlerName">Имя обработчика нажатия на CheckBox</param>
        /// <param name="checkBoxAllHtmlId">Идентификатор CheckBox Выделить все</param>
        /// <param name="idsCheckedCurrencies">Список идентификаторов отмеченных валют</param>

        private void WriteMarkupTableCurrency(StringWriter w, string checkBoxCssClass, string handlerName, string checkBoxAllHtmlId, string idsCheckedCurrencies)
        {
            var idsTopList = _currencyTop.Split(',').Select(int.Parse).ToList();

            var idsList = _currencies.Select(x => x.Key).ToList();

            idsList = idsTopList.Union(idsList).ToList();

            var idsCheckedList = string.IsNullOrEmpty(idsCheckedCurrencies) 
                ? new List<int>() : idsCheckedCurrencies.Split(',').Select(int.Parse).ToList();

            string stateChecked = string.Empty;

            if (idsCheckedList.Count == idsList.Count)
                stateChecked = "checked='checked'";
            else if (idsCheckedList.Count > 0)
                stateChecked = $"class='{IndeterminateCssClass}'";

            w.Write("<div class='v4DivTable'>");

            w.Write("<div class='v4DivTableRow'>");

            w.Write("<div class='v4DivTableCell v4PaddingCell'>");

            string strOnClickAll = string.Format(
                "v4_columnValuesChecked(this.checked,'{0}');var ids=rate_getCurrencyValuesFilter('{0}');cmd('cmd','{1}','Ids',ids);",
                checkBoxCssClass, handlerName);
            
            w.Write("<input type='checkbox' id='{0}' onclick =\"{1}\" {2}>", 
                checkBoxAllHtmlId, strOnClickAll, stateChecked);

            w.Write("</div>");

            w.Write("<div class='v4DivTableCell v4PaddingCell' style='text-align:left; white-space: nowrap;'>");

            w.Write("({0})", Resx.GetString("lblSelectAll"));

            w.Write("</div>");

            w.Write("</div>");

            foreach (var id in idsList)
            {
                bool isChecked = idsCheckedList.Any(x => x.Equals(id));

                w.Write("<div class='v4DivTableRow'>");

                w.Write("<div class='v4DivTableCell v4PaddingCell'>");

                string strOnClick = string.Format(
                    "v4_setStateCheckAllValues('{0}','{1}');var ids=rate_getCurrencyValuesFilter('{1}');cmd('cmd','{2}','Ids',ids);",
                    checkBoxAllHtmlId, checkBoxCssClass, handlerName);

                w.Write("<input type='checkbox' class='{0}' data-id='{1}' onclick=\"{2}\" {3}>",
                    checkBoxCssClass, id, strOnClick, isChecked ? "checked='checked'" : "");

                w.Write("</div>");

                w.Write("<div class='v4DivTableCell v4PaddingCell' style='text-align:left; white-space: nowrap;'>");

                w.Write("<label for=''>{0}</label>", GetLocalCurrencyName(_currencies[id]));

                w.Write("</div>");

                w.Write("</div>");
            }

            w.Write("</div>");
        }

        /// <summary>
        /// Получение локализованного названия валюты
        /// </summary>
        /// <param name="currency">Экземпляр валюты</param>
        /// <returns>Название</returns>
        private string GetLocalCurrencyName(Currency currency)
        {
            if (IsRusLocal || string.IsNullOrWhiteSpace(currency.ResourceLat))
                return currency.Name;
            else
                return currency.ResourceLat;
        }

        /// <summary>
        ///     Инициализация фильтра Период
        /// </summary>
        /// <param name="picker">Контрол</param>
        /// <param name="parameters">Параметры контрола</param>
        private void InitFilterPeriod(PeriodTimePicker picker, PeriodTimePickerParameters parameters)
        {
            var period = _paramsManager.GetDbParameterValue(parameters.ParamPeriod);
            var strDateFrom = _paramsManager.GetDbParameterValue(parameters.ParamDateFrom);
            var strDateTo = _paramsManager.GetDbParameterValue(parameters.ParamDateTo);
            DateTime dateFrom = !string.IsNullOrEmpty(strDateFrom) ? Lib.ConvertExtention.Convert.Str2DateTime(strDateFrom) : parameters.DefaultDateFrom;
            DateTime dateTo = !string.IsNullOrEmpty(strDateTo) ? Lib.ConvertExtention.Convert.Str2DateTime(strDateTo) : parameters.DefaultDateTo;
            picker.ValuePeriod = period;
            picker.ValueDateFrom = dateFrom;
            picker.ValueDateTo = dateTo;
        }

        /// <summary>
        ///     Инициализация таблицы Курсы валют
        /// </summary>
        private void InitGridRate()
        {
            gridRate.ShowGroupPanel = false;
            gridRate.ShowFilterOptions = false;
            gridRate.RowsPerPage = 40;

            if (_allowEdit)
            {
                gridRate.ExistServiceColumn = true;
                gridRate.SetServiceColumnAdd("rate_add", Resx.GetString("ExRate_btnAddPosition"));

                var condition = new List<object> {(byte) 0};
                gridRate.RenderConditionServiceColumnDelete.Add("Состояние", condition);
                gridRate.RenderConditionServiceColumnEdit.Add("Состояние", condition);

                gridRate.SetServiceColumnDelete("rate_delete", new List<string> {"КодКурсаВалюты"},
                    new List<string> {"РесурсРус", "ДатаКурса"}, Resx.GetString("ExRate_btnDeletePosition"));

                gridRate.SetServiceColumnEdit("rate_edit", new List<string> {"КодКурсаВалюты"},
                    Resx.GetString("ExRate_btnEditPosition"));
            }
        }

        /// <summary>
        ///     Инициализация таблицы Средний курс
        /// </summary>
        private void InitGridRateAvg()
        {
            gridRateAvg.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            gridRateAvg.ShowGroupPanel = false;
            gridRateAvg.ShowFilterOptions = false;
            gridRateAvg.ShowPageBar = false;
        }

        /// <summary>
        ///     Инициализация таблицы Кросс-курс
        /// </summary>
        private void InitGridRateCross()
        {
            gridRateCross.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            gridRateCross.ShowGroupPanel = false;
            gridRateCross.ShowFilterOptions = false;
            gridRateCross.RowsPerPage = 45;
        }

        /// <summary>
        ///     Инициализация выпадающего списка с выбором валюты на форме редактирования
        /// </summary>
        private void InitComboBoxRate()
        {
            dbsCurrencyEdit.Filter.CurrencyId.Add(_currencyNot);
            dbsCurrencyEdit.Filter.CurrencyId.HowSearch = SearchIds.NotIn;
        }

        /// <summary>
        ///     Инициализация выпадающего списка с выбором базовой валюты на вкладке Кросс-курсы
        /// </summary>
        private void InitComboBoxRateCrossSource()
        {
            int id;
            if (int.TryParse(_paramsManager.GetDbParameterValue("ExRateCurrencyCrsSrc"), out id))
                dbsCurrencyCrossSource.ValueInt = id;
        }

        /// <summary>
        ///     Инициализация выпадающего списка с выбором валюты котировкина вкладке Кросс-курсы
        /// </summary>
        private void InitComboBoxRateCrossTarget()
        {
            int id;
            if (int.TryParse(_paramsManager.GetDbParameterValue("ExRateCurrencyCrsTgt"), out id))
                dbsCurrencyCrossTarget.ValueInt = id;
        }

        /// <summary>
        ///     Получение параметров SQL-запроса для таблицы Курсы валют
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetSqlParamsForGridRate()
        {
            var sqlParams = new Dictionary<string, object>();
            var valueDateFrom = periodRate.ValueDateFrom.HasValue ? periodRate.ValueFromODBC : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRate.ValueDateTo.HasValue ? periodRate.ValueToODBC : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

            sqlParams.Add("@КодыВалют", _idsCheckedCurrencies);
            sqlParams.Add("@ДатаКурсаОт", valueDateFrom);
            sqlParams.Add("@ДатаКурсаПо", valueDateTo);

            return sqlParams;
        }

        /// <summary>
        ///     Получение параметров SQL-запроса для таблицы Средний курс
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetSqlParamsForGridRateAvg()
        {
            var sqlParams = new Dictionary<string, object>();
            var valueDateFrom = periodRateAvg.ValueDateFrom.HasValue ? periodRateAvg.ValueFromODBC : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRateAvg.ValueDateTo.HasValue ? periodRateAvg.ValueToODBC : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

            sqlParams.Add("@КодыВалют", _idsCheckedCurrenciesAvg);
            sqlParams.Add("@ДатаКурсаОт", valueDateFrom);
            sqlParams.Add("@ДатаКурсаПо", valueDateTo);

            return sqlParams;
        }

        /// <summary>
        /// Получение параметров SQL-запроса для таблицы Кросс-курс
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetSqlParamsForGridRateCross()
        {
            var sqlParams = new Dictionary<string, object>();
            var valueDateFrom = periodRateCross.ValueDateFrom.HasValue ? periodRateCross.ValueFromODBC : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRateCross.ValueDateTo.HasValue ? periodRateCross.ValueToODBC : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

            sqlParams.Add("@КодВалютыИсточник", dbsCurrencyCrossSource.Value);
            sqlParams.Add("@КодВалютыЦель", dbsCurrencyCrossTarget.Value);
            sqlParams.Add("@ДатаКурсаОт", valueDateFrom);
            sqlParams.Add("@ДатаКурсаПо", valueDateTo);

            return sqlParams;
        }

        /// <summary>
        ///     Задание настроек для таблицы Курсы валют
        /// </summary>
        private void SetSettingsGridRate()
        {
            var listColumnVisible = new List<string>
            {
                "КодКурсаВалюты",
                "КодВалюты",
                "Состояние",
                "КодСотрудника",
                IsRusLocal ? "РесурсЛат" : "РесурсРус",
                IsRusLocal ? "FIO" : "ФИО"
            };
            gridRate.Settings.SetColumnDisplayVisible(listColumnVisible, false);
            gridRate.Settings.SetColumnHeaderAlias(IsRusLocal ? "РесурсРус" : "РесурсЛат", Resx.GetString("ExRate_lblCurrency"));
            gridRate.Settings.SetColumnNoWrapText(IsRusLocal ? "РесурсРус" : "РесурсЛат");
            gridRate.Settings.SetColumnHeaderAlias("ДатаКурса", Resx.GetString("ExRate_lblRateDate"));
            gridRate.Settings.SetColumnFormat("ДатаКурса", "dd.MM.yyyy");
            gridRate.Settings.SetColumnHeaderAlias("Курс", Resx.GetString("ExRate_lblRateValue"));
            gridRate.Settings.SetColumnFormat("Курс", "N");
            gridRate.Settings.SetColumnFormatDefaultScale("Курс", 4);
            gridRate.Settings.SetColumnHeaderAlias("Единиц", Resx.GetString("ExRate_lblRateUnits"));
            gridRate.Settings.SetColumnHeaderAlias(IsRusLocal ? "ФИО" : "FIO", Resx.GetString("ExRate_lblRateChangedPerson"));
            gridRate.Settings.SetColumnNoWrapText(IsRusLocal ? "ФИО" : "FIO");
            gridRate.Settings.SetColumnHrefEmployee(IsRusLocal ? "ФИО" : "FIO", "КодСотрудника");
            gridRate.Settings.SetColumnHeaderAlias("Изменено", Resx.GetString("ExRate_lblRateChangedDate"));
            gridRate.Settings.SetColumnNoWrapText("Изменено");
            gridRate.Settings.SetColumnLocalTime("Изменено");
        }

        /// <summary>
        ///     Задание настроек для таблицы Средний курс
        /// </summary>
        private void SetSettingsGridRateAvg()
        {
            var listColumnVisible = new List<string>
            {
                "КодВалюты",
                IsRusLocal ? "РесурсЛат" : "РесурсРус"
            };
            gridRateAvg.Settings.SetColumnDisplayVisible(listColumnVisible, false);
            gridRateAvg.Settings.SetColumnHeaderAlias(IsRusLocal ? "РесурсРус" : "РесурсЛат", Resx.GetString("ExRate_lblCurrency"));
            gridRateAvg.Settings.SetColumnNoWrapText(IsRusLocal ? "РесурсРус" : "РесурсЛат");
            gridRateAvg.Settings.SetColumnHeaderAlias("Курс", Resx.GetString("ExRate_lblExRate"));
            gridRateAvg.Settings.SetColumnFormat("Курс", "N");
            gridRateAvg.Settings.SetColumnFormatDefaultScale("Курс", 4);
        }

        /// <summary>
        ///     Задание настроек для таблицы Кросс-курс
        /// </summary>
        private void SetSettingsGridRateCross()
        {
            gridRateCross.Settings.SetColumnHeaderAlias("Дата", Resx.GetString("ExRate_lblDate"));
            gridRateCross.Settings.SetColumnFormat("Дата", "dd.MM.yyyy");
            gridRateCross.Settings.SetColumnHeaderAlias("Курс", Resx.GetString("ExRate_lblExRate"));
            gridRateCross.Settings.SetColumnFormat("Курс", "N");
            gridRateCross.Settings.SetColumnFormatDefaultScale("Курс", 4);
            gridRateCross.Settings.SetColumnHeaderAlias("Изменение", Resx.GetString("ExRate_lblChange") + ", %");
            gridRateCross.Settings.SetColumnFormat("Изменение", "N");
            gridRateCross.Settings.SetColumnFormatDefaultScale("Изменение", 2);
        }

        /// <summary>
        ///     Проверить заполненность фильтров на вкладке Курсы валют
        /// </summary>
        /// <param name="msg">Строка сообщения с ошибками</param>
        /// <returns>Возвращает true, если все фильтры заполнены</returns>
        private bool CheckFiltersTabRate(out string msg)
        {
            StringBuilder sb = new StringBuilder();

            if (!periodRate.ValueDateFrom.HasValue || !periodRate.ValueDateTo.HasValue)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgPeriodNotSelected"));
            }
            else if (_idsCheckedCurrencies.Length == 0)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgCurrencyNotSelected"));
            }

            msg = sb.ToString();
            return sb.Length == 0;
        }

        /// <summary>
        ///     Проверить заполненность фильтров на вкладке Средние курсы
        /// </summary>
        /// <param name="msg">Строка сообщения с ошибками</param>
        /// <returns>Возвращает true, если все фильтры заполнены</returns>
        private bool CheckFiltersTabRateAvg(out string msg)
        {
            StringBuilder sb = new StringBuilder();

            if (!periodRateAvg.ValueDateFrom.HasValue || !periodRateAvg.ValueDateTo.HasValue)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgPeriodNotSelected"));
            }
            else if (_idsCheckedCurrenciesAvg.Length == 0)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgCurrencyNotSelected"));
            }

            msg = sb.ToString();
            return sb.Length == 0;
        }

        /// <summary>
        ///     Проверить заполненность фильтров на вкладке Кросс-курсы
        /// </summary>
        /// <param name="msg">Строка сообщения с ошибками</param>
        /// <returns>Возвращает true, если все фильтры заполнены</returns>
        private bool CheckFiltersTabRateCross(out string msg)
        {
            StringBuilder sb = new StringBuilder();

            if (!periodRateCross.ValueDateFrom.HasValue || !periodRateCross.ValueDateTo.HasValue)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgPeriodNotSelected"));
            }
            else if (!dbsCurrencyCrossSource.ValueInt.HasValue || !dbsCurrencyCrossTarget.ValueInt.HasValue)
            {
                sb.AppendLine(Resx.GetString("ExRate_msgCurrencyNotSelected"));
            }

            msg = sb.ToString();
            return sb.Length == 0;
        }

        /// <summary>
        ///     Загрузка данных в таблицу Курсы валют 
        /// </summary>
        private void LoadDataGridRate()
        {
            string emptyDataString;

            if (!CheckFiltersTabRate(out emptyDataString))
            {
                gridRate.EmptyDataString = emptyDataString;
                gridRate.ClearGridData();
                gridRate.RefreshGridData();
                btnAddRate.Visible = _allowEdit && gridRate.GеtRowCount() == 0;
                return;
            }

            gridRate.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRate();
            bool reloadDbSourceSettings = gridRate.Settings == null;
            var cols = !reloadDbSourceSettings ? gridRate.Settings.TableColumns : null;

            try
            {
                gridRate.SetDataSource(SQLQueries.SELECT_КурсыВалютЗаПериод, Config.DS_person, CommandType.Text, sqlParams, reloadDbSourceSettings);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRate"), MessageStatus.Error, "", 500, 200);
            }
            catch (Exception e)
            {
                var dex = new DetailedException(Resx.GetString("ExRate_errFailedGettingRate") + ": " + e.Message, e);
                Logger.WriteEx(dex);
                throw dex;
            }

            if (reloadDbSourceSettings)
                SetSettingsGridRate();
            else
                gridRate.Settings.TableColumns = cols;

            gridRate.RefreshGridData();
            btnAddRate.Visible = _allowEdit && gridRate.GеtRowCount() == 0;
        }

        /// <summary>
        ///     Загрузка данных в таблицу Средний курс 
        /// </summary>
        private void LoadDataGridRateAvg()
        {
            string emptyDataString;

            if (!CheckFiltersTabRateAvg(out emptyDataString))
            {
                gridRateAvg.EmptyDataString = emptyDataString;
                gridRateAvg.ClearGridData();
                gridRateAvg.RefreshGridData();
                return;
            }

            gridRateAvg.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRateAvg();

            try
            {
                gridRateAvg.SetDataSource(SQLQueries.SELECT_СредниеКурсыВалютЗаПериод, Config.DS_person, CommandType.Text, sqlParams);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRateAvg"), MessageStatus.Error, "", 500, 200);
            }
            catch (Exception e)
            {
                var dex = new DetailedException(Resx.GetString("ExRate_errFailedGettingRateAvg") + ": " + e.Message, e);
                Logger.WriteEx(dex);
                throw dex;
            }

            SetSettingsGridRateAvg();
            gridRateAvg.RefreshGridData();
        }

        /// <summary>
        ///     Загрузка данных в таблицу Кросс-курс
        /// </summary>
        private void LoadDataGridRateCross()
        {
            string emptyDataString;

            if (!CheckFiltersTabRateCross(out emptyDataString))
            {
                gridRateCross.EmptyDataString = emptyDataString;
                gridRateCross.ClearGridData();
                gridRateCross.RefreshGridData();
                return;
            }

            gridRateCross.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRateCross();

            try
            {
                gridRateCross.SetDataSource(SQLQueries.SELECT_КроссКурсВалютЗаПериод, Config.DS_person,
                    CommandType.Text, sqlParams);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRateCross"), MessageStatus.Error, "", 500, 200);
            }
            catch (Exception e)
            {
                var dex = new DetailedException(Resx.GetString("ExRate_errFailedGettingRateCross") + ": " + e.Message, e);
                Logger.WriteEx(dex);
                throw dex;
            }

            SetSettingsGridRateCross();
            gridRateCross.RefreshGridData();
        }
       
        /// <summary>
        ///     Сохранение записи курса валют
        /// </summary>
        private void SaveDataRate()
        {
            var sqlParams = new Dictionary<string, object>
            {
                {"@ДатаКурса", dpRateDate.ValueDate},
                {"@КодВалюты", dbsCurrencyEdit.ValueInt},
                {"@Курс", numRateValue.ValueDecimal},
                {"@Единиц", numRateUnits.ValueInt},
                {"@Состояние", 0}
            };

            string sqlQuery;

            if (_rateId == 0)
            {
                sqlQuery = SQLQueries.INSERT_КурсВалют;
            }
            else
            {
                sqlParams.Add("@id", _rateId);
                sqlQuery = SQLQueries.UPDATE_КурсВалют;
            }

            try
            {
                DBManager.ExecuteNonQuery(sqlQuery, CommandType.Text, Config.DS_person, sqlParams);
                JS.Write("closeRateEditForm();");
                LoadDataGridRate();
            }
            catch (DetailedException e)
            {
                string message = e.Message;

                //var sqlEx = e.InnerException as SqlException;
                //if (sqlEx != null && sqlEx.Number == 2627)
                //{
                //    message = Resx.GetString("ExRate_msgRateExists");
                //}

                var dex = new DetailedException(message, e);
                Logger.WriteEx(dex);
                throw dex;
            }
        }

        /// <summary>
        ///     Проверка корректности вводимых полей курса
        /// </summary>
        private bool ValidateDataRate(out List<string> errors)
        {
            errors = new List<string>();

            if (dbsCurrencyEdit.ValueInt == null)
                errors.Add(Resx.GetString("ExRate_msgCurrencyNotSelected"));

            if (dpRateDate.Value.IsNullEmptyOrZero())
                errors.Add(Resx.GetString("ExRate_msgRateDateNotSelected"));

            if (numRateValue.Value.IsNullEmptyOrZero())
                errors.Add(Resx.GetString("ExRate_msgRateValueNotEntered"));

            if (numRateUnits.Value.IsNullEmptyOrZero())
                errors.Add(Resx.GetString("ExRate_msgRateUnitsNotEntered"));

            return errors.Count <= 0;
        }

        /// <summary>
        /// Установить стиль недоступности для таблицы
        /// </summary>
        /// <param name="ctrlId">Идентификатор контрола</param>
        private void SetReadOnlyGrid(string ctrlId)
        {
            JS.Write("if (document.all('{0}')) document.all('{0}').className = 'GridGrayed';", ctrlId);
        }
    }
}