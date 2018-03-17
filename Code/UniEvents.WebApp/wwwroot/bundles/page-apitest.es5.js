"use strict";
(function (factory) {
    factory(window, window.document, window.jQuery, window.U, window.ZMBA);
}(function Factory(window, document, $, U, ZMBA) {
    $.ajax({
        cache: false,
        url: 'webapi/metadata',
        success: function (data) {
            U.dictMetaData = data.result;
            var webMethodSelect = document.getElementById('webMethodSelect');
            webMethodSelect.addEventListener('change', handleWebMethodSelected);
            for (var route in U.dictMetaData) {
                var option = document.createElement('option');
                option.value = route;
                option.innerText = route;
                webMethodSelect.appendChild(option);
            }
        }
    });
    var metadata;
    var oBody = {};
    var btnClear = document.getElementById('btnClear');
    var btnExecute = document.getElementById('btnExecute');
    var currentHttpType = document.getElementById('httpType');
    var route = document.getElementById('route');
    var postBody = document.getElementById('postBody');
    var inputParams = document.getElementById('InputParams');
    var resultJson = document.getElementById('resultJson');
    function handleWebMethodSelected(ev) {
        btnClear.disabled = true;
        btnExecute.enable = false;
        while (inputParams.firstElementChild) {
            inputParams.removeChild(inputParams.firstElementChild);
        }
        var option = this.selectedOptions[0];
        metadata = U.dictMetaData[option.value];
        if (metadata) {
            btnExecute.disabled = false;
            currentHttpType.value = metadata.httpMethod.toUpperCase();
            route.value = metadata.path;
            if (!metadata.params) {
                metadata.params = [];
                metadata.input.forEach(function (param) {
                    param.elem = Element.From("<li class=\"inputparam\" name=\"" + param.name + "\"> \n                                             <span>(" + param.typeName + ")</span>\n                                             <label>" + param.name + ":</label>\n                                             <input type=\"text\" param=\"" + param.name + "\" source=\"" + param.source + "\"/>\n                                          </li>");
                    param.elemInput = param.elem.getElementsByTagName('input')[0];
                    param.elemInput.addEventListener('change', handleParamChanage);
                    param.elemInput.addEventListener('keyup', handleParamChanage);
                    metadata.params.push(param.elemInput);
                });
            }
            metadata.input.forEach(function (param) {
                inputParams.appendChild(param.elem);
            });
            handleParamChanage();
        }
        else {
            btnExecute.disabled = true;
        }
    }
    function handleParamChanage() {
        btnClear.disabled = false;
        var oData = U.buildAjaxRequestFromInputs(metadata.params, { url: metadata.path });
        route.value = oData.url;
        postBody.value = JSON.stringify(oData.data, null, '\t');
    }
    btnClear.addEventListener('click', function () {
        for (var i = 0, len = metadata.params.length; i < len; i++) {
            metadata.params[i].elemInput.value = '';
        }
    });
    btnExecute.addEventListener('click', function () {
        function callback(data) {
            resultJson.value = JSON.stringify(data, null, '\t');
        }
        var request = {
            cache: false,
            type: currentHttpType.value,
            url: route.value,
            success: callback,
            error: callback
        };
        if (postBody.value) {
            request.data = JSON.parse(postBody.value);
        }
        $.ajax(request);
    });
}));
