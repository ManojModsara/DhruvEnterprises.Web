(function ($) {
    'use strict';
    function UserFilterRuleReport() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    {}, //date
                    { "sName": "User.Username" },
                    { "sName": "Circle.CircleName" },
                    { "sName": "Oerator.Name" },
                    { "sName": "AmtPercent" },
                    { "sName": "Amount" },
                    { "sName": "Roffer" },
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "TotalCount" },
                    { "sName": "TotalAmount" },
                    { "sName": "RofferCount" },
                    { "sName": "RofferRcAmount" },
                ],
               
                "order": [[0, "desc"]],
                "aoColumnDefs": [

                    { 'bSortable': false, 'aTargets': [0] }
                ]

            }, "DailyReport/GetUserFilterRuleReport", filterdata, function () { });
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
                var Isa = $('#Isa').val();
                var requrl = '/DailyReport/UserFilterRuleReport?i=' + Isa;

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
            
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var Uid = $('select#ddUser :Selected').val();
            var Apiid = $('select#ddVendor :Selected').val();
            var Opid = $('select#ddOperator :Selected').val();
            var Cid = $('select#ddCircle :Selected').val();
            var Amount = $('#txtAmount').val();
          
            if (Sdate != '' && Sdate != '0') {
                filterdata.FromDate = Sdate;
                routeval += '&f=' + Sdate;
            }
                
            if (Edate != '' && Edate != '0') {
                filterdata.ToDate = Edate;
                routeval += '&e=' + Edate;
            }
            
            if (Uid != '' && Uid != '0' && Uid != undefined) {
                filterdata.UserId = Uid;
                routeval += '&u=' + Uid;
            }
               
            if (Apiid != '' && Apiid != '0' && Apiid != undefined) {
                filterdata.ApiId = Apiid;
                routeval += '&v=' + Apiid;
            }
            
            if (Opid != '' && Opid != '0' && Opid != undefined) {
                filterdata.OpId = Opid;
                routeval += '&o=' + Opid;
            }


            if (Cid != '' && Cid != '0' && Cid != undefined) {
                filterdata.CircleId = Cid;
                routeval += '&c=' + Cid;
            }
            
            if (Amount != '' && Amount != '0') {
                filterdata.Amount = Amount;
                routeval += '&a=' + Amount;
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
        var self = new UserFilterRuleReport();
        self.init();
    });



}(jQuery));