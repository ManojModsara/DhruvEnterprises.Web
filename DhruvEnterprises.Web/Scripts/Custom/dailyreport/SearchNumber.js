(function ($) {
    'use strict';
    function SearchNumber() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeModalWithForm() {

            if ($(".divclass > table").length > 0) {

                $("#btnExport").show();
            }
            else {
                $("#btnExport").hide();
            }

            //$('#btnSearch').on('click', function () {

            //    debugger;
            //    var number = $('#txtCustomerNo').val();
             
            //    var requrl = '/DailyReport/SearchNumber?n=' + number;
              
              
            //    window.location.href = requrl;
                

            //});

            $("#btnExport").click(function (e) {
              
                var filename = "RechargeByNumber.xls";
                let file = new Blob([$('.divclass').html()], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });

            $('#txtCustomerNo').on('blur', function () {

              
                var number = $('#txtCustomerNo').val();

                var requrl = '/DailyReport/SearchNumber?n=' + number;


                window.location.href = requrl;


            });

            $(document).on("click", "a[name='complaint']", function (e) {
                var runid = $(this).data('runid');
                $("#recid").val(runid);
                $("#myModal").modal('show');                
            });
            $('#cComplaint').on('click', function () {
                $("#pageloader").css("display", "block");
                CreateComplaint();
            });
            function CreateComplaint() {
                var recId = $("#recid").val();
                var remarks = $("#remark").val();
                $.post(Global.DomainName + 'RechargeReport/CreateComplaint1', { RecId: recId, Remark: remarks }, function (result) {
                    $("#pageloader").css("display", "none");
                    if (result == 1) {
                        alert("Complaint Created Succesfully.");
                        //$("#myModal").modal('hide');
                    }
                    $("#myModal").modal('hide');
                });
            }
            //$(document).on("click", "a[name='reqRes']", function (e) {
            //    var runid = $(this).data('runid1');
            //    $("#myModalReqRes").modal('show');
            //    GetReqRes(runid);
            //});
            //function GetReqRes(recid) {                
            //    $.post(Global.DomainName + 'RequestResponse/GetRequestResponse1', { recid: recid }, function (result) {
            //        console.log(result);
            //        alert(result);
            //        $("#pageloader").css("display", "none");
            //        if (result == 1) {
            //            alert("Complaint Created Succesfully.");
            //            $("#myModalReqRes").modal('hide');
            //        }
            //    });
            //}
           
        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new SearchNumber();
        self.init();
    });



}(jQuery));