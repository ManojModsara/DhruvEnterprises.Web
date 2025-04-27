(function ($) {
    'use strict';
    function ApiWalletIndex() {
        var $this = this, locationGrid, formAddEditLocation;

        function initGridControlsWithEvents() {
         
        }

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ApiSource.ApiName"  },
                    { "sName": "TxnDate" },
                    { "sName": "RecId" },
                    { "sName": "RefTxnId" },
                    { "sName": "OP_Bal" },
                    { "sName": "CR_Amt" },
                    { "sName": "Ins_Amt" },
                    { "sName": "DB_Amt" },
                    { "sName": "CL_Bal" },
                    { "sName": "TxnType.TypeName" },
                    { "sName": "AmtType.AmtTypeName" },
                    { "sName": "Remark" },
                    { "sName": "ApiClBal" },
                    { "sName": "ClBalDiff" },
                    { "sName": "AddedById" }
                ],
                "order": [[0, "desc"]]
            }, "ApiWallet/GetApiWalletTxns", function () {
                initGridControlsWithEvents();
            });
        }

        function initializeModalWithForm() {
            $("#modal-add-edit-addapiwallet").on('show.bs.modal', function (event) {
                $('#modal-add-edit-addapiwallet .modal-content').load($(event.relatedTarget).prop('href'));
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


                var requrl = '/ApiWallet/Index?y=' + IsShow;

                var routeval = '';
                if (RecId != '' && RecId != '0' && RecId != undefined) routeval += '&ri=' + RecId;
                if (TxnId != '' && TxnId != '0' && TxnId != undefined) TxnId += '&ti=' + TxnId;
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


        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ApiWalletIndex();
        self.init();
    });

}(jQuery));