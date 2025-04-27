(function ($) {
    'use strict';
    function DealerLapuDaybook() {
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



            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });

            //$('#txtFromDate').datepicker('setDate', today);
            //$('#txtToDate').datepicker('setDate', today);

            $('#btnSearch').on('click', function () {

                var Sdate = $('#txtFromDate').val();
                var edate = $('#txtToDate').val();
                var Uid = $('select#ddDealer :Selected').val();

                var requrl = '/DailyReport/DealerLapuDaybook?i=1';

                var routeval = '';
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&d=' + Uid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (edate != '' && edate != undefined && edate != '0') routeval += '&e=' + edate;
                window.location.href = requrl + routeval;


            });

            $("#btnExport").click(function (e) {

                var date1 = new Date($('#txtFromDate').val());

                var filename = "DealerLapuDaybook_" + date1.getFullYear() + "_" + date1.getMonth() + "_" + date1.getDate() +".xls";

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
        var self = new DealerLapuDaybook();
        self.init();
    });



}(jQuery));