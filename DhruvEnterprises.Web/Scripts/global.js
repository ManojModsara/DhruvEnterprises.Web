/*global window, $*/
var Global = {
    MessageType: {
        Success: 0,
        Error: 1,
        Warning: 2,
        Info: 3
    }
};

Global.FormHelper = function (formElement, options, onSucccess, onError) {
    "use strict";
    var settings = {};
    settings = $.extend({}, settings, options);
    formElement.validate(settings.validateSettings);

    formElement.submit(function (e) {

        var submitBtn = formElement.find(':submit');
        if (formElement.validate().valid()) {
            // submitBtn.find('i').removeClass("fa fa-arrow-circle-right");
            submitBtn.find('i').addClass("fa fa-spinner fa-spin");
            submitBtn.prop('disabled', true);
            // submitBtn.find('span').html('Submiting..');

            $.ajax(formElement.attr("action"), {
                type: "POST",
                data: formElement.serializeArray(),
                success: function (result) {
                    if (onSucccess === null || onSucccess === undefined) {
                        if (result.isSuccess) {
                            window.location.href = result.redirectUrl;
                        } else {
                            if (settings.updateTargetId) {
                                $("#" + settings.updateTargetId).html(result);
                            }
                        }
                    } else {
                        onSucccess(result);
                    }
                },
                error: function (jqXHR, status, error) {
                    if (onError !== null && onError !== undefined) {
                        onError(jqXHR, status, error);
                    }
                },
                complete: function () {
                    submitBtn.find('i').removeClass("fa fa-spinner fa-spin");
                    //  submitBtn.find('i').addClass("fa fa-arrow-circle-right");
                    //  submitBtn.find('span').html('Submit');
                    submitBtn.prop('disabled', false);
                }
            });
        }

        e.preventDefault();
    });

    return formElement;
};

Global.GridHelper = function (gridElement, options) {
    if ($(gridElement).find("thead tr th").length > 1) {
        var settings = {};
        settings = $.extend({}, settings, options);
        $(gridElement).dataTable(settings);
        return $(gridElement);
    }
};

Global.GridAjaxHelper = function (gridElement, options, serviceUrl, filterdata, callback) {
    if ($(gridElement).find("thead tr th").length >= 1) {
        var settings = {
            "processing": true,
            "serverSide": true,
            "ajax": Global.DomainName + serviceUrl,
            "bLengthChange": true,
            "fnServerData": function (sSource, aoData, fnCallback) {
                var aoDataServer = {
                    start: 0,
                    draw: 0,
                    length: 0,
                    columns: [],
                    order: [],
                    search: {
                        regex: false,
                        value: ""
                    },
                    multisearch: []
                };

                for (var i = 0; i < aoData.length; i++) {
                    if (aoData[i].name == "start") {
                        aoDataServer.start = aoData[i].value;
                    } else if (aoData[i].name == "draw") {
                        aoDataServer.draw = aoData[i].value;
                    } else if (aoData[i].name == "length") {
                        aoDataServer.length = aoData[i].value;
                    } else if (aoData[i].name == "order") {
                        for (var j = 0; j < aoData[i].value.length; j++) {
                            aoDataServer.order.push({
                                column: aoData[i].value[j].column,
                                dir: aoData[i].value[j].dir
                            });
                        }
                    } else if (aoData[i].name == "search") {
                        aoDataServer.search.regex = aoData[i].value.regex;
                        aoDataServer.search.value = aoData[i].value.value;
                    } else if (aoData[i].name == "columns") {
                        for (var j = 0; j < aoData[i].value.length; j++) {
                            aoDataServer.columns.push({
                                data: aoData[i].value[j].data,
                                name: aoData[i].value[j].name,
                                orderable: aoData[i].value[j].orderable,
                                search: { regex: aoData[i].value[j].search.regex, value: aoData[i].value[j].search.value },
                                searchable: aoData[i].value[j].searchable
                            });
                        }
                    }
                }
                if (Global.DataServer != null && Global.DataServer.multisearch != null && Global.DataServer.dataURL == serviceUrl) {
                    aoDataServer.multisearch = Global.DataServer.multisearch;
                }
                if (Global.Filter != null) {
                    aoDataServer.filter = Global.Filter;
                }
                aoDataServer.filterdata = filterdata;
                   $.ajax({
                    "dataType": 'json',
                    "contentType": "application/json; charset=utf-8",
                    "type": "POST",
                    "cache": false,
                    "url": Global.DomainName + serviceUrl,
                    "data": JSON.stringify(aoDataServer),
                    "success": function (result) {
                        fnCallback(result);
                    },
                    error: function (xhr, textStatus, error) {
                        if (typeof console == "object") {
                            console.log(xhr.status + "," + xhr.responseText + "," + textStatus + "," + error);
                        }
                    }
                });
            },
            fnDrawCallback: function (oSettings) {
                if (callback)
                    callback();
            }             
        };
        var dop =  { "aLengthMenu": [[25, 50, 100, 250, 500, 1000], [25, 50, 100, 250, 500, 1000]] }
        options = $.extend({}, options, dop);
        settings = $.extend({}, settings, options);
        return $(gridElement).dataTable(settings);
    }
};
//Global.CheckDecorator = function () {
//    $('input[type="checkbox"], input[type="radio"]').iCheck({
//        checkboxClass: 'icheckbox_square-blue',
//        radioClass: 'iradio_flat-blue'
//    });
//}



