(function ($) {
    'use strict';
    function AdminRoleIndex() {
        var $this = this, locationGrid, formAddEditRole;
        function initGridControlsWithEvents() {
            $('.switchBox').each(function (index, element) {
                if ($(element).data('bootstrapSwitch')) {
                    $(element).off('switch-change');
                    $(element).bootstrapSwitch('destroy');
                }

                $(element).bootstrapSwitch()
                    .on('switch-change', function () {
                        var switchElement = this;
                        $.get(Global.DomainName + 'Product/active', { id: this.value }, function (result) {
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
        function initializeGrid() {
            var fdata = {};
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    {},
                    { "sName": "Id" },
                    {
                        "sName": "ProductImage.ImageUrl", "mRender": function (data, type, row) {
                            return '<img src="' + row[2] + '" style="height:3em;" title="' + row[3] + '"/>'
                        }
                    },
                    { "sName": "Name" },
                    { "sName": "StockQuantity" },
                    { "sName": "Price" },
                    { "sname": "ProductCost" },
                    {
                        "sName": "Published", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[1] + '" ' + (row[0] ? "" : " disabled") + '/>';
                                }
                                else {
                                    return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[1] + '" ' + (row[0] ? "" : " disabled") + '/>';
                                }
                            }
                            return data;
                        }
                    }
                ],
                "order": [[1, "desc"]]
            }, "Product/GetProducts", fdata, function () {
                initGridControlsWithEvents();
            });
        }


        function initializeModalWithForm() {
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
        var self = new AdminRoleIndex();
        self.init();
    });

}(jQuery));