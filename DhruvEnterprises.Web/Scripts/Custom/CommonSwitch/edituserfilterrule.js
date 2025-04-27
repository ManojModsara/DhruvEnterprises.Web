(function ($) {
    'use strict';
    function EditUserFilterRule() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {

            $('select#OperatorIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });

            $('select#CircleIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });
           
            $('select#UserIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });
            
            $('select#VendorIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });


            $("#btn-submit").on("click", function () {

                var odata = $("#OperatorIds").select2('data');
                var cdata = $("#CircleIds").select2('data');
                var udata = $("#UserIds").select2('data');
                var vdata = $("#VendorIds").select2('data');
               
                var oids = odata.map(function (val) {
                    return val.id;
                }).join(',');
                var cids = cdata.map(function (val) {
                    return val.id;
                }).join(',');

                var uids = udata.map(function (val) {
                    return val.id;
                }).join(',');

                var vids = vdata.map(function (val) {
                    return val.id;
                }).join(',');

                $("#OperatorIds :Selected").val(oids);
                $("#CircleIds :Selected").val(cids);
                $("#UserIds :Selected").val(uids);
                $("#VendorIds :Selected").val(vids);

            });
            
        }

        $this.init = function () {

            initGridControlsWithEvents();
        };
    }
    $(function () {
        var self = new EditUserFilterRule();
        self.init();
    });

}(jQuery));