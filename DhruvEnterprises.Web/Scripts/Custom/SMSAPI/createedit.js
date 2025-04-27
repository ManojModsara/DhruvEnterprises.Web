(function ($) {
    'use strict';
    function CreateEdit() {
        var $this = formAddEditLocation;

        $this.init = function () {
            formAddEditLocation = new Global.FormHelper($("#model-createedit-SMSAPI"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit();
        self.init();
    });

}(jQuery));