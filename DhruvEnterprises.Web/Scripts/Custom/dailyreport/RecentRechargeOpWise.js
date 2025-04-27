(function ($) {
    'use strict';
    function RecentRechargeOpWise() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {





            if ($(".divclass > table").length > 0) {

                $("#btnExport").show();
            }
            else {
                $("#btnExport").hide();
            }

            $("#btnExport").click(function (e) {
              
                var filename = "RecentRechargeOpWise.xls";
                let file = new Blob([$('.divclass').html()], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });


            //$('.select2').select2({
            //    placeholder: "--select--",
            //    allowClear: true
            //});
            //$('#ddOperator').on('change', function () {

            //    var opid = 0;
            //    var ddOpId = $('select#ddOperator :Selected').val();
              

            //    if (ddOpId != '' && ddOpId != '0' && ddOpId != undefined)
            //    {
            //        opid = ddOpId;
            //    }
                   

            //    var requrl = '/DailyReport/RecentRechargeOpWise?o=' + opid;
                
            //    window.location.href = requrl;


            //});

        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new RecentRechargeOpWise();
        self.init();
    });



}(jQuery));