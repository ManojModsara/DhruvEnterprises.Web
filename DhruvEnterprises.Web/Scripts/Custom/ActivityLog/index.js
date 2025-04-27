(function ($) {
    'use strict';
    function ActivityLogIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "ActivityName" },
                    { "sName": "ActivityDate" },
                    { "sName": "IPAddress" },
                    { "sName": "ActivityPage" },
                    { "sName": "Remark" }
                ],
                "order": [[0, "desc"]]
            }, "ActivityLog/GetActivityLogReport");
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

            $('#btnSearch').on('click', function () {

                var Isa = $('#Isa').val();
                var IPAddress = $('#txtIPAddress').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Uid = $('select#ddUser :Selected').val();

                var ActivityName = $('#txtActivityName').val();
                var ActivityUrl = $('#txtUrl').val();
                var Remark = $('#txtRemark').val();

                var requrl = '/ActivityLog/Index?i=' + Isa;

                var routeval = '';
                if (IPAddress != '' &&  IPAddress != undefined) routeval += '&a=' + IPAddress;
                if (Sdate != '' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != ''  && Edate != undefined) routeval += '&e=' + Edate;
                if (Uid != ''  && Uid != undefined) routeval += '&u=' + Uid;
                if (ActivityName != '' && ActivityName != undefined) routeval += '&n=' + ActivityName;
                if (ActivityUrl != '' && ActivityUrl != undefined) routeval += '&p=' + ActivityUrl;
                if (Remark != '' && Remark != undefined) Remark += '&r=' + Remark;

                window.location.href = requrl + routeval;
                
            });

            $('#btnExport').on('click', function () {
                
                var IPAddress = $('#txtIPAddress').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Uid = $('select#ddUser :Selected').val();

                var ActivityName = $('#txtActivityName').val();
                var ActivityUrl = $('#txtUrl').val();
                var Remark = $('#txtRemark').val();

                var requrl = '/Export/ExportCSV?rt=5';

                var routeval = '';
                if (IPAddress != '' && IPAddress != undefined) routeval += '&m=' + IPAddress;
                if (Sdate != '' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != undefined) routeval += '&e=' + Edate;
                if (Uid != '' && Uid != undefined) routeval += '&u=' + Uid;
                if (ActivityName != '' && ActivityName != undefined) routeval += '&rto=' + ActivityName;
                if (ActivityUrl != '' && ActivityUrl != undefined) routeval += '&ut=' + ActivityUrl;
                if (Remark != '' && Remark != undefined) Remark += '&rm=' + Remark;

                window.location.href = requrl + routeval;


            });

            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();

            });




            $("#modal-view-user-detail").on('show.bs.modal', function (event) {
                $('#modal-view-user-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-url-detail").on('show.bs.modal', function (event) {
                $('#modal-view-url-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-rec-detail").on('show.bs.modal', function (event) {
                $('#modal-view-rec-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });


        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ActivityLogIndex();
        self.init();
    });

}(jQuery));