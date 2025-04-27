(function () {
    'use strict';
    function ReportIndex() {
        var $this = this, locationGrid, formRequestResponse;
        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    {},
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "TxnId" },
                    { "sName": "Amount" },
                    { "sName": "RequestTime" }
                ],
                "order": [[1, "desc"]],
                //"aoColumnDefs": [
                //    Global.CurrentRoleId == 3 ? { 'visible': false, 'aTargets': [0] } : { 'bSortable': false, 'aTargets': [0] }
                //]
            }, "Order/MyOrder", filterdata);
        }
        function initializeModalWithForm() {
            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }

            var date = new Date();
            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());

            $('#txtFromDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                endDate: end,
                autoclose: true
            });

            $('#txtToDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                endDate: end,
                autoclose: true
            });

            $('#txtFromDate', '#txtToDate').datepicker('setDate', today);

            $('#btnSearch').on('click', function () {

                var Isa = $('#Isa').val();
                var Searchid = $('#txtSearchId').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();

                var requrl = '/Order/Index?i=' + Isa;

                var routeval = '';
                if (Searchid != '' && Searchid != '0' && Searchid != undefined) routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                window.location.href = requrl + routeval;
            });

            //$('#btnExport').on('click', function () {
            //    var Searchid = $('#txtSearchId').val();
            //    var Sdate = $('#txtFromDate').val();
            //    var Edate = $('#txtToDate').val();

            //    var requrl = '/Export/ExportCSV?rt=1';

            //    var routeval = '';
            //    if (Searchid != '' && Searchid != '0') routeval += '&rto=' + Searchid;
            //    if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
            //    if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;

            //    window.location.href = requrl + routeval;
            //});

            $('#btnClose').on('click', function () {
                if ($('#dvSearchPanel').is(":hidden")) {
                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();
            });

            $("#modal-Order-Detail").on('show.bs.modal', function (event) {
                $('#modal-Order-Detail .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }
        function GetFilterData() {
            var filterdata = {};
            var Searchid = $('#txtSearchId').val();
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();

            if (Searchid != '' && Searchid != '0')
                filterdata.RefId = Searchid;

            if (Sdate != '' && Sdate != '0')
                filterdata.FromDate = Sdate;

            if (Edate != '' && Edate != '0')
                filterdata.ToDate = Edate;
            filterdata.EndDate = Edate;

            return filterdata;
        }
        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ReportIndex();
        self.init();
    });
}(jQuery));