(function ($) {
    'use strict';
    function DueAmountIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
         
        }

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "ApiId" },
                    { "sName": "SentAmt" },
                    { "sName": "ReceivedAmt" },
                    { "sName": "Amount" },
                    { "sName": "RefId" },
                    { "sName": "PaymentRemark" },
                    { "sName": "AddedDate" },
                    { "sName": "AddedById" }
                ]
            }, "DueAmount/GetDueAmounts", function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            $("#modal-add-edit-DueAmountIndex").on('show.bs.modal', function (event) {
                $('#modal-add-edit-DueAmountIndex .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new DueAmountIndex();
        self.init();
    });

}(jQuery));