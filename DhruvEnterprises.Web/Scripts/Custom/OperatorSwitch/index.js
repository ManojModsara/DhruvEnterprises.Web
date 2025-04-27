(function ($) {
    'use strict';
    function OperatorIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "Name" },
                    { "sName": "API1_Id" },
                    { "sName": "API2_Id" },
                    { "sName": "API3_Id" }
                ],
                "order": [[0, "desc"]]
                
            }, "OperatorSwitch/GetOperatorSwitch");
        }

        function initializeModalWithForm() {

           
            $("#modal-view-user-detail").on('show.bs.modal', function (event) {
                $('#modal-view-user-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-url-detail").on('show.bs.modal', function (event) {
                $('#modal-view-url-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-rec-detail").on('show.bs.modal', function (event) {
                $('#modal-view-rec-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });


        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new OperatorIndex();
        self.init();
    });

}(jQuery));