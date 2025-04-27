(function ($) {
    'use strict';
    function ReportIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {
            
            var date = new Date();

            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var start = new Date(date.getFullYear(), date.getMonth(), date.getDate() - 30);

            $('#txtFromDate').datepicker({
                format: "mm/dd/yyyy",
                todayHighlight: true,
               // startDate: start,
                endDate: end,
                autoclose: true
            });
            $('#txtToDate').datepicker({
                format: "mm/dd/yyyy",
                todayHighlight: true,
              //  startDate: start,
                endDate: end,
                autoclose: true
            });

            //$('#txtFromDate').datepicker('setDate', today);
            //$('#txtToDate').datepicker('setDate', today);

            $('#btnSearch').on('click', function () {

                var Sdate = $('#txtFromDate').val();
                var edate = $('#txtToDate').val();
                var requrl = '/DailyReport/UserDayBook?i=1';

                var routeval = '';
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (edate != '' && edate != undefined && edate != '0') routeval += '&e=' + edate;
                window.location.href = requrl + routeval;


            });

            $("#btnExport").click(function (e) {
                debugger;

                var date1 = new Date($('#txtFromDate').val());

                var filename = "userDayBook_" + date1.getFullYear() + "_" + date1.getMonth() + "_" + date1.getDate() +".xls";

                let file = new Blob([$('.divclass').html()], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
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