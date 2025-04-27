(function ($) {
    'use strict';
    function EditStopRouteMessage() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {

            $('select#OperatorIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });
           
            $('select#VendorIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });
           
            $("#btn-submit").on("click", function () {

                var odata = $("#OperatorIds").select2('data');
                var udata = $("#VendorIds").select2('data');

                var oids = odata.map(function (val) {
                    return val.id;
                }).join(',');
               
                var uids = udata.map(function (val) {
                    return val.id;
                }).join(',');

                $("#OperatorIds :Selected").val(oids);
                $("#VendorIds :Selected").val(cids);

            });
            
        }

        $this.init = function () {

            initGridControlsWithEvents();
        };
    }
    $(function () {
        var self = new EditStopRouteMessage();
        self.init();
    });

}(jQuery));