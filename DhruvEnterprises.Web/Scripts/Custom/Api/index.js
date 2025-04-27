(function ($) {
    'use strict';
    function AdminUserIndex() {
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
            var filterdata = {};
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ApiName" },
                    { "sName": "Remark" }, 
                    {
                        "sName": "IsActive", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[0] + '" />';
                                }
                                else {
                                    return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[0] + '" />';
                                }
                            }
                            return data;
                        }
                    }, 
                    { "sName": "cl_bal" },
                    {},
                    {
                        "mRender": function (data, type, row) {
               
                                    var bal = parseFloat(row[4]).toFixed(2);
                                    var vbal = parseFloat(row[5]).toFixed(2);

                                 var dff = parseFloat(bal - vbal).toFixed(2);

                                 return dff;
                          
                        }
                    },
                    {},
                    {},
                    
                 
                ],
                "order": [[0, "desc"]]
            }, "ApiSource/GetApiSourceList", filterdata, function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            $("#modal-delete-api").on('show.bs.modal', function (event) {
                $('#modal-delete-api .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-add-edit-addapiwallet").on('show.bs.modal', function (event) {
                $('#modal-add-edit-addapiwallet .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
            
        };


    }
    $(function () {
        var self = new AdminUserIndex();
        self.init();

       
    });


   
}(jQuery));

