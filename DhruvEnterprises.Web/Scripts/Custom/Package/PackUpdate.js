(function ($) {
    'use strict';
    function PackCommRange() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {


            $('select#ddOpIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddCircleIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddUserIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddOpTypeIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddPackages').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddAmtType').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $("#btnAdd").on("click", function () {

                var ddOpTypeIds = $("#ddOpTypeIds :Selected");
                var opId = $("#ddOpIds :Selected");
                var circleId = $("#ddCircleIds :Selected");
                var ddCommType = $("#ddCommType :Selected");
                var ddAmtType = $("#ddAmtType :Selected");

                var amountrange = $("#AmountRange").val();
                var commamt = $("#CommAmt").val();
                var PackName = $("#ddPackages :Selected");

                var optypedata = $("#ddOpTypeIds").select2('data');
                var opdata = $("#ddOpIds").select2('data');
                var crdata = $("#ddCircleIds").select2('data');

                var optypeids = optypedata.map(function (val) {
                    return val.id;
                }).join(',');

                var optypenames = optypedata.map(function (val) {
                    return val.text;
                }).join(',');

                var opids = opdata.map(function (val) {
                    return val.id;
                }).join(',');

                var opnames = opdata.map(function (val) {
                    return val.text;
                }).join(',');

                var circleids = crdata.map(function (val) {
                    return val.id;
                }).join(',');

                var circlenames = crdata.map(function (val) {
                    return val.text;
                }).join(',');

                if (amountrange == '' || amountrange == undefined) {
                    alertify.error("Invalid Amount Range");
                }
                else if (commamt == '' || commamt == undefined) {
                    alertify.error("Invalid Comm Amount");
                }
                else {
                    var cObj = {};
                    cObj.Id = 0;
                    cObj.PackName = PackName;
                    cObj.OpTypeIds = optypeids;
                    cObj.OpTypeNames = optypenames;
                    cObj.OperatorIds = opids;
                    cObj.OperatorNames = opnames;
                    cObj.CircleIds = circleids;
                    cObj.CircleNames = circlenames;
                    cObj.AmountRange = amountrange;
                    cObj.CommAmt = commamt;
                    cObj.AmtTypeId = ddAmtType.val();
                    cObj.CommTypeId = ddCommType.val();
                    $.post(Global.DomainName + 'package/PackCommUpdate', { data: cObj }, function (result) {
                        if (result == 0) {
                            alertify.success("Data Saved Successfully.");
                            //Get the reference of the Table's TBODY element.
                            window.location.reload(true);
                        }
                        else {
                            alertify.error("Something went wrong!");
                        }

                    });
                }
            });
        }

        function initializeModalWithForm() {
            
        }

        $this.init = function () {

            initGridControlsWithEvents();
            initializeModalWithForm();

        };
    }
    $(function () {

        var self = new PackCommRange();
        self.init();


    });


}(jQuery));