
/// <reference path="ZMBA.js" />

/*! UniEvents.js */
(function (window, document, $, factory) {

	window.U = factory(window, document, $, factory); //Export as U

}(window, window.document, window.jQuery,
function UniEventFactory(window, document, $, factory) {

	const ReadyListener = (function () {
		function addCallback(cb) {
			var i = 1, len = arguments.length, args = new Array(len - i + 1); for (; i < len; i++) { args[i - 1] = arguments[i]; }
			this._isReady ? cb.apply(null, args) : this._callbacks.push({ cb, args });
		}
		function runQueued(ev) {
			while (this._callbacks.length > 0) {
				var ob = this._callbacks.shift();
				try {
					ob.cb.apply(null, ob.args);
				} catch (ex) { console.error(ex, ob); }
			}
		}
		function ReadyListener(evName) {
			this._callbacks = [];
			this._isReady = false;
			this.add = addCallback.bind(this);
			window.addEventListener(evName, runQueued.bind(this));
		}
		Object.assign(ReadyListener.prototype, {
			get isReady() { return this._isReady; },
			set isReady(value) { if ((this._isReady = value)) { this.runQueued(); } }
		});
		return ReadyListener;
	}());


	const onReadyListener = new ReadyListener("DOMContentLoaded");
	const onLoadedListener = new ReadyListener("load");


	function UniEvents() {
		this.onReady = onReadyListener.add;
		this.onLoaded = onLoadedListener.add;
	}


	return new UniEvents();

}));






