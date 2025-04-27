(function ($) {
    'use strict';
    function EditBlockRoute() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {

            $('select#OpTypeIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#OperatorIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#CircleIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });
                                
            $('select#CommTypeId').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#AmtTypeId').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#OpTypeIds').on('change', function () {
                debugger;

                var ddOpIds = $('select#OperatorIds');
                var data = $(this).select2('data');

                var optypeids = data.map(function (val) {
                    return val.id;
                }).join(',');

                $.post(Global.DomainName + 'CommonSwitch/GetOperatorByType', { OpTypeIds: optypeids }, function (result) {

                    debugger;
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else if (result == "1") {

                        alertify.error("Something went wrong.");
                    }
                    else {
                        ddOpIds.empty().append($("<option></option>").val('0').html('All'));
                        $.each(result, function () {
                            ddOpIds.append($("<option></option>").val(this['OperatorId']).html(this['OperatorName']));
                        });
                    }
                });
                
            });

            $("#btn-submit").on("click", function () {
                debugger;
                var otdata = $("#OpTypeIds").select2('data');
                var odata = $("#OperatorIds").select2('data');
                var cdata = $("#CircleIds").select2('data');

                var otids = otdata.map(function (val) {
                    return val.id;
                }).join(',');
                var oids = odata.map(function (val) {
                    return val.id;
                }).join(',');
                var cids = cdata.map(function (val) {
                    return val.id;
                }).join(',');

                $("#OpTypeIds :Selected").val(otids);
                $("#OperatorIds :Selected").val(oids);
                $("#CircleIds :Selected").val(cids);
               
            });            
        }

        $this.init = function () {

            initGridControlsWithEvents();
        };
    }
    $(function () {
        var self = new EditBlockRoute();
        self.init();
    });

}(jQuery));