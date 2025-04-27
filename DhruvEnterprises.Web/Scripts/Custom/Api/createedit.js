
(function ($) {
    'use strict';
    function CreateEdit() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;

        function initModalControlsWithEvents() {
            // set select2
            $('select#ApiTypeId').select2();
            $('select#ApiTypeId').change(function () {
                var value = $(this).val();
                if (value == '' || value == null) {
                    value = "0";
                }
                else {
                    $(this).closest('div').find('div.select2-container').removeClass("error").addClass("valid");
                    $(this).closest('div').find('label.error').hide();
                    $(this).removeClass("error").addClass("valid");
                }
            });

        }
      
        $this.init = function () {
            initModalControlsWithEvents();
            formAddEditLocation = new Global.FormHelper($("#model-createedit-apiuser"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit();
        self.init();
    });

}(jQuery));