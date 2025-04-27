(function ($) {
    'use strict';
    function ReportPendingApiWise() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper2('RechargeReport/apCount', '#apCount','#grid-PendingApiWise', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ApiName" },
                    { "sName": "RcCount" },
                    { }
                ],
                "order": [[0, "desc"]]

            }, "RechargeReport/GetPendingRecharges");
        }

       

        $this.init = function () {
            initializeGrid();
           // initializeModalWithForm();
            
        };
    }
    $(function () {
        var self = new ReportPendingApiWise();
        self.init();

    });


   

}(jQuery));