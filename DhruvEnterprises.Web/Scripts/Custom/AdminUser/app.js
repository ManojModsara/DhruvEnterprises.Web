var historytable = null;
$(document).on('click', '#loginBtn', function (event) {
    debugger;
    var loginRequestData = {};
    loginRequestData["merchantId"] = $("#userName").val();//mandatory
    loginRequestData["merchantPin"] = $("#pwd").val();//mandatory
    loginRequestData["superMerchantId"] = 982; //changes //mandatory
    var myJSON = JSON.stringify(loginRequestData);
    console.log("request json :" + myJSON);
    myJSON = encrypt(myJSON);
    $.ajax({
        url: "Login",
        type: "post",
        data: myJSON,
        success: function (response) {
            console.log(response);
            if (response.status) {
                sessionStorage.setItem("token", response.token);
                //window.location.href = "services.html";
            } else {
                alert(response.message);
            }
        }
    });
});

function showStatus(text) {
    debugger;

    $(".status").remove();
    $(".loadingoverlay").append("<span class='status'>" + text + "</span>");

}
$(document).on('change', '#mode', function (event) {
    debugger;

    connectDevice($(this).val());
});


function connectDevice(mode) {
    debugger;
    $("#navbar-right a").removeClass("activeLink");
    $(".bi").addClass("activeLink");
    sessionStorage.setItem("txnFlag", 1);
    $.LoadingOverlay("show");
    showStatus("Connecting device....");
    $.ajax({
        url: 'http://localhost:8680/matmweb/ConnectDevice',
        headers: {
            "token": sessionStorage.getItem("token"),
            "mode": mode
        },
        success: function (response) {
            console.log(response);
            if (response.status) {
                showStatus(response.message);
                setTimeout(function () {
                    $.LoadingOverlay("hide");
                }, 2000);
            } else {
                showStatus(response.message);
                setTimeout(function () {
                    $.LoadingOverlay("hide");
                }, 1000);
            }
        }
    });
}


