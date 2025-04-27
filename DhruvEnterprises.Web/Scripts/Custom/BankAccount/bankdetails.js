(function ($) {
    'use strict';
    function BankAccountIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "BankName" },
                    { "sName": "BranchName" },
                    { "sName": "BranchAddress" },
                    { "sName": "AccountNo" },
                    { "sName": "IFSCCode" },
                    { "sName": "HolderName" },
                ],
                "order": [[0, "desc"]]
            }, "bankaccount/GetBankDetailAccounts", function () {

            });
        }


        $this.init = function () {
            initializeGrid();
        };
    }
    $(function () {
        var self = new BankAccountIndex();
        self.init();
    });

}(jQuery));