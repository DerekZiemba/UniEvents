
(function (window, document, $, U, factory) {
	factory(window, document, $, U, factory);

}(window, window.document, window.jQuery, window.U,
function Factory(window, document, $, U, factory) {


	U.onReady(() => {
		var divLocationSearch = document.getElementById('LocationSearch');
		var btn = divLocationSearch.querySelector('button');

		btn.addEventListener('click', function (ev) {
			var route = btn.dataset.route;
			Array.forEach(divLocationSearch.querySelectorAll('.loc-param'), (input) => {
				if (input.value != null && input.value.length > 0) {
					route += input.dataset.name + "=" + input.value + "&"; //Append querystring param
				}
			});

			$.getJSON(route, function (data) {
				divLocationSearch.getElementsByTagName('textarea')[0].innerText = JSON.stringify(data, null, 2);
			});
		});

	});

}));