$(document).on('click', '#submitBtn', function (event) {
    debugger;
    $.LoadingOverlay("show");
    showStatus("Processing....");
    var doTransaction = {};
    var _mobileNum = $("#mobileNumber").val();
    var _remark = $("#remarks").val();
    var _txnType = sessionStorage.getItem("txnFlag");
    //alert(_txnType);
    var _amt = 0;
    doTransaction["mobileNumber"] = _mobileNum;   //mandatory
    doTransaction["remarks"] = _remark				//mandatory
    doTransaction["txnType"] = _txnType;//sessionStorage.getItem("txnFlag");//mandatory
    if (sessionStorage.getItem("txnFlag") == 2) {
        doTransaction["amount"] = $("#amount").val();
        _amt = $("#amount").val();
    }
    var _merchantTransactionId = "ezytm" + new Date().getUTCMilliseconds();
   // doTransaction["merchantTransactionId"] = _merchantTransactionId;//"ezytm"+ new Date().getUTCMilliseconds();//mandatory
    var _deviceImei = "test";
    doTransaction["deviceImei"] = _deviceImei;//mandatory
    var _latitude = $("#latitude").val();
    var _longitude = $("#longitude").val();
    if (_latitude == "" || _longitude == "") {
        _latitude = "10.0";
        _longitude = "10.1";
    }
    //var _latitude = "10.0";
    //var _longitude = "10.1";
    doTransaction["latitude"] = _latitude;//mandatory
    doTransaction["longitude"] = _longitude;//mandatory
    var myJSON = JSON.stringify(doTransaction);
   // myJSON = encrypt(myJSON);
    //console.log(myJSON);
    if (_txnType == "2") {
       var Domain= $("#DomainName").val()
        $.post(Domain +'MicroAtm/MicroAtmService', { mobileNumber: _mobileNum, remark: _remark, txnType: _txnType, amt: _amt, merchantTransactionId: _merchantTransactionId, deviceImei: _deviceImei, latitude: _latitude, longitude: _longitude }, function (result) {// yaha se data ko hm apni application me send kar rhe hain usko db me save karna hai, ager save ho jayega tb do transaction wala ajax call karna hai, device information ke liye me dekhta hu sir..
            debugger;
            var s = JSON.parse(result);
            doTransaction["merchantTransactionId"] = s.TXNID;
            myJSON = JSON.stringify(doTransaction);
            myJSON = encrypt(myJSON);
            if (result.STATUS = "1") {
                $.ajax({
                    url: "http://localhost:8680/matmweb/DoTransaction",
                    headers: {
                        "token": sessionStorage.getItem("token")
                    },
                    type: "post",
                    data: myJSON,
                    success: function (response) {
                        console.log(response);
                        $(".transactionDiv").show();
                        updatedata(response, s.TXNID);
                        debugger;
                        if (response.status) {
                            showStatus(response.message);
                            $(".statusSub").remove();
                            if (response.data.errorCode == "00") {
                                $("#statusTag").append('<span class="glyphicon glyphicon-ok successSymbol statusSub"></span>');
                            } else {
                                $("#statusTag").append('<span class="glyphicon glyphicon-remove statusSub" style="color: #f42a2a; font-size: 20px;"></span></span>');
                            }

                            $("#txnTable tr").remove();
                            $("#txnTable").append("<tr><caption><span >Transaction status :</span><span class='glyphicon glyphicon-ok successSymbol'></span></caption><td>Message</td><td>" + response.data.errorMessage + "</td></tr>" +
                                "<tr><td>Bank Name</td><td>" + response.data.bankName + "</td></tr>" +
                                "<tr><td>Card Number</td><td>" + response.data.cardNumber + "</td></tr>" +
                                "<tr><td>Transaction Amount</td><td>" + response.data.transactionAmount + "</td></tr>" +
                                "<tr><td>Balance Amount</td><td>" + response.data.balanceAmount + "</td></tr>" +
                                "<tr><td>Bank RRN</td><td>" + response.data.bankRRN + "</td></tr>" +
                                "<tr><td>TerminalId</td><td>" + response.data.terminalId + "</td></tr>");

                            if (response.data.miniStatementStructureModel != null) {
                                $("#miniStatementList").show();
                                $("#miniStatementList tbody tr").remove();
                                for (var i = 0; i < response.data.miniStatementStructureModel.length; i++) {
                                    $("#miniStatementList tbody").append("<tr><td>" + response.data.miniStatementStructureModel[i].date + "</td><td>" + response.data.miniStatementStructureModel[i].txnType + "</td><td>" + response.data.miniStatementStructureModel[i].amount + "</td><td>" + response.data.miniStatementStructureModel[i].narration + "</td></tr>");
                                }
                            } else {
                                $("#miniStatementList").hide();
                            }

                            setTimeout(function () {
                                $.LoadingOverlay("hide");
                            }, 1000);
                        } else {
                            showStatus(response.message);
                            setTimeout(function () {
                                $.LoadingOverlay("hide");
                            }, 1000);
                        }

                    }
                });
            }
            else {
                alert(result.MESSAGE);
            }

        });
    }
    else {
        var Domain = $("#DomainName").val()
        $.post(Domain +'MicroAtm/MicroAtmService', { mobileNumber: _mobileNum, remark: _remark, txnType: _txnType, amt: _amt, merchantTransactionId: _merchantTransactionId, deviceImei: _deviceImei, latitude: _latitude, longitude: _longitude }, function (result) {// yaha se data ko hm apni application me send kar rhe hain usko db me save karna hai, ager save ho jayega tb do transaction wala ajax call karna hai, device information ke liye me dekhta hu sir..
            debugger;
            var s = JSON.parse(result);
            doTransaction["merchantTransactionId"] = s.TXNID;
            myJSON = JSON.stringify(doTransaction);
            myJSON = encrypt(myJSON);
            if (result.STATUS = "1") {
                $.ajax({
                    url: "http://localhost:8680/matmweb/DoTransaction",
                    headers: {
                        "token": sessionStorage.getItem("token")
                    },
                    type: "post",
                    data: myJSON,
                    success: function (response) {
                        console.log(response);
                        console.log(JSON.stringify(response));
                        updatedata(response, s.TXNID);

                        $(".transactionDiv").show();
                        if (response.status) {
                            showStatus(response.message);
                            $(".statusSub").remove();
                            if (response.data.errorCode == "00") {
                                $("#statusTag").append('<span class="glyphicon glyphicon-ok successSymbol statusSub"></span>');
                            } else {
                                $("#statusTag").append('<span class="glyphicon glyphicon-remove statusSub" style="color: #f42a2a; font-size: 20px;"></span></span>');
                            }

                            $("#txnTable tr").remove();
                            $("#txnTable").append("<tr><caption><span >Transaction status :</span><span class='glyphicon glyphicon-ok successSymbol'></span></caption><td>Message</td><td>" + response.data.errorMessage + "</td></tr>" +
                                "<tr><td>Bank Name</td><td>" + response.data.bankName + "</td></tr>" +
                                "<tr><td>Card Number</td><td>" + response.data.cardNumber + "</td></tr>" +
                                "<tr><td>Transaction Amount</td><td>" + response.data.transactionAmount + "</td></tr>" +
                                "<tr><td>Balance Amount</td><td>" + response.data.balanceAmount + "</td></tr>" +
                                "<tr><td>Bank RRN</td><td>" + response.data.bankRRN + "</td></tr>" +
                                "<tr><td>TerminalId</td><td>" + response.data.terminalId + "</td></tr>");

                            if (response.data.miniStatementStructureModel != null) {
                                $("#miniStatementList").show();
                                $("#miniStatementList tbody tr").remove();
                                for (var i = 0; i < response.data.miniStatementStructureModel.length; i++) {
                                    $("#miniStatementList tbody").append("<tr><td>" + response.data.miniStatementStructureModel[i].date + "</td><td>" + response.data.miniStatementStructureModel[i].txnType + "</td><td>" + response.data.miniStatementStructureModel[i].amount + "</td><td>" + response.data.miniStatementStructureModel[i].narration + "</td></tr>");
                                }
                            } else {
                                $("#miniStatementList").hide();
                            }

                            setTimeout(function () {
                                $.LoadingOverlay("hide");
                            }, 1000);
                        } else {
                            showStatus(response.message);
                            setTimeout(function () {
                                $.LoadingOverlay("hide");
                            }, 1000);
                        }

                    }
                });
            }
        });
    }


});

