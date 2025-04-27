(function ($) {
    'use strict';
    function CricleListIndex() {
        var $this = this, locationGrid, formRequestResponse;
        function initModalControlsWithEvents() {
            // set select2
            $('select#data_API1_Id').select2();
            $('select#data_API2_Id').select2();
            $('select#data_API3_Id').select2();
            
            $('select#data_API1_Id').change(function () {

                var value = $(this).val();
                if (value == '' || value == null) {
                    value = "0";
                }
                else {
                    $(this).closest('div').find('div.select2-container').removeClass("error").addClass("valid");
                    $(this).closest('div').find('label.error').hide();
                    $(this).removeClass("error").addClass("valid");
                }
            });
            $('select#data_API2_Id').change(function () {

                var value = $(this).val();
                if (value == '' || value == null) {
                    value = "0";
                }
                else {
                    $(this).closest('div').find('div.select2-container').removeClass("error").addClass("valid");
                    $(this).closest('div').find('label.error').hide();
                    $(this).removeClass("error").addClass("valid");
                }
            });
            $('select#data_API3_Id').change(function () {

                var value = $(this).val();
                if (value == '' || value == null) {
                    value = "0";
                }
                else {
                    $(this).closest('div').find('div.select2-container').removeClass("error").addClass("valid");
                    $(this).closest('div').find('label.error').hide();
                    $(this).removeClass("error").addClass("valid");
                }
            });
           
        }
        function initGridControlsWithEvents() {
            $("#UpdateApi").on("click", function () {
                var ObjList = [];
                $('#grid-index >tbody>  tr ').each(function (i, row) {
                    
                    var $row = $(row)

                        ObjList.push({
                            CircleId: $row.find("td:eq(0)").html().trim(),
                            OpId: $row.find("td:eq(1)").html().trim(),
                            API1_Id: $row.find("#data_API1_Id :Selected").val(), 
                            API2_Id: $row.find("#data_API2_Id :Selected").val(), 
                            API3_Id: $row.find("#data_API3_Id :Selected").val(), 
                            IsRoffer: $row.find("#data_IsRoffer").is(":checked") ? true : false
                        });
                    
                });
                //alert(JSON.stringify(ObjList));
                $.post(Global.DomainName + 'OperatorSwitch/CircleSwitch', { data: ObjList }, function (result) {
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
            
                var $row = $(this).closest('tr');

                var ObjList= {
                        CircleId: $row.find("td:eq(0)").html().trim(),
                        OpId: $row.find("td:eq(1)").html().trim(),
                        API1_Id: $row.find("#data_API1_Id :Selected").val(),
                        API2_Id: $row.find("#data_API2_Id :Selected").val(),
                        API3_Id: $row.find("#data_API3_Id :Selected").val(),
                        IsRoffer: $row.find("#data_IsRoffer").is(":checked") ? true : false
                    };
                    
                //alert(JSON.stringify(ObjList));
                $.post(Global.DomainName + 'OperatorSwitch/UpdateCRoute', { data: ObjList }, function (result) {
                    if (!result) {
                        alertify.error("Internal Error. Something went wrong.");
                    } 
                    else {
                        alertify.success("Data Saved Successfully.");
                    }
                });
                });

        }
        $this.init = function () {
            initModalControlsWithEvents();
            initGridControlsWithEvents();
        };
    }
    $(function () {
        var self = new CricleListIndex();
        self.init();
    });

}(jQuery));