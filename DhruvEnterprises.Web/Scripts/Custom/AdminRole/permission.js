(function ($) {
    'use strict';
    function PermissionIndex() {
        var $this = this;

        function initControlsWithEvents() {

            $("input[type='checkbox'].chk_parent").change(function () {
                $(this).siblings(".divchild").find("input[type='checkbox']").prop('checked', this.checked);
                //$(this).closest('divchild').parent().parent().parent().first('div').find("input[type='checkbox'].chk_parent").prop('checked', true);
            });

            $("input[type='checkbox'].chk_child").change(function () {
                $(this).parent().parent().parent().find("input[type='checkbox'].chk_parent").prop('checked', true);
            });


            $("#btn-submit").click(function () {

               
                var ObjList = [];
                var RoleID = $('#CurrentRoleId').val();

                $('#tabid > tbody  > tr').each(function (i, row) {
                    var $row = $(row)
                    ObjList.push({
                        RoleId: RoleID,
                        MenuId: $row.find("td:eq(0)").html().trim(), // get current row 1st TD value
                        IsMenuAllow: $row.find("#chk_parent").is(":checked"), // get current row 1st TD value
                        IsCreate: $row.find("#chk_create").is(":checked"), // get current row 2nd TD
                        IsEdit: $row.find("#chk_edit").is(":checked"), // get current row 2nd TD
                        IsDelete: $row.find("#chk_delete").is(":checked") // get current row 2nd TD
                    });
                });
            
                //$.ajax({
                //    url: "/role/Permission",
                //    type: "POST",
                //    data: JSON.stringify(ObjList),
                //    dataType: "json",
                //    contentType: "application/json; charset=utf-8",
                //    success: function (result) {
                //        console.log(result);
                //    }
                //});
                
                $.post(Global.DomainName + 'role/Permission', { data: ObjList }, function (result) {
                    if (!result) {
                        alertify.error('An internal Error occurred.');
                    }
                    else {
                        alertify.success('Status Updated.');

                    }
                });

            });
        }
      
        $this.init = function () {
            initControlsWithEvents();
        };
    }
    $(function () {
        var self = new PermissionIndex();
        self.init();

       
    });

}(jQuery));