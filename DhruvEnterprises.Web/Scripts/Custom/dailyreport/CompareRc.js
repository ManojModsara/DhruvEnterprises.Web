(function ($) {
    'use strict';
    function CompareRc() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    {},
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "CustomerNo" },
                    { "sName": "Amount" },
                    { "sName": "StatusTxt" },
                    { "sName": "OurRefId" },
                    { "sName": "ApiTxnId" },
                    { "sName": "CompareRcStatu.Name" },
                    {}
                ],
               
                "order": [[0, "desc"]],
                "aoColumnDefs": [

                    { 'bSortable': false, 'aTargets': [1, 9] }
                ]

            }, "DailyReport/GetCompareRcReport", filterdata, function () { });
        }

        function initializeModalWithForm() {

            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }

            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });
            
            var date = new Date();
          
            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var start = new Date(date.getFullYear(), date.getMonth(), date.getDate()-30);

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

            $('#txtFromDate').datepicker('setDate', start);
            $('#txtToDate').datepicker('setDate', today );
            
            $('#btnSearch').on('click', function () {
                
                var requrl = '/dailyreport/CompareRc?i=' + Isa;

                var routeval = '';

                var fdata = GetFilterData();

                window.location.href = requrl + fdata.RouteVal;


            });
            
            $('#btnClose').on('click', function () {
                
                if ($('#dvSearchPanel').is(":hidden")) {
                   
                    $('#Isa').val('1');
                }
                else 
                {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();

            });

        }

        function GetFilterData() {
            var filterdata = {};
            var routeval = '';

            var Isa = $('#Isa').val();
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var Sid = $('select#ddStatus :Selected').val();
            var Apiid = $('select#ddVendor :Selected').val();
            var Customerno = $('#txtCustomerNo').val();
            var RefId = $('#txtRefId').val();
            var ApiTxnId = $('#txtApiTxnId').val();
            
            if (Sdate != '' && Sdate != '0') {
                filterdata.FromDate = Sdate;
                routeval += '&f=' + Sdate;
            }
                
            if (Edate != '' && Edate != '0') {
                filterdata.ToDate = Edate;
                routeval += '&e=' + Edate;
            }
               
            if (Sid != '' && Sid != '0') {
                filterdata.StatusId = Sid;
                routeval += '&s=' + Sid;
            }
               
            if (Apiid != '' && Apiid != '0' && Apiid != undefined) {
                filterdata.ApiId = Apiid;
                routeval += '&v=' + Apiid;
            }
            
            if (Customerno != '' && Customerno != '0') {
                filterdata.CustomerNo = Customerno;
                routeval += '&m=' + Customerno;
            }
            
            if (RefId != '' && RefId != '0') {
                filterdata.RefId = RefId;
                routeval += '&rf=' + RefId;
            }

            if (ApiTxnId != '' && ApiTxnId != '0') {
                filterdata.ApiTxnId = ApiTxnId;
                routeval += '&vt=' + ApiTxnId;
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
        var self = new CompareRc();
        self.init();
    });



}(jQuery));