
(function ($) {
    'use strict';
    function AddEditBank() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;
     
        function initModalControlsWithEvents() {
            // set select2
            $('select#AccountTypeId').select2();
            $('select#UserId').select2();
            $('select#ApiId').select2();
        }

        $this.init = function () {
            initModalControlsWithEvents();
            formAddEditLocation = new Global.FormHelper($("#model-AddEditBank-bankaccount"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new AddEditBank();
        self.init();
    });

}(jQuery));