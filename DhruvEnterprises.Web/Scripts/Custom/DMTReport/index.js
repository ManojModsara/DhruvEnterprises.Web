(function ($) {
    'use strict';
    function ReportIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "TxnId" },
                    { "sName": "UserId" },
                    { "sName": "AccountNo" },
                    //{ "sName": "Bank_Name" },
                    { "sName": "Operator.Name" },
                    { "sName": "ApiSource.ApiName" },
                    { "sName": "Amount" },
                    { "sName": "StatusType.TypeName" },
                    { "sName":"ApiSource.ApiName"},
                    { "sName": "ApiTxnId" },
                    { "sName": "OptTxnId" },
                    { "sName": "RequestTime" },
                    //{ "sName": "UpdatedDate" },
                    {},
                    { "sName": "OurRefTxnId" },
                    { "sName": "UserTxnId" },
                    { "sName": "UpdatedById" },
                    {},
                    {}
                   
                ],
               
                "order": [[0, "desc"]],
                "aoColumnDefs": [
                    Global.CurrentRoleId == 3 ? { 'visible': false, 'aTargets': [4,8] } : { 'bSortable': false, 'aTargets': [1] }
                ]

                
            }, "DMTReport/GetRechargeReport", filterdata);
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
            $('#btnExport').on('click', function () {

                debugger;
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Sid = $('select#ddStatus :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();

                var Customerno = $('#txtCustomerNo').val();
                var OpTxnid = $('#txtOpTxnid').val();
                var UserReqid = $('#txtUserReqid').val();
                var Cid = $('select#ddCircle :Selected').val();
                var ApiTxnid = $('#txtApiTxnid').val();

                var Uid2 = $('select#ddUser2 :Selected').val();
                var fdata = GetFilterData();
                var requrl = '/Export/ExportCSV?rt=9';

                var routeval = '';
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;
                if (Sid != '' && Sid != '0') routeval += '&s=' + Sid;
                if (Opid != '' && Opid != '0') routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0') routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0') routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0') routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0') routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined) routeval += '&vt=' + ApiTxnid;
                if (Uid2 != '' && Uid2 != '0' && Uid2 != undefined) routeval += '&u2=' + Uid2;
                window.location.href = requrl + routeval;


            });
            $('#btnSearch').on('click', function () {

                var Isa = $('#Isa').val();
                var Searchid = $('#txtSearchId').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Sid = $('select#ddStatus :Selected').val();
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();
            
                var Customerno = $('#txtCustomerNo').val();
                var OpTxnid = $('#txtOpTxnid').val();
                var UserReqid = $('#txtUserReqid').val();
                var Cid = $('select#ddCircle :Selected').val();
                var ApiTxnid = $('#txtApiTxnid').val();

                var Uid2 = $('select#ddUser2 :Selected').val();

                var requrl = '/DMTReport/Index?i=' + Isa;

                var routeval = '';
                if (Searchid != '' && Searchid != '0' && Searchid != undefined) routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                if (Sid != '' && Sid != '0' && Sid != undefined) routeval += '&s=' + Sid;
                if (Apiid != '' && Apiid != '0' && Apiid != undefined) routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0' && Opid != undefined) routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0' && Customerno != undefined) routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0' && OpTxnid != undefined) routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0' && UserReqid != undefined) routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0' && Cid != undefined) routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined) routeval += '&vt=' + ApiTxnid;

                if (Uid2 != '' && Uid2 != '0' && Uid2 != undefined) routeval += '&u2=' + Uid2;

                window.location.href = requrl + routeval;


            });
            
            
            $('#btnClose').on('click', function () {
                
                if ($('#dvSearchPanel').is(":hidden")) {
                   
                    $('#Isa').val('1');
                }
                else 
                {
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
            $("#modal-change-recharge-status").on('show.bs.modal', function (event) {
                $('#modal-change-recharge-status .modal-content').load($(event.relatedTarget).prop('href'));
            });
            $("#modal-generate-complaint").on('show.bs.modal', function (event) {
                $('#modal-generate-complaint .modal-content').load($(event.relatedTarget).prop('href'));
            });
            //Generate-Complaint
        }

        function GetFilterData() {
            var filterdata = {};
                        
            var Searchid = $('#txtSearchId').val();
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var Sid = $('select#ddStatus :Selected').val();
            var Apiid = $('select#ddVendor :Selected').val();
            var Uid = $('select#ddUser :Selected').val();
            var Opid = $('select#ddOperator :Selected').val();

            var Customerno = $('#txtCustomerNo').val();
            var OpTxnid = $('#txtOpTxnid').val();
            var UserReqid = $('#txtUserReqid').val();
            var Cid = $('select#ddCircle :Selected').val();
            var ApiTxnid = $('#txtApiTxnid').val();

            var Uid2 = $('select#ddUser2 :Selected').val();


            if (Searchid != '' && Searchid != '0')
                filterdata.RefId = Searchid;

            if (Sdate != '' && Sdate != '0')
                filterdata.FromDate = Sdate;

            if (Edate != '' && Edate != '0')
                filterdata.ToDate = Edate;
            filterdata.EndDate = Edate;

            if (Sid != '' && Sid != '0')
                filterdata.StatusId = Sid;

            if (Apiid != '' && Apiid != '0' && Apiid != undefined)
                filterdata.ApiId = Apiid;

            if (Uid != '' && Uid != '0' && Uid != undefined)
                filterdata.UserId = Uid;

            if (Opid != '' && Opid != '0')
                filterdata.OpId = Opid;

            if (Customerno != '' && Customerno != '0')
                filterdata.CustomerNo = Customerno;

            if (OpTxnid != '' && OpTxnid != '0')
                filterdata.OpTxnId = OpTxnid;

            if (UserReqid != '' && UserReqid != '0')
                filterdata.UserTxnId = UserReqid;

            if (Cid != '' && Cid != '0')
                filterdata.CircleId = Cid;

            if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined)
                filterdata.ApiTxnId = ApiTxnid;

            if (Uid2 != '' && Uid2 != '0' && Uid2 != undefined)
                filterdata.UserId2 = Uid2;

            return filterdata;

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