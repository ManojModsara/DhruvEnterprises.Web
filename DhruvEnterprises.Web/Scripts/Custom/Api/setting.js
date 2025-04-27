(function ($) {
    'use strict';
    function TagValueSettingIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {

            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });

            $("#btnAdd").on("click", function () {

                debugger;
                //Reference the Name and Country TextBoxes.
                var tagddl = $("#txtName :Selected");
                var CompareTxt = $("#CompareTxt");
                var PreTxt = $("#PreTxt");
                //PreTxt = PreTxt.replaceAll("<", "&lt");
                //PreTxt = PreTxt.replaceAll(">", "&gt");
                var PostText = $("#PostText");
                //PostText = PostText.replaceAll("<", "&lt");
                //PostText = PostText.replaceAll(">", "&gt");
                var PreMargin = $("#PreMargin");
                var PostMargin = $("#PostMargin");
                var TagMsg = $("#TagMsg");
                var ResSeparator = $("#ResSeparator");
                var TagIndex = $("#TagIndex");

                if (tagddl.val() > 0 && ((PreTxt.val() != '' && PostText.val() != '') || (ResSeparator.val() != '' && TagIndex.val() != ''))) {
                    //Get the reference of the Table's TBODY element.
                    var tBody = $("#TblTagValues > TBODY")[0];

                    //Add Row.
                    var row = tBody.insertRow(-1);

                    var cell = $(row.insertCell(-1));
                    cell.html(tagddl.val());
                    cell.hide();
                    //Add tagddl cell.
                    cell = $(row.insertCell(-1));
                    cell.html(tagddl.text());
                    //Add CompareTxt Cell.
                    cell = $(row.insertCell(-1));
                    cell.html(CompareTxt.val());
                    //Add PreTxt cell.
                    cell = $(row.insertCell(-1));
                    cell.html(Global.htmlEncode(PreTxt.val()));
                    //Add PostText cell.
                    cell = $(row.insertCell(-1));
                    cell.html(Global.htmlEncode(PostText.val()));
                    //Add PreMargin cell.
                    cell = $(row.insertCell(-1));
                    cell.html(PreMargin.val());
                    //Add PostMargin cell.
                    cell = $(row.insertCell(-1));
                    cell.html(PostMargin.val());
                    //Add PostMargin cell.
                    cell = $(row.insertCell(-1));
                    cell.html(TagMsg.val());

                    //Add ResSeparator cell.
                    cell = $(row.insertCell(-1));
                    cell.html(ResSeparator.val());

                    //Add TagIndex cell.
                    cell = $(row.insertCell(-1));
                    cell.html(TagIndex.val());

                    //Add Button cell.
                    cell = $(row.insertCell(-1));

                    var btnRemove = $("<input />");
                    btnRemove.attr("type", "button");
                    btnRemove.attr("onclick", "Remove(this);");
                    btnRemove.attr("class", "btn btn-danger");

                    btnRemove.val("Remove");
                    cell.append(btnRemove);
                    //Clear the TextBoxes.
                    PreTxt.val("");
                    PostText.val("");
                    PreMargin.val("");
                    PostMargin.val("");
                    TagMsg.val("");
                    CompareTxt.val("");

                    ResSeparator.val("");
                    TagIndex.val("");
                }
                else {
                    alertify.error("Invalid data. Please Select TagName and Enter Check Val & Enter PreText & Post Text");
                }
            });

            $("#btnSave").on("click", function () {

                //Loop through the Table rows and build a JSON array.
                var ObjList = [];
                $("#TblTagValues TBODY TR").each(function () {
                    var row = $(this);
                    var TagValue = {};
                    TagValue.UrlId = $("#UrlId").val();
                    TagValue.ApiId = $("#ApiId").val();
                    TagValue.Tagid = row.find("TD").eq(0).html();
                    TagValue.CompareTxt = row.find("TD").eq(2).html();
                    TagValue.PreTxt = row.find("TD").eq(3).html();
                    TagValue.PostText = row.find("TD").eq(4).html();
                    TagValue.PreMargin = row.find("TD").eq(5).html();
                    TagValue.PostMargin = row.find("TD").eq(6).html();
                    TagValue.TagMsg = row.find("TD").eq(7).html();

                    TagValue.ResSeparator = row.find("TD").eq(8).html();
                    TagValue.TagIndex = row.find("TD").eq(9).html();
                    ObjList.push(TagValue);

                });


                // alert(JSON.stringify(ObjList));

                $.post(Global.DomainName + 'ApiSource/TagValueSetting', { data: ObjList }, function (result) {
                    if (!result) {
                        alertify.error('An internal Error occurred.');
                    }
                    else {
                        alertify.success('data Updated.');

                    }
                });

            });

            $("#btnTest").on("click", function () {

                debugger;
                var urlid = $("#UrlId").val();
                var resp = $("#txtResponse").val();

                if (resp == '') {

                    alertify.error('Response string is required.');
                }
                else {
                    $.get(Global.DomainName + 'ApiSource/TestResponse', { id: urlid, apires: resp }, function (result) {
                        if (!result) {
                            alertify.error('An internal Error occurred.');
                        }
                        else {
                            alertify.success("success");
                            $('#tblTestResponse').remove();
                            $('#divTest').append(result);

                        }
                    });
                }
               


            });

            $("#btnCopy").on("click", function () {

                debugger;
                var urlid = $("#UrlId").val();
                var copyapiid = $("#ddVendor :Selected").val();

                if (copyapiid > 0) {
                    $.get(Global.DomainName + 'ApiSource/CopyResponse', { id: urlid, apiid: copyapiid }, function (result) {
                        if (!result) {
                            alertify.error('An internal Error occurred.');
                        }
                        else if (result == 0) {
                            alertify.error('Url not set for selected vendor.');
                        }
                        else if (result > 0) {
                            alertify.success('Success');
                            window.location.href = Global.DomainName + 'ApiSource/TagValueSetting/' + result
                        }

                    });
                }
                else {
                    alertify.error('Select Vendor.');
                }

            });

            $('#divTest').hide();
            $('#divCopy').hide();


            $('#btnCloseTest').on('click', function () {

                $('#divTest').toggle();

            });

            $('#btnCloseCopy').on('click', function () {

                $('#divCopy').toggle();

            });
        }


        function initializeModalWithForm() {
            $("#modal-edit-tagvalue").on('show.bs.modal', function (event) {
                $('#modal-edit-tagvalue .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }

        $this.init = function () {
            initGridControlsWithEvents();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new TagValueSettingIndex();
        self.init();
    });


}(jQuery));