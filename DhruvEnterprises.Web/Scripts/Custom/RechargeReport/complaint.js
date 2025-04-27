(function ($) {
    'use strict';
    function ComplaintIndex() {
        var $this = this, locationGrid, formRequestResponse;

        function initializeGrid() {
            var filterdata = GetFilterData();
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "ComplaintById" },
                    {},
                    { "sName": "StatusId" },
                    { "sName": "RecId" },
                    {},
                    { "sName": "Recharge.StatusId" },
                    { "sName": "Recharge.CustomerNo" },
                    { "sName": "Recharge.ApiId" },
                    { "sName": "Recharge.OpId" },
                    { "sName": "Recharge.Amount" },
                    {},
                    { "sName": "Remark" },
                    { "sName": "Comment" },
                    { "sName": "ResolvedById" },
                    { "sName": "Recharge.OptTxnId" },
                    { "sName": "IsRefund" },
                    { "sName": "Recharge.OurRefTxnId" },
                    { "sName": "LastDate" },
                    { "sName": "ExpiryDate" }
                ],
               
                "order": [[0, "desc"]],
                "aoColumnDefs": [

                    Global.CurrentRoleId == 3 ? { 'visible': false, 'aTargets': [8,14] } : { 'bSortable': false, 'aTargets': [0] }
                ]
                
            }, "RechargeReport/GetComplaintReport", filterdata);
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
            var start = new Date(date.getFullYear(), date.getMonth(), date.getDate()-30);

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

            $('#txtFromDate').datepicker('setDate', start);
            $('#txtToDate').datepicker('setDate', today );
            
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

                var Searchid = $('#txtRecId').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var Sid = $('select#ddStatus :Selected').val();
                var Apiid = $('select#ddVendor :Selected').val();
                var Uid = $('select#ddUser :Selected').val();
                var Opid = $('select#ddOperator :Selected').val();

                var Customerno = $('#txtCustomerNo').val();
                var OpTxnid = $('#txtOptTxnId').val();
                var UserReqid = $('#txtUserReqid').val();
                var Cid = $('select#ddCircle :Selected').val();
                var ApiTxnid = $('#txtApiTxnid').val();
                
                var requrl = '/Export/ExportCSV?rt=6';

                var routeval = '';
                if (Searchid != '' && Searchid != '0') routeval += '&rto=' + Searchid;
                if (Sdate != '' && Sdate != '0') routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0') routeval += '&e=' + Edate;

                if (Sid != '' && Sid != '0') routeval += '&s=' + Sid;
                else routeval += '&s=25';

                if (Apiid != '' && Apiid != '0' && Apiid != undefined) routeval += '&v=' + Apiid;
                if (Uid != '' && Uid != '0' && Uid != undefined) routeval += '&u=' + Uid;
                if (Opid != '' && Opid != '0') routeval += '&o=' + Opid;

                if (Customerno != '' && Customerno != '0') routeval += '&m=' + Customerno;
                if (OpTxnid != '' && OpTxnid != '0') routeval += '&ot=' + OpTxnid;
                if (UserReqid != '' && UserReqid != '0' && UserReqid != undefined) routeval += '&ut=' + UserReqid;
                if (Cid != '' && Cid != '0') routeval += '&c=' + Cid;
                if (ApiTxnid != '' && ApiTxnid != '0' && ApiTxnid != undefined) routeval += '&vt=' + ApiTxnid;

               

                window.location.href = requrl + routeval;


            });


            $('#btnEmail').on('click', function () {

                debugger;

                var ftdata = GetFilterData();
               // var fdata = JSON.stringify({ FilterData: ftdata })
                $.post(Global.DomainName + 'Export/EmailComplaint', { fdata: ftdata }, function (result) {
                    if (result == 0) {
                        alertify.success('Mail Sent.');
                    }
                    else if (result == 1) {
                        alertify.error('No Vendor Selected');
                    }
                    else if (result == 1) {
                        alertify.error('Vendor mail-Id not available');
                    }
                    else {
                        alertify.error('OPPS! Something went wrong.');
                    }
                });


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

        function GetFilterData() {
            var filterdata = {};
            
            var Isa = $('#Isa').val();
            var Sdate = $('#txtFromDate').val();
            var Edate = $('#txtToDate').val();
            var Sid = $('select#ddStatus :Selected').val();
            var Apiid = $('select#ddVendor :Selected').val();
            var Uid = $('select#ddUser :Selected').val();
            var Opid = $('select#ddOperator :Selected').val();

            var Customerno = $('#txtCustomerNo').val();
            var Cid = $('select#ddCircle :Selected').val();

            var RecId = $('#txtRecId').val();
            var OpTxnId = $('#txtOptTxnId').val();
           
            if (Sdate != '' && Sdate != '0')
                filterdata.FromDate = Sdate;

            if (Edate != '' && Edate != '0')
                filterdata.ToDate = Edate;

            if (Sid != '' && Sid != '0')
                filterdata.StatusId = Sid;
            else
                filterdata.StatusId=25

            if (Apiid != '' && Apiid != '0' && Apiid != undefined)
                filterdata.ApiId = Apiid;

            if (Uid != '' && Uid != '0' && Uid != undefined)
                filterdata.UserId = Uid;

            if (Opid != '' && Opid != '0')
                filterdata.OpId = Opid;

            if (Customerno != '' && Customerno != '0')
                filterdata.CustomerNo = Customerno;

            if (Cid != '' && Cid != '0')
                filterdata.CircleId = Cid;

            if (RecId != '' && RecId != '0')
                filterdata.RecId = RecId;

            if (OpTxnId != '' && OpTxnId != '0')
                filterdata.OpTxnId = OpTxnId;
            

            return filterdata;

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