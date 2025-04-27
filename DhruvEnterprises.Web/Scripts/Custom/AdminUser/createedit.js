
(function ($) {
    'use strict';
    function CreateEdit() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;
     
        function initModalControlsWithEvents() {
            // set select2
            $('select#RoleId').select2();           
            $('select#RoleId').change(function () {

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
            $('select#PackageId').select2();
            $('select#PackageId').change(function () {

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
            $('select#VendorId').select2();
            $('select#VendorId').change(function () {

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
            formAddEditLocation = new Global.FormHelper($("#model-createedit-adminuser"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit();
        self.init();
    });

}(jQuery));
function showIMG(input, div) {
    if (input.files && input.files[0]) {
        var filerdr = new FileReader();
        filerdr.onload = function (e) {
            $('#' + div + '').attr('src', e.target.result);
        }
        filerdr.readAsDataURL(input.files[0]);
    }
}