(function ($) {
    'use strict';
    function RandomKeyGen() {
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
            $("#modal-delete-adminuser").on('hidden.bs.modal', function () {
                $(this).removeData('bs.modal');
            });
            $("#btnExport").click(function (e) {

                var filename = "RandoKeys.xls";
                let file = new Blob([$('#tblResult').html()], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });


            $("#btnGenerateKeys").on("click", function () {
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();
                //Loop through the Table rows and build a JSON array.
                var cObjList = [];
                $("#tblRandomKeys TBODY TR").each(function () {
                    var row = $(this);
                    debugger;

                    var ddKeyType = row.find("#ddKeyType :Selected").val();
                    var lenghtOrText = row.find("#txtLengthOrText").val();
                    var noOfkeys = $("#txtNoOfKeys").val();
                    if (noOfkeys == '' || noOfkeys == '0' || noOfkeys == undefined) {
                        noOfkeys = 10;
                    }

                    var id = row.find("TD").eq(0).html()

                    cObjList.push
                        ({
                            Id: id,
                            UserId: Uid,
                            OpId: Opid,
                            Apiid: Apiid,
                            KeyTypeId: ddKeyType,
                            LengthOrText: lenghtOrText,
                            NoOfKeys: noOfkeys
                        });


                });


                $.post(Global.DomainName + 'dailyreport/OpGenrateList', { data: cObjList }, function (result) {
                    debugger;
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else {

                        window.location.href ='/dailyreport/OperatorGenrateUserWithVendor';
                    }
                });

            });


        }

        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new RandomKeyGen();
        self.init();
    });



}(jQuery));