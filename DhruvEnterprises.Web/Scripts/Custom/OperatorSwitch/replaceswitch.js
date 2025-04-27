
(function ($) {
    'use strict';
    function ReplaceSwitch() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;
     
        function initModalControlsWithEvents() {
           
            // set select2
            $('select#OperatorId').select2();
            $('select#SwitchTypeId').select2();
            $('select#CurrentApiId').select2();
            $('select#NewApiId').select2();
            
        }

        $this.init = function () {
            initModalControlsWithEvents();
            formAddEditLocation = new Global.FormHelper($("#model-ReplaceSwitch"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new ReplaceSwitch();
        self.init();
    });

}(jQuery));