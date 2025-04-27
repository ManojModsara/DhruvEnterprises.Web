(function ($) {
    'use strict';
    function CompareRcVendor() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    {},
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "FilesName" },
                    { "sName": "MissMatchCount" },
                    { "sName": "MatchCount" },
                    { "sName": "TotalCount" },
                    { "sName": "RcCount" },
                    { "sName": "UploadCount" },
                    {},
                    {}
                ],
               
                "order": [[0, "desc"]],
                "aoColumnDefs": [

                    { 'bSortable': false, 'aTargets': [1, 9, 10] }
                ]
                
            }, "DailyReport/GetCompareRcVendorReport", filterdata);
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
                
                var requrl = '/dailyreport/CompareRcVendor?i=' + Isa;

                var routeval = '';

                var filtedata = GetFilterData();

                window.location.href = requrl + filtedata.RouteVal;


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
            var Apiid = $('select#ddVendor :Selected').val();

            if (Sdate != '' && Sdate != '0') {
                filterdata.FromDate = Sdate;
                routeval += '&f=' + Sdate;
            }
                
            if (Edate != '' && Edate != '0') {
                filterdata.ToDate = Edate;
                routeval += '&e=' + Edate;
            }
               
            if (Apiid != '' && Apiid != '0' && Apiid != undefined) {
                filterdata.ApiId = Apiid;
                routeval += '&v=' + Apiid;
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
        var self = new CompareRcVendor();
        self.init();
    });



}(jQuery));