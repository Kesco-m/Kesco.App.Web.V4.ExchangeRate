<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Kesco.App.Web.V4.ExchangeRate.Search" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="csg" Namespace="Kesco.Lib.Web.Controls.V4.Grid" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title><%= Resx.GetString("ExRate_lblExRates") %></title>
    <link rel="stylesheet" type="text/css" href="Kesco.ExchangeRate.css"/>
    <script src="Kesco.ExchangeRate.js" type="text/javascript"></script>
</head>
<body>

<div class="marginD"><%= RenderDocumentHeader() %></div>

<div class="v4formContainer">
    <div id="tabs">
        <ul>
            <li id="tabs1">
                <a href="#tabs-1">

                </a>
            </li>
            <li id="tabs2">
                <a href="#tabs-2">

                </a>
            </li>
            <li id="tabs3">
                <a href="#tabs-3">

                </a>
            </li>
        </ul>

        <div id="tabs-1">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRate" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterRates" runat="server" CSSClass="aligned_control"></v4control:Button>
                <div id="msgReportRate" class="label" style="height: auto; margin-left: 20px; width: auto;"></div>
            </div>
            <div class="marginT"></div>
            <div style="display: table;">
                <div style="display: table-cell; vertical-align: top;">
                    <div style="width: 170px;">
                        <div class="label"><%= Resx.GetString("ExRate_lblCurrencies") %>:</div>
                        <div class="spacer"></div>
                        <div id="divCurrencyRate"><%= RenderCheckBoxListCurrency(1) %></div>
                        <div class="marginT"></div>
                        <v4control:CheckBox runat="server" ID="chkShowRatesOnHomePage"/>
                    </div>
                </div>
                <div style="display: table-cell; width: 100%;">
                    <div class="spacer"></div>
                    <div id="divGridRate" style="float: left; margin-bottom: 10px; margin-right: 30px; overflow: auto; padding-right: 1px;">
                        <csg:Grid runat="server" ID="gridRate" GridAutoSize="False"/>
                    </div>
                    <iframe id="frameReportRate" style="border-width: 0px; height: 400px; width: 600px;"></iframe>
                </div>
            </div>
        </div>

        <div id="tabs-2">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRateAvg" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterAvgRates" runat="server" CSSClass="aligned_control"></v4control:Button>
                <div id="msgReportRateAvg" class="label" style="height: auto; margin-left: 20px; width: auto;"></div>
            </div>
            <div class="marginT"></div>
            <div style="display: table;">
                <div style="display: table-cell; vertical-align: top;">
                    <div style="width: 170px;">
                        <div class="label"><%= Resx.GetString("ExRate_lblCurrencies") %>:</div>
                        <div class="spacer"></div>
                        <div id="divCurrencyRateAvg"><%= RenderCheckBoxListCurrency(2) %></div>
                    </div>
                </div>
                <div style="display: table-cell; width: 100%;">
                    <div class="spacer"></div>
                    <div id="divGridRateAvg" style="min-width: 200px;">
                        <csg:Grid runat="server" ID="gridRateAvg" MarginBottom="116"/>
                    </div>
                </div>
            </div>
        </div>

        <div id="tabs-3">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRateCross" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterCrossRates" runat="server" CSSClass="aligned_control"></v4control:Button>
                <div id="msgReportRateCross" class="label" style="height: auto; margin-left: 20px; width: auto;"></div>
            </div>
            <div class="marginT"></div>
            <div style="display: table;">
                <div style="display: table-cell; vertical-align: top;">
                    <div style="width: 170px;">
                        <div class="label"><%= Resx.GetString("ExRate_lblCurrencyPair") %>:</div>
                        <div class="spacer"></div>
                        <v4dbselect:DBSCurrency ID="dbsCurrencyCrossSource" runat="server" IsRequired="True" NextControl="dbsCurrencyCrossTarget" CSSClass="aligned_control" Width="100px"></v4dbselect:DBSCurrency>
                        <div class="spacer"></div>
                        <v4dbselect:DBSCurrency ID="dbsCurrencyCrossTarget" runat="server" IsRequired="True" NextControl="dpRateDate" CSSClass="aligned_control" Width="100px"></v4dbselect:DBSCurrency>
                        <v4control:Button ID="btnInverseCurrencies" runat="server" Style="height: 22px; top: -12px; width: 27px;"></v4control:Button>
                        <div class="marginT"></div>
                        <v4control:CheckBox runat="server" ID="chkShowCrossRatesOnHomePage"/>
                    </div>
                </div>
                <div style="display: table-cell; width: 100%;">
                    <div class="spacer"></div>
                    <div id="divGridRateCross" style="float: left; margin-bottom: 10px; margin-right: 30px; overflow: auto; padding-right: 1px;">
                        <csg:Grid runat="server" ID="gridRateCross" GridAutoSize="False"/>
                    </div>
                    <iframe id="frameReportRateCross" style="border-width: 0px; height: 400px; width: 600px;"></iframe>
                </div>
            </div>
        </div>
    </div>
</div>

<div id="divRateEdit" style="display: none;">
    <div class="predicate_block">
        <div class="label"><%= Resx.GetString("ExRate_lblCurrency") %>:</div>
        <v4dbselect:DBSCurrency ID="dbsCurrencyEdit" runat="server" IsRequired="True" NextControl="dpRateDate" CSSClass="aligned_control" Width="125px"></v4dbselect:DBSCurrency>
    </div>

    <div class="predicate_block">
        <div class="label"><%= Resx.GetString("ExRate_lblRateDate") %>:</div>
        <v4control:DatePicker ID="dpRateDate" runat="server" IsRequired="True" NextControl="numRateValue" CSSClass="aligned_control" Width="100px"></v4control:DatePicker>
    </div>

    <div class="predicate_block">
        <div class="label"><%= Resx.GetString("ExRate_lblRateValueRub") %>:</div>
        <v4control:Number ID="numRateValue" runat="server" IsRequired="True" NextControl="numRateUnits" CSSClass="aligned_control" Width="100px"></v4control:Number>
    </div>

    <div class="predicate_block">
        <div class="label"><%= Resx.GetString("ExRate_lblRateUnits") %>:</div>
        <v4control:Number ID="numRateUnits" runat="server" IsRequired="True" NextControl="btnRateEditApply" CSSClass="aligned_control" Width="100px"></v4control:Number>
    </div>
</div>

</body>
</html>