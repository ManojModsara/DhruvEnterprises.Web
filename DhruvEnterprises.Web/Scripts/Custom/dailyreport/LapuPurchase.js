(function ($) {
    'use strict';
    function LapuPurchase() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {


            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });


           
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

                
                $('#dvSearchPanel').toggle();

            });

            $('#btnSearch').on('click', function () {

                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var requrl = '/DailyReport/LapuPurchase?i=1';
                var Uid = $('select#ddUser :Selected').val();
                var apiid = $('select#ddVendor :Selected').val();
                var opid = $('select#ddOperator :Selected').val();
                var cid = $('select#ddCircle :Selected').val();
                var routeval = '';
                if (opid != '' && opid != '0' && opid != undefined) routeval += '&o=' + opid;
                if (cid != '' && cid != '0' && cid != undefined) routeval += '&c=' + cid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (apiid != '' && apiid != '0' && apiid != undefined) routeval += '&v=' + apiid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;

                window.location.href = requrl + routeval;



            });

            $("#btnExport").click(function (e) {

                var filename = "LapuPurchaseDaybook.xls";
                let file = new Blob(["<style> td{border:1px solid black;} </style>" + $('.divExport').html()], { type: "application/vnd.ms-excel" });
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
        var self = new LapuPurchase();
        self.init();
    });



}(jQuery));