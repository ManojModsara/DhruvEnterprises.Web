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
                    //{ "sName": "RecId" },
                    { "sName": "RefId" },
                    { "sName": "CustomerNo" },
                    { "sName": "UserTxnId" }
                    //{ "sName": "Recharge.OpId" },
                    //{ "sName": "Recharge.StatusId" }
                    
                ],
                "aoColumnDefs": [{
                    "aTargets": [7],
                    "mRender": function (data, type, full) {
                        return $("<div/>").html(data).text();
                    }
                }],
                "order": [[0, "desc"]]
            }, "RequestResponse/GetRequestResponses", filterdata);
        }

        function initializeModalWithForm() {

            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }

            $('.select2').select2({
                placeholder: "--select--",
                allowClear: true
            });

            var date = new Date();

            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            // var start = new Date(date.getFullYear(), date.getMonth(), date.getDate()-30);

            $('#txtFromDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                //  startDate: start,
                endDate: end,
                autoclose: true
            });
            $('#txtToDate').datepicker({
                format: "dd/mm/yyyy",
                todayHighlight: true,
                //  startDate: start,
                endDate: end,
                autoclose: true
            });

            $('#txtFromDate', '#txtToDate').datepicker('setDate', today);

            $('#btnSearch').on('click', function () {
                      //RecId,CustomerNo,RefId,UserTxnId,Remark,UserId,Apiid,Opid,Sid,Sdate,Edate,Isa,SdateNow,EdateNow
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
                var requrl = '/RequestResponse/Index?i=' + Isa;
                var routeval = '';
                if (RecId != '' && RecId != undefined) routeval += '&r=' + RecId;
                if (CustomerNo != '' && CustomerNo != undefined) routeval += '&m=' + CustomerNo;
                if (RefId != '' &&   RefId != undefined) routeval += '&rf=' + RefId;
                if (UserTxnId != '' && UserTxnId != undefined) routeval += '&ut=' + UserTxnId;
                if (Remark != '' && Remark != undefined) routeval += '&rm=' + Remark;
                if (FromDate != '' && FromDate != undefined) routeval += '&f=' + FromDate;
                if (ToDate != '' && ToDate != undefined) routeval += '&e=' + ToDate;

                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (OpId != '' && OpId != '0' && OpId != undefined) routeval += '&o=' + OpId;
                if (StatusId != '' && StatusId != '0' && StatusId != undefined) routeval += '&s=' + StatusId;
               
                window.location.href = requrl + routeval;


            });
            
            $('#btnExport').on('click', function () {

                //RecId,CustomerNo,RefId,UserTxnId,Remark,UserId,Apiid,Opid,Sid,Sdate,Edate,Isa,SdateNow,EdateNow

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

                var requrl = '/Export/ExportCSV?rt=3';

                var routeval = '';
                if (RecId != '' && RecId != undefined) routeval += '&rto=' + RecId;
                if (CustomerNo != '' && CustomerNo != undefined) routeval += '&m=' + CustomerNo;
                if (RefId != '' && RefId != undefined) routeval += '&ot=' + RefId;
                if (UserTxnId != '' && UserTxnId != undefined) routeval += '&ut=' + UserTxnId;
                if (Remark != '' && Remark != undefined) routeval += '&rm=' + Remark;
                if (FromDate != '' && FromDate != undefined) routeval += '&f=' + FromDate;
                if (ToDate != '' && ToDate != undefined) routeval += '&e=' + ToDate;

                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (OpId != '' && OpId != '0' && OpId != undefined) routeval += '&o=' + OpId;
                if (StatusId != '' && StatusId != '0' && StatusId != undefined) routeval += '&s=' + StatusId;

                window.location.href = requrl + routeval;


            });


            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();

            });


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