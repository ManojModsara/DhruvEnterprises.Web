(function ($) {
    'use strict';
    function ReportIndex() {
        var $this = this, locationGrid, formRequestResponse;

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
            var start = new Date(date.getFullYear(), date.getMonth(), date.getDate() - 30);

            $('#txtFromDate').datepicker({
                format: "mm/dd/yyyy",
                todayHighlight: true,
                //startDate: start,
                endDate: end,
                autoclose: true
            });
            $('#txtToDate').datepicker({
                format: "mm/dd/yyyy",
                todayHighlight: true,
               // startDate: start,
                endDate: end,
                autoclose: true
            });

            $('#txtFromDate', '#txtToDate').datepicker('setDate', today);
            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();

            });
            $('#btnSearch').on('click', function () {

                var Sdate = $('#txtFromDate').val();
                var Uid = $('select#ddUser :Selected').val();
                var requrl = '/DailyReport/AdminUserDayBook?';

                var routeval = '';
                if (Sdate != '' && Sdate != '0') routeval += 'f=' + Sdate;
                if (Uid != '' && Uid != '0') routeval += '&u=' + Uid;
                window.location.href = requrl + routeval;


            });

        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ReportIndex();
        self.init();
    });



}(jQuery));