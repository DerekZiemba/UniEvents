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
    function handleWebMethodSelected(ev) {
        var option = this.selectedOptions[0];
        var metadata = U.dictMetaData[option.value];
        document.getElementById('routeTemplate').value = metadata.route;
        var currentRoute = document.getElementById('currentRoute');
        currentRoute.value = metadata.path;
        function handleChange() {
        }
        if (!metadata.params) {
            var params = [];
            metadata.input.forEach(function (param) {
                param.elem = Element.From("<div class=\"inputparam\" name=\"" + param.name + "\"> \n                                       <label>" + param.name + " : " + param.jsType + "</label>\n                                       <input type=\"text\"/>\n                                     </div>");
                param.elemInput = param.elem.getElementsByTagName('input')[0];
                param.elemInput.addEventListener('change', handleChange);
                params.push(param);
            });
            metadata.params = params;
        }
        var inputParams = document.getElementById('InputParams');
        while (inputParams.firstElementChild) {
            inputParams.removeChild(inputParams.firstElementChild);
        }
        metadata.params.forEach(function (param) {
            inputParams.appendChild(param.elem);
        });
        console.log(option, metadata);
    }
}));
