using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Grid;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.Settings.Parameters;
using SQLQueries = Kesco.Lib.Entities.SQLQueries;

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
        private const string MsgLabelHtmlId = "msgReportRate";
        private const string MsgLabelAvgHtmlId = "msgReportRateAvg";
        private const string MsgLabelCrossHtmlId = "msgReportRateCross";
        private readonly string _currencyNot = ConfigurationManager.AppSettings["CURRENCY_NOT"];

        private readonly string _currencyTop = ConfigurationManager.AppSettings["CURRENCY_TOP"];
        private bool _allowEdit;
        private Dictionary<int, Currency> _currencies = new Dictionary<int, Currency>();
        private string _idsCheckedCurrencies = string.Empty;
        private string _idsCheckedCurrenciesAvg = string.Empty;

        private AppParamsManager _paramsManager;
        private int _rateId;

        /// <summary>
        ///     Инициализирует новый экземпляр класса Search
        /// </summary>
        public Search()
        {
            EnableViewState = true;
        }

        /// <summary>
        ///     Ссылка на справку
        /// </summary>
        public override string HelpUrl { get; set; } = "hlp/help.htm?id=1";

        /// <summary>
        ///     Сохранить параметры пользователя
        /// </summary>
        public override void SaveParameters()
        {
            var parameters = new Dictionary<string, string>
            {
                {"ExRatePeriod", periodRate.ValuePeriod},
                {"ExRatePeriodAvg", periodRateAvg.ValuePeriod},
                {"ExRatePeriodCross", periodRateCross.ValuePeriod},
                {"ExRateDateFrom", periodRate.ValueDateFrom.HasValue ? periodRate.ValueFromODBC : string.Empty},
                {
                    "ExRateDateFromAvg",
                    periodRateAvg.ValueDateFrom.HasValue ? periodRateAvg.ValueFromODBC : string.Empty
                },
                {
                    "ExRateDateFromCross",
                    periodRateCross.ValueDateFrom.HasValue ? periodRateCross.ValueFromODBC : string.Empty
                },
                {"ExRateDateTo", periodRate.ValueDateTo.HasValue ? periodRate.ValueToODBC : string.Empty},
                {"ExRateDateToAvg", periodRateAvg.ValueDateTo.HasValue ? periodRateAvg.ValueToODBC : string.Empty},
                {
                    "ExRateDateToCross",
                    periodRateCross.ValueDateTo.HasValue ? periodRateCross.ValueToODBC : string.Empty
                },
                {"ExRateCurrency", _idsCheckedCurrencies},
                {"ExRateCurrencyAvg", _idsCheckedCurrenciesAvg},
                {"ExRateCurrencyCrsSrc", dbsCurrencyCrossSource.Value},
                {"ExRateCurrencyCrsTgt", dbsCurrencyCrossTarget.Value},
                {"ExRateOnHomePage", chkShowRatesOnHomePage.Checked ? "1" : "0"},
                {"ExRateCrsOnHomePage", chkShowCrossRatesOnHomePage.Checked ? "1" : "0"}
            };

            foreach (var key in parameters.Keys) _paramsManager.SetDbParameterValue(key, parameters[key]);

            _paramsManager.SaveParams();
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
            JS.Write("allowEdit = {0};", _allowEdit.ToString().ToLower());

            IsRememberWindowProperties = true;
            WindowParameters =
                new WindowParameters("ExRateWndLeft", "ExRateWndTop", "ExRateWndWidth", "ExRateWndHeight");
            IsSilverLight = false;

            var _params = new StringCollection
            {
                "ExRateDateFrom", "ExRateDateTo", "ExRatePeriod",
                "ExRateDateFromAvg", "ExRateDateToAvg", "ExRatePeriodAvg",
                "ExRateDateFromCross", "ExRateDateToCross", "ExRatePeriodCross",
                "ExRateCurrency", "ExRateCurrencyAvg", "ExRateCurrencyCrsSrc", "ExRateCurrencyCrsTgt",
                "ExRateOnHomePage", "ExRateCrsOnHomePage"
            };

            _paramsManager = new AppParamsManager(ClId, _params);

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

            JS.Write("checkBoxCssClass='{0}';checkBoxAvgCssClass='{1}';", CheckBoxCssClass, CheckBoxAvgCssClass);

            var form = Request.QueryString["form"];
            switch (form)
            {
                case "avg":
                    JS.Write("$('#btnTabRateAvg').click();");
                    break;
                case "cross":
                    JS.Write("$('#btnTabRateCross').click();");
                    break;
                default:
                    JS.Write("$('#btnTabRate').click();");
                    break;
            }

            JS.Write("$('.ui-tabs-nav').hide();");
        }

        /// <summary>
        ///     Инициализация вкладки "Курсы валют"
        /// </summary>
        private void InitTabRate()
        {
            periodRate.Changed += delegate { SetReadOnlyDataRate(true); };
            btnFilterRates.Text = Resx.GetString("ExRate_lblSelect");
            btnFilterRates.OnClick = "btnFilterRates_Click()";
            chkShowRatesOnHomePage.Text = "&nbsp;" + Resx.GetString("ExRate_lblShowRatesOnHomePage");
            chkShowRatesOnHomePage.Checked = (_paramsManager.GetDbParameterValue("ExRateOnHomePage") ?? "1") == "1";

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

            var checkPeriod = periodRate.ValueDateFrom.HasValue && periodRate.ValueDateTo.HasValue;
            var checkCur = _idsCheckedCurrencies.Length > 0;

            OutputChecksResult(MsgLabelHtmlId, checkPeriod, checkCur);

            InitGridRate();
        }

        private void DbsCurrencyEdit_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var currencyId = dbsCurrencyEdit.ValueInt;
            var numUnits = GetLastUnitsNumber(currencyId);
            numRateUnits.Value = numUnits.ToString();
        }

        /// <summary>
        ///     Инициализация вкладки "Средние курсы"
        /// </summary>
        private void InitTabRateAvg()
        {
            periodRateAvg.Changed += delegate { SetReadOnlyDataRateAvg(true); };
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

            var checkPeriod = periodRateAvg.ValueDateFrom.HasValue && periodRateAvg.ValueDateTo.HasValue;
            var checkCur = _idsCheckedCurrenciesAvg.Length > 0;

            OutputChecksResult(MsgLabelAvgHtmlId, checkPeriod, checkCur);

            InitGridRateAvg();
        }

        /// <summary>
        ///     Инициализация вкладки "Кросс-курсы"
        /// </summary>
        private void InitTabRateCross()
        {
            periodRateCross.Changed += delegate { SetReadOnlyDataRateCross(true); };
            btnFilterCrossRates.Text = Resx.GetString("ExRate_lblSelect");
            btnFilterCrossRates.OnClick = "btnFilterCrossRates_Click()";
            chkShowCrossRatesOnHomePage.Text = "&nbsp;" + Resx.GetString("ExRate_lblShowRatesOnHomePage");
            chkShowCrossRatesOnHomePage.Checked =
                (_paramsManager.GetDbParameterValue("ExRateCrsOnHomePage") ?? "1") == "1";

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

            dbsCurrencyCrossSource.Title = Resx.GetString("ExRate_lblBaseCurrency");
            dbsCurrencyCrossTarget.Title = Resx.GetString("ExRate_lblQuotedCurrency");

            dbsCurrencyCrossSource.BeforeSearch += DbsCurrencyCross_BeforeSearch;
            dbsCurrencyCrossTarget.BeforeSearch += DbsCurrencyCross_BeforeSearch;

            dbsCurrencyCrossSource.TextChanged += DbsCurrencyCross_TextChanged;
            dbsCurrencyCrossTarget.TextChanged += DbsCurrencyCross_TextChanged;

            var checkPeriod = periodRateCross.ValueDateFrom.HasValue && periodRateCross.ValueDateTo.HasValue;
            var checkCur = dbsCurrencyCrossSource.ValueInt.HasValue && dbsCurrencyCrossTarget.ValueInt.HasValue;

            OutputChecksResult(MsgLabelCrossHtmlId, checkPeriod, checkCur);

            btnInverseCurrencies.OnClick += "btnInverseCurrencies_Click();";
            btnInverseCurrencies.Title = Resx.GetString("ExRate_lblSwapCurrencies");

            btnInverseCurrencies.IconJQueryUI = ButtonIconsEnum.Swap;
        }

        private void DbsCurrencyCross_BeforeSearch(object sender)
        {
            var dbs = (DBSCurrency) sender;
            DbsFilterItems(dbs, dbs.ValueInt);
            SetReadOnlyDataRateCross(true);
        }

        private void DbsCurrencyCross_TextChanged(object sender, ValueChangedEventArgs e)
        {
            var checkPeriod = periodRateCross.ValueDateFrom.HasValue && periodRateCross.ValueDateTo.HasValue;
            var checkCur = dbsCurrencyCrossSource.ValueInt.HasValue && dbsCurrencyCrossTarget.ValueInt.HasValue;

            OutputChecksResult(MsgLabelCrossHtmlId, checkPeriod, checkCur);
        }

        private void DbsFilterItems(DBSCurrency control, int? itemId)
        {
            control.Filter.CurrencyId.Clear();
            control.Filter.CurrencyId.Add(_currencyNot);
            if (itemId.HasValue) control.Filter.CurrencyId.Add(itemId.ToString());
            control.Filter.CurrencyId.HowSearch = SearchIds.NotIn;
        }

        protected override void EntityInitialization(Entity copy = null)
        {
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Название команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            List<string> validList;
            switch (cmd)
            {
                case "FilterRates":
                    _idsCheckedCurrencies = param["Ids"];
                    LoadDataTabRate();
                    break;
                case "FilterRatesAvg":
                    _idsCheckedCurrenciesAvg = param["Ids"];
                    LoadDataTabRateAvg();
                    break;
                case "FilterRatesCross":
                    LoadDataTabRateCross();
                    break;
                case "RateAdd":
                    _rateId = 0;
                    var currencyId = GetDefaultCurrencyId();
                    var numUnits = GetLastUnitsNumber(currencyId);
                    dbsCurrencyEdit.ValueInt = currencyId;
                    dpRateDate.Value = DateTime.Now.ToString();
                    numRateValue.Value = "";
                    numRateUnits.Value = numUnits.ToString();
                    JS.Write("rateEdit_dialogShow('{0}','{1}','{2}','{3}');", Resx.GetString("ExRate_lblAddingRate"),
                        Resx.GetString("cmdSave"), Resx.GetString("cmdCancel"), cmd);
                    break;
                case "RateEdit":
                    _rateId = int.Parse(param["RateId"]);
                    var sqlParams = new Dictionary<string, object> {{"@id", _rateId}};
                    var dt = DBManager.GetData(SQLQueries.SELECT_КурсВалюты, Config.DS_person, CommandType.Text,
                        sqlParams);
                    if (dt.Rows.Count > 0)
                    {
                        dbsCurrencyEdit.ValueInt = (int) dt.Rows[0]["КодВалюты"];
                        dpRateDate.Value = dt.Rows[0]["ДатаКурса"].ToString();
                        numRateValue.Value = dt.Rows[0]["Курс"].ToString();
                        numRateUnits.Value = dt.Rows[0]["Единиц"].ToString();
                    }

                    JS.Write("rateEdit_dialogShow('{0}','{1}','{2}','{3}');", Resx.GetString("ExRate_lblEditingRate"),
                        Resx.GetString("cmdSave"), Resx.GetString("cmdCancel"), cmd);
                    break;
                case "RateDelete":
                    if (!param["RateId"].IsNullEmptyOrZero())
                    {
                        var rateId = Convert.ToInt32(param["RateId"]);
                        DBManager.ExecuteNonQuery(SQLQueries.DELETE_КурсВалют, rateId, CommandType.Text,
                            Config.DS_person);
                        LoadDataTabRate();
                    }

                    break;
                case "SetRate":
                    if (ValidateDataRate(out validList))
                        SaveDataRate();
                    else
                        RenderErrors(validList, "<br/> " + Resx.GetString("_Msg_НеСохраняется"));
                    break;
                case "CurrenciesChanged":
                    _idsCheckedCurrencies = param["Ids"];
                    SetReadOnlyDataRate(true);
                    break;
                case "CurrenciesAvgChanged":
                    _idsCheckedCurrenciesAvg = param["Ids"];
                    SetReadOnlyDataRateAvg(true);
                    break;
                case "InverseCurrencies":
                    var oldValueSource = dbsCurrencyCrossSource.Value;
                    var oldValueTarget = dbsCurrencyCrossTarget.Value;
                    var newValueSource = param["targetId"];
                    var newValueTarget = param["sourceId"];
                    dbsCurrencyCrossSource.Value = newValueSource;
                    dbsCurrencyCrossTarget.Value = newValueTarget;
                    dbsCurrencyCrossSource.OnValueChanged(new ValueChangedEventArgs(newValueSource, oldValueSource));
                    dbsCurrencyCrossTarget.OnValueChanged(new ValueChangedEventArgs(newValueTarget, oldValueTarget));
                    SetReadOnlyDataRateCross(true);
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        /// <summary>
        ///     Получить последнее значение единиц для данной валюты
        /// </summary>
        /// <returns>Количество единиц</returns>
        private int GetLastUnitsNumber(int? currencyId)
        {
            if (!currencyId.HasValue) return 1;
            var sqlParams = new Dictionary<string, object> {{"@КодВалюты", currencyId}};
            var dt = DBManager.GetData(SQLQueries.SELECT_ПоследнийКурсВалюты, Config.DS_person, CommandType.Text,
                sqlParams);
            return dt.Rows.Count > 0 ? (int) dt.Rows[0]["Единиц"] : 1;
        }

        /// <summary>
        ///     Получить валюту по умолчанию для формы добавления
        /// </summary>
        /// <returns> Идентификатор валюты, если в списке валют отмечено только одно значение, иначе - null</returns>
        private int? GetDefaultCurrencyId()
        {
            var ids = _idsCheckedCurrencies.Split(',');

            if (ids.Length == 1 && int.TryParse(ids[0], out var id))
                return id;
            return null;
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
                    SetMenuButtons();
                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(
                        Resx.GetString("ExRate_errFailedGenerateButtons") + ": " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnTabRate = new Button
            {
                ID = "btnTabRate",
                V4Page = this,
                Text = Resx.GetString("ExRate_lblExRateDetail"),
                Width = 120,
                OnClick = "tabActivate(0);",
                Style = "margin-left: 1px;",
                CSSClass = "nav",
                TabIndex = 0
            };
            AddMenuButton(btnTabRate);

            var btnTabRateAvg = new Button
            {
                ID = "btnTabRateAvg",
                V4Page = this,
                Text = Resx.GetString("ExRate_lblExRateAverage"),
                Width = 120,
                OnClick = "tabActivate(1);",
                CSSClass = "nav",
                TabIndex = 1
            };
            AddMenuButton(btnTabRateAvg);

            var btnTabRateCross = new Button
            {
                ID = "btnTabRateCross",
                V4Page = this,
                Text = Resx.GetString("ExRate_lblExRateCross"),
                Width = 120,
                OnClick = "tabActivate(2);",
                CSSClass = "nav",
                TabIndex = 2
            };
            AddMenuButton(btnTabRateCross);
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
        ///     Отрисовка фильтра Список валют
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
                            WriteMarkupTableCurrency(w, CheckBoxCssClass, CurrenciesChangedHandlerName,
                                CheckBoxAllHtmlId, _idsCheckedCurrencies, MsgLabelHtmlId);
                            break;
                        case 2:
                            WriteMarkupTableCurrency(w, CheckBoxAvgCssClass, CurrenciesAvgChangedHandlerName,
                                CheckBoxAllAvgHtmlId, _idsCheckedCurrenciesAvg, MsgLabelAvgHtmlId);
                            break;
                    }
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(
                        Resx.GetString("ExRate_errFailedGenerateCurrencyList") + ": " + e.Message, e);
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
        private void WriteMarkupTableCurrency(StringWriter w, string checkBoxCssClass, string handlerName,
            string checkBoxAllHtmlId, string idsCheckedCurrencies, string msgLabelHtmlId)
        {
            var idsTopList = _currencyTop.Split(',').Select(int.Parse).ToList();

            var idsList = _currencies.Select(x => x.Key).ToList();

            idsList = idsTopList.Union(idsList).ToList();

            var idsCheckedList = string.IsNullOrEmpty(idsCheckedCurrencies)
                ? new List<int>()
                : idsCheckedCurrencies.Split(',').Select(int.Parse).ToList();

            var stateChecked = string.Empty;

            if (idsCheckedList.Count == idsList.Count)
                stateChecked = "checked='checked'";
            else if (idsCheckedList.Count > 0)
                stateChecked = $"class='{IndeterminateCssClass}'";

            w.Write("<div class='v4DivTable'>");

            w.Write("<div class='v4DivTableRow'>");

            w.Write("<div class='v4DivTableCell v4PaddingCell'>");

            var strOnClickAll = string.Format(
                "v4_columnValuesChecked(this.checked,'{0}');" +
                "var ids=rate_getCurrencyValuesFilter('{0}');" +
                "setMessageLabel('{1}', !this.checked, '{2}');" +
                "cmd('cmd','{3}','Ids',ids);",
                checkBoxCssClass,
                msgLabelHtmlId,
                Resx.GetString("ExRate_msgCurrencyNotSelected"),
                handlerName);

            w.Write("<input type='checkbox' id='{0}' onclick =\"{1}\" {2}>",
                checkBoxAllHtmlId, strOnClickAll, stateChecked);

            w.Write("</div>");

            w.Write("<div class='v4DivTableCell v4PaddingCell' style='text-align:left; white-space: nowrap;'>");

            w.Write("({0})", Resx.GetString("lblSelectAll"));

            w.Write("</div>");

            w.Write("</div>");

            foreach (var id in idsList)
            {
                var isChecked = idsCheckedList.Any(x => x.Equals(id));

                w.Write("<div class='v4DivTableRow'>");

                w.Write("<div class='v4DivTableCell v4PaddingCell'>");

                var strOnClick = string.Format(
                    "v4_setStateCheckAllValues('{0}','{1}');" +
                    "var ids=rate_getCurrencyValuesFilter('{1}');" +
                    "setMessageLabel('{2}', ids.length == 0, '{3}');" +
                    "cmd('cmd','{4}','Ids',ids);",
                    checkBoxAllHtmlId,
                    checkBoxCssClass,
                    msgLabelHtmlId,
                    Resx.GetString("ExRate_msgCurrencyNotSelected"),
                    handlerName);

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
        ///     Получение локализованного названия валюты
        /// </summary>
        /// <param name="currency">Экземпляр валюты</param>
        /// <returns>Название</returns>
        private string GetLocalCurrencyName(Currency currency)
        {
            if (IsRusLocal || string.IsNullOrWhiteSpace(currency.ResourceLat))
                return currency.Name;
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
            var dateFrom = !string.IsNullOrEmpty(strDateFrom)
                ? Lib.ConvertExtention.Convert.Str2DateTime(strDateFrom)
                : parameters.DefaultDateFrom;
            var dateTo = !string.IsNullOrEmpty(strDateTo)
                ? Lib.ConvertExtention.Convert.Str2DateTime(strDateTo)
                : parameters.DefaultDateTo;
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
            gridRate.ShowPageBar = false;
            gridRate.SetModifyInfoTooltip("ФИО", "Изменено");
            gridRate.AlwaysShowHeader = true;

            if (_allowEdit)
            {
                gridRate.ExistServiceColumn = true;
                gridRate.SetServiceColumnAdd("rate_add", Resx.GetString("ExRate_btnAddPosition"));

                var condition = new List<object> {(byte) 0};
                gridRate.RenderConditionServiceColumnDelete.Add("Состояние", condition);
                gridRate.RenderConditionServiceColumnEdit.Add("Состояние", condition);

                gridRate.SetServiceColumnDelete("rate_delete", new List<string> {"КодКурсаВалюты"},
                    new List<string> {"РесурсРус", "ДатаКурсаСтрока"}, Resx.GetString("ExRate_btnDeletePosition"));

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
            gridRateAvg.AlwaysShowHeader = true;
        }

        /// <summary>
        ///     Инициализация таблицы Кросс-курс
        /// </summary>
        private void InitGridRateCross()
        {
            gridRateCross.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            gridRateCross.ShowGroupPanel = false;
            gridRateCross.ShowFilterOptions = false;
            gridRateCross.ShowPageBar = false;
            gridRateCross.AlwaysShowHeader = true;
        }

        /// <summary>
        ///     Инициализация выпадающего списка с выбором валюты на форме редактирования
        /// </summary>
        private void InitComboBoxRate()
        {
            dbsCurrencyEdit.Filter.CurrencyId.Add(_currencyNot);
            dbsCurrencyEdit.Filter.CurrencyId.HowSearch = SearchIds.NotIn;
            dbsCurrencyEdit.ValueChanged += DbsCurrencyEdit_ValueChanged;
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
            var valueDateFrom = periodRate.ValueDateFrom.HasValue
                ? periodRate.ValueFromODBC
                : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRate.ValueDateTo.HasValue
                ? periodRate.ValueToODBC
                : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

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
            var valueDateFrom = periodRateAvg.ValueDateFrom.HasValue
                ? periodRateAvg.ValueFromODBC
                : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRateAvg.ValueDateTo.HasValue
                ? periodRateAvg.ValueToODBC
                : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

            sqlParams.Add("@КодыВалют", _idsCheckedCurrenciesAvg);
            sqlParams.Add("@ДатаКурсаОт", valueDateFrom);
            sqlParams.Add("@ДатаКурсаПо", valueDateTo);

            return sqlParams;
        }

        /// <summary>
        ///     Получение параметров SQL-запроса для таблицы Кросс-курс
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetSqlParamsForGridRateCross()
        {
            var sqlParams = new Dictionary<string, object>();
            var valueDateFrom = periodRateCross.ValueDateFrom.HasValue
                ? periodRateCross.ValueFromODBC
                : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
            var valueDateTo = periodRateCross.ValueDateTo.HasValue
                ? periodRateCross.ValueToODBC
                : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

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
                "FIO",
                "ФИО",
                "Изменено",
                "ДатаКурсаСтрока"
            };
            gridRate.Settings.SetColumnDisplayVisible(listColumnVisible, false);
            gridRate.Settings.SetColumnHeaderAlias(IsRusLocal ? "РесурсРус" : "РесурсЛат",
                Resx.GetString("ExRate_lblCurrency"));
            gridRate.Settings.SetColumnNoWrapText(IsRusLocal ? "РесурсРус" : "РесурсЛат");
            gridRate.Settings.SetColumnHeaderAlias("ДатаКурса", Resx.GetString("ExRate_lblRateDate"));
            gridRate.Settings.SetColumnFormat("ДатаКурса", "dd.MM.yyyy");
            gridRate.Settings.SetColumnHeaderAlias("Курс", Resx.GetString("ExRate_lblRateValueRub"));
            gridRate.Settings.SetColumnFormat("Курс", "N");
            gridRate.Settings.SetColumnFormatDefaultScale("Курс", 4);
            gridRate.Settings.SetColumnHeaderAlias("Единиц", Resx.GetString("ExRate_lblRateUnits"));
            //gridRate.Settings.SetColumnHeaderAlias(IsRusLocal ? "ФИО" : "FIO", Resx.GetString("ExRate_lblRateChangedPerson"));
            //gridRate.Settings.SetColumnNoWrapText(IsRusLocal ? "ФИО" : "FIO");
            //gridRate.Settings.SetColumnHrefEmployee(IsRusLocal ? "ФИО" : "FIO", "КодСотрудника");
            //gridRate.Settings.SetColumnHeaderAlias("Изменено", Resx.GetString("ExRate_lblRateChangedDate"));
            //gridRate.Settings.SetColumnNoWrapText("Изменено");
            //gridRate.Settings.SetColumnLocalTime("Изменено");
            gridRate.Settings.SetSortingColumns(IsRusLocal ? "РесурсРус" : "РесурсЛат", "ДатаКурса");
            gridRate.RowsPerPage = gridRate.MaxPrintRenderRows;
            gridRate.SetOrderBy("ДатаКурса", GridColumnOrderByDirectionEnum.Desc);
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
            gridRateAvg.Settings.SetColumnHeaderAlias(IsRusLocal ? "РесурсРус" : "РесурсЛат",
                Resx.GetString("ExRate_lblCurrency"));
            gridRateAvg.Settings.SetColumnNoWrapText(IsRusLocal ? "РесурсРус" : "РесурсЛат");
            gridRateAvg.Settings.SetColumnHeaderAlias("Курс", Resx.GetString("ExRate_lblExRateRub"));
            gridRateAvg.Settings.SetColumnFormat("Курс", "N");
            gridRateAvg.Settings.SetColumnFormatDefaultScale("Курс", 4);
            gridRateAvg.Settings.SetSortingColumns(IsRusLocal ? "РесурсРус" : "РесурсЛат");
            gridRateAvg.SetOrderBy(IsRusLocal ? "РесурсРус" : "РесурсЛат", GridColumnOrderByDirectionEnum.Asc);
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
            gridRateCross.Settings.SetSortingColumns("Дата");
            gridRateCross.RowsPerPage = gridRateCross.MaxPrintRenderRows;
            gridRateCross.SetOrderBy("Дата", GridColumnOrderByDirectionEnum.Desc);
        }

        /// <summary>
        ///     Проверить заполненность фильтров на вкладке Средние курсы
        /// </summary>
        /// <param name="msg">Строка сообщения с ошибками</param>
        /// <returns>Возвращает true, если все фильтры заполнены</returns>
        private bool CheckFiltersTabRateAvg(out string msg)
        {
            var result = true;
            var sb = new StringBuilder();

            if (!periodRateAvg.ValueDateFrom.HasValue || !periodRateAvg.ValueDateTo.HasValue)
            {
                result = false;
                sb.AppendLine(Resx.GetString("ExRate_msgPeriodNotSelected"));
            }
            else if (_idsCheckedCurrenciesAvg.Length == 0)
            {
                result = false;
                //sb.AppendLine(Resx.GetString("ExRate_msgCurrencyNotSelected"));
            }

            msg = sb.ToString();
            return result;
        }

        /// <summary>
        ///     Проверить заполненность фильтров на вкладке Кросс-курсы
        /// </summary>
        /// <param name="msg">Строка сообщения с ошибками</param>
        /// <returns>Возвращает true, если все фильтры заполнены</returns>
        private bool CheckFiltersTabRateCross(out string msg)
        {
            var result = true;
            var sb = new StringBuilder();

            if (!periodRateCross.ValueDateFrom.HasValue || !periodRateCross.ValueDateTo.HasValue)
            {
                result = false;
                sb.AppendLine(Resx.GetString("ExRate_msgPeriodNotSelected"));
            }
            else if (!dbsCurrencyCrossSource.ValueInt.HasValue || !dbsCurrencyCrossTarget.ValueInt.HasValue)
            {
                result = false;
                sb.AppendLine(Resx.GetString("ExRate_msgCurrencyNotSelected"));
            }

            msg = sb.ToString();
            return result;
        }

        private void OutputChecksResult(string msgLabelHtmlId, bool checkPeriod, bool checkCur)
        {
            var msg = string.Empty;

            if (!checkPeriod)
                msg = Resx.GetString("ExRate_msgPeriodNotSelected");

            if (!checkCur)
            {
                msg += msg.Length > 0 ? ". " : "";
                msg += Resx.GetString("ExRate_msgCurrencyNotSelected");
            }

            JS.Write("setMessageLabel('{0}', true, '{1}');", msgLabelHtmlId, msg);
        }

        private void HideReport(string frameId)
        {
            JS.Write("$('#{0}').attr('src', '');", frameId);
            JS.Write("$('#{0}').hide();", frameId);
        }

        /// <summary>
        ///     Загрузка данных на вкладку Курсы валют
        /// </summary>
        private void LoadDataTabRate()
        {
            var checkPeriod = periodRate.ValueDateFrom.HasValue && periodRate.ValueDateTo.HasValue;
            var checkCur = _idsCheckedCurrencies.Length > 0;
            var frameId = "frameReportRate";

            OutputChecksResult(MsgLabelHtmlId, checkPeriod, checkCur);

            if (checkPeriod && checkCur)
            {
                SetReadOnlyDataRate(false);
                LoadDataGridRate();
            }
            else
            {
                HideReport(frameId);
                return;
            }

            var checkPeriodReport =
                periodRate.ValueDateTo.Value.AddDays(1).Subtract(periodRate.ValueDateFrom.Value).Days > 3;
            var checkRows = gridRate.GеtRowCount() > 3;

            if (checkPeriodReport && checkRows)
            {
                var strDateFrom = periodRate.ValueDateFrom.HasValue
                    ? periodRate.ValueFromODBC
                    : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
                var strDateTo = periodRate.ValueDateTo.HasValue
                    ? periodRate.ValueToODBC
                    : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

                LoadReportData("ExchangeRate", frameId, periodRate.ValuePeriod, strDateFrom, strDateTo);
            }
            else
            {
                HideReport(frameId);
            }
        }

        /// <summary>
        ///     Загрузка данных в таблицу Курсы валют
        /// </summary>
        private void LoadDataGridRate()
        {
            gridRate.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRate();
            var reloadDbSourceSettings = gridRate.Settings == null;
            var cols = !reloadDbSourceSettings ? gridRate.Settings.TableColumns : null;

            try
            {
                gridRate.SetDataSource(SQLQueries.SELECT_КурсыВалютЗаПериод, Config.DS_person, CommandType.Text,
                    sqlParams, reloadDbSourceSettings);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRate"), MessageStatus.Error, "", 500,
                    200);
                return;
            }
            catch (Exception e)
            {
                var dex = new DetailedException(Resx.GetString("ExRate_errFailedGettingRate") + ": " + e.Message,
                    e);
                Logger.WriteEx(dex);
                throw dex;
            }

            if (reloadDbSourceSettings)
                SetSettingsGridRate();
            else
                gridRate.Settings.TableColumns = cols;

            gridRate.RefreshGridData();
        }

        /// <summary>
        ///     Загрузка данных на вкладку Средний курс
        /// </summary>
        private void LoadDataTabRateAvg()
        {
            var checkPeriod = periodRateAvg.ValueDateFrom.HasValue && periodRateAvg.ValueDateTo.HasValue;
            var checkCur = _idsCheckedCurrenciesAvg.Length > 0;

            OutputChecksResult(MsgLabelAvgHtmlId, checkPeriod, checkCur);

            if (checkPeriod && checkCur)
            {
                SetReadOnlyDataRateAvg(false);
                LoadDataGridRateAvg();
            }
        }

        /// <summary>
        ///     Загрузка данных в таблицу Средний курс
        /// </summary>
        private void LoadDataGridRateAvg()
        {
            gridRateAvg.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRateAvg();
            var reloadDbSourceSettings = gridRateAvg.Settings == null;
            var cols = !reloadDbSourceSettings ? gridRateAvg.Settings.TableColumns : null;

            try
            {
                gridRateAvg.SetDataSource(SQLQueries.SELECT_СредниеКурсыВалютЗаПериод, Config.DS_person,
                    CommandType.Text, sqlParams, reloadDbSourceSettings);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRateAvg"), MessageStatus.Error, "",
                    500, 200);
            }
            catch (Exception e)
            {
                var dex = new DetailedException(Resx.GetString("ExRate_errFailedGettingRateAvg") + ": " + e.Message,
                    e);
                Logger.WriteEx(dex);
                throw dex;
            }

            if (reloadDbSourceSettings)
                SetSettingsGridRateAvg();
            else
                gridRateAvg.Settings.TableColumns = cols;

            gridRateAvg.RefreshGridData();
        }

        /// <summary>
        ///     Загрузка данных на вкладку Кросс-курсы
        /// </summary>
        private void LoadDataTabRateCross()
        {
            var checkPeriod = periodRateCross.ValueDateFrom.HasValue && periodRateCross.ValueDateTo.HasValue;
            var checkCur = dbsCurrencyCrossSource.ValueInt.HasValue && dbsCurrencyCrossTarget.ValueInt.HasValue;
            var frameId = "frameReportRateCross";

            OutputChecksResult(MsgLabelCrossHtmlId, checkPeriod, checkCur);

            if (checkPeriod && checkCur)
            {
                SetReadOnlyDataRateCross(false);
                LoadDataGridRateCross();
            }
            else
            {
                HideReport(frameId);
                return;
            }

            var checkPeriodReport = periodRateCross.ValueDateTo.Value.AddDays(1)
                                        .Subtract(periodRateCross.ValueDateFrom.Value).Days > 3;
            var checkRows = gridRateCross.GеtRowCount() > 3;

            if (checkPeriodReport && checkRows)
            {
                var strDateFrom = periodRateCross.ValueDateFrom.HasValue
                    ? periodRateCross.ValueFromODBC
                    : DateTimeExtensionMethods.MinDateTime.ToSqlDate();
                var strDateTo = periodRateCross.ValueDateTo.HasValue
                    ? periodRateCross.ValueToODBC
                    : DateTimeExtensionMethods.EndDateTime.ToSqlDate();

                LoadReportData("ExchangeRateCross", frameId, periodRateCross.ValuePeriod, strDateFrom, strDateTo);
            }
            else
            {
                HideReport(frameId);
            }
        }

        /// <summary>
        ///     Загрузка данных в таблицу Кросс-курс
        /// </summary>
        private void LoadDataGridRateCross()
        {
            gridRateCross.EmptyDataString = Resx.GetString("ExRate_msgEmptyData");
            var sqlParams = GetSqlParamsForGridRateCross();
            var reloadDbSourceSettings = gridRateCross.Settings == null;
            var cols = !reloadDbSourceSettings ? gridRateCross.Settings.TableColumns : null;

            try
            {
                gridRateCross.SetDataSource(SQLQueries.SELECT_КроссКурсВалютЗаПериод, Config.DS_person,
                    CommandType.Text, sqlParams, reloadDbSourceSettings);
            }
            catch (DetailedException e)
            {
                ShowMessage(e.Message, Resx.GetString("ExRate_errFailedGettingRateCross"), MessageStatus.Error, "",
                    500, 200);
            }
            catch (Exception e)
            {
                var dex = new DetailedException(
                    Resx.GetString("ExRate_errFailedGettingRateCross") + ": " + e.Message, e);
                Logger.WriteEx(dex);
                throw dex;
            }

            if (reloadDbSourceSettings)
                SetSettingsGridRateCross();
            else
                gridRateCross.Settings.TableColumns = cols;

            gridRateCross.RefreshGridData();
        }

        /// <summary>
        ///     Загрузка отчета с графиком
        /// </summary>
        /// <param name="reportName">Название отчета</param>
        /// <param name="frameId">Идентификатор фрейма, в который загружается отчет</param>
        /// <param name="valuePeriod">Значение типа периода</param>
        /// <param name="strDateFrom">Дата начала периода</param>
        /// <param name="strDateTo">Дата окончания периода</param>
        private void LoadReportData(string reportName, string frameId, string valuePeriod, string strDateFrom,
            string strDateTo)
        {
            var parameters = string.Empty;

            switch (reportName)
            {
                case "ExchangeRate":
                    parameters = string.Format("&curId={0}&isRusLocal={1}",
                        _idsCheckedCurrencies,
                        IsRusLocal.ToString());
                    break;
                case "ExchangeRateCross":
                    parameters = string.Format("&sourceId={0}&targetId={1}",
                        dbsCurrencyCrossSource.Value,
                        dbsCurrencyCrossTarget.Value);
                    break;
            }

            var src = string.Format("{0}/Pages/ReportViewer.aspx?/Persons/" +
                                    "{1}" +
                                    "&rc:Toolbar=false" +
                                    "&rc:Parameters=false" +
                                    "&rs:ClearSession=true" +
                                    "&DT={2}" +
                                    "&period={3}" +
                                    "&dateFrom={4}" +
                                    "&dateTo={5}" +
                                    "{6}",
                Config.uri_Report,
                reportName,
                DateTime.Now.ToString("HHmmss"),
                valuePeriod,
                strDateFrom,
                strDateTo,
                parameters);

            src = HttpUtility.JavaScriptStringEncode(src);
            JS.Write("$('#{0}').attr('src', '{1}');", frameId, src);
            JS.Write("$('#{0}').show();", frameId);
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
                LoadDataTabRate();
            }
            catch (DetailedException e)
            {
                var message = e.Message;

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
        ///     Установить стиль недоступности для контрола
        /// </summary>
        /// <param name="ctrlId">Идентификатор контрола</param>
        /// <param name="readOnly">Признак "только чтение"</param>
        private void SetReadOnly(string ctrlId, bool readOnly)
        {
            var cssClass = "ControlGrayed";

            if (readOnly)
                JS.Write("if (!$('#{0}').hasClass('{1}')) $('#{0}').addClass('{1}');", ctrlId, cssClass);
            else
                JS.Write("if ($('#{0}').hasClass('{1}')) $('#{0}').removeClass('{1}');", ctrlId, cssClass);
        }

        private void SetReadOnlyDataRate(bool readOnly)
        {
            SetReadOnly("thead_gridRate", readOnly);
            SetReadOnly("divGridRate", readOnly);
            SetReadOnly("frameReportRate", readOnly);
        }

        private void SetReadOnlyDataRateAvg(bool readOnly)
        {
            SetReadOnly("thead_gridRateAvg", readOnly);
            SetReadOnly("divGridRateAvg", readOnly);
        }

        private void SetReadOnlyDataRateCross(bool readOnly)
        {
            SetReadOnly("thead_gridRateCross", readOnly);
            SetReadOnly("divGridRateCross", readOnly);
            SetReadOnly("frameReportRateCross", readOnly);
        }
    }
}