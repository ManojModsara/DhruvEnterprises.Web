
(function ($) {
    'use strict';
    function AddPullAccount() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;
     
        function initModalControlsWithEvents() {
            // set select2
            $('select#AccountId').select2();
            $('select#TrTypeId').select2();
            $('select#RefAccountId').select2();
            $('select#UserId').select2();
            $('select#ApiId').select2();

            var date = new Date();
            var start = new Date(date.getFullYear(), date.getMonth() - 1, date.getDate());
            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());

            $('#PaymentDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                //  startDate: start,
                endDate: end,
                autoclose: true
            });
            
        }

        $this.init = function () {
            initModalControlsWithEvents();
            formAddEditLocation = new Global.FormHelper($("#model-addpullaccount-bankaccount"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new AddPullAccount();
        self.init();
    });

}(jQuery));