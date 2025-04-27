(function ($) {
    'use strict';
    function VendorOpComm() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
           
        }

      
        function initializeModalWithForm() {
            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });

            $('#btnSearch').on('click', function () {

                var Isa = $('#Isa').val();
              
                var apiid = $('select#ddVendor :Selected').val();
                var opid = $('select#ddOperator :Selected').val();
              
                var requrl = '/package/vendoropcomm?i=1'

                var routeval = '';
                if (apiid != '' && apiid != '0' && apiid != undefined) routeval += '&v=' + apiid;
                if (opid != '' && opid != '0' && opid != undefined) routeval += '&o=' + opid;
               
                window.location.href = requrl + routeval;


            });
        }

        $this.init = function () {
            initGridControlsWithEvents();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new VendorOpComm();
        self.init();
    });

   
}(jQuery));