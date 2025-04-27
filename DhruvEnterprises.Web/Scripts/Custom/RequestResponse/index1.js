(function ($) {
    'use strict';
    function RequestResponseIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {

            var filterdata = GetFilterData();

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "UrlId" },
                    { "sName": "Remark" },
                    {},
                    { "sName": "RequestTxt" },
                    { "sName": "ResponseText" },
                    {},
                    { "sName": "RecId" },
                    { "sName": "RefId" },
                    { "sName": "CustomerNo" },
                    { "sName": "UserTxnId" },
                    { "sName": "Recharge.OpId" },
                    { "sName": "Recharge.StatusId" }
                    
                ],
                "aoColumnDefs": [{
                    "aTargets": [7],
                    "mRender": function (data, type, full) {
                        return $("<div/>").html(data).text();
                    }
                }],
                "order": [[0, "desc"]]
            }, "RequestResponse/GetRequestResponses1", filterdata);
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
        function GetFilterData() {
            var filterdata = {};

            var Isa = $('#Isa').val();

            var RecId = $('#txtRecId').val();
            var CustomerNo = $('#txtCustomerNo').val();
            var RefId = $('#txtRefId').val();
            var UserTxnId = $('#txtUserTxnId').val();
            var Remark = $('#txtRemark').val();
            var FromDate = $('#txtFromDate').val();
            var ToDate = $('#txtToDate').val();

            var UserId = $('select#ddUser :Selected').val();
            var ApiId = $('select#ddVendor :Selected').val();
            var OpId = $('select#ddOperator :Selected').val();
            var StatusId = $('select#ddStatus :Selected').val();


            if (RecId != '' && RecId != '0')
                filterdata.RecId = RecId;

            if (CustomerNo != '' && CustomerNo != '0')
                filterdata.CustomerNo = CustomerNo;

            if (RefId != '' && RefId != '0')
                filterdata.RefId = RefId;

            if (UserTxnId != '' && UserTxnId != '0')
                filterdata.UserTxnId = UserTxnId;

            if (Remark != '' && Remark != '0')
                filterdata.Remark = Remark;

            if (FromDate != '' && FromDate != '0')
                filterdata.FromDate = FromDate;

            if (ToDate != '' && ToDate != '0')
                filterdata.ToDate = ToDate;

            if (UserId != '' && UserId != '0' && UserId != undefined)
                filterdata.UserId = UserId;

            if (ApiId != '' && ApiId != '0' && ApiId != undefined)
                filterdata.ApiId = ApiId;

            if (OpId != '' && OpId != '0')
                filterdata.StatusId = OpId;

            if (StatusId != '' && StatusId != '0')
                filterdata.OpId = StatusId;
            
            return filterdata;

        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new RequestResponseIndex();
        self.init();
    });

}(jQuery));