Global.GridAjaxHelper2 = function (iUrl, iLabel, gridElement, options, serviceUrl, callback) {
    if ($(gridElement).find("thead tr th").length >= 1) {      
        var settings = {
            "processing": true,
            "serverSide": true,
            "ajax": Global.DomainName + serviceUrl,
            "bLengthChange": true,
            "fnServerData": function (sSource, aoData, fnCallback) {
                var aoDataServer = {
                    start: 0,
                    draw: 0,
                    length: 0,
                    columns: [],
                    order: [],
                    search: {
                        regex: false,
                        value: ""
                    },
                    multisearch: []
                };

                for (var i = 0; i < aoData.length; i++) {
                    if (aoData[i].name == "start") {
                        aoDataServer.start = aoData[i].value;
                    } else if (aoData[i].name == "draw") {
                        aoDataServer.draw = aoData[i].value;
                    } else if (aoData[i].name == "length") {
                        aoDataServer.length = aoData[i].value;
                    } else if (aoData[i].name == "order") {
                        for (var j = 0; j < aoData[i].value.length; j++) {
                            aoDataServer.order.push({
                                column: aoData[i].value[j].column,
                                dir: aoData[i].value[j].dir
                            });
                        }
                    } else if (aoData[i].name == "search") {
                        aoDataServer.search.regex = aoData[i].value.regex;
                        aoDataServer.search.value = aoData[i].value.value;
                    } else if (aoData[i].name == "columns") {
                        for (var j = 0; j < aoData[i].value.length; j++) {
                            aoDataServer.columns.push({
                                data: aoData[i].value[j].data,
                                name: aoData[i].value[j].name,
                                orderable: aoData[i].value[j].orderable,
                                search: { regex: aoData[i].value[j].search.regex, value: aoData[i].value[j].search.value },
                                searchable: aoData[i].value[j].searchable
                            });
                        }
                    }
                }

                if (Global.DataServer != null && Global.DataServer.multisearch != null && Global.DataServer.dataURL == serviceUrl) {
                    aoDataServer.multisearch = Global.DataServer.multisearch;
                }

                if (Global.Filter != null) {
                    aoDataServer.filter = Global.Filter;
                }

                $.ajax({
                    "dataType": 'json',
                    "contentType": "application/json; charset=utf-8",
                    "type": "POST",
                    "cache": false,
                    "url": Global.DomainName + serviceUrl,
                    "data": JSON.stringify(aoDataServer),
                    "success": function (result) {
                        fnCallback(result);
                    },
                    error: function (xhr, textStatus, error) {
                        if (typeof console == "object") {
                            console.log(xhr.status + "," + xhr.responseText + "," + textStatus + "," + error);
                        }
                    }
                });
            },
            fnDrawCallback: function (oSettings) {
                if (callback)
                    callback();
            },
            "initComplete": function (settings, json) {
                $.post(Global.DomainName + iUrl, function (result) {
                    $(iLabel).text(result);
                    $(iLabel).val(result);
                });
            }
        };

        var dop = { "aLengthMenu": [[25, 50, 100, 250, 500, 1000], [25, 50, 100, 250, 500, 1000]] }
        options = $.extend({}, options, dop);
        settings = $.extend({}, settings, options);
        return $(gridElement).dataTable(settings);
    }
};


Global.FormValidationReset = function (formElement, validateOption) {
    if ($(formElement).data('validator')) {
        $(formElement).data('validator', null);
    }

    $(formElement).validate(validateOption);

    return $(formElement);
};

Global.ShowMessage = function (message, type) {
    if (type == Global.MessageType.Success)
        alertify.success(message);
    else if (type == Global.MessageType.Error)
        alertify.error(message);
    else if (type == Global.MessageType.Warning)
        alertify.warning(message);
    else if (type == Global.MessageType.Info)
        alertify.message(message);
};

Global.Alert = function (title, message, callback) {
    alertify.alert(title, message, function () {
        if (callback)
            callback();
    });
};

Global.Confirm = function (title, message, okCallback, cancelCallback) {
    alertify.confirm(title, message, function () {
        if (okCallback)
            okCallback();
    }, function () {
        if (cancelCallback)
            cancelCallback();
    });
};

Global.htmlEncode = function(html) {
    html = $.trim(html);
    return html.replace(/[&"'\<\>]/g, function (c) {
        switch (c) {
            case "&":
                return "&amp;";
            case "'":
                return "&#39;";
            case '"':
                return "&quot;";
            case "<":
                return "&lt;";
            default:
                return "&gt;";
        }
    });
};

Global.htmlDecode = function (encodedStr) {
    var parser = new DOMParser;
    var dom = parser.parseFromString(
        '<!doctype html><body>' + encodedStr,
        'text/html');

    var decodedString = dom.body.textContent;
    return decodedString;
}