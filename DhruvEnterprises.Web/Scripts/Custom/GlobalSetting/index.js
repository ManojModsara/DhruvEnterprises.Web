(function ($) {
    'use strict';
    function GlobalIndex() {
        var $this = this;
        function initControlsWithEvents() {
            $("input[type='checkbox'].chk_parent").change(function () {
                $(this).siblings(".divchild").find("input[type='checkbox']").prop('checked', this.checked);                
            });
            $("input[type='checkbox'].chk_child").change(function () {
                $(this).parent().parent().parent().find("input[type='checkbox'].chk_parent").prop('checked', true);
            });
            $("#btn-submit").click(function () {
                var ObjList = [];                
                $('#tabid > tbody  > tr').each(function (i, row) {
                    var $row = $(row)
                    ObjList.push({                       
                        id: $row.find("td:eq(0)").html().trim(), // get current row 1st TD value
                        Actionname: $row.find("td:eq(1)").html().trim(), // get current row 1st TD value
                        Displayname: $row.find("td:eq(2) span").html().trim(), // get current row 1st TD value
                        AllowEmail: $row.find("#chk_create").is(":checked"), // get current row 2nd TD
                        AllowSMS: $row.find("#chk_edit").is(":checked"), // get current row 2nd TD                        
                    });
                });               

                $.post(Global.DomainName + 'GlobalSetting/GlobalSetting', { data: ObjList }, function (result) {
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
        var self = new GlobalIndex();
        self.init();
    });
}(jQuery));