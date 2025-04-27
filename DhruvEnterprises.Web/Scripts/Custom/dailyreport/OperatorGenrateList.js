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
                        $.get(Global.DomainName + 'Dailyreport/Active', { id: this.value }, function (result) {
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
            var fdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "UserProfile.FullName" },
                    { "sName": "Operator.Name" },
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "NoLength" },
                    { "sName": "KeyTypeId" },
                    { "sName": "TextLength" },
                    {
                        "sName": "IsActive", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[0] + '" ' + (row[8] ? "" : " disabled") + '/>';
                                }
                                else {
                                    return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[0] + '" ' + (row[8] ? "" : " disabled") + '/>';
                                }
                            }
                            return data;
                        }
                    },
                    {}
                ],
                "order": [[0, "desc"]]
            }, "DailyReport/GetOperatorGenrateReport", fdata, function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }

            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });


            $("#modal-delete-adminrole").on('show.bs.modal', function (event) {
                $('#modal-delete-adminrole .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $('#btnSearch').on('click', function () {
                var fdata = GetFilterData();
                var Isa = $('#Isa').val();
                var UserId = $('select#ddUser :Selected').val();
                var OperatorId = $('select#ddOperator :Selected').val();
                var VendorId = $('select#ddVendor :Selected').val();
                var requrl = '/DailyReport/OperatorGenrateUserWithVendor?a=' + fdata.Isa;
                var routeval = '';
                if (OperatorId != '' && OperatorId != '0' && OperatorId != undefined) routeval += 'opid=' + OperatorId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&UserId=' + UserId;
                if (VendorId != '' && VendorId != '0' && VendorId != undefined) routeval += '&VendorId=' + VendorId;
                window.location.href = requrl + routeval;

            });

            $('#btnClose').on('click', function () {
                if ($('#dvSearchPanel').is(":hidden")) {
                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();
            });
        }

        function GetFilterData() {
            var filterdata = {};
            var Isa = $('#Isa').val();
            var UserId = $('select#ddUser :Selected').val();
            var OperatorId = $('select#ddOperator :Selected').val();
            var VendorId = $('select#ddVendor :Selected').val();
            filterdata.Uid = (UserId != '' && UserId != '0' && UserId != undefined) ? UserId : 0;
            filterdata.Opid = (OperatorId != '' && OperatorId != '0' && OperatorId != undefined) ? OperatorId : 0;
            filterdata.Apiid = (VendorId != '' && VendorId != '0' && VendorId != undefined) ? VendorId : 0;
            filterdata.Isa = (Isa != '' && Isa != '0' && Isa != undefined) ? Isa : 0;
            return filterdata;

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