(function ($) {
    'use strict';
    function ReportPendingOpWise() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper2('RechargeReport/opCount', '#opCount','#grid-PendingOpWise', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "OpId" },
                    { "sName": "ApiName" },
                    { "sName": "Name" },
                    { "sName": "RcCount" },
                    {}
                ],
                "order": [[0, "desc"]]

            }, "RechargeReport/GetOpPendingRecharges");
        }



        $this.init = function () {
            initializeGrid();

        };
    }
    $(function () {
        var self = new ReportPendingOpWise();
        self.init();
    });

    

}(jQuery));