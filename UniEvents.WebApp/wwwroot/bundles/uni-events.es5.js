"use strict";
(function (window, $, polyfills, factory) {
    polyfills(window, $);
    factory(window);
}(window, window.jQuery, function Polyfills(window, $) {
    Object.assign = Object.assign || $.extend;
}, function ZMBAFactory(window) {
    function extendBuiltInType(proto, obj, enumerable) {
        var descriptors = Object.getOwnPropertyDescriptors(obj);
        for (var desc in descriptors) {
            var prop = descriptors[desc];
            prop.enumerable = enumerable ? true : false;
            Object.defineProperty(proto, desc, prop);
        }
    }
    (function StringPrototypeExtensions() {
        extendBuiltInType(String.prototype, {
            ReplaceAll: function ReplaceAll(sequence, value) {
                return this.split(sequence).join(value);
            },
            TrimStart: function trimStart(ch) {
                var str = this;
                if (ch == null || !ch.length) {
                    ch = ' ';
                }
                var len = ch.length;
                while (str.startsWith(ch)) {
                    str = str.substr(len);
                }
                return str;
            },
            TrimEnd: function trimEnd(ch) {
                var str = this;
                if (ch == null || !ch.length) {
                    ch = ' ';
                }
                var len = ch.length;
                while (str.endsWith(ch)) {
                    str = str.substr(0, str.length - len);
                }
                return str;
            },
            SubstrBefore: function substrBefore(sequence, bIncludeSequence) {
                var idx = this.indexOf(sequence);
                if (idx >= 0) {
                    if (bIncludeSequence) {
                        idx += sequence.length;
                    }
                    if (idx <= this.length) {
                        return this.substr(0, idx);
                    }
                }
                return this;
            },
            SubstrAfter: function substrAfter(sequence, bIncludeSequence) {
                var idx = this.indexOf(sequence);
                if (idx >= 0) {
                    if (!bIncludeSequence) {
                        idx += sequence.length;
                    }
                    if (idx <= this.length) {
                        return this.substr(idx);
                    }
                }
                return this;
            },
            SubstrBeforeLast: function substrBeforeLast(sequence, bIncludeSequence) {
                var idx = this.lastIndexOf(sequence);
                if (idx >= 0) {
                    if (bIncludeSequence) {
                        idx += sequence.length;
                    }
                    if (idx <= this.length) {
                        return this.substr(0, idx);
                    }
                }
                return this;
            },
            SubstrAfterLast: function substrAfterLast(sequence, bIncludeSequence) {
                var idx = this.lastIndexOf(sequence);
                if (idx >= 0) {
                    if (!bIncludeSequence) {
                        idx += sequence.length;
                    }
                    if (idx <= this.length) {
                        return this.substr(idx);
                    }
                }
                return this;
            }
        });
    }());
    (function ElementExtensions() {
        extendBuiltInType(Element, {
            From: (function (doc, rgx) {
                return function CreateElementFromHTML(html) {
                    html = html.trim();
                    var bodystart = html.indexOf('>') + 1, bodyend = html.lastIndexOf('<');
                    var elemStart = html.substr(0, bodystart);
                    var innerHTML = html.substr(bodystart, bodyend - bodystart);
                    rgx.lastIndex = 0;
                    var elem = doc.createElement(rgx.exec(elemStart)[4]);
                    var match;
                    while ((match = rgx.exec(elemStart))) {
                        if (match[1] === undefined) {
                            elem.setAttribute(match[4], "");
                        }
                        else {
                            elem.setAttribute(match[1], match[3]);
                        }
                    }
                    elem.innerHTML = innerHTML;
                    return elem;
                };
            }(window.document, /(\S+)=(["'])(.*?)(?:\2)|(\w+)/g))
        });
    }());
    (function DOMTokenListExtensions() {
        extendBuiltInType(DOMTokenList.prototype, {
            ToggleMultiple: function ToggleMultiple() {
                var len = arguments.length - 1;
                var bHasForce = typeof arguments[len - 1] === 'boolean';
                var bResult;
                for (var i = 0; i < len; i++) {
                    bResult = bHasForce ? this.toggle(arguments[i], arguments[len]) : this.toggle(arguments[i]);
                }
                return bResult;
            }
        });
    }());
    (function RegexPrototypeExtensions() {
        extendBuiltInType(RegExp.prototype, {
            GetMatches: function GetMatches(str) {
                this.lastIndex = 0;
                var matches = [], match = null;
                while ((match = this.exec(str))) {
                    matches.push(match);
                }
                return matches;
            }
        });
    }());
    (function CollectionExtensions() {
        var descriptor = {
            get Last() { return this[this.length - 1]; },
            set Last(value) { this[this.length - 1] = value; }
        };
        extendBuiltInType(NodeList.prototype, descriptor);
        extendBuiltInType(HTMLCollection.prototype, descriptor);
        extendBuiltInType(Array.prototype, descriptor);
        Array.forEach = function (arr, cb) {
            if (!arr) {
                return;
            }
            for (var i = 0, len = arr.length; i < len; i++) {
                cb(arr[i], i);
            }
            ;
        };
    }());
    (function NodeListExtensions() {
        var matches = Element.prototype.matches;
        var descriptor = {
            ToArray: function () {
                var len = this.length, args = new Array(len);
                for (var i = 0; i < len; i++) {
                    args[i] = this[i];
                }
                return args;
            },
            Remove: function (selector) {
                var i = 0, len = 0;
                if (selector == null) {
                    for (i = 0, len = this.length; i < len; i++) {
                        this[i].remove();
                    }
                }
                else if (typeof selector === 'string') {
                    for (i = 0, len = this.length; i < len; i++) {
                        if (matches.call(this[i], selector)) {
                            this[i].remove();
                        }
                    }
                }
                else if (selector instanceof Array) {
                    for (i = 0, len = selector.length; i < len; i++) {
                        NodeList.prototype.Remove.call(this, selector[i]);
                    }
                }
            },
            CSS: function (style) {
                if (style) {
                    var keys = Object.getOwnPropertyNames(style);
                    for (var i = 0, len = this.length; i < len; i++) {
                        for (var k = 0, klen = keys.length; k < klen; k++) {
                            var key = keys[k], value = style[key];
                            if (typeof value === 'number') {
                                value = value + 'px';
                            }
                            this[i].style[key] = value;
                        }
                    }
                }
            }
        };
        extendBuiltInType(NodeList.prototype, descriptor);
        extendBuiltInType(HTMLCollection.prototype, descriptor);
    }());
}));
(function (window, document, $, factory) {
    window.U = factory(window, document, $, factory);
}(window, window.document, window.jQuery, function UniEventFactory(window, document, $, factory) {
    var ReadyListener = (function () {
        function addCallback(cb) {
            var i = 1, len = arguments.length, args = new Array(len - i + 1);
            for (; i < len; i++) {
                args[i - 1] = arguments[i];
            }
            this._isReady ? cb.apply(null, args) : this._callbacks.push({ cb: cb, args: args });
        }
        function runQueued(ev) {
            while (this._callbacks.length > 0) {
                var ob = this._callbacks.shift();
                try {
                    ob.cb.apply(null, ob.args);
                }
                catch (ex) {
                    console.error(ex, ob);
                }
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
            set isReady(value) { if ((this._isReady = value)) {
                this.runQueued();
            } }
        });
        return ReadyListener;
    }());
    var onReadyListener = new ReadyListener("DOMContentLoaded");
    var onLoadedListener = new ReadyListener("load");
    function UniEvents() {
        this.onReady = onReadyListener.add;
        this.onLoaded = onLoadedListener.add;
    }
    return new UniEvents();
}));
