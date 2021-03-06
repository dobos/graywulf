﻿/* This is an auto-generated proxy */

function __serviceName__Service(serviceUrl) {
    this.serviceUrl = serviceUrl ? serviceUrl : "__serviceUrl__";
}

__serviceName__Service.prototype.error = function (xhr, status, message) {
    alert(message);
}

__serviceName__Service.prototype.__createUrl = function (pathParts, queryParts) {
    var finalUrl = this.serviceUrl;
    $.each(pathParts, function (i, part) {
        finalUrl += "/" + part;
    });

    if (queryParts) {
        finalUrl += "?";
        $.each(queryParts, function (key, value) {
            finalUrl += key + "=" + value + "&";
        });
    }
    console.info(finalUrl);
    return finalUrl;
}

__serviceName__Service.prototype.__callService = function (url, httpMethod, params, on_success, on_error) {
    var mimeType = "text/html";
    var contentType = "application/json";
    var accept = "application/json";
    var dataType = "json";

    if (params) {
        $.each(params, function (key, value) {
            console.log(key + ": " + value);
            switch (key) {
                case "mimeType":
                    mimeType = value;
                    break;
                case "contentType":
                    contentType = value;
                    break;
                case "accept":
                    accept = value;
                    break;
                case "dataType":
                    dataType = value;
                    break;
                case "headers":
                    $.ajaxSetup({ headers: value });
                    break;
                case "data":
                    $.ajaxSetup({ data: value });
                    break;
            };
        });
    }

    $.ajax({
        url: url,
        type: httpMethod,
        mimeType: mimeType,
        contentType: contentType,
        dataType: dataType,
        headers: {
            Accept: accept,
        },
        success: on_success,
        error: on_error
    });
}

