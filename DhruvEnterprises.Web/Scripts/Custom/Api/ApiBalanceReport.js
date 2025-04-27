// Chart
(function ($) {
    'use strict';
    function TagValueSettingIndex() {
        var $this = this, locationGrid, formAddEditLocation;
        function initGridControlsWithEvents() {
            //Loop through the Table rows and build a JSON array.
            var ObjList = [];
            $.post(Global.DomainName + '/apisource/GetApiBalanceList', function (result) {
                if (!result) {
                    success: OnSuccess(result);
                }
                else {
                    success: OnSuccess(result);
                }
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
function OnSuccess(response) {
    var response = jQuery.parseJSON(response);
    var obj = response.apilist;
    $("#grid-index").append(tbl);
    if (obj != null) {
        if (obj.length > 0) {
            
            var tbl = $("<table/>").attr("id", "mytable");
            var rowsPerPageS = obj;
            splitRows = [],
                rows = $("#grid-index").find("div tr").toArray();
            for (i = 0; i < rows.length; i += rows) {
                splitRows.push(rows.slice(i, i + rowsPerPageS))
            }
            for (var i = 0; i < obj.length; i++) {
                var tr = "<tr>";
                var td1 = "<td>" + obj[i]["Id"] + "</td>";
                var td3 = "<td>" + obj[i]["ApiName"] + "</td>";
                var td4 = "<td>" + obj[i]["Amount"] + "</td>";
                var de = "</tr>"

                $("#grid-index").append(tr + td1 + td3 + td4 + de)
                event.preventDefault();
            }
        }
    }
}