(function ($) {
    'use strict';
    function CommonSwitchSettingIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {


            $('select#OpId').select2();
            $('select#CircleId').select2({
                placeholder: "--Select--",
                allowClear: true

            });
            $('select#ApiId').select2();
            $('select#FilterTypeId').select2();
            $('select#UserId').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $("#btnAdd").on("click", function () {                
                var opId = $("#OpId :Selected");
                /*var LapuId = $("#LapuIds :Selected");*/
                /*var circleId = $("#CircleId :Selected");*/
                var apiId = $("#ApiId :Selected");
                var filterTypeId = $("#FilterTypeId :Selected");

                //var data = $("#CircleId").select2('data');
                //var circleids = data.map(function (val) {
                //    return val.id;
                //}).join(',');
                //var circlenames = data.map(function (val) {
                //    return val.text;
                //}).join(',');


                /*var data1 = $("#LapuIds").select2('data');*/
                //var lapuids = data1.map(function (val) {
                //    return val.id;
                //}).join(',');

                var userId = $("#UserId :Selected");

                var udata = $("#UserId").select2('data');
                var userids = udata.map(function (val) {
                    return val.id;
                }).join(',');
                var usernames = udata.map(function (val) {
                    return val.text;
                }).join(',');


                var amountFilter = $("#txtAmountFilter");
                var blockUser = $("#txtBlockUser");
                var minRO = $("#txtMinRO");
                var priority = $("#txtPriority");
                var routeOp1 = $("#txtRouteOP1").val();
                //var routeRo2 = $("#txtPriority");

                //if (circleId.val() == undefined || circleId.text() == '') {

                //    circlenames = 'All';
                //}

                if (userId.val() == undefined || userId.text() == '') {

                    usernames = 'All';
                }

                if (opId.val() == '--Select--' || opId.val() <= 0) {
                    alertify.error("Please Select Operator");

                }

                else if (apiId.val() == '--Select--' || apiId.val() <= 0) {
                    alertify.error("Please Select Vendor");

                }
                else if (amountFilter.val() == '') {
                    alertify.error("Enter amount for filter");

                }
                else if (priority.val() == '') {
                    alertify.error("Enter priority");

                }
                else if (filterTypeId.val() == '') {
                    alertify.error("Please Select Filter Type");
                }
                else {
                    var cObjList = [];
                    cObjList.push
                        ({

                            AmountFilter: amountFilter.val(),
                            ApiId: apiId.val(),
                            /*LapuIds: lapuids.split(','),*/
                            ApiName: 'NA',
                            /*CircleId: circleids,*/
                            /*CircleName: circlenames,*/
                            Id: 0,
                            FilterTypeId: filterTypeId.val(),
                            FilterTypeName: 'NA',
                            OperatorId: opId.val(),
                            OperatorName: 'NA',
                            Priority: priority.val(),
                            BlockUser: blockUser.val(),
                            MinRO: minRO.val(),
                            UserId: userids,
                            UserName: usernames,
                            RouteOP1: routeOp1
                        });

                    $.post(Global.DomainName + 'commonswitch/commonswitchsetting', { data: cObjList }, function (result) {
                        if (result == "3") {

                            alertify.error("Duplicate Priority for the operator.");
                        }
                        else if (result == "4") {
                            alertify.error("Invalid Range.");
                        }
                        else if (result == "2") {
                            alertify.error("Internal Error. Something went wrong.");
                        }
                        else if (result == "0") {

                            alertify.success("Data Saved Successfully.");
                            //Get the reference of the Table's TBODY element.
                            var tBody = $("#tblCommonSwitch > TBODY")[0];

                            //Add Row.
                            var row = tBody.insertRow(-1);

                            var cell = $(row.insertCell(-1));
                            cell.html(0);
                            cell.hide();

                            //edit/delete button cell
                            cell = $(row.insertCell(-1));
                            var editbutton = '<a data-toggle="modal" title="Edit" style="height:30px;width:34px;" data - target="#modal-edit-switchsetting" href = "@Url.Action("EditSwitchSetting","",new { id=item.Id})" class="btn btn-success" ><i class="fa fa-edit  grid-btn btn-sm" ></i></a>'
                            var deletebtn = '<input type="button" title="Remove" onclick="Remove(this);" class="btn btn-danger  grid-btn btn-sm" value="X" />';
                            var buttons = "&nbsp;&nbsp;" + editbutton + "&nbsp;&nbsp;" + deletebtn;

                            cell.append(buttons);

                            //Add user Cell.
                            cell = $(row.insertCell(-1));
                            cell.html('<input id="lblUserId"  type="hidden" value="' + userids + '" /><input id="lblUserName"  type="hidden" value="' + usernames + '" />  ' + usernames);

                            //Add operator cell.
                            var cell = $(row.insertCell(-1));
                            cell.html('<input id="lblOpId"  type="hidden" value="' + opId.val() + '" /> ' + opId.text());

                            //Add circleId cell.
                            //cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblCircleId"  type="hidden" value="' + circleids + '" /><input id="lblCircleName"  type="hidden" value="' + circlenames + '" />  ' + circlenames);

                            //Add ApiId Cell.
                            cell = $(row.insertCell(-1));
                            cell.html('<input id="lblApiId"  type="hidden" value="' + apiId.val() + '" /> ' + apiId.text());

                            // Add LapuIds Cell.
                            //cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblLapuIds"  type="hidden" value="' + lapuids + '" /> ' + lapuids);

                            //Add ApiId Cell.
                            cell = $(row.insertCell(-1));
                            cell.html('<input id="lblFilterTypeId"  type="hidden" value="' + filterTypeId.val() + '" /> ' + filterTypeId.text());

                            //Add amountFilter cell.
                            cell = $(row.insertCell(-1));
                            cell.html(amountFilter.val());

                            //Add minRO cell.
                            cell = $(row.insertCell(-1));
                            cell.html(minRO.val());


                            //Add priority cell.
                            cell = $(row.insertCell(-1));
                            cell.html(priority.val());

                            //Add block user cell.
                            cell = $(row.insertCell(-1));
                            cell.html(blockUser.val());

                            //Add blank  cell.
                            cell = $(row.insertCell(-1));
                            //Add blank cell.
                            cell = $(row.insertCell(-1));


                            //Clear the TextBoxes.
                            amountFilter.val("");
                            blockUser.val("");
                            minRO.val("");
                            priority.val("");
                        }
                        else {
                            alertify.error("OOPS! Something went wrong.");
                        }

                    });

                }

            });

            $("#btnSave").on("click", function () {

                //Loop through the Table rows and build a JSON array.
                var cObjList = [];
                $("#tblCommonSwitch TBODY TR").each(function () {
                    var row = $(this);
                    var id = row.find("TD").eq(0).html()
                    if (!(id > 0)) {                        
                        cObjList.push
                            ({

                                AmountFilter: row.find("TD").eq(5).html(),
                                ApiId: row.find("#lblApiId").val(),
                                //LapuIds: row.find("#lblLapuIds").val(),
                                ApiName: 'NA',
                               /* CircleId: row.find("#lblCircleId").val(),*/
                                CircleName: row.find("#lblCircleName").val(),
                                Id: id,
                                FilterTypeId: row.find("#lblFilterTypeId").val(),
                                FilterTypeName: 'NA',
                                OperatorId: row.find("#lblOpId").val(),
                                OperatorName: 'NA',
                                Priority: row.find("TD").eq(8).html(),
                                UserFilter: row.find("TD").eq(6).html(),
                                WaitTime: row.find("TD").eq(7).html(),
                            });
                    }

                });

                $.post(Global.DomainName + 'commonswitch/commonswitchsetting', { data: cObjList }, function (result) {
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else {
                        alertify.success("Data Saved Successfully.");
                    }
                });

            });

            $('select#LapuIds').select2();

            //$('select#ApiId').on('change', function () {

            //    debugger;

            //  /*  var ddlLapuIds = $('select#LapuIds');*/
            //    var apiId = $("#ApiId :Selected").val();
            //    var opId = $("#OpId :Selected").val();
            //    //var data = $("#CircleId").select2('data');

            //    //var circleids = data.map(function (val) {
            //    //    return val.id;
            //    //}).join(',');

            //    $.post(Global.DomainName + 'Lapu/GetLapuListByApiId', { apiid: apiId, opid: opId/*, cids: circleids*/ }, function (result) {

            //        debugger;
            //        if (!result) {
            //            alertify.error("Internal Error. Something went wrong.");
            //        }
            //        else if (result == "1") {

            //            alertify.error("Something went wrong.");
            //        }
            //        //else {
            //        //    ddlLapuIds.empty().append('<option selected="selected" value="0">Select Lapu</option>');
            //        //    $.each(result, function () {
            //        //        ddlLapuIds.append($("<option></option>").val(this['Value']).html(this['Text']));
            //        //    });
            //        //}
            //    });


            //});

            $('.switchBox').each(function (index, element) {
                if ($(element).data('bootstrapSwitch')) {
                    $(element).off('switch-change');
                    $(element).bootstrapSwitch('destroy');
                }

                $(element).bootstrapSwitch()
                    .on('switch-change', function () {
                        var switchElement = this;
                        debugger;
                        $.get(Global.DomainName + 'commonswitch/active', { id: this.value }, function (result) {
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

        function initializeModalWithForm() {
            $("#modal-edit-switchsetting").on('show.bs.modal', function (event) {
                $('#modal-edit-switchsetting .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {

            initGridControlsWithEvents();
            initializeModalWithForm();

        };
    }

    $(function () {

        var self = new CommonSwitchSettingIndex();
        self.init();


    });


}(jQuery));