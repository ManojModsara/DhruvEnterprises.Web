(function ($) {
    'use strict';
    function AdminRoleIndex() {
        var $this = this, locationGrid, formAddEditRole;
        
        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "RoleName" },
                    {
                    }
                ],
                "order": [[0, "asc"]]
            }, "role/getroles");
        }

        function initializeModalWithForm() {
            
            $("#modal-add-edit-adminrole").on('show.bs.modal', function (event)
            {
                $('#modal-add-edit-adminrole .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-delete-adminrole").on('show.bs.modal', function (event) {
                $('#modal-delete-adminrole .modal-content').load($(event.relatedTarget).prop('href'));
            });


        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
            //addeditpopup(btn);
        };
    }
    $(function () {
        var self = new AdminRoleIndex();
        self.init();
    });

}(jQuery));