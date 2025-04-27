(function ($) {
    'use strict';
    function AdminUserIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
            $('select#UserID').select2();
            $('select#UserID').change(function () {
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

            $('select#Type').select2();
            
            $('select#Type').change(function () {
                debugger;
                $('#Message').val(' ');
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

        function initializeGrid() {
                initGridControlsWithEvents();
        }
        function GetMessage(user) {
            var val = $("#Type").val();
            $.get(Global.DomainName + 'SMSApi/GetMessage', { type: val, userid: user}, function (result) {
                if (!result) {
                    $('#Message').val(result);
                }
                else {
                    $('#Message').val(result);
                }
            });
        }

        $this.init = function () {
            initializeGrid();
        };
    }
    $(function () {
        var self = new AdminUserIndex();
        self.init();
    });

}(jQuery));