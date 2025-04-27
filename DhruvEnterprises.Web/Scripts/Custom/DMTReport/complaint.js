(function ($) {
    'use strict';
    function ComplaintIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {

            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ComplaintById" },
                    {},
                    { "sName": "StatusId" },
                    { "sName": "RecId" },
                    { "sName": "Recharge.OurRefTxnId" },
                    { "sName": "Recharge.TxnId" },
                    { "sName": "Recharge.StatusId" },
                    {},
                    { "sName": "Recharge.CustomerNo" },
                    { "sName": "Recharge.OpId" },
                    { "sName": "Recharge.Amount" },
                    { "sName": "Recharge.Amount" },
                    { "sName": "Recharge.ApiId" },
                    { },
                    { "sName": "IsRefund" },
                    { "sName": "RefundTxnId" },
                    { "sName": "ResolvedById" }
                ],
               
                "order": [[0, "desc"]],

                
            }, "RechargeReport/GetComplaintReport");
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

                var Isa = $('#Isa').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Sid = $('select#ddStatus :Selected').val();
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();
            
                var Customerno = $('#txtCustomerNo').val();
                var Cid = $('select#ddCircle :Selected').val();
                
                var requrl = '/rechargereport/complaint?i=' + Isa;

                var routeval = '';
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                if (Sid != '' && Sid != '0' && Sid != undefined) routeval += '&s=' + Sid;
                else {
                    routeval += '&s=0';}
                if (Apiid != '' && Apiid != '0' && Apiid != undefined) routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0' && Opid != undefined) routeval += '&o=' + Opid;
                if (Customerno != '' && Customerno != '0' && Customerno != undefined) routeval += '&m=' + Customerno;
                if (Cid != '' && Cid != '0' && Cid != undefined) routeval += '&c=' + Cid;
               
                window.location.href = requrl + routeval;


            });
            
            $('#btnExport').on('click', function () {

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

                
                var requrl = '/Export/ExportCSV?rt=1';

                var routeval = '';
                if (Searchid != '' && Searchid != '0') routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;
                if (Sid != '' && Sid != '0') routeval += '&s=' + Sid;
                if (Apiid != '' && Apiid != '0') routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0') routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0') routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0') routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0') routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0') routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0') routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0') routeval += '&vt=' + ApiTxnid;

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

           

            $("#modal-view-rec-detail").on('show.bs.modal', function (event) {
                $('#modal-view-rec-detail .modal-content').load($(event.relatedTarget).prop('href'));
            });
           
            $("#modal-resolve-complaint").on('show.bs.modal', function (event) {
                $('#modal-resolve-complaint .modal-content').load($(event.relatedTarget).prop('href'));
            });
            //Generate-Complaint
        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new ComplaintIndex();
        self.init();
    });



}(jQuery));