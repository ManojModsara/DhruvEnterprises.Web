(function ($) {
    'use strict';
    function BankStatement() {
        var $this = this, locationGrid, formAddEditLocation;

        function initializeGrid() {
          var fdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "AccountId" },
                    { "sName": "PaymentDate" },
                    { "sName": "AddedDate" },
                    { "sName": "OP_Bal" },
                    { "sName": "CR_Amt" },
                    { "sName": "DB_Amt" },
                    { "sName": "CL_Bal" },
                    { "sName": "TrTypeId" },
                    { "sName": "TxnTypeId" },
                    { "sName": "AmtTypeId" },

                    { "sName": "PaymentRef" },
                    { "sName": "Remark" },
                    { "sName": "Comment" },
                    { "sName": "RefAccountId" },
                    {}, //user
                    { "sName": "TxnId" },
                    { "sName": "ApiId" },
                    { "sName": "ApiTxnId" },
                    {}

                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [14] }
                   //, { 'visible': false, 'aTargets': [14] }
                ],
                "order": [[0, "desc"]]
            }, "bankaccount/getbankstatement", fdata);
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

                var fdata = GetFilterData();
                window.location.href = fdata.URL;


            });

            $('#btnExport').on('click', function () {
                var PaymentRef = $('#txtPaymentRef').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();
                var StatementId = $('#txtStatementId').val();
                var Remark = $('#txtRemark').val();
                var AccountId = $('select#ddAccount :Selected').val();
                var TrTypeId = $('select#ddTrType :Selected').val();
                var TxnTypeId = $('select#ddTxnType :Selected').val();
                var AmtTypeId = $('select#ddAmtType :Selected').val();
                var RefAccountId = $('select#ddRefAccount :Selected').val();

                var requrl = '/Export/ExportCSV?rt=4';

                var routeval = '';
                if (PaymentRef != '' && PaymentRef != undefined) routeval += '&vt=' + PaymentRef;
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                if (StatementId != '' && StatementId != undefined) routeval += '&ut=' + StatementId;
                if (Remark != '' && Remark != undefined) routeval += '&rm=' + Remark;
                if (AccountId != '' && AccountId != '0' && AccountId != undefined) routeval += '&o=' + AccountId;
                if (TrTypeId != '' && TrTypeId != '0' && TrTypeId != undefined) routeval += '&c=' + TrTypeId;
                if (TxnTypeId != '' && TxnTypeId != '0' && TxnTypeId != undefined) routeval += '&m=' + TxnTypeId;
                if (AmtTypeId != '' && AmtTypeId != '0' && AmtTypeId != undefined) routeval += '&rto=' + AmtTypeId;
                if (RefAccountId != '' && RefAccountId != '0' && RefAccountId != undefined) routeval += '&s=' + RefAccountId;

                window.location.href = requrl + routeval;


            });

            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#IsShow').val('1');
                }
                else {
                    $('#IsShow').val('0');
                }
                $('#dvSearchPanel').toggle();

            });
        }

        function GetFilterData() {

            var filterdata = {};

            var Isa = $('#IsShow').val();
            var PaymentRef = $('#txtPaymentRef').val();
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var ApiId = $('select#ddVendor :Selected').val();
            var UserId = $('select#ddUser :Selected').val();
            var StatementId = $('#txtStatementId').val();
            var Remark = $('#txtRemark').val();
            var AccountId = $('select#ddAccount :Selected').val();
            var TrTypeId = $('select#ddTrType :Selected').val();
            var TxnTypeId = $('select#ddTxnType :Selected').val();
            var AmtTypeId = $('select#ddAmtType :Selected').val();
          //  var RefAccountId = $('select#ddRefAccount :Selected').val();

            //int? i, int? v, int? u, int? a, int? tr, int? tx, int? am, string p="", string s="", string r=""
            
            var requrl = '/bankaccount/bankstatement?i=' + Isa;

            var routeval = '';
            if (PaymentRef != '' && PaymentRef != undefined) {
                filterdata.RefId = PaymentRef;
                routeval += '&cq=' + PaymentRef;
            }
               

            if (Sdate != '' && Sdate != '0' && Sdate != undefined) {
                filterdata.FromDate = Sdate;
                routeval += '&f=' + Sdate;
            }
           
            if (Edate != '' && Edate != '0' && Edate != undefined) {
                filterdata.ToDate = Edate;
                routeval += '&e=' + Edate;
            }

            if (ApiId != '' && ApiId != '0' && ApiId != undefined) {
                filterdata.ApiId = Apiid;
                routeval += '&v=' + ApiId;
            }

            if (UserId != '' && UserId != '0' && UserId != undefined) {
                filterdata.UserId = Uid;
                routeval += '&u=' + UserId;
            }

            if (StatementId != '' && StatementId != undefined)
                filterdata.TxnId = StatementId;
            routeval += '&s=' + StatementId;

            if (Remark != '' && Remark != undefined) {
                filterdata.Remark = Remark;
                routeval += '&r=' + Remark;
            }
            if (AccountId != '' && AccountId != '0' && AccountId != undefined) {
                filterdata.AccountId = AccountId;
                routeval += '&a=' + AccountId;
            }

            //if (RefAccountId != '' && RefAccountId != '0' && RefAccountId != undefined) {
            //    filterdata.RefAccountId = RefAccountId;
            //    routeval += '&ra=' + RefAccountId;
            //}

            if (TrTypeId != '' && TrTypeId != '0' && TrTypeId != undefined) {
                filterdata.TrTypeId = TrTypeId;
                routeval += '&tr=' + TrTypeId;
            }

            if (TxnTypeId != '' && TxnTypeId != '0' && TxnTypeId != undefined) {
                filterdata.TxnTypeId = TxnTypeId;
                routeval += '&tx=' + TxnTypeId;
            }

            if (AmtTypeId != '' && AmtTypeId != '0' && AmtTypeId != undefined) {
                filterdata.AmtTypeId = AmtTypeId;
                routeval += '&at=' + AmtTypeId;
            }
            
            filterdata.URL = requrl + routeval;
            
            return filterdata;
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new BankStatement();
        self.init();
    });

}(jQuery));