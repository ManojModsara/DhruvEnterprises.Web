(function ($) {
    'use strict';
    function ShowKYC() {
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
                        $.get(Global.DomainName + 'user/approved', { id: this.value }, function (result) {
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
                    //{},
                    //{ "sName": "Id" },
                    //{ "sName": "UserId" }, 
                    {},
                    { "sName": "Id" },
                    { "sName": "FullName" },
                    { "sName": "Role.RoleName" },
                    { "sName": "Email" },
                    { "sName": "MobileNumber" },
                    //{ "sName": "AdharNo" },
                    //{
                    //    "sName": "AdFrntImg", "mRender": function (data, type, row) {
                    //        return '<img src="/KYCData/' + row[7] + '" alt="adhar" width="50" height="35" onclick="viewImage(this)">'

                    //        }

                    //        //"AdFrntImg"
                    //    },

                    //{
                    //    "sName": "AdBackndImg", "mRender": function (data, type, row) {
                    //        return '<img src="/KYCData/' + row[8] + '" alt="adhar" width="50" height="35" onclick="viewImage(this)">'

                    //    }

                    //},

                    //{
                    //    "sName": "IsKYC", "mRender": function (data, type, row) {
                    //        if (type === 'display') {
                    //            if (data) {
                    //                return '<input type="checkbox" checked="checked" class="switchBox switch-small simple" value="' + row[1] + '" ' + (row[0] ? "" : " disabled") + '/>';
                    //            }
                    //            else {
                    //                return '<input type="checkbox" class="switchBox switch-small simple"  value="' + row[1] + '" ' + (row[0] ? "" : " disabled") + '/>';
                    //            }
                    //        }
                    //        return data
                    //        //"AdFrntImg"
                    //    }
                    //},
                    { "sName": "IsKYC" },
                    { "sName": "FullName" },
                    { "sName": "KYCRequestDate" },
                    { "sName": "KYCApprovedDate" },
                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [1] }
                    , { 'visible': false, 'aTargets': [0] }
                ],
                "order": [[1, "desc"]]
            }, "user/getKYCUsers", fdata, function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            if ($('#Isa').val() == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }
            $("#modal-delete-adminuser").on('hidden.bs.modal', function () {
                $(this).removeData('bs.modal');
            });

            $('#btnSearch').on('click', function () {
                                var fdata = GetFilterData();
                var requrl = '/User/ShowKYCUsers?a=' + fdata.Isa;
                var routeval = '';
                if (fdata.CustomerNo != '') routeval += '&n=' + fdata.CustomerNo;
                if (fdata.EmailId != '') routeval += '&e=' + fdata.EmailId;
                //if (fdata.ApiKey != '') routeval += '&k=' + fdata.ApiKey;
                //if (fdata.IPAddress != '') routeval += '&i=' + fdata.IPAddress;

                //if (fdata.UserId > 0) routeval += '&u=' + fdata.UserId;
                //if (fdata.RoleId > 0) routeval += '&r=' + fdata.RoleId;
                //if (fdata.PackId > 0) routeval += '&p=' + fdata.PackId;
                //if (fdata.StatusId > 0) routeval += '&s=' + fdata.StatusId;

                window.location.href = requrl + routeval;
                var table = $("#grid-index").DataTable();
                /*   table.search(xh).draw();*/
                //table.search(routeval).draw();


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

            $('#btnExport').on('click', function () {

                var fdata = GetFilterData();

                var requrl = '/Export/ExportCSV?rt=8';

                var routeval = '';

                if (fdata.CustomerNo != '') routeval += '&m=' + fdata.CustomerNo;
                if (fdata.EmailId != '') routeval += '&e=' + fdata.EmailId;
                window.location.href = requrl + routeval;


            });

        }

        function GetFilterData() {

            var filterdata = {};

            var Isa = $('#Isa').val();
            var ContactNo = $('#txtContactNo').val();
            var EmailId = $('#txtEmailId').val();
            var ApiKey = $('#txtApiKey').val();
            filterdata.CustomerNo = (ContactNo != '' && ContactNo != undefined) ? ContactNo : '';
            filterdata.EmailId = (EmailId != '' && EmailId != undefined) ? EmailId : '';
            filterdata.ApiKey = (ApiKey != '' && ApiKey != undefined) ? ApiKey : '';
            filterdata.Isa = (Isa != '' && Isa != '0' && Isa != undefined) ? Isa : 0;

            return filterdata;

        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();

        };
    }
    $(function () {
        var self = new ShowKYC();
        self.init();



    });


}(jQuery));

function viewImage(t) {
    // Get the modal
    var modal = document.getElementById("myModal");

    // Get the image and insert it inside the modal - use its "alt" text as a caption
    var img = t;
    var modalImg = document.getElementById("img01");
    var captionText = document.getElementById("caption");
    img.onclick = function () {
        modal.style.display = "block";
        modalImg.src = this.src;
        captionText.innerHTML = this.alt;
    }

    // Get the <span> element that closes the modal
    var span = document.getElementsByClassName("close")[0];

    // When the user clicks on <span> (x), close the modal
    span.onclick = function () {
        modal.style.display = "none";
    }
}