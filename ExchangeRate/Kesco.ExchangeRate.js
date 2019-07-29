var isFirstActivateTabRates = false;
var isFirstActivateTabAvgRates = false;
var isFirstActivateTabCrossRates = false;
var checkBoxCssClass = "";
var checkBoxAvgCssClass = "";
var activeButton = null;
var allowEdit = false;

$(document).ready(function() {
    $(function() {
        $("#tabs").tabs({ heightStyle: "content" });

        /* Установить способность изменять расположение inline элементов при отображении страницы в диалоге IE */
        v4_setResizableInDialog();
        $(".indeterminate").each(function(index, item) {
            item.indeterminate = true;
        });

        $("#gridRate").css("overflow", "");
        $("#gridRateCross").css("overflow", "");

        setTabsMinSize();
        setGridMaxHeight();
    });

    $(window).resize(function() {
        setTabsMinSize();
        setGridMaxHeight();
    });


    //$('frameReportRateCross').load(function () {
    //    $('frameReportRateCross').contents().find("head")
    //        .append($("<style type='text/css'>  .my-class{display:none;}  </style>"));
    //});

    $(".nav").button().click(function() {
        if (activeButton)
            activeButton.button("enable").removeClass("ui-state-active ui-state-hover");
        activeButton = $(this);
        activeButton.button("disable").addClass("ui-state-active").removeClass("ui-state-disabled");

        switch (activeButton[0].id) {
        case "btnTabRate":
            window.v4_insert = insertHandler;
            window.v4_save = saveHandler;
            if (!isFirstActivateTabRates) {
                btnFilterRates_Click();
                isFirstActivateTabRates = true;
            }
            break;
        case "btnTabRateAvg":
            window.v4_insert = function() {};
            window.v4_save = function() {};
            if (!isFirstActivateTabAvgRates) {
                btnFilterAvgRates_Click();
                isFirstActivateTabAvgRates = true;
            }
            break;
        case "btnTabRateCross":
            window.v4_insert = function() {};
            window.v4_save = function() {};
            if (!isFirstActivateTabCrossRates) {
                btnFilterCrossRates_Click();
                isFirstActivateTabCrossRates = true;
            }
            break;
        }
    });

    function setTabsMinSize() {
        $("#tabs").css("min-height", $(window).height() - 35);
    }

    function setGridMaxHeight() {
        var maxHeight = $(window).height() - 126;
        $("#gridRate").css("max-height", maxHeight);
        $("#gridRateCross").css("max-height", maxHeight);
    }

    function insertHandler() {
        if (allowEdit && rateEdit_dialogShow.form == null) {
            $("#btnRateAdd").focus();
            cmd("cmd", "RateAdd");
        }
    }

    function saveHandler() {
        if (allowEdit && rateEdit_dialogShow.form != null) {
            $("#btnRateEditApply").focus();
            cmd("cmd", "SetRate");
        }
    }
});

$(window).load(function() {
    $("#btnTabRate").focus();
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
    var activeTab = $("#tabs .ui-tabs-panel:visible");
    if (!activeTab) return;
    activeTab.find("." + className).each(function() {
        $(this).removeClass(className);
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
    cmdasync("cmd", "FilterRates", "Ids", ids);
}

function btnFilterAvgRates_Click() {
    var ids = rate_getCurrencyValuesFilter(checkBoxAvgCssClass);
    cmdasync("cmd", "FilterRatesAvg", "Ids", ids);
}

function btnFilterCrossRates_Click() {
    cmdasync("cmd", "FilterRatesCross");
}

function setMessageLabel(ctrlId, visible, text) {
    var label = $("#" + ctrlId);
    if (visible) {
        label.text(text);
        label.show();
    } else
        label.hide();
}