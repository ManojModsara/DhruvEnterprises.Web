(function ($) {
    'use strict';
    function WalletRequest() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {

            var filterdata = GetFilterData();

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {

                "aoColumns": [
                    {},
                    { "sName": "Id" },
                    {},
                    {},
                    { "sName": "UserId" },
                    { "sName": "Amount" },
                    { "sName": "TransferType.Name" },
                    { "sName": "TxnType.TypeName" },
                    { "sName": "AmtType.AmtTypeName" },
                    { "sName": "StatusType.TypeName" },
                    //{
                    //    "sName": "ImageUrl", "mRender": function (data, type, row) {
                    //        return '<img src="' + row[10] + '" style="height:3em;" title="Screenshot"/>'
                    //    }
                    //},
                    { "sName": "Bankname" },
                    { "sName": "Chequeno" },
                    { "sName": "PaymentRemark" },
                    {},
                    { "sName": "TxnId" }
                    , { "sName": "Comment" }
                    , { "sName": "UpdatedById" }
                    , {}

                ],
                "order": [[1, "desc"]],
                "aoColumnDefs": [

                    Global.CurrentRoleId == 3 ? { 'visible': false, 'aTargets': [15, 16] } : { 'bSortable': false, 'aTargets': [1] }
                ],

            }, "Wallet/GetPenWalletRequests", filterdata);
        }

        function initializeModalWithForm() {

            $('.select2').select2();

            $("#btnExport").click(function (e) {


                var date1 = new Date();

                var filename = "WalletRequest_" + date1.getFullYear() + "_" + date1.getMonth() + "_" + date1.getDate() + ".xls";
                var htmltext = '<div><table>' + $('#grid-index').html() + '</table></div>';

                let file = new Blob([htmltext], { type: "application/vnd.ms-excel" });
                let url = URL.createObjectURL(file);
                let a = $("<a />", {
                    href: url,
                    download: filename
                }).appendTo("body").get(0).click();
                e.preventDefault();
            });

            $("#modal-createedit-wallet-request").on('show.bs.modal', function (event) {
                $('#modal-createedit-wallet-request .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-cancel-wallet-request").on('show.bs.modal', function (event) {
                $('#modal-cancel-wallet-request .modal-content').load($(event.relatedTarget).prop('href'));
            });

            $("#modal-approve-wallet-request").on('show.bs.modal', function (event) {
                $('#modal-approve-wallet-request .modal-content').load($(event.relatedTarget).prop('href'));
            });
            $("#modal-viewImage-wallet-request").on('show.bs.modal', function (event) {
                $('#modal-viewImage-wallet-request .modal-content').load($(event.relatedTarget).prop('href'));
            });

            if (isactive == '1') {
                $('#dvSearchPanel').show();
            }
            else {
                $('#dvSearchPanel').hide();
            }
            $('#btnClose').on('click', function () {

                if ($('#dvSearchPanel').is(":hidden")) {

                    $('#Isa').val('1');
                }
                else {
                    $('#Isa').val('0');
                }
                $('#dvSearchPanel').toggle();

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

                var Isa = $('#Isa').val();
                var requrl = '/Wallet/PendingWalletRequest?i=' + Isa;
                var filterdata = GetFilterData();
                var routeval = filterdata.RouteVal;

                window.location.href = requrl + routeval;

            });

        }

        function GetFilterData() {
            var filterdata = {};

            var routeval = '';

            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var chequeno = $('#txtChequeNo').val();
            var remark = $('#txtRemark').val();
            var comment = $('#txtComment').val();
            var accid = $('select#ddAccount :Selected').val();
            var trtypeid = $('select#ddTrType :Selected').val();
            var userid = $('select#ddUser :Selected').val();
            var userid2 = $('select#ddUpdatedBy :Selected').val();
            var statusid = $('select#ddStatus :Selected').val();

            if (chequeno != '' && chequeno != undefined) {
                routeval += '&cq=' + chequeno;
                filterdata.RefId = chequeno;
            }


            if (Sdate != '' && Sdate != '0') {
                routeval += '&f=' + Sdate;
                filterdata.FromDate = Sdate;
            }


            if (Edate != '' && Edate != '0') {
                routeval += '&e=' + Edate;
                filterdata.ToDate = Edate;
            }


            if (remark != '' && remark != undefined) {
                routeval += '&r=' + remark;
                filterdata.Remark = remark;
            }


            if (comment != '' && comment != undefined) {
                routeval += '&c=' + comment;
                filterdata.Comment = comment;
            }


            if (accid != '' && accid != '0' && accid != undefined) {
                routeval += '&a=' + accid;
                filterdata.AccountId = accid;
            }


            if (trtypeid != '' && trtypeid != '0' && trtypeid != undefined) {
                routeval += '&t=' + trtypeid;
                filterdata.TrTypeId = trtypeid;
            }


            if (userid != '' && userid != '0' && userid != undefined) {
                routeval += '&u=' + userid;
                filterdata.UserId = userid;
            }


            if (userid2 != '' && userid2 != '0' && userid2 != undefined) {
                routeval += '&u2=' + userid2;
                filterdata.UserId2 = userid2;
            }

            if (statusid != '' && statusid != undefined) {
                routeval += '&s=' + statusid;
                filterdata.StatusId = statusid;
            }


            filterdata.RouteVal = routeval;

            return filterdata;

        }


        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new WalletRequest();
        self.init();
    });
    $('#grid-index').on('draw.dt', function () {
        GetFooter();
    });


}(jQuery));
function GetFooter() {

    $.ajax({
        url: "/Wallet/VirtualBankReportSuccessSum",
        type: "POST",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $('#sucess').html(data.SuccessSum.toFixed(2));
        }
    });
}