

/*! ZMBA.js */
(function (window, document, extensionsAndPolyfills) {
   const extendType = (function () {
      function defMult(proto, name, prop, options, obj) { for (var i = 0, len = proto.length; i < len; i++) { defOne(proto[i], name, prop, options, obj); } };
      function defOne(proto, name, prop, options, obj) {
         if (name in proto) {
            if (options.override) {
               Object.defineProperty(proto, name, prop);
            } else if (options.merge) {
               var target = proto[name];
               var src = obj[name];
               extendType(target, src, options);
            }
         } else {
            Object.defineProperty(proto, name, prop);
         }
      };
      function extendType(proto, obj, options) {
         if (!options) { options = { enumerable: false, configurable: undefined, writable: undefined, override: true, merge: false }; }
         var define = proto instanceof Array ? defMult : defOne;
         var descriptors = Object.getOwnPropertyDescriptors(obj);

         for (var name in descriptors) {
            var opts = options.hasOwnProperty(name) ? Object.assign({}, options, options[name]) : options;
            var prop = descriptors[name];

            prop.enumerable = opts.enumerable ? true : false;
            if (opts.configurable === false) { prop.configurable = false; } else if (opts.configurable === true) { prop.configurable = true; }
            if ('value' in prop) {
               if (opts.writable === false) { prop.writable = false; } else if (opts.writable === true) { prop.writable = true; }
            }
            define(proto, name, prop, opts, obj);
         }
      }
      return extendType;
   }());

   extensionsAndPolyfills(window, document, extendType);

   const ZMBA = window.ZMBA = {
      domainName: window.location.hostname.match(/(?:(\w{3,})(?=(?:\.[a-z]{2,4}){1,2}$))|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})|(localhost)/igm)[0],
      extendType: extendType,
      loadJSFile: (function () {
         function append(src, cfg) {
            if (typeof src == 'string') {
               cfg.id = cfg.id || src.SubstrAfterLast('/').ReplaceAll('.', '_');
               var elem = Element.From(`<script id="${cfg.id}" type="text/javascript" ${cfg.attribs || ''}></script>`);
               elem.onload = (ev) => {
                  if (cfg.success) { cfg.success(ev); }
               };
                      elem.onerror = (ev) => {
                           elem.remove();
                           console.warn(`Failed to load resource.`, cfg.id, ev);
                           append(cfg.srcs.shift() || ev, cfg);
               };
               elem.src = src;
               document.head.appendChild(elem);
            } else {
               if (cfg.error) { cfg.error(src); }
            }
         }
         return function (cfg) {
            append(cfg.srcs.shift(), cfg);
         };
      }()),
      isEmptyArray: function IsEmptyArray(arr, isNothing) {
         if (!isNothing) { isNothing = ZMBA.isNothing; }
         for (var i = 0, len = arr && arr.length >>> 0; i < len; i++) { if (!isNothing(arr[i])) { return false; } }
         return true;
      },
      isNothing: function isNothing(el) { return el == null || typeof el === 'number' && isNaN(el); },
      isNullOrEmpty: function isNullOrEmpty(str) { return ZMBA.isNothing(str) || !str.length; },
      isNullOrWhitespace: (function () {
         const rgx = /^\s+$/;
         return function isNullOrWhitespace(str) {
            return ZMBA.isNullOrEmpty(str) || (typeof str === 'string' && str.match(rgx) !== null) || (str instanceof Array && ZMBA.isEmptyArray(str, ZMBA.isNullOrWhitespace));
         };
      }())
   };


   (function CookieHelperModule() {
      const _rgxVerifyKey = /^(?:expires|max\-age|path|domain|secure)$/i;

      function CookieHelper(document) {
         this.document = document;
      }

      CookieHelper.prototype = {
         getCookie: function (sKey) {
            if (!sKey) { return null; }
            var rgx = new RegExp("(?:(?:^|.*;)\\s*" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=\\s*([^;]*).*$)|^.*$");
            return decodeURIComponent(this.document.cookie.replace(rgx, "$1")) || null;
         },
         getCookieObject: function (sKey) {
            var str = this.getCookie(sKey);
            return !str ? null : JSON.parse(str);
         },
         setCookie: function (sKey, sValue, expires, sPath, sDomain, bSecure) {
            if (!sKey || _rgxVerifyKey.test(sKey)) { return false; }
            var sExpires = "";
            if (expires) {
               switch (expires.constructor) {
                  case Number: sExpires = expires === Infinity ? "; expires=Fri, 31 Dec 9999 23:59:59 GMT" : "; max-age=" + expires; break;
                  case String: sExpires = "; expires=" + expires; break;
                  case Date: sExpires = "; expires=" + expires.toUTCString(); break;
               }
            }
            if (typeof sValue !== 'string') {
               sValue = JSON.stringify(sValue);
            }
            this.document.cookie = encodeURIComponent(sKey) + "=" + encodeURIComponent(sValue) + sExpires + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "") + (bSecure ? "; secure" : "");
            return true;
         },
         removeCookie: function (sKey, sPath, sDomain) {
            if (!this.hasCookie(sKey)) { return false; }
            this.document.cookie = encodeURIComponent(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT" + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "");
            return true;
         },
         hasCookie: function (sKey) {
            if (!sKey || _rgxVerifyKey.test(sKey)) { return false; }
            return (new RegExp("(?:^|;\\s*)" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=")).test(this.document.cookie);
         },
         get keys() {
            var aKeys = this.document.cookie.replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, "").split(/\s*(?:\=[^;]*)?;\s*/);
            for (var nLen = aKeys.length, nIdx = 0; nIdx < nLen; nIdx++) { aKeys[nIdx] = decodeURIComponent(aKeys[nIdx]); }
            return aKeys;
         }
      }

      ZMBA.extendType(Document.prototype, {
         get cookies() { return new CookieHelper(this); }
      });

   }());

   (function PageReadyModule() {

      function addCallback(cb) {
         var i = 1, len = arguments.length, args = new Array(len - i + 1); for (; i < len; i++) { args[i - 1] = arguments[i]; }
         this._isReady ? cb.apply(null, args) : this._callbacks.push({ cb, args });
      }
      function ReadyListener(name) {
         this._callbacks = [];
         this._isReady = false;
         this.name = name;
         this.add = addCallback.bind(this);
      }
      ReadyListener.prototype = {
         get isReady() { return this._isReady; },
         set isReady(value) {
            if (!this._isReady && value) {
               this._isReady = value;
               while (this._callbacks.length > 0) {
                  var ob = this._callbacks.shift();
                  try {
                     ob.cb.apply(null, ob.args);
                  } catch (ex) { console.error(ex, ob); }
               }
               console.log(this.name + ": " + performance.now() + "ms");
            }
         }
      };

      const documentReadyListener = new ReadyListener("documentReady");
      const windowReadyListener = new ReadyListener("windowReady");

      ZMBA.ReadyListener = ReadyListener;
      ZMBA.onDocumentReady = documentReadyListener.add;
      ZMBA.onWindowReady = windowReadyListener.add;

      if (document.readyState === 'loaded') { documentReadyListener.isReady = true; }
      if (document.readyState === 'interactive' || document.readyState === 'complete') { documentReadyListener.isReady = true; windowReadyListener.isReady = true; }

      if (!documentReadyListener.isReady) {
         window.addEventListener("DOMContentLoaded", () => documentReadyListener.isReady = true);
      }
      if (!windowReadyListener.isReady) {
         window.addEventListener("load", () => { documentReadyListener.isReady = true; windowReadyListener.isReady = true; });
      }

   }());


}(window, window.document,
   function ExtensionsAndPolyfills(window, document, extendType) {

      if (!Object.getOwnPropertyDescriptors) {
         Object.getOwnPropertyDescriptors = function getOwnPropertyDescriptors(obj) {
            var desc = {};
            var names = Object.getOwnPropertyNames(obj);
            for (var i = 0, len = names.length; i < len; i++) {
               var name = names[i];
               desc[name] = Object.getOwnPropertyDescriptor(obj, name);
            }
            return desc;
         }
      }

      extendType(Object, {
         assign: function assign(target) { //Polyfill for InternetExplorer, Edge, Opera, and Android
            var to = Object(target);
            for (var i = 1, len = arguments.length; i < len; i++) {
               var src = arguments[i];
               if (src != null) {
                  for (var key in src) {
                     if (Object.prototype.hasOwnProperty.call(src, key)) {
                        to[key] = src[key];
                     }
                  }
               }
            }
            return to;
         }
      }, { override: false });


      extendType(String.prototype, {
         includes: function includes(search, start) {
            start = start | 0;
            if (start + search.length > this.length) {
               return false;
            } else {
               return this.indexOf(search, start) !== -1;
            }
         },
         ReplaceAll: function ReplaceAll(sequence, value) {
            return this.split(sequence).join(value);
         },
         Trim: (function () {
            const rgxBoth = /^\s+|\s+$/g;
            const rgxStart = /^\s+/;
            const rgxEnd = /\s+$/;
            function whitespaceTrim(str, option) {
               if (option == null || (option & 3) === 3) { return str.replace(rgxBoth, ''); }
               if ((option & 1) === 1) { return str.replace(rgxStart, ''); }
               if ((option & 2) === 2) { return str.replace(rgxEnd, ''); }
               return str;
            }
            function specialTrim(str, ch, option) {
               var len = ch.length;
               var left = 0, right = str.length, pos = 0;
               if (option == null || (option & 1) === 1) { while ((pos = str.indexOf(ch, left)) === left) { left = pos + len; } }
               if (option == null || (option & 2) === 2) { while ((pos = str.lastIndexOf(ch, right)) === right - len) { right = pos; } }
               return left > 0 || right < str.length ? str.substr(left, right - left) : str;
            }
            return function trim(ch, option) { return ch ? specialTrim(this, ch, option) : whitespaceTrim(this, option); };
         }()),
         TrimStart: function trimStart(ch) {
            return this.Trim(ch, 1);
         },
         TrimEnd: function trimEnd(ch) {
            return this.Trim(ch, 2);
         },
         SubstrBefore: function substrBefore(sequence, bIncludeSequence) {
            var idx = this.indexOf(sequence);
            if (idx >= 0) {
               if (bIncludeSequence) { idx += sequence.length; }
               if (idx <= this.length) { return this.substr(0, idx); }
            }
            return this;
         },
         SubstrAfter: function substrAfter(sequence, bIncludeSequence) {
            var idx = this.indexOf(sequence);
            if (idx >= 0) {
               if (!bIncludeSequence) { idx += sequence.length; }
               if (idx <= this.length) { return this.substr(idx); }
            }
            return this;
         },
         SubstrBeforeLast: function substrBeforeLast(sequence, bIncludeSequence) {
            var idx = this.lastIndexOf(sequence);
            if (idx >= 0) {
               if (bIncludeSequence) { idx += sequence.length; }
               if (idx <= this.length) { return this.substr(0, idx); }
            }
            return this;
         },
         SubstrAfterLast: function substrAfterLast(sequence, bIncludeSequence) {
            var idx = this.lastIndexOf(sequence);
            if (idx >= 0) {
               if (!bIncludeSequence) { idx += sequence.length; }
               if (idx <= this.length) { return this.substr(idx); }
            }
            return this;
         }
      }, { override: false });


      extendType(Date.prototype, {
         AddSeconds: function (value) {
            this.setSeconds(this.getSeconds() + value);
            return this;
         },
         AddMinutes: function (value) {
            this.setMinutes(this.getMinutes() + value);
            return this;
         },
         AddHours: function (value) {
            this.setHours(this.getHours() + value);
            return this;
         },
         AddDays: function (value) {
            this.setHours(this.getHours() + value);
            return this;
         }
      });

      extendType(Date, {
         get Current() { return new Date(); }
      });


      extendType(Element, {
         From: (function () {
            const doc = window.document;
            const rgx = /(\S+)=(["'])(.*?)(?:\2)|(\w+)/g;
            return function CreateElementFromHTML(html) { //Almost 3x performance compared to jQuery and only marginally slower than manually creating element: https://jsbench.github.io/#02fe05ed4fdd9ff6582f364b01673425
               var innerHtmlStart = html.indexOf('>') + 1;
               var elemStart = html.substr(0, innerHtmlStart);
               var match = rgx.exec(elemStart)[4];
               var elem = doc.createElement(match);
               while ((match = rgx.exec(elemStart)) !== null) {
                  if (match[1] === undefined) {
                     elem.setAttribute(match[4], "");
                  } else {
                     elem.setAttribute(match[1], match[3]);
                  }
               }
               elem.innerHTML = html.substr(innerHtmlStart, html.lastIndexOf('<') - innerHtmlStart);
               rgx.lastIndex = 0; //reset regex
               return elem;
            };
         }())

      });


      extendType(Element.prototype, { //For InternetExplorer and Edge
         remove: function remove() { //Polyfill for InternetExplorer and Edge
            if (this.parentNode) {
               this.parentNode.removeChild(this);
            }
         },
         matches: Element.prototype.matchesSelector || Element.prototype.webkitMatchesSelector || Element.prototype.msMatchesSelector || Element.prototype.mozMatchesSelector || Element.prototype.oMatchesSelector || function polyMatch(selector) {
            var matches = (this.document || this.ownerDocument).querySelectorAll(selector), i = matches.length;
            while (--i >= 0 && matches.item(i) !== this) { };
            return i > -1;
         },
         closest: function closest(s) { //For IE
            var el = this;
            do {
               if (el.matches(s)) { return el; }
               el = el.parentElement || el.parentNode;
            } while (el !== null && el.nodeType === 1);
         }
      }, { enumerable: true, override: false });


      extendType([Element.prototype, Document.prototype, DocumentFragment.prototype], {
         append: function append() { //Polyfill for InternetExplorer and Edge
            var frag = document.createDocumentFragment();
            for (var i = 0, len = arguments.length; i < len; i++) {
               var item = arguments[i];
               frag.appendChild(item instanceof Node ? item : document.createTextNode(String(item)));
            }
            this.appendChild(frag);
         }
      }, { enumerable: true, override: false });


      extendType(DOMTokenList.prototype, {
         ToggleMultiple: function ToggleMultiple() {
            var len = arguments.length - 1;
            var bHasForce = typeof arguments[len - 1] === 'boolean';
            var bResult = false;
            for (var i = 0; i < len; i++) {
               bResult = bHasForce ? this.toggle(arguments[i], arguments[len]) : this.toggle(arguments[i]);
            }
            return bResult;
         }
      });


      extendType(RegExp.prototype, {
         GetMatches: function GetMatches(str) {
            this.lastIndex = 0;
            var matches = [], match = null;
            while ((match = this.exec(str))) {
               matches.push(match);
            }
            return matches;
         },
      });


      extendType(Array, {
         isArray: function isArray(arg) { return Object.prototype.toString.call(arg) === '[object Array]'; }, //Polyfill for InternetExplorer and Edge

         Every: function Every(arr, cb) {
            if (!arr) { return false; }
            for (var i = 0, len = arr.length >>> 0; i < len; i++) { if (!cb(arr[i], i, arr)) { return false; } }
            return true;
         },
         ForEach: function ForEach(arr, cb) {
            if (arr) { for (var i = 0, len = arr.length >>> 0; i < len; i++) { cb(arr[i], i, arr); } }
            return arr;
         },
         Filter: function Filter(arr, cb) {
            var len = arr ? arr.length >>> 0 : 0;
            var res = new Array(len), c = 0;
            if (!cb) { cb = ZMBA.IsNothing; }
            if (len > 0) { for (var i = 0; i < len; i++) { if (cb(arr[i], i, arr)) { res[c] = arr[i]; c++; } } }
            res.length = c;
            return res;
         },
         Find: function Find(arr, cb) {
            if (arr) { for (var i = 0, len = arr.length >>> 0; i < len; i++) { if (!cb(arr[i], i, arr)) { return arr[i]; } } }
            return undefined;
         },
         FindIndex: function FindIndex(arr, cb) {
            if (arr) { for (var i = 0, len = arr.length >>> 0; i < len; i++) { if (!cb(arr[i], i, arr)) { return i; }; }; }
            return -1;
         },
         Includes: (function () {
            const includesNaN = (arr, fromIndex) => { for (var i = fromIndex | 0, len = arr.length >>> 0; i < len; i++) { if (typeof arr[i] === 'number' && isNaN(arr[i])) { return true; } } return false; } //edge case
            const includesNormal = (arr, el, fromIndex) => { for (var i = fromIndex | 0, len = arr.length >>> 0; i < len; i++) { if (arr[i] === el) { return true; }; }; return false; }
            return function Includes(arr, el, fromIndex) { return arr && typeof el === 'number' && isNaN(el) ? includesNaN(arr, fromIndex) : includesNormal(arr, el, fromIndex); }
         }()),
         Map: function Map(arr, cb) {
            var len = arr ? arr.length >>> 0 : 0;
            var res = new Array(len);
            if (len > 0) { for (var i = 0; i < len; i++) { res[i] = cb(arr[i], i, arr); } }
            return res;
         }
      }, { override: true, isArray: { override: false } });


      extendType([NodeList.prototype, HTMLCollection.prototype, Array.prototype], { //both Polyfills and extensions
         every: function (cb, thisArg) { return Array.Every(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         forEach: function (cb, thisArg) { return Array.ForEach(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         find: function (cb, thisArg) { return Array.Find(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         findIndex: function (cb, thisArg) { return Array.FindIndex(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         filter: function (cb, thisArg) { return Array.Filter(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         includes: function (searchElement, fromIndex) { return Array.Includes(this, searchElement, fromIndex); },
         map: function (cb, thisArg) { return Array.Map(this, !thisArg ? cb : (el, i, arr) => cb.call(thisArg, el, i, arr)); },
         Last: function (value) { if (arguments.length > 0) { this[Math.max(0, this.length - 1)] = arguments[0]; } return this.length > 0 ? this[this.length - 1] : undefined; },
         First: function (value) { if (arguments.length > 0) { this[0] = arguments[0]; } return this.length > 0 ? this[0] : undefined; }
      }, { override: false });


      extendType([NodeList.prototype, HTMLCollection.prototype], {
         ToArray: function () {
            var len = this.length, args = new Array(len); for (var i = 0; i < len; i++) { args[i] = this[i]; }
            return args;
         },
         Remove: (function () {
            const matches = Element.prototype.matches;
            function Remove(selector) {
               var i = 0, len = 0;
               if (selector == null) {
                  for (i = 0, len = this.length; i < len; i++) {
                     this[i].remove();
                  }
               } else if (typeof selector === 'string') {
                  for (i = 0, len = this.length; i < len; i++) {
                     if (matches.call(this[i], selector)) { this[i].remove(); }
                  }
               } else if (selector instanceof Array) {
                  for (i = 0, len = selector.length; i < len; i++) {
                     Remove.call(this, selector[i]);
                  }
               }
            }
            return Remove;
         }()),
         CSS: function (style) {
            if (style) {
               var keys = Object.getOwnPropertyNames(style);
               for (var i = 0, len = this.length; i < len; i++) {
                  for (var k = 0, klen = keys.length; k < klen; k++) {
                     var key = keys[k], value = style[key];
                     if (typeof value === 'number') { value = value + 'px'; };
                     this[i].style[key] = value;
                  }
               }
            }
         }
      });


   }))