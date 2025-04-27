(function ($) {
    'use strict';
    function PackCommRange() {
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

            $('select#ddCommType').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $('select#ddAmtType').select2({
                placeholder: "--Select--",
                allowClear: true

            });

            $("#btnAdd").on("click", function () {

                //var ddOpTypeIds = $("#ddOpTypeIds :Selected");
                var opId = $("#ddOpIds :Selected");
                /*var circleId = $("#ddCircleIds :Selected");*/
                var ddCommType = $("#ddCommType :Selected");
                var ddAmtType = $("#ddAmtType :Selected");

                var amountrange = $("#AmountRange").val();
                var commamt = $("#CommAmt").val();
                var packid = $("#PackId").val();

                var opdata = $("#ddOpIds").select2('data');
                /*var crdata = $("#ddCircleIds").select2('data');*/
                /*var optypedata = $("#ddOpTypeIds").select2('data');*/

                //var optypeids = optypedata.map(function (val) {
                //    return val.id;
                //}).join(',');

                //var optypenames = optypedata.map(function (val) {
                //    return val.text;
                //}).join(',');

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

                if (amountrange == '' || amountrange == undefined) {
                    alertify.error("Invalid Amount Range");
                }
                else if (commamt == '' || commamt == undefined) {
                    alertify.error("Invalid Comm Amount");
                }
                else if (ddCommType.val() == '' || ddCommType.val() == undefined) {
                    alertify.error("Select Comm Type");
                }
                else if (ddAmtType.val() == '' || ddAmtType.val() == undefined) {
                    alertify.error("Select Amt Type");
                }
                else {
                    var cObj = {};
                    cObj.Id = 0;
                    cObj.PackId = packid;
                    /*cObj.OpTypeIds = optypeids;*/
                    /*cObj.OpTypeNames = optypenames;*/
                    cObj.OperatorIds = opids;
                    cObj.OperatorNames = opnames;
                    /*cObj.CircleIds = circleids;*/
                    /*cObj.CircleNames = circlenames;*/
                    cObj.AmountRange = amountrange;
                    cObj.CommAmt = commamt;
                    cObj.AmtTypeId = ddAmtType.val();
                    cObj.CommTypeId = ddCommType.val();


                    $.post(Global.DomainName + 'package/packcommrange', { data: cObj }, function (result) {
                        
                        if (result == 0) {

                            alertify.success("Data Saved Successfully.");

                            //Get the reference of the Table's TBODY element.
                            var tBody = $("#tblPackCommRange > TBODY")[0];

                            //Add Row.
                            var row = tBody.insertRow(-1);

                            var cell = $(row.insertCell(-1));
                            cell.html(result);
                            cell.hide();

                            //var cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblOpTypeIds"  type="hidden" value="' + optypeids + '" /> ' + optypenames);


                            var cell = $(row.insertCell(-1));
                            cell.html('<input id="lblOpIds"  type="hidden" value="' + opids + '" /> ' + opnames);

                            //Add circleId cell.
                            //cell = $(row.insertCell(-1));
                            //cell.html('<input id="lblCircleIds"  type="hidden" value="' + circleids + '" /> ' + circlenames);


                            //Add amountFilter cell.
                            cell = $(row.insertCell(-1));
                            cell.html(amountrange);

                            //Add amountFilter cell.
                            cell = $(row.insertCell(-1));
                            cell.html(commamt);

                            cell = $(row.insertCell(-1));
                            cell.html(ddCommType.text());

                            cell = $(row.insertCell(-1));
                            cell.html(ddAmtType.text());

                            //Add edit row cell.
                            cell = $(row.insertCell(-1));

                            var editbtn = '<a data-toggle="modal" title="Edit" data - target="#modal-edit-packcommRrange" href = "@Url.Action("EditPackCommRange","",new { id=item.Id})" class="btn btn-success" ><i class="fa fa-edit" ></i></a>'
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
                            commamt.val("");
                            amountrange.val("");

                            window.location.reload(true);
                        }
                        else {
                            alertify.error("Something went wrong!");
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
            $("#modal-edit-packcommrange").on('show.bs.modal', function (event) {
                $('#modal-edit-packcommrange .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {

            initGridControlsWithEvents();
            initializeModalWithForm();

        };
    }
    $(function () {

        var self = new PackCommRange();
        self.init();


    });


}(jQuery));