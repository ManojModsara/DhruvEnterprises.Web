
(function ($) {
    'use strict';
    function CreateEdit() {
        var $this = this, locationGrid, formAddEditLocation, uploadObj;

        function initModalControlsWithEvents() {
            // set select2
            $('select#data_ApiID1').select2();
            $('select#data_ApiID2').select2();
            $('select#data_ApiID3').select2();
            $('select#data_SwitchTypeId').select2();
            $('select#data_FetchApiId').select2();
            $('select#data_ValidTypeId').select2();
            
            
        }
        function initGridControlsWithEvents() {
            $("#UpdateApi").on("click", function () {
                debugger;
                var ObjList = [];
                $('#grid-index >tbody>  tr ').each(function (i, row) {
                    var $row = $(row)


                    var isfetch = false;
                    var ispartial = false;

                    if ($row.find("#data_IsFetch").is(':checked')) {
                        isfetch = true; 
                    }
                    if ($row.find("#data_IsPartial").is(':checked')) {
                        ispartial = true;
                    }

                     ObjList.push({
                            Opid: $row.find("td:eq(0)").html().trim(),
                            ApiID1: $row.find("#data_ApiID1 :Selected").val(), // get current row 1st TD value
                            ApiID2: $row.find("#data_ApiID2 :Selected").val(), // get current row 2st TD value
                            ApiID3: $row.find("#data_ApiID3 :Selected").val(), // get current row 3st TD value
                            SwitchTypeId: $row.find("#data_SwitchTypeId :Selected").val(), // get current row 2st TD value
                         FetchApiId: $row.find("#data_FetchApiId :Selected").val(),
                         ValidTypeId: $row.find("#data_ValidTypeId :Selected").val(),
                         IsFetch: isfetch,
                         IsPartial: ispartial
                        });
                });
          

                $.post(Global.DomainName + 'OperatorSwitch/Index', { data: ObjList }, function (result) {
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else {
                        alertify.success("Data Saved Successfully.");
                    }
                });

            });

            $(".btSave").on("click", function () {
                debugger;
               
                  var $row = $(this).closest('tr')
                
                    var isfetch = false;
                    var ispartial = false;

                    if ($row.find("#data_IsFetch").is(':checked')) {
                        isfetch = true;
                    }
                    if ($row.find("#data_IsPartial").is(':checked')) {
                        ispartial = true;
                    }

                var ObjList =  {
                        Opid: $row.find("td:eq(0)").html().trim(),
                        ApiID1: $row.find("#data_ApiID1 :Selected").val(), // get current row 1st TD value
                        ApiID2: $row.find("#data_ApiID2 :Selected").val(), // get current row 2st TD value
                        ApiID3: $row.find("#data_ApiID3 :Selected").val(), // get current row 3st TD value
                        SwitchTypeId: $row.find("#data_SwitchTypeId :Selected").val(), // get current row 2st TD value
                        FetchApiId: $row.find("#data_FetchApiId :Selected").val(),
                        ValidTypeId: $row.find("#data_ValidTypeId :Selected").val(),
                        IsFetch: isfetch,
                        IsPartial: ispartial
                    };

                $.post(Global.DomainName + 'OperatorSwitch/UpdateSwitch', { model: ObjList }, function (result) {
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    }
                    else {
                        alertify.success("Data Saved Successfully.");
                    }
                });
            });
            
            $("#modal-add-edit-operator").on('show.bs.modal', function (event) {
                $('#modal-add-edit-operator .modal-content').load($(event.relatedTarget).prop('href'));
            });

        }
        $this.init = function () {
            initModalControlsWithEvents();
            initGridControlsWithEvents();
            formAddEditLocation = new Global.FormHelper($("#model-createedit-adminuser"), { updateTargetId: "validation-summary", validateSettings: { ignore: '' } });
        };
    }
    $(function () {
        var self = new CreateEdit();
        self.init();
    });

}(jQuery));