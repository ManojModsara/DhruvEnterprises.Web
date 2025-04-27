(function ($) {
    'use strict';
    function AdminOtpVerfyIndex() {
        var $this = this, locationGrid, formAddEditRole;

        function initializeModalWithForm() {

            $("#modal-add-edit-Otprole").on('show.bs.modal', function (event) {
                $('#modal-add-edit-Otprole .modal-content').load($(event.relatedTarget).prop('href'));
            });

        }

        $this.init = function () {
            initializeModalWithForm();
            //addeditpopup(btn);
        };
    }
    $(function () {
        var self = new AdminOtpVerfyIndex();
        self.init();
    });

}(jQuery));