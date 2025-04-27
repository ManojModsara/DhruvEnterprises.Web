(function ($) {
    'use strict';
    function ApiWalletAddEdit() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
            $('select#OperatorId').select2();
            $('select#CircleIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#UserIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#LapuIds').select2();

            $('select#apiId').select2();

            $('select#FilterTypeId').select2();

            $("#btn-submit").on("click", function () {

                var data = $("#CircleIds").select2('data');

                var circleids = data.map(function (val) {
                    return val.id;
                }).join(',');
                
                $("#CircleIds :Selected").val(circleids);

                debugger;
                var udata = $("#UserIds").select2('data');

                var userids = udata.map(function (val) {
                    return val.id;
                }).join(',');

                $("#UserIds :Selected").val(userids);
                
            });

            $('select#apiId').on('change', function () {

                debugger;

                var vid = $(this).val();

                var ddlLapuIds = $('select#LapuIds');
              
                var opId = $("#OperatorId :Selected").val();
                var data = $("#CircleIds").select2('data');

                var circleids = data.map(function (val) {
                    return val.id;
                }).join(',');

                $.post(Global.DomainName + 'Lapu/GetLapuListByApiId', { apiid: vid, opid: opId, cids: circleids }, function (result) {

                    debugger;
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else if (result == "1") {

                        alertify.error("Something went wrong.");
                    }
                    else {
                        ddlLapuIds.empty().append('<option selected="selected" value="0">Select Lapu</option>');
                        $.each(result, function () {
                            ddlLapuIds.append($("<option></option>").val(this['Value']).html(this['Text']));
                        });
                    }
                });


            });

        }

        $this.init = function () {

            initGridControlsWithEvents();
        };
    }
    $(function () {
        var self = new ApiWalletAddEdit();
        self.init();
    });

}(jQuery));