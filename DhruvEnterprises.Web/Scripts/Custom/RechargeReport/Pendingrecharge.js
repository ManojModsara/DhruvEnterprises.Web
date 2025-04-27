
(function ($) {
    'use strict';
    function AdminUserIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initializeGrid() {
            var fdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    {},
                    { "sName": "Id" },
                    { "sName": "UserId" },
                    { "sName": "ApiSource.ApiName" },
                    //{ "sName": "TxnId" },
                    { "sName": "CustomerNo" },
                    { "sName": "Operator.Name" },
                    { "sName": "Amount" },
                    //{ "sName": "RCTypeId" },
                    { "sName": "StatusId" },
                    { "sName": "RequestTime" },
                    { "sName": "ResponseTime" },
                    //{ "sName": "MediumId" },
                    { "sName": "StatusMsg" },
                    //{ "sName": "IPAddress" },
                    //{ "sName": "CircleId" },
                    //{ "sName": "ROfferAmount" },
                    { "sName": "UserTxnId" },
                    { "sName": "OurRefTxnId" },
                    { "sName": "ApiTxnId" },
                    { "sName": "OptTxnId" },
                    { "sName": "UpdatedDate" }
                ],

                "order": [[1, "desc"]],
                "aoColumnDefs":
                    [
                        Global.CurrentRoleId == 3 ?
                            { 'targets': [3, 4, 14, 18, 19, 20], 'visible': false } :
                            { 'targets': [2], 'orderable': false }
                    ]

            }, "/RechargeReport/PendingRecharge", fdata, function () {
          
            });
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
            if ($("#IsResentOnly").is(":checked"))
                filterdata.IsResentOnly=1

            return filterdata;

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
                debugger;
                var Isa = $('#Isa').val();
                var Searchid = $('#txtSearchId').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                //var Sid = $('select#ddStatus :Selected').val();
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();

                var Customerno = $('#txtCustomerNo').val();
                var OpTxnid = $('#txtOpTxnid').val();
                var UserReqid = $('#txtUserReqid').val();
                var Cid = $('select#ddCircle :Selected').val();
                var ApiTxnid = $('#txtApiTxnid').val();

                var Uid2 = $('select#ddUser2 :Selected').val();

                var requrl = '/RechargeReport/PendingRecharge?s=2&i=' + Isa;

                var routeval = '';
                if (Searchid != '' && Searchid != '0' && Searchid != undefined) routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                //if (Sid != '' && Sid != '0' && Sid != undefined) routeval += '&s=' + Sid;
                if (Apiid != '' && Apiid != '0' && Apiid != undefined) routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0' && Opid != undefined) routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0' && Customerno != undefined) routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0' && OpTxnid != undefined) routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0' && UserReqid != undefined) routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0' && Cid != undefined) routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined) routeval += '&vt=' + ApiTxnid;

                if (Uid2 != '' && Uid2 != '0' && Uid2 != undefined) routeval += '&u2=' + Uid2;
                if ($("#IsResentOnly").is(":checked")) routeval += '&ck=' + 1;

                window.location.href = requrl + routeval;


            });

            $('#btnExport').on('click', function () {

                var Searchid = $('#txtSearchId').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                //var Sid = $('select#ddStatus :Selected').val();
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();

                var Customerno = $('#txtCustomerNo').val();
                var OpTxnid = $('#txtOpTxnid').val();
                var UserReqid = $('#txtUserReqid').val();
                var Cid = $('select#ddCircle :Selected').val();
                var ApiTxnid = $('#txtApiTxnid').val();

                var Uid2 = $('select#ddUser2 :Selected').val();

                var requrl = '/Export/ExportCSV?rt=1&s=2';

                var routeval = '';
                if (Searchid != '' && Searchid != '0') routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;
                //if (Sid != '' && Sid != '0') routeval += '&s=2' + Sid;
                if (Apiid != '' && Apiid != '0' && Apiid != undefined) routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0') routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0') routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0') routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0') routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0') routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined) routeval += '&vt=' + ApiTxnid;
                if (Uid2 != '' && Uid2 != '0' && Uid2 != undefined) routeval += '&u2=' + Uid2;
                if ($("#IsResentOnly").is(":checked")) routeval += '&ck=' + 1;
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

            $("#modal-change-recharge-statusbulk").on('show.bs.modal', function (event) {

                $('#modal-change-recharge-statusbulk .modal-content').load($(event.relatedTarget).prop('href'));

            });

            $("#modal-Resend-rechargebulk").on('show.bs.modal', function (event) {

                $('#modal-Resend-rechargebulk .modal-content').load($(event.relatedTarget).prop('href'));

            });

            $("#modal-change-recharge-status").on('show.bs.modal', function (event) {
                $('#modal-change-recharge-status .modal-content').load($(event.relatedTarget).prop('href'));
            });
        }


        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new AdminUserIndex();
        self.init();
    });




}(jQuery));

