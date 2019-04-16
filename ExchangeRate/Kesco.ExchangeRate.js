var isFirstActivateTabAvgRates = true;
var isFirstActivateTabCrossRates = true;
var checkBoxCssClass = "";
var checkBoxAvgCssClass = "";

$(document).ready(function () {
    $(function () {
        /* Установить способность изменять расположение inline элементов при отображении страницы в диалоге IE */
        v4_setResizableInDialog();
        window.v4_insert = insertHandler;
        window.v4_save = saveHandler;
        $(".indeterminate").each(function(index, item) {
            item.indeterminate = true;
        });
    });

    $("#tabs").tabs({
        
        activate: function (event, ui) {
            window.v4_insert = function () { };
            window.v4_save = function () { };

            var tabId = ui.newPanel[0].id;
            switch (tabId) {
                case "tabs-1":
                    window.v4_insert = insertHandler;
                    window.v4_save = saveHandler;
                    break;
                case "tabs-2":
                    if (isFirstActivateTabAvgRates) {
                        btnFilterAvgRates_Click();
                        isFirstActivateTabAvgRates = false;
                    }
                    break;
                case "tabs-3":
                    if (isFirstActivateTabCrossRates) {
                        btnFilterCrossRates_Click();
                        isFirstActivateTabCrossRates = false;
                    }
                    break;
            }
        }
    });

    function insertHandler() {
        if (rateEdit_dialogShow.form == null) {
            $("#btnRateAdd").focus();
            cmd("cmd", "RateAdd");
        }
    }

    function saveHandler() {
        if (rateEdit_dialogShow.form != null) {
            $("#btnRateEditApply").focus();
            cmd("cmd", "SetRate");
        }
    }
});

rateEdit_dialogShow.form = null;

// Функция показа формы редактирования записи
function rateEdit_dialogShow(title, oktext, canceltext) {
    var idContainer = "divRateEdit";
    if (null == rateEdit_dialogShow.form) {
        var width = 320;
        var height = 150;
        var onOpen = function() { openRateEditForm(); };
        var onClose = function() { closeRateEditForm(); };
        var buttons = [
            {
                id: "btnRateEditApply",
                text: oktext + " (F2)",
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                click: function() { cmd("cmd", "SetRate"); }
            },
            {
                id: "btnRateEditCancel",
                text: canceltext,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: closeRateEditForm
            }
        ];

        rateEdit_dialogShow.form =
            v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }

    $("#divRateEdit").dialog("option", "title", title);
    rateEdit_dialogShow.form.dialog("open");
    $("#divRateEdit").find($("select")).focus();

}

// Функция активации вкладки
function tabActivate(n) {
    $("#tabs").tabs({ active: n });
}

// Функция закрытия формы редактирования записи
function closeRateEditForm() {
    if (null != rateEdit_dialogShow.form) {
        rateEdit_dialogShow.form.dialog("close");
        rateEdit_dialogShow.form = null;
    }
}

// Функция открытия формы редактирования записи
function openRateEditForm() {
    if (null != rateEdit_dialogShow.form) {

    }
}

// Функция добавления записи
function rate_add() {
    cmd("cmd", "RateAdd");
}

// Функция редактирования записи
function rate_edit(id) {
    rate_RecordsEdit("Курс", id);
}

// Функция удаления записи
function rate_delete(id) {
    cmd("cmd", "RateDelete", "RateId", id);
}

rate_RecordsEdit.form = null;

// Функция редактирования записи
function rate_RecordsEdit(titleForm, id) {
    cmd("cmd", "RateEdit", "RateId", id);
}

// Фукнкция получения значений фильтра по валютам
function rate_getCurrencyValuesFilter(className) {
    var selector = "." + className + ":checkbox:checked";

    var values = "";
    $(selector).each(function(index, item) {
        values += (values.length > 0 ? "," : "") + parseInt($(item).attr("data-id"));
    });

    return values;
}

// Функция удаления класса стилей по всему документу
function rate_removeCssClass(className) {
    var elems = document.querySelectorAll("." + className);
    [].forEach.call(elems,
        function(el) {
            el.classList.remove(className);
        });
}

function btnInverseCurrencies_Click() {
    var sourceId = $("#dbsCurrencyCrossSource_0").attr("v");
    var targetId = $("#dbsCurrencyCrossTarget_0").attr("v");
    if (!sourceId && !targetId) return;
    cmd("cmd", "InverseCurrencies", "SourceId", sourceId, "TargetId", targetId);
}

function btnFilterRates_Click() {
    var ids = rate_getCurrencyValuesFilter(checkBoxCssClass);
    Wait.render(true);
    cmdasync("cmd", "FilterRates", "Ids", ids); 
    rate_removeCssClass("GridGrayed");
}

function btnFilterAvgRates_Click() {
    var ids = rate_getCurrencyValuesFilter(checkBoxAvgCssClass);
    Wait.render(true);
    cmdasync("cmd", "FilterRatesAvg", "Ids", ids); 
    rate_removeCssClass("GridGrayed");
}

function btnFilterCrossRates_Click() {
    Wait.render(true);
    cmdasync("cmd", "FilterRatesCross");
    rate_removeCssClass("GridGrayed"); 
}