function updatedata(response, TXNID)   {
    debugger;
    var data = {};
    var myJSON = JSON.stringify(response);

    data["BankName"] = response?.data?.bankName;
    data["OperatorTxnId"] = response?.data?.bankRRN;
    data["BalanceAmount"] = response?.data?.balanceAmount;
    data["TransactionAmount"] = response?.data?.transactionAmount;
    data["Amount"] = response?.data?.balanceAmount;
    data["TerminalId"] = response?.data?.terminalId;
    data["ERRORCODE"] = response?.data?.errorCode;
    data["DATA"] = myJSON;
    data["ORDERID"] = TXNID;
    var Domain = $("#DomainName").val()
    $.post(Domain +'MicroAtm/MicroAtmStatus',
        {
            BankName: data.BankName, OperatorTxnId: data.OperatorTxnId,
            BalanceAmount: data.BalanceAmount, TransactionAmount: data.TransactionAmount, ERRORCODE: data.ERRORCODE,
            ORDERID: data.ORDERID, Amount: data.Amount, TerminalId: data.TerminalId, DATA: myJSON
        }, function (result) {// yaha se data ko hm apni application me send kar rhe hain usko db me save karna hai, ager save ho jayega tb do transaction wala ajax call karna hai, device information ke liye me dekhta hu sir..
    });
}

$(document).on('click', '#logOutBtn', function (event) {
    debugger;
    $.ajax({
        url: "LogOut",
        type: "get",
        success: function (response) {
            console.log(response);
            if (response.status) {
                window.location.href = "home.html";
            } else {
                window.location.href = "/microatm/index";
                //alert(response.message);
            }

        }
    });
});


$(document).on('click', '#navbar-right a', function (event) {
    debugger;

    //$("input").val("");
    $(".transactionDiv").hide();
    $(".historyDiv").hide();
    $(".inputDiv").show();
    
    $("#navbar-right a").removeClass("active");
    $(this).addClass("active");

    //if ($(this).hasClass("bi")) {
    //    sessionStorage.setItem("txnFlag", 1);
    //} else if ($(this).hasClass("cw")) {
    //    sessionStorage.setItem("txnFlag", 2);
    //} else if ($(this).hasClass("mini")) {
    //    sessionStorage.setItem("txnFlag", 4);
    //}
    //if ($(this).hasClass("cw")) {

    //    $(".amountDiv").show();
    //} else {
    //    $(".amountDiv").hide();
    //}

});
function printData() {
    debugger;

    $("#printDiv").empty();
    $("#printDiv").append("<img src='images/logo.jpg' class='' style='display: block;  margin: auto;'></img>");
    $("#txnTable td").css("padding", "5px");
    $("#txnTable").clone().appendTo("#printDiv").css({
        "margin": "auto",
        "padding-top": "20px"
    });
    if ($("#miniStatementList tbody tr").length > 0) {
        $("#miniStatementList").clone().appendTo("#printDiv").css({
            "margin": "auto",
            "padding-top": "20px"
        });;
    }
    var divToPrint = document.getElementById("printDiv")

    newWin = window.open("");
    newWin.document.write(divToPrint.outerHTML);
    newWin.print();
    newWin.close()

    /*  var printDiv=document.getElementById("printDiv");
   printDiv.append("<span>Print receipt</span>");
   var divToPrint=document.getElementById("txnTable");
   printDiv.append(divToPrint);
   printDiv.append("<h1>Hello</h1>");
   newWin= window.open("");
   newWin.document.write(printDiv.outerHTML);  
   newWin.print();
   newWin.close();*/
}
$(document).on('click', '#printReceipt', function (event) {
    debugger;

    printData();
});


