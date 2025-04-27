(function ($) {
    'use strict';
    function BankAccountIndex() {
        var $this = this, locationGrid, formAddEditLocation;
        
        function initializeGrid() {
            
            var locationGrid = new Global.GridAjaxHelper('#grid-index', {
                "aoColumns": [
                    { "sName": "Id" },
                    { "sName": "BankName" },
                    { "sName": "BranchName" },
                    { "sName": "BranchAddress" },
                    { "sName": "AccountNo" },
                    { "sName": "IFSCCode" },
                    { "sName": "HolderName" },
                    { "sName": "UpiAdress" },
                    { "sName": "UserId" },
                    { "sName": "ApiId" },
                    { "sName": "Remark" },
                    {},
                    { },
                    { }
                ],
                "aoColumnDefs": [
                    { 'bSortable': false, 'aTargets': [11,12,13] },
                    { 'visible': false, 'aTargets': [14] }
                ],
                "order": [[0, "desc"]]
            }, "bankaccount/getbankaccounts", function () {
                
            });
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
                var AccountNo = $('#txtAccountNo').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();

                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();
                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();


                var HolderName = $('#txtHolderName').val();
                var UpiAddress = $('#txtUpiAddress').val();
                var Remark = $('#txtRemark').val();
                var BankName = $('#txtBankName').val();

                var requrl = '/bankaccount/index?i=' + Isa;
               
                var routeval = '';
                if (AccountNo != '' && AccountNo != undefined) routeval += '&a=' + AccountNo;
                if (Sdate != '' && Sdate != '0' && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != '' && Edate != '0' && Edate != undefined) routeval += '&e=' + Edate;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;
                
                if (HolderName != '' && HolderName != undefined) routeval += '&h=' + HolderName;
                if (UpiAddress != '' && UpiAddress != undefined) routeval += '&m=' + UpiAddress;
                if (Remark != '' && Remark != undefined) routeval += '&r=' + Remark;
                if (BankName != '' && BankName != undefined) routeval += '&b=' + BankName;

                window.location.href = requrl + routeval;


            });

            $('#btnExport').on('click', function () {

                var Isa = $('#Isa').val();
                var AccountNo = $('#txtAccountNo').val();
                var Sdate = $('#txtFromDate').val();
                var Edate = $('#txtToDate').val();
                var ApiId = $('select#ddVendor :Selected').val();
                var UserId = $('select#ddUser :Selected').val();
                var HolderName = $('#txtHolderName').val();
                var UpiAddress = $('#txtUpiAddress').val();
                var Remark = $('#txtRemark').val();
                var CustomerNo = $('#txtCustomerNo').val();

                var requrl = '/bankaccount/index?i=' + Isa;

                var routeval = '';
                if (AccountNo != '' && AccountNo != undefined) routeval += '&ac=' + AccountNo;
                if (Sdate != ''  && Sdate != undefined) routeval += '&f=' + Sdate;
                if (Edate != ''  && Edate != undefined) routeval += '&e=' + Edate;
                if (ApiId != '' && ApiId != '0' && ApiId != undefined) routeval += '&v=' + ApiId;
                if (UserId != '' && UserId != '0' && UserId != undefined) routeval += '&u=' + UserId;

                if (HolderName != '' && HolderName != undefined) routeval += '&hn=' + HolderName;
                if (UpiAddress != '' && UpiAddress != undefined) routeval += '&upi=' + UpiAddress;
                if (Remark != '' && Remark != undefined) routeval += '&rm=' + Remark;
                if (CustomerNo != '' && CustomerNo != undefined) routeval += '&cno=' + CustomerNo;
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

        }

        $this.init = function () {
            initializeGrid();
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new BankAccountIndex();
        self.init();
    });

}(jQuery));