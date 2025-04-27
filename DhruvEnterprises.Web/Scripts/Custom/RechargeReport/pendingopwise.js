(function ($) {
    'use strict';
    function ReportPendingOpWise() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var locationGrid = new Global.GridAjaxHelper2('RechargeReport/opCount', '#opCount', '#grid-PendingOpWise', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "ApiId" },
                    { "sName": "TxnId" },
                    { "sName": "CustomerNo" },
                    { "sName": "OpId" },
                    { "sName": "Amount" },
                    { "sName": "RCTypeId" },
                    { "sName": "StatusId" },
                    { "sName": "RequestTime" },
                    { "sName": "ResponseTime" },
                    { "sName": "MediumId" },
                    { "sName": "StatusMsg" },
                    { "sName": "IPAddress" },
                    { "sName": "CircleId" },
                    { "sName": "ROfferAmount" },
                    { "sName": "UserTxnId" },
                    { "sName": "OurRefTxnId" },
                    { "sName": "ApiTxnId" },
                    { "sName": "OptTxnId" },
                    { "sName": "UpdatedDate" },
                    { "sName": "AccountNo" },
                    { "sName": "AccountOther" },
                    { "sName": "Optional1" },
                    { "sName": "Optional2" },
                    { "sName": "Optional3" },
                    { "sName": "Optional4" },
                    { "sName": "ResendTime" },
                    { "sName": "ResendById" },
                    { "sName": "UpdatedById" },
                    { "sName": "LapuId" },
                    { "sName": "LapuNo" },
                    { "sName": "LapuId2" },
                    { "sName": "LapuNo2" },
                    { "sName": "UserComm" },
                    { "sName": "ApiComm" },
                    { "sName": "ApiBal" },
                    { "sName": "UserBal" },
                    { "sName": "ResendCount" },
                    { "sName": "IsROChecked" },
                    { "sName": "IsValidRO" },
                    { "sName": "Comment" },
                    { "sName": "beneficiaryIFSC" },
                    { "sName": "beneficiaryName" },
                    { "sName": "transferMode" },
                    { "sName": "AccountBalance" },
                    { "sName": "SettlementType" },
                    { "sName": "QrIntent" },
                    { "sName": "PaymentUrl" },
                    { "sName": "Integration_id" }
                ],
                "order": [[0, "desc"]]
            }, "RechargeReport/PendingRecharge");
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