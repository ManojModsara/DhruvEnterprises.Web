(function ($) {
    'use strict';
    function CreateEdit1() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;
        function initModalControlsWithEvents() {
            $('select#DocId').select2();
            $('select#DocId').change(function () {
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
            formAddEditLocation = new Global.FormHelper($("#model-signup-account"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit1();
        self.init();
    });

}(jQuery));
