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
    <script src="/styles/Kesco.V4/JS/jquery.floatThead.min.js" type="text/javascript"></script>
    <script src="/styles/Kesco.V4/JS/Kesco.Grid.js" type="text/javascript"></script>
</head>
<body>

<div class="marginD"><%= RenderDocumentHeader() %></div>
<%--    <span style="font-weight: bold;">Курсы валют</span>--%>
<div class="v4formContainer">
    <div id="tabs">
        <ul>
            <li id="tabs1">
                <a href="#tabs-1">
                    <nobr>&nbsp;<%= Resx.GetString("ExRate_lblExRateDetail") %></nobr>
                </a>
            </li>
            <li id="tabs2">
                <a href="#tabs-2">
                    <nobr>&nbsp;<%= Resx.GetString("ExRate_lblExRateAverage") %></nobr>
                </a>
            </li>
            <li id="tabs3">
                <a href="#tabs-3">
                    <nobr>&nbsp;<%= Resx.GetString("ExRate_lblExRateCross") %></nobr>
                </a>
            </li>
        </ul>                    

        <div id="tabs-1">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRate" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterRates" runat="server" CSSClass="aligned_control"></v4control:Button>
            </div>
            <div class="marginT"></div>
            <div style="display: table;">
                <div style="display: table-cell; vertical-align: top;">
                    <div style="width: 170px;">
                        <div class="label"><%= Resx.GetString("ExRate_lblCurrencies") %>:</div>
                        <div class="spacer"></div>
                        <div id="divCurrencyRate"><%= RenderCheckBoxListCurrency(1) %></div>
                    </div>
                </div>
                <div style="display: table-cell; width: 100%;">
                    <v4control:Button ID="btnAddRate" runat="server"></v4control:Button>
                    <div class="spacer"></div>
                    <div id="divGridRate">
                        <csg:Grid runat="server" ID="gridRate" MarginBottom="166"/>
                    </div>
                </div>
            </div>
        </div>

        <div id="tabs-2">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRateAvg" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterAvgRates" runat="server" CSSClass="aligned_control"></v4control:Button>
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
                        <csg:Grid runat="server" ID="gridRateAvg" MarginBottom="146"/>
                    </div>
                </div>
            </div>
        </div>

        <div id="tabs-3">
            <div class="predicate_block">
                <v4control:PeriodTimePicker runat="server" ID="periodRateCross" Width="200px" CSSClass="aligned_control"></v4control:PeriodTimePicker>
                <v4control:Button ID="btnFilterCrossRates" runat="server" CSSClass="aligned_control"></v4control:Button>
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
                        <v4control:Button ID="btnInverseCurrencies" runat="server" Style="width: 27px; height: 22px; top: -12px;"></v4control:Button>
                    </div>
                </div>
                <div style="display: table-cell; width: 100%;">
                    <div class="spacer"></div>
                    <div id="divGridRateCross">
                        <csg:Grid runat="server" ID="gridRateCross" MarginBottom="166"/>
                    </div>
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
        <div class="label"><%= Resx.GetString("ExRate_lblRateValue") %>:</div>
        <v4control:Number ID="numRateValue" runat="server" IsRequired="True" NextControl="numRateUnits" CSSClass="aligned_control" Width="100px"></v4control:Number>
    </div>

    <div class="predicate_block">
        <div class="label"><%= Resx.GetString("ExRate_lblRateUnits") %>:</div>
        <v4control:Number ID="numRateUnits" runat="server" IsRequired="True" NextControl="btnRateEditApply" CSSClass="aligned_control" Width="100px"></v4control:Number>
    </div>
</div>

</body>
</html>