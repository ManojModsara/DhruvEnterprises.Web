(function ($) {
    'use strict';
    function EditBlockRoute() {
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

            $('select#ddTypeId').select2();
            $('select#ddStatus').select2();

            $('select#OpTypeIds').select2({
                placeholder: "--Select--",
                allowClear: true
            });


            $("#btn-submit").on("click", function () {

                var odata = $("#OperatorIds").select2('data');
                var cdata = $("#CircleIds").select2('data');
                var udata = $("#UserIds").select2('data');
               
                var oids = odata.map(function (val) {
                    return val.id;
                }).join(',');
                var cids = cdata.map(function (val) {
                    return val.id;
                }).join(',');

                var uids = udata.map(function (val) {
                    return val.id;
                }).join(',');

                $("#OperatorIds :Selected").val(oids);
                $("#CircleIds :Selected").val(cids);
                $("#UserIds :Selected").val(uids);

                var otdata = $("#OpTypeIds").select2('data');
                var otids = otdata.map(function (val) {
                    return val.id;
                }).join(',');
                $("#OpTypeIds :Selected").val(otids);

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