$(document).on('click', '#getHistoryBtn', function (event) {
    debugger;

    $.LoadingOverlay("show");
    showStatus("Processing....");
    var lTable = document.getElementById("table");
    lTable.style.display = "table";
    var historyReqObj = {};
    historyReqObj["fromDate"] = $("#fromDate").val();
    historyReqObj["toDate"] = $("#toDate").val();
    var myJSON = JSON.stringify(historyReqObj);
    console.log(myJSON);
    $.ajax({
        url: "History",
        headers: {
            "token": sessionStorage.getItem("token"),
            "superMerchantId": 2
        },
        type: "post",
        data: myJSON,
        success: function (response) {
            console.log(response.data);
            if (response.status) {
                if (historytable == null) {
                    historytable = $('#table').DataTable({
                        "data": response.data,

                        retrieve: true,
                        "columns": [
							{
							    data: 'transactionType',
							    render: function (data, type) {
							        if (data === 1) {
							            return "CW";

							        }
							        else if (data === 2) {
							            return "CD";
							        }
							    }
							},
								{ "data": "transactionTimestamp" },
								{ "data": "transactionStatus" },
								{ "data": "amount" }

                        ]
                    });
                }
                else {

                    historytable.clear().draw();
                    historytable.rows.add(response.data);
                    historytable.columns.adjust().draw();

                }
                $.LoadingOverlay("hide");
            }
            else {
                $.LoadingOverlay("hide");
                alert(response.message);
            }

        }
    });
});

$(document).on('click', '#HistoryBtn', function (event) {
    debugger;

    $(".inputDiv").hide();
    $(".transactionDiv").hide();
    $(".historyDiv").show();
});

$(document).on('click', '#navbar-right', function (event) {
    debugger;

    //AEPSTAB(event);
});
function AEPSTAB(type) {
    debugger;
    if (!$(this).hasClass("active")) {
        // Remove the class from anything that is active
        $("li").removeClass("active");
        // And make this active
        $(this).addClass("active");
    }
    if ((type) == ("bi")) {
        sessionStorage.setItem("txnFlag", 1);
    } else if ((type) == ("cw")) {
        sessionStorage.setItem("txnFlag", 2);
    } else if ((type) == ("mini")) {
        sessionStorage.setItem("txnFlag", 4);
    }
    if ((type) == ("cw")) {

        $(".amountDiv").show();
    } else {
        $(".amountDiv").hide();
    }
    $("#txnTable tr").remove();
    $("#transactionType").val(type);
}
function encrypt(data) {
    var keyHex = CryptoJS.enc.Utf8.parse("284908D75CAB6D9C9DE7281CBA76EF9D");
    var encrypted = CryptoJS.AES.encrypt(data, keyHex, {
        mode: CryptoJS.mode.ECB,
        padding: CryptoJS.pad.Pkcs7
    });
    return encrypted.toString();
}
function toHex(str) {
    var result = '';
    for (var i = 0; i < str.length; i++) {
        result += str.charCodeAt(i).toString(16);
    }
    return result;
}

$(document).ready(function () {
    debugger;
        navigator.geolocation.getCurrentPosition(
            // Success callback
            function (position) {
                $('#latitude').val(position.coords.latitude)
                $('#longitude').val(position.coords.longitude);
            },
            
            // Optional error callback
            function (error) {
                if (error) {
                    $('#locateStoreModal .card-body').html('<p>We are unable to fetch your location. Kindly allow location access to locate a <strong>Digi&nbsp;Kendra</strong> near you.</p>');
                    return;
                }
            }
    );
});


