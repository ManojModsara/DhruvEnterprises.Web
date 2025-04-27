(function ($) {
    'use strict';
    function ReportIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    {  },
                    { "sName": "Id" },
                    { "sName": "TxnId" },
                    { "sName": "UserId" },
                    { "sName": "CustomerNo" },
                    { "sName": "Operator.Name" },
                    { "sName": "Circle.CircleName" },
                    { "sName": "Amount" },
                    { "sName": "StatusType.TypeName" },
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "ApiTxnId" },
                    { "sName": "OptTxnId" },
                    { "sName": "RequestTime" },
                    { "sName": "UpdatedDate" },
                    { "sName": "OurRefTxnId" },
                    { "sName": "UserTxnId" }

                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [0] }
                ],
                "order": [[1, "desc"]],


            }, "RechargeReport/ProcessingRecharge");
        }

        function initializeModalWithForm() {

            $('#checkBoxAll').click(function () {
                if ($(this).is(":checked")) {
                    $(".chkCheckBoxId").prop("checked", true)
                }
                else {
                    $(".chkCheckBoxId").prop("checked", false)
                }
            });  
            
            $('#btnStatusCheck').on('click', function () {
                debugger;
                var oTable = $("#grid-index").dataTable();
                var arr = '';
                $('input:checkbox.chkCheckBoxId:checked', oTable.fnGetNodes()).each(function () {
                    arr += $(this).val() + ',';
                });
                
             
                $.post(Global.DomainName + 'RechargeReport/CheckStatusBulk', { recIds: arr }, function (result) {
                    if (!result) {
                        alertify.error('An internal Error occurred.');
                    }
                    else {
                        alertify.success(result);

                    }
                });

            });

          

            $("#modal-change-recharge-statusbulk").on('show.bs.modal', function (event) {
               
                $('#modal-change-recharge-statusbulk .modal-content').load($(event.relatedTarget).prop('href'));
              
            });
            $("#modal-Resend-rechargebulk").on('show.bs.modal', function (event) {

                $('#modal-Resend-rechargebulk .modal-content').load($(event.relatedTarget).prop('href'));

            });

           
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ReportIndex();
        self.init();
    });



}(jQuery));