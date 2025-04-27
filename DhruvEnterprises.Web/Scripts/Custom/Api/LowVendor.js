(function ($) {
    'use strict';
    function LowVendor() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
            $('.switchBox').each(function (index, element) {
                if ($(element).data('bootstrapSwitch')) {
                    $(element).off('switch-change');
                    $(element).bootstrapSwitch('destroy');
                }

                $(element).bootstrapSwitch()
                    .on('switch-change', function () {
                        var switchElement = this;
                        $.get(Global.DomainName + 'apisource/active', { id: this.value }, function (result) {
                            if (!result) {
                                $(switchElement).bootstrapSwitch('toggleState', true);
                                alertify.error('Error occurred.');
                            }
                            else {
                                alertify.success('Status Updated.');
                            }
                        });
                    });
            });

           

        }

        function initModalPopupControlsWithEvents() {

        }

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ApiName" },
                    { "sName": "Remark" }, 
                    { "sName": "BlockAmount" },
                    { "sName": "ApiBal" }
                ],
                "order": [[4, "asc"]]
            }, "ApiSource/GetLowVendors", function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            $("#modal-delete-api").on('show.bs.modal', function (event) {
                $('#modal-delete-api .modal-content').load($(event.relatedTarget).prop('href'));
            });

           
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
            
        };


    }
    $(function () {
        var self = new LowVendor();
        self.init();

       
    });


   
}(jQuery));

