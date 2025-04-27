(function ($) {
    'use strict';
    function WalletRequest() {
        var $this = this, locationGrid, formRequestResponse;


        function initializeModalWithForm() {

            $('.select2').select2();

            var date = new Date();

            var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var end = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            var start = new Date(date.getFullYear(), date.getMonth(), date.getDate() - 30);
            $(document).ready(function () {
                $('#submit').click(function (e) {
                    //e.preventDefault();
                    debugger;
                    $('.loader_box').css("display", "block");
                    var fromDate = $('#txtFromDate').val();

                    var bankAccountId = $('#ddAccount').val();
                    var file = $('#FileUpload')[0].files[0];
                    var formData = new FormData();
                    formData.append('Date', fromDate);
                    formData.append('BankAccountId', bankAccountId);
                    formData.append('File', file);

                    if (bankAccountId != "" && bankAccountId != undefined &&  file != undefined) {
                        $.ajax({
                            url: '/Wallet/Import',
                            type: 'POST',
                            data: formData,
                            processData: false,
                            contentType: false,
                            success: function (response) {
                                $('.loader_box').css("display", "none");

                                if (response.success) {
                                    debugger;
                                    // Create a hidden iframe
                                    var iframe = $('<iframe>', {
                                        src: '/Wallet/Download?fileName=' + response.fileName,
                                        style: 'display: none'
                                    }).appendTo('body');
                                    location.reload();
                                }
                                else {
                                    alert(response.message);
                                }
                            },
                            error: function (xhr, status, error) {
                                // Handle error
                                console.error(xhr.responseText);
                            }

                        });
                    }
                    else {
                        $('.loader_box').css("display", "none");
                        alertify.error('Select All Filds.');
                    }
                });
            });
            //$(document).ready(function () {
            //    $('#submit').click(function (e) {
            //        // Prevent the default form submission behavior
            //        e.preventDefault();

            //        var fromDate = $('#txtFromDate').val();
            //        var bankAccountId = $('#ddAccount').val();
            //        var file = $('#FileUpload')[0].files[0];

            //        // Create a FormData object and append form data
            //        var formData = new FormData();
            //        formData.append('Date', fromDate);
            //        formData.append('BankAccountId', bankAccountId);
            //        formData.append('File', file);


            //        var hiddenForm = $('<form></form>').attr({
            //            method: 'POST',
            //            action: '/Wallet/Import',
            //            enctype: 'multipart/form-data',
            //            style: 'display:none;'
            //        });

            //        for (var pair of formData.entries()) {
            //            hiddenForm.append($('<input>').attr({
            //                type: 'hidden',
            //                name: pair[0],
            //                value: pair[1]
            //            }));
            //        }


            //        $('body').append(hiddenForm);


            //        hiddenForm.submit();
            //    });
            //});

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

        }
        $this.init = function () {
            initializeModalWithForm();
        };
    }
    $(function () {
        var self = new WalletRequest();
        self.init();
    });

}(jQuery));
