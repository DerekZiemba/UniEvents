"use strict";
(function (factory) {
    factory(window, window.document, window.jQuery, window.U);
}(function Factory(window, document, $, U) {
    U.onReady(function () {
        var divLocationSearch = document.getElementById('LocationSearch');
        var btn = divLocationSearch.querySelector('button');
        btn.addEventListener('click', function (ev) {
            var route = btn.dataset.route;
            Array.forEach(divLocationSearch.querySelectorAll('.loc-param'), function (input) {
                if (input.value != null && input.value.length > 0) {
                    route += input.dataset.name + "=" + input.value + "&";
                }
            });
            $.getJSON(route, function (data) {
                divLocationSearch.getElementsByTagName('textarea')[0].innerText = JSON.stringify(data, null, 2);
            });
        });
    });
}));
