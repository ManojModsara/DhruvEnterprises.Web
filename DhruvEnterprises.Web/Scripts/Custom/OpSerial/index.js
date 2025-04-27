(function ($) {
    'use strict';
    function OperatorSerialIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
           
        }

        function initializeGrid() {
            var fdata = {};
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "ID" },
                    { "sName": "Operator.Name" },
                    { "sName": "Circle.CircleName" },
                    { "sName": "Series" },
                    {}
                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [2, 3] }
                ],
                "order": [[0, "asc"]]
            }, "OperatorSerial/GetOpSerialList", fdata, function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            //$("#modal-delete-OperatorSerial").on('show.bs.modal', function (event) {
            //    $('#modal-OperatorSerial .modal-content').load($(event.relatedTarget).prop('href'));
            //});

           
        }
     

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new OperatorSerialIndex();
        self.init();
    });

}(jQuery));