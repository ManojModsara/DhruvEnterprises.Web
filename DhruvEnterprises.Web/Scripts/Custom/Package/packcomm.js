(function ($) {
    'use strict';
    function TagValueSettingIndex() {
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
              
                var Uid = $('select#ddUser :Selected').val();
              
                var requrl = '/package/packagecomm?i=1'

                var routeval = '';
              if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
               
                window.location.href = requrl + routeval;


            });
        }

        $this.init = function () {
            initGridControlsWithEvents();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new TagValueSettingIndex();
        self.init();
    });

   
}(jQuery));