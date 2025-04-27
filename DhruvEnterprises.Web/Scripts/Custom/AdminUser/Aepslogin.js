(function ($) {
    'use strict';
    function CreateEdit() {
        var $this = this, locationGrid;     
        function initModalControlsWithEvents() {
            $(document).on('click', '#loginBtn', function (event) {
                var loginRequestData = {};
                debugger;
                loginRequestData["merchantId"] = $("#UName").val();    //mandatory
                loginRequestData["merchantPin"] = $("#MPin").val();   //mandatory
                loginRequestData["superMerchantId"] = 982; //changes //mandatory
                var myJSON = JSON.stringify(loginRequestData);
                myJSON = encrypt(myJSON); //encrypt is a function include this function in implementation
                //console.log(myJSON);
                $.ajax({
                    url: "http://localhost:8680/matmweb/Login",
                    type: "POST",
                    dataType: 'json',
                    contentType: "application/json",
                    data: myJSON,
                    success: function (response) {
                        debugger;
                        var s = JSON.stringify(response);
                        if (response.status) {
                            debugger;
                           sessionStorage.setItem("token", response.token);
                          // window.location.href = "MicroATMServices.html";
                           window.location.href = '/MicroAtm/MicroATMServices';
                        } else {
                            alert(response.message);
                        }
                    }
                });
            });
        }
        function encrypt(data) {
            debugger;
            var keyHex = CryptoJS.enc.Utf8.parse("284908D75CAB6D9C9DE7281CBA76EF9D");
            var encrypted = CryptoJS.AES.encrypt(data, keyHex, {
                mode: CryptoJS.mode.ECB,
                padding: CryptoJS.pad.Pkcs7
            });
            return encrypted.toString();
        }
        $this.init = function () {
            initModalControlsWithEvents();
         };
    }  
    $(function () {
        var self = new CreateEdit();
        self.init();
    });
}(jQuery));
