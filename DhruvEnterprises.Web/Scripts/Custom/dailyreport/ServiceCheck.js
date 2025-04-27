(function ($) {
    'use strict';
    function RecentRechargeCrWise() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {



            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });


            if ($(".divclass > table").length > 0) {

                $("#btnExport").show();
            }
            else {
                $("#btnExport").hide();
            }

            $("#btnExport").click(function (e) {

                var filename = "RecentRechargeCrWise.xls";
                let file = new Blob([$('.divclass').html()], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });

            $('#btnSearch').on('click', function () {

                var requrl = '/DailyReport/ServiceCheck?i=0';
                var routeval = '';
                var ddOpId = $('select#ddOperator :Selected').val();
                var ddCircleId = $('select#ddCircle :Selected').val();
                if (ddOpId != '' && ddOpId != '0' && ddOpId != undefined) {
                    routeval += '&o=' + ddOpId;
                }
                if (ddCircleId != '' && ddCircleId != '0' && ddCircleId != undefined) {
                    routeval += '&c=' + ddCircleId;
                }
                window.location.href = requrl + routeval;


            });


            //$('#ddOperator').on('change', function () {

            //    var opid = 0;
            //    var ddOpId = $('select#ddOperator :Selected').val();


            //    if (ddOpId != '' && ddOpId != '0' && ddOpId != undefined) {
            //        opid = ddOpId;
            //    }


            //    var requrl = '/DailyReport/ServiceCheck?o=' + opid;

            //    window.location.href = requrl;


            //});

        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new RecentRechargeCrWise();
        self.init();
    });



}(jQuery));