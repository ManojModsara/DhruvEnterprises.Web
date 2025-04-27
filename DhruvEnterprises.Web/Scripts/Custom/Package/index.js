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
                        $.get(Global.DomainName + 'Package/IsUserLossPackage', { id: this.value }, function (result) {
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
            var fdata = {};

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "PackageName" },
                    //{
                    //    "sName": "IsUserLoss", "mRender": function (data, type, row) {
                    //        if (type === 'display') {
                    //            if (data) {
                    //                return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[0] + '" ' + (row[3] ? "" : " disabled") + '/>';
                    //            }
                    //            else {
                    //                return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[0] + '" ' + (row[3] ? "" : " disabled") + '/>';
                    //            }
                    //        }
                    //        return data;
                    //    }
                    //},
                    {}
                ],
                "order": [[0, "desc"]]
            }, "Package/GetPackage", fdata, function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            $("#modal-delete-adminuser").on('hidden.bs.modal', function () {
                $(this).removeData('bs.modal');
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