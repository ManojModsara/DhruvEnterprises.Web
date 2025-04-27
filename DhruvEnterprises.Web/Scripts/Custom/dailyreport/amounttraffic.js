(function ($) {
    'use strict';
    function AmountTraffic() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {


            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });


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
                var Edate = $('#txtToDate').val();
                var requrl = '/DailyReport/AmountTraffic?i=1';
                var Uid = $('select#ddUser :Selected').val();
                var apiid = $('select#ddVendor :Selected').val();
                var opid = $('select#ddOperator :Selected').val();
                var cid = $('select#ddCircle :Selected').val();
                var routeval = '';
                if (opid != '' && opid != '0' && opid != undefined) routeval += '&o=' + opid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (apiid != '' && apiid != '0' && apiid != undefined) routeval += '&v=' + apiid;
                if (cid != '' && cid != '0' && cid != undefined) routeval += '&c=' + cid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;

                window.location.href = requrl + routeval;



            });

        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new AmountTraffic();
        self.init();
    });



}(jQuery));