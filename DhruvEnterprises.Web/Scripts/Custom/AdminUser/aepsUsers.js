(function ($) {
    'use strict';
    function AdminUserIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
            $("#UpdateAEPSRoute").on("click", function () {
                var ObjList = [];
                $('#grid-index >tbody>  tr ').each(function (i, row) {
                    var $row = $(row)
                    var isadminRoute = false;
                    if ($row.find("#data_IsAdminRoute").is(':checked')) {
                        isadminRoute = true;
                    }
                    ObjList.push({
                        Id: $row.find("td:eq(0)").html().trim(),
                        IsAdminRoute: isadminRoute
                    });
                });
                $.post(Global.DomainName + 'User/RedirectToadmin', { data: ObjList }, function (result) {
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else {
                        alertify.success("Data Saved Successfully.");
                    }
                });

            });



            $('.switchBox').each(function (index, element) {
                if ($(element).data('bootstrapSwitch')) {
                    $(element).off('switch-change');
                    $(element).bootstrapSwitch('destroy');
                }

                $(element).bootstrapSwitch()
                    .on('switch-change', function () {
                        var switchElement = this;
                        $.get(Global.DomainName + 'user/AePSactive', { id: this.value }, function (result) {
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
                    { "sName": "MerchantUsername" },
                    { "sName": "MerchantPassword" },
                    { "sName": "MerchantName" },
                    { "sName": "MerchantAddress" },
                    { "sName": "MerchantPhoneNumber" },
                    { "sName": "CompanyLegalName" },
                    {
                        "sName": "IsActive", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<label>Activated</lable>';
                                }
                                else {
                                    return '<label>De Activated</lable>';
                                }
                            }
                            return data;
                        }
                    },
                    {
                        "sName": "IsLocked", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[0] + '" ' + (row[0] ? "" : " disabled") + '/>';
                                }
                                else {
                                    return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[0] + '" ' + (row[0] ? "" : " disabled") + '/>';
                                }
                            }
                            return data;
                        }
                    },
                    {
                        "sName": "IsAdminRoute", "mRender": function (data, type, row) {
                            if (type === 'display') {
                                if (data) {
                                    return '<input type="checkbox"  id="data_IsAdminRoute" name="data_IsAdminRoute" checked="checked"  value="' + row[0] + '" />';
                                }
                                else {
                                    return '<input type="checkbox" id="data_IsAdminRoute" name="data_IsAdminRoute"   value="' + row[0] + '" />';
                                }
                            }
                            return data;
                        }
                    },

                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [1, 2, 5, 6, 7, 8] }
                   // , { 'visible': false, 'aTargets': [9] }
                ],
                "order": [[0, "desc"]]
            }, "user/ShowAEPSUsers", fdata, function () {
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
            $("#modal-delete-adminuser").on('hidden.bs.modal', function () {
                $(this).removeData('bs.modal');
            });
            $('#btnSearch').on('click', function () {
                var fdata = GetFilterData();
                var Isa = $('#Isa').val();
                var ContactNo = $('#txtContactNo').val();
                var EmailId = $('#txtEmailId').val();
                var ApiKey = $('#txtApiKey').val();
                var IPAddress = $('#txtIPAddress').val();
                var UserId = $('select#ddUser :Selected').val();
                var RoleId = $('select#ddRole :Selected').val();
                var PackId = $('select#ddPackage :Selected').val();
                var StatusId = $('select#ddStatus :Selected').val();

                var requrl = '/User/ShowAEPSUsers?a=' + fdata.Isa;
                var routeval = '';
                if (ContactNo != '' && ContactNo != '0' && ContactNo != undefined) routeval += '&n=' + ContactNo;
                if (EmailId != '' && EmailId != '0' && EmailId != undefined) routeval += '&e=' + EmailId;
                if (ApiKey != '' && ApiKey != '0' && ApiKey != undefined) routeval += '&k=' + ApiKey;
                if (IPAddress != '' && IPAddress != '0' && IPAddress != undefined) routeval += '&i=' + IPAddress;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                if (RoleId != '' && RoleId != '0' && RoleId != undefined) routeval += '&r=' + RoleId;
                if (PackId != '' && PackId != '0' && PackId != undefined) routeval += '&p=' + PackId;
                if (StatusId != '' && StatusId != '0' && StatusId != undefined) routeval += '&s=' + StatusId;
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
            var ContactNo = $('#txtContactNo').val();
            var EmailId = $('#txtEmailId').val();
            var IPAddress = $('#txtIPAddress').val();
            //var UserId = $('select#ddUser :Selected').val();
            //var RoleId = $('select#ddRole :Selected').val();
            //var PackId = $('select#ddPackage :Selected').val();
            //var StatusId = $('select#ddStatus :Selected').val();
            filterdata.CustomerNo = (ContactNo != '' && ContactNo != undefined) ? ContactNo : '';
            filterdata.Email = (EmailId != '' && EmailId != undefined) ? EmailId : '';
            //filterdata.ApiKey = (ApiKey != '' && ApiKey != undefined) ? ApiKey : '';
            //filterdata.IPAddress = (IPAddress != '' && IPAddress != undefined) ? IPAddress : '';
            //filterdata.UserId = (UserId != '' && UserId != '0' && UserId != undefined) ? UserId : 0;
            //filterdata.RoleId = (RoleId != '' && RoleId != '0' && RoleId != undefined) ? RoleId : 0;
            //filterdata.PackId = (PackId != '' && PackId != '0' && PackId != undefined) ? PackId : 0;
            //filterdata.StatusId = (StatusId != '' && StatusId != '0' && StatusId != undefined) ? StatusId : 0;
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