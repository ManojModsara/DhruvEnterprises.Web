(function ($) {
    'use strict';
    function CricleListIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "CircleName" },
                    {}
                ]

            }, "OperatorSwitch/GetCricleList");
        }

        function initializeModalWithForm() {

            $("#modal-view-user-detail").on('show.bs.modal', function (event) {
                $('#modal-view-user-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-url-detail").on('show.bs.modal', function (event) {
                $('#modal-view-url-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-view-rec-detail").on('show.bs.modal', function (event) {
                $('#modal-view-rec-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });


        }
        function initGridControlsWithEvents() {
            $("#UpdateApi").on("click", function () {
                var ObjList = [];
                $('#grid-index >tbody>  tr ').each(function (i, row) {
                    var $row = $(row)
                    var apiid = $row.find("#data_ApiID1 :Selected").val().trim();
                    var apiid1 = $row.find("#data_ApiID2 :Selected").val().trim();
                    var apiid2 = $row.find("#data_ApiID3 :Selected").val().trim();
                    var IsRoffer = $row.find("#Roffer :checked")?"True":"false".trim();
                    if (apiid != "" && apiid1 != "" && apiid2 != "" && SwitchTypeId != "") {
                        ObjList.push({
                            CircleId: $row.find("td:eq(0)").html().trim(),
                            Opid: $row.find("td:eq(1)").html().trim(),
                            API1_Id: apiid, // get current row 1st TD value
                            API2_Id: apiid1, // get current row 2st TD value
                            API3_Id: apiid2, // get current row 3st TD value
                            IsRoffer: IsRoffer, // get current row 2st TD value
                        });
                    }
                });
                //alert(JSON.stringify(ObjList));
                $.ajax({
                    url: "/OperatorSwitch/CircleSwitch",
                    type: "POST",
                    data: JSON.stringify(ObjList),
                    //dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (!data) {
                            alertify.error('An internal Error occurred.');
                        }
                        else {
                            alertify.success('Status Updated.');
                        }
                    }

                });
            });
        }
        $this.init = function () {
            initializeGrid();
            initGridControlsWithEvents();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new CricleListIndex();
        self.init();
    });

}(jQuery));