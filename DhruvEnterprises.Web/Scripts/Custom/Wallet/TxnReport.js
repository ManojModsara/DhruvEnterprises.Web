(function ($) {
    'use strict';
    function TXNReportIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "RecId" },
                    { "sName": "UserId" },
                    { "sName": "OP_Bal" },
                    { "sName": "CR_Amt" },
                    { "sName": "DB_Amt" },
                    { "sName": "CL_Bal" },
                    { "sName": "AmtType.AmtTypeName" },
                    { "sName": "TxnType.TypeName" },
                    { "sName": "TxnDate" },
                    { "sName": "Remark" }
                ],
                "order": [[0, "desc"]]
            }, "Wallet/GetTxnReport", filterdata);
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
             //txtRecId,txtTxnId,txtFromDate,txtToDate,txtRemark,ddTxnType,ddAmtType,ddVendor,ddUser

            $('#btnSearch').on('click', function () {

                var IsShow = $('#IsShow').val();
                var RecId = $('#txtRecId').val();
                var TxnId = $('#txtTxnId').val();
                var FromDate = $('#txtFromDate').val();
                var ToDate = $('#txtToDate').val();
                var Remark = $('#txtRemark').val();
                
                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();
                var TxnTypeId = $('select#ddTxnType :Selected').val();
                var AmtTypeId = $('select#ddAmtType :Selected').val();


                var requrl = '/Wallet/TxnReport?y=' + IsShow;

                var routeval = '';
                if (RecId != '' && RecId != '0' && RecId != undefined) routeval += '&ri=' + RecId;
                if (TxnId != '' && TxnId != '0' && TxnId != undefined) routeval += '&ti=' + TxnId;
                if (FromDate != '' && FromDate != '0' && FromDate != undefined) routeval += '&fd=' + FromDate;
                if (ToDate != '' && ToDate != '0' && ToDate != undefined) routeval += '&ed=' + ToDate;
                if (Remark != '' && Remark != '0' && Remark != undefined) routeval += '&rm=' + Remark;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&vi=' + ApiId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&ui=' + UserId;
                if (TxnTypeId != '' && TxnTypeId != '0' && TxnTypeId != undefined) routeval += '&tt=' + TxnTypeId;
                if (AmtTypeId != '' && AmtTypeId != '0' && AmtTypeId != undefined) routeval += '&at=' + AmtTypeId;

               

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


            $('#btnExport').on('click', function () {
                var RecId = $('#txtRecId').val();
                var TxnId = $('#txtTxnId').val();
                var FromDate = $('#txtFromDate').val();
                var ToDate = $('#txtToDate').val();
                var Remark = $('#txtRemark').val();

                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();
                var TxnTypeId = $('select#ddTxnType :Selected').val();
                var AmtTypeId = $('select#ddAmtType :Selected').val();


                var requrl = '/Export/ExportCSV?rt=2';

                var routeval = '';
                if (RecId != '' && RecId != '0' && RecId != undefined) routeval += '&rto=' + RecId;
                if (TxnId != '' && TxnId != '0' && TxnId != undefined) TxnId += '&ut=' + TxnId;
                if (FromDate != '' && FromDate != '0' && FromDate != undefined) routeval += '&f=' + FromDate;
                if (ToDate != '' && ToDate != '0' && ToDate != undefined) routeval += '&e=' + ToDate;
                if (Remark != '' && Remark != '0' && Remark != undefined) routeval += '&rm=' + Remark;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                if (TxnTypeId != '' && TxnTypeId != '0' && TxnTypeId != undefined) routeval += '&ot=' + TxnTypeId;
                if (AmtTypeId != '' && AmtTypeId != '0' && AmtTypeId != undefined) routeval += '&vt=' + AmtTypeId;
                window.location.href = requrl + routeval;



            });

        }
        function GetFilterData() {
            var fdata = {};

            var RecId = $('#txtRecId').val();
            var TxnId = $('#txtTxnId').val();
            var FromDate = $('#txtFromDate').val();
            var ToDate = $('#txtToDate').val();
            var Remark = $('#txtRemark').val();

            var ApiId = $('select#ddVendor :Selected').val();
            var UserId = $('select#ddUser :Selected').val();
            var TxnTypeId = $('select#ddTxnType :Selected').val();
            var AmtTypeId = $('select#ddAmtType :Selected').val();

            if (RecId != '' && RecId != '0' && RecId != undefined)
                fdata.RecId = RecId;
            if (TxnId != '' && TxnId != '0' && TxnId != undefined)
                fdata.TxnId = TxnId;
            if (FromDate != '' && FromDate != '0' && FromDate != undefined)
                fdata.FromDate = FromDate;
            if (ToDate != '' && ToDate != '0' && ToDate != undefined)
                fdata.ToDate=ToDate;
            if (Remark != '' && Remark != '0' && Remark != undefined)
                fdata.Remark = Remark;
            if (ApiId != '' && ApiId != '0' && ApiId != undefined)
                fdata.ApiId = ApiId;
            if (UserId != '' && UserId != '0' && UserId != undefined)
                fdata.UserId = UserId;
            if (TxnTypeId != '' && TxnTypeId != '0' && TxnTypeId != undefined)
                fdata.TxnTypeId = TxnTypeId;
            if (AmtTypeId != '' && AmtTypeId != '0' && AmtTypeId != undefined)
                fdata.AmtTypeId = AmtTypeId;
            
            return fdata;

        }


        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new TXNReportIndex();
        self.init();
    });

}(jQuery));