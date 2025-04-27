
(function ($) {
    'use strict';
    function CreateEdit() {

        var $this = this, locationGrid, formAddEditLocation, uploadObj;

        function initModalControlsWithEvents() {
            // set select2
           

        }
        function initializeModalWithForm() {
        }
        $this.init = function () {
            initModalControlsWithEvents();
            initializeModalWithForm();
            formAddEditLocation = new Global.FormHelper($("#model-createedit-adminuser"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit();
        self.init();
    });



}(jQuery));

