(function ($) {
    'use strict';
    function BlockRoute() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {


            $('select#ddOpIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });
            //$('select#ddCircleIds').select2({
            //    placeholder: "--Select--",
            //    allowClear: true

            //});
            $('select#ddUserIds').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            //$('select#ddOpTypeIds').select2({
            //    placeholder: "--Select--",
            //    allowClear: true

            //});
            

            $("#btnAdd").on("click", function () {
                var BlockId = "0";
                var opId = $("#ddOpIds :Selected");
                /*var circleId = $("#ddCircleIds :Selected");*/
                var userId = $("#ddUserIds :Selected");
                var typeid = $("#ddRouteType :Selected").val();
                var statusid = $("#ddStatus :Selected").val();
              
                var opdata = $("#ddOpIds").select2('data');
                /*var crdata = $("#ddCircleIds").select2('data');*/
                var usrdata = $("#ddUserIds").select2('data');
               

                var opids = opdata.map(function (val) {
                    return val.id;
                }).join(',');
                var opnames = opdata.map(function (val) {
                    return val.text;
                }).join(',');

                //var circleids = crdata.map(function (val) {
                //    return val.id;
                //}).join(',');
                //var circlenames = crdata.map(function (val) {
                //    return val.text;
                //}).join(',');

                var userids = usrdata.map(function (val) {
                    return val.id;
                }).join(',');
                var usernames = usrdata.map(function (val) {
                    return val.text;
                }).join(',');

                /*var ddOpTypeIds = $("#ddOpTypeIds :Selected");*/
                /*var optypedata = $("#ddOpTypeIds").select2('data');*/
                //var optypeids = optypedata.map(function (val) {
                //    return val.id;
                //}).join(',');
                //var optypenames = optypedata.map(function (val) {
                //    return val.text;
                //}).join(',');


                var amountFilter = $("#txtAmounts");

                if (typeid == '' || typeid == undefined ) {
                    alertify.error("select Route Type");
                }
                else {
                    var cObjList = [];
                    cObjList.push
                        ({
                            OperatorIds: opids,
                            OperatorNames: opnames,
                            /*CircleIds: circleids,*/
                            /*CircleNames: circlenames,*/
                            UserIds: userids,
                            UserNames: usernames,
                            Amounts: amountFilter.val(),
                            Id: 0,
                            TypeId: typeid,
                            StatusId: statusid,
                            /*OpTypeIds: optypeids,*/
                           /* OpTypeNames: optypenames*/
                        });

                    $.post(Global.DomainName + 'commonswitch/blockroute', { data: cObjList }, function (result) {
                        if (result == "0") {
                            alertify.error("Something went wrong!");
                        }

                        else {
                            BlockId = result;
                            alertify.success("Data Saved Successfully.");

                            //Get the reference of the Table's TBODY element.
                            var tBody = $("#tblBlockRoute > TBODY")[0];

                            //Add Row.
                            var row = tBody.insertRow(-1);

                            var cell = $(row.insertCell(-1));
                            cell.html(BlockId);
                            cell.hide();

                            //var cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblOpTypeIds"  type="hidden" value="' + optypeids + '" /> ' + optypenames);


                            var cell = $(row.insertCell(-1));
                            cell.html('<input id="lblOpIds"  type="hidden" value="' + opids + '" /> ' + opnames);

                            //Add circleId cell.
                            //cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblCircleIds"  type="hidden" value="' + circleids + '" /> ' + circlenames);

                            //Add ApiId Cell.
                            cell = $(row.insertCell(-1));
                            cell.html('<input id="lblUserIds"  type="hidden" value="' + userids + '" /> ' + usernames);


                            //Add amountFilter cell.
                            cell = $(row.insertCell(-1));
                            cell.html(amountFilter.val());

                            //Add type cell.
                            cell = $(row.insertCell(-1));
                            var stype = typeid == 2 ? 'Callback' : 'Block';
                            cell.html('<input id="lblTypeId"  type="hidden" value="' + typeid + '" /> ' + stype);

                            //Add status cell.
                            cell = $(row.insertCell(-1));
                            var status = typeid == 2 ? statusid == 1 ? 'Success' : 'Processing' : '';
                            cell.html('<input id="lblStatusId"  type="hidden" value="' + statusid + '" /> ' + status);


                            //Add edit row cell.
                            cell = $(row.insertCell(-1));

                            var editbtn = '<a data-toggle="modal" title="Edit" data - target="#modal-edit-blockroute" href = "@Url.Action("EditBlockRoute","",new { id=item.Id})" class="btn btn-success" ><i class="fa fa-edit" ></i></a>'
                            cell.append(editbtn);

                            cell = $(row.insertCell(-1));

                            var btnRemove = $("<input />");
                            btnRemove.attr("type", "button");
                            btnRemove.attr("onclick", "Remove(this);");
                            btnRemove.attr("class", "btn btn-danger");
                            btnRemove.attr("title", "Remove");
                            btnRemove.val("x");
                            cell.append(btnRemove);

                            //Clear the TextBoxes.
                            amountFilter.val("");
                        }

                    });

                }
              
                
            });


            //$('select#ddOpTypeIds').on('change', function () {
            //    debugger;
            //    var ddOpIds = $('select#ddOpIds');
            //    var data = $(this).select2('data');

            //    var optypeids = data.map(function (val) {
            //        return val.id;
            //    }).join(',');

            //    $.post(Global.DomainName + 'CommonSwitch/GetOperatorByType', { OpTypeIds: optypeids }, function (result) {

            //        debugger;
            //        if (!result) {
            //            alertify.error("Internal Error. Something went wrong.");
            //        }
            //        else if (result == "1") {

            //            alertify.error("Something went wrong.");
            //        }
            //        else {
            //            ddOpIds.empty().append($("<option></option>").val('0').html('All'));
            //            $.each(result, function () {
            //                ddOpIds.append($("<option></option>").val(this['OperatorId']).html(this['OperatorName']));
            //            });
            //        }
            //    });


            //});
        }

        function initializeModalWithForm() {
            $("#modal-edit-blockroute").on('show.bs.modal', function (event) {
                $('#modal-edit-blockroute .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {

            initGridControlsWithEvents();
            initializeModalWithForm();

        };
    }
    $(function () {

        var self = new BlockRoute();
        self.init();


    });


}(jQuery));