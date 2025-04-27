(function ($) {
    'use strict';
    function WalletExchange() {
        var $this = this,  formRequestResponse;

        function initializeGrid() {

            var filterdata = GetFilterData();

            //var locationGrid = new Global.GridAjaxHelper('#grid-index', {

            //    "aoColumns": [                   
            //        { "sName": "UserId" },
            //        { "sName": "Amount" },
            //        {},                    
            //         { "sName": "Comment" }
            //        , { "sName": "UpdatedById" }
            //    ,{ "sName": "TransferType.Name" }
            //    ],
            //    "order": [[0, "desc"]],
            //    "aoColumnDefs": [
            //        Global.CurrentRoleId == 3 ? { 'visible': false, 'aTargets': [0] } : { 'bSortable': false, 'aTargets': [1] }
            //    ]
            //}, "Wallet/GetWalletExchange", filterdata);
        }

        function initializeModalWithForm() {
            $("#btnExport").click(function (e) {


                var date1 = new Date();

                var filename = "WalletRequest_" + date1.getFullYear() + "_" + date1.getMonth() + "_" + date1.getDate() + ".xls";
                var htmltext = '<div><table>' + $('#grid-index').html() + '</table></div>';

                let file = new Blob([htmltext], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });           

            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }
            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#IsShow').val('1');
                }
                else {
                    $('#IsShow').val('0');
                }
                $('#dvSearchPanel').toggle();

            });

            var date = new Date();

            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            // var start = new Date(date.getFullYear(), date.getMonth(), date.getDate()-30);

            $('#txtFromDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                //  startDate: start,
                endDate: end,
                autoclose: true
            });

            $('#txtToDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                //  startDate: start,
                endDate: end,
                autoclose: true
            });
            
            $('#txtFromDate', '#txtToDate').datepicker('setDate', today);

            

           
        }

        function GetFilterData() {
            var filterdata = {};
            var Isa = $('#IsShow').val();
            var routeval = '';            
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();            
            var trtypeid = 7;
           
            if (Sdate != '' && Sdate != '0') {
                routeval += '&f=' + Sdate;
                filterdata.FromDate = Sdate;
            }
            if (Edate != '' && Edate != '0') {
                routeval += '&e=' + Edate;
                filterdata.ToDate = Edate;
            }
            filterdata.RouteVal = routeval;            
            return filterdata;
        }
        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new WalletExchange();
        self.init();
    });

}(jQuery));