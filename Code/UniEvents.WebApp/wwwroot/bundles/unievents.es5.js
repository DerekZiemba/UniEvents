"use strict";
(function (factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory);
    }
    else if (typeof exports === 'object' && typeof require === 'function') {
        factory(require('jquery'));
    }
    else {
        factory(jQuery);
    }
}(function ($) {
    'use strict';
    var utils = (function () {
        return {
            escapeRegExChars: function (value) {
                return value.replace(/[|\\{}()[\]^$+*?.]/g, "\\$&");
            },
            createNode: function (containerClass) {
                var div = document.createElement('div');
                div.className = containerClass;
                div.style.position = 'absolute';
                div.style.display = 'none';
                return div;
            }
        };
    }()), keys = {
        ESC: 27,
        TAB: 9,
        RETURN: 13,
        LEFT: 37,
        UP: 38,
        RIGHT: 39,
        DOWN: 40
    }, noop = $.noop;
    function Autocomplete(el, options) {
        var that = this;
        that.element = el;
        that.el = $(el);
        that.suggestions = [];
        that.badQueries = [];
        that.selectedIndex = -1;
        that.currentValue = that.element.value;
        that.timeoutId = null;
        that.cachedResponse = {};
        that.onChangeTimeout = null;
        that.onChange = null;
        that.isLocal = false;
        that.suggestionsContainer = null;
        that.noSuggestionsContainer = null;
        that.options = $.extend({}, Autocomplete.defaults, options);
        that.classes = {
            selected: 'autocomplete-selected',
            suggestion: 'autocomplete-suggestion'
        };
        that.hint = null;
        that.hintValue = '';
        that.selection = null;
        that.initialize();
        that.setOptions(options);
    }
    Autocomplete.utils = utils;
    $.Autocomplete = Autocomplete;
    Autocomplete.defaults = {
        ajaxSettings: {},
        autoSelectFirst: false,
        appendTo: 'body',
        serviceUrl: null,
        lookup: null,
        onSelect: null,
        width: 'auto',
        minChars: 1,
        maxHeight: 300,
        deferRequestBy: 0,
        params: {},
        formatResult: _formatResult,
        formatGroup: _formatGroup,
        delimiter: null,
        zIndex: 9999,
        type: 'GET',
        noCache: false,
        onSearchStart: noop,
        onSearchComplete: noop,
        onSearchError: noop,
        preserveInput: false,
        containerClass: 'autocomplete-suggestions',
        tabDisabled: false,
        dataType: 'text',
        currentRequest: null,
        triggerSelectOnValidInput: true,
        preventBadQueries: true,
        lookupFilter: _lookupFilter,
        paramName: 'query',
        transformResult: _transformResult,
        showNoSuggestionNotice: false,
        noSuggestionNotice: 'No results',
        orientation: 'bottom',
        forceFixPosition: false
    };
    function _lookupFilter(suggestion, originalQuery, queryLowerCase) {
        return suggestion.value.toLowerCase().indexOf(queryLowerCase) !== -1;
    }
    ;
    function _transformResult(response) {
        return typeof response === 'string' ? $.parseJSON(response) : response;
    }
    ;
    function _formatResult(suggestion, currentValue) {
        if (!currentValue) {
            return suggestion.value;
        }
        var pattern = '(' + utils.escapeRegExChars(currentValue) + ')';
        return suggestion.value
            .replace(new RegExp(pattern, 'gi'), '<strong>$1<\/strong>')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/&lt;(\/?strong)&gt;/g, '<$1>');
    }
    ;
    function _formatGroup(suggestion, category) {
        return '<div class="autocomplete-group">' + category + '</div>';
    }
    ;
    Autocomplete.prototype = {
        initialize: function () {
            var that = this, suggestionSelector = '.' + that.classes.suggestion, selected = that.classes.selected, options = that.options, container;
            that.element.setAttribute('autocomplete', 'off');
            that.noSuggestionsContainer = $('<div class="autocomplete-no-suggestion"></div>')
                .html(this.options.noSuggestionNotice).get(0);
            that.suggestionsContainer = Autocomplete.utils.createNode(options.containerClass);
            container = $(that.suggestionsContainer);
            container.appendTo(options.appendTo || 'body');
            if (options.width !== 'auto') {
                container.css('width', options.width);
            }
            container.on('mouseover.autocomplete', suggestionSelector, function () {
                that.activate($(this).data('index'));
            });
            container.on('mouseout.autocomplete', function () {
                that.selectedIndex = -1;
                container.children('.' + selected).removeClass(selected);
            });
            container.on('click.autocomplete', suggestionSelector, function () {
                that.select($(this).data('index'));
            });
            container.on('click.autocomplete', function () {
                clearTimeout(that.blurTimeoutId);
            });
            that.fixPositionCapture = function () {
                if (that.visible) {
                    that.fixPosition();
                }
            };
            $(window).on('resize.autocomplete', that.fixPositionCapture);
            that.el.on('keydown.autocomplete', function (e) { that.onKeyPress(e); });
            that.el.on('keyup.autocomplete', function (e) { that.onKeyUp(e); });
            that.el.on('blur.autocomplete', function () { that.onBlur(); });
            that.el.on('focus.autocomplete', function () { that.onFocus(); });
            that.el.on('change.autocomplete', function (e) { that.onKeyUp(e); });
            that.el.on('input.autocomplete', function (e) { that.onKeyUp(e); });
        },
        onFocus: function () {
            var that = this;
            that.fixPosition();
            if (that.el.val().length >= that.options.minChars) {
                that.onValueChange();
            }
        },
        onBlur: function () {
            var that = this;
            that.blurTimeoutId = setTimeout(function () {
                that.hide();
            }, 200);
        },
        abortAjax: function () {
            var that = this;
            if (that.currentRequest) {
                that.currentRequest.abort();
                that.currentRequest = null;
            }
        },
        setOptions: function (suppliedOptions) {
            var that = this, options = $.extend({}, that.options, suppliedOptions);
            that.isLocal = Array.isArray(options.lookup);
            if (that.isLocal) {
                options.lookup = that.verifySuggestionsFormat(options.lookup);
            }
            options.orientation = that.validateOrientation(options.orientation, 'bottom');
            $(that.suggestionsContainer).css({
                'max-height': options.maxHeight + 'px',
                'width': options.width + 'px',
                'z-index': options.zIndex
            });
            this.options = options;
        },
        clearCache: function () {
            this.cachedResponse = {};
            this.badQueries = [];
        },
        clear: function () {
            this.clearCache();
            this.currentValue = '';
            this.suggestions = [];
        },
        disable: function () {
            var that = this;
            that.disabled = true;
            clearTimeout(that.onChangeTimeout);
            that.abortAjax();
        },
        enable: function () {
            this.disabled = false;
        },
        fixPosition: function () {
            var that = this, $container = $(that.suggestionsContainer), containerParent = $container.parent().get(0);
            if (containerParent !== document.body && !that.options.forceFixPosition) {
                return;
            }
            var orientation = that.options.orientation, containerHeight = $container.outerHeight(), height = that.el.outerHeight(), offset = that.el.offset(), styles = { 'top': offset.top, 'left': offset.left };
            if (orientation === 'auto') {
                var viewPortHeight = $(window).height(), scrollTop = $(window).scrollTop(), topOverflow = -scrollTop + offset.top - containerHeight, bottomOverflow = scrollTop + viewPortHeight - (offset.top + height + containerHeight);
                orientation = (Math.max(topOverflow, bottomOverflow) === topOverflow) ? 'top' : 'bottom';
            }
            if (orientation === 'top') {
                styles.top += -containerHeight;
            }
            else {
                styles.top += height;
            }
            if (containerParent !== document.body) {
                var opacity = $container.css('opacity'), parentOffsetDiff;
                if (!that.visible) {
                    $container.css('opacity', 0).show();
                }
                parentOffsetDiff = $container.offsetParent().offset();
                styles.top -= parentOffsetDiff.top;
                styles.top += containerParent.scrollTop;
                styles.left -= parentOffsetDiff.left;
                if (!that.visible) {
                    $container.css('opacity', opacity).hide();
                }
            }
            if (that.options.width === 'auto') {
                styles.width = that.el.outerWidth() + 'px';
            }
            $container.css(styles);
        },
        isCursorAtEnd: function () {
            var that = this, valLength = that.el.val().length, selectionStart = that.element.selectionStart, range;
            if (typeof selectionStart === 'number') {
                return selectionStart === valLength;
            }
            if (document.selection) {
                range = document.selection.createRange();
                range.moveStart('character', -valLength);
                return valLength === range.text.length;
            }
            return true;
        },
        onKeyPress: function (e) {
            var that = this;
            if (!that.disabled && !that.visible && e.which === keys.DOWN && that.currentValue) {
                that.suggest();
                return;
            }
            if (that.disabled || !that.visible) {
                return;
            }
            switch (e.which) {
                case keys.ESC:
                    that.el.val(that.currentValue);
                    that.hide();
                    break;
                case keys.RIGHT:
                    if (that.hint && that.options.onHint && that.isCursorAtEnd()) {
                        that.selectHint();
                        break;
                    }
                    return;
                case keys.TAB:
                    if (that.hint && that.options.onHint) {
                        that.selectHint();
                        return;
                    }
                    if (that.selectedIndex === -1) {
                        that.hide();
                        return;
                    }
                    that.select(that.selectedIndex);
                    if (that.options.tabDisabled === false) {
                        return;
                    }
                    break;
                case keys.RETURN:
                    if (that.selectedIndex === -1) {
                        that.hide();
                        return;
                    }
                    that.select(that.selectedIndex);
                    break;
                case keys.UP:
                    that.moveUp();
                    break;
                case keys.DOWN:
                    that.moveDown();
                    break;
                default:
                    return;
            }
            e.stopImmediatePropagation();
            e.preventDefault();
        },
        onKeyUp: function (e) {
            var that = this;
            if (that.disabled) {
                return;
            }
            switch (e.which) {
                case keys.UP:
                case keys.DOWN:
                    return;
            }
            clearTimeout(that.onChangeTimeout);
            if (that.currentValue !== that.el.val()) {
                that.findBestHint();
                if (that.options.deferRequestBy > 0) {
                    that.onChangeTimeout = setTimeout(function () {
                        that.onValueChange();
                    }, that.options.deferRequestBy);
                }
                else {
                    that.onValueChange();
                }
            }
        },
        onValueChange: function () {
            if (this.ignoreValueChange) {
                this.ignoreValueChange = false;
                return;
            }
            var that = this, options = that.options, value = that.el.val(), query = that.getQuery(value);
            if (that.selection && that.currentValue !== query) {
                that.selection = null;
                (options.onInvalidateSelection || $.noop).call(that.element);
            }
            clearTimeout(that.onChangeTimeout);
            that.currentValue = value;
            that.selectedIndex = -1;
            if (options.triggerSelectOnValidInput && that.isExactMatch(query)) {
                that.select(0);
                return;
            }
            if (query.length < options.minChars) {
                that.hide();
            }
            else {
                that.getSuggestions(query);
            }
        },
        isExactMatch: function (query) {
            var suggestions = this.suggestions;
            return (suggestions.length === 1 && suggestions[0].value.toLowerCase() === query.toLowerCase());
        },
        getQuery: function (value) {
            var delimiter = this.options.delimiter, parts;
            if (!delimiter) {
                return value;
            }
            parts = value.split(delimiter);
            return $.trim(parts[parts.length - 1]);
        },
        getSuggestionsLocal: function (query) {
            var that = this, options = that.options, queryLowerCase = query.toLowerCase(), filter = options.lookupFilter, limit = parseInt(options.lookupLimit, 10), data;
            data = {
                suggestions: $.grep(options.lookup, function (suggestion) {
                    return filter(suggestion, query, queryLowerCase);
                })
            };
            if (limit && data.suggestions.length > limit) {
                data.suggestions = data.suggestions.slice(0, limit);
            }
            return data;
        },
        getSuggestions: function (q) {
            var response, that = this, options = that.options, serviceUrl = options.serviceUrl, params, cacheKey, ajaxSettings;
            options.params[options.paramName] = q;
            if (options.onSearchStart.call(that.element, options.params) === false) {
                return;
            }
            params = options.ignoreParams ? null : options.params;
            if ($.isFunction(options.lookup)) {
                options.lookup(q, function (data) {
                    that.suggestions = data.suggestions;
                    that.suggest();
                    options.onSearchComplete.call(that.element, q, data.suggestions);
                });
                return;
            }
            if (that.isLocal) {
                response = that.getSuggestionsLocal(q);
            }
            else {
                if ($.isFunction(serviceUrl)) {
                    serviceUrl = serviceUrl.call(that.element, q);
                }
                cacheKey = serviceUrl + '?' + $.param(params || {});
                response = that.cachedResponse[cacheKey];
            }
            if (response && Array.isArray(response.suggestions)) {
                that.suggestions = response.suggestions;
                that.suggest();
                options.onSearchComplete.call(that.element, q, response.suggestions);
            }
            else if (!that.isBadQuery(q)) {
                that.abortAjax();
                ajaxSettings = {
                    url: serviceUrl,
                    data: params,
                    type: options.type,
                    dataType: options.dataType
                };
                $.extend(ajaxSettings, options.ajaxSettings);
                that.currentRequest = $.ajax(ajaxSettings).done(function (data) {
                    var result;
                    that.currentRequest = null;
                    result = options.transformResult(data, q);
                    that.processResponse(result, q, cacheKey);
                    options.onSearchComplete.call(that.element, q, result.suggestions);
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    options.onSearchError.call(that.element, q, jqXHR, textStatus, errorThrown);
                });
            }
            else {
                options.onSearchComplete.call(that.element, q, []);
            }
        },
        isBadQuery: function (q) {
            if (!this.options.preventBadQueries) {
                return false;
            }
            var badQueries = this.badQueries, i = badQueries.length;
            while (i--) {
                if (q.indexOf(badQueries[i]) === 0) {
                    return true;
                }
            }
            return false;
        },
        hide: function () {
            var that = this, container = $(that.suggestionsContainer);
            if ($.isFunction(that.options.onHide) && that.visible) {
                that.options.onHide.call(that.element, container);
            }
            that.visible = false;
            that.selectedIndex = -1;
            clearTimeout(that.onChangeTimeout);
            $(that.suggestionsContainer).hide();
            that.signalHint(null);
        },
        suggest: function () {
            if (!this.suggestions.length) {
                if (this.options.showNoSuggestionNotice) {
                    this.noSuggestions();
                }
                else {
                    this.hide();
                }
                return;
            }
            var that = this, options = that.options, groupBy = options.groupBy, formatResult = options.formatResult, value = that.getQuery(that.currentValue), className = that.classes.suggestion, classSelected = that.classes.selected, container = $(that.suggestionsContainer), noSuggestionsContainer = $(that.noSuggestionsContainer), beforeRender = options.beforeRender, html = '', category, formatGroup = function (suggestion, index) {
                var currentCategory = suggestion.data[groupBy];
                if (category === currentCategory) {
                    return '';
                }
                category = currentCategory;
                return options.formatGroup(suggestion, category);
            };
            if (options.triggerSelectOnValidInput && that.isExactMatch(value)) {
                that.select(0);
                return;
            }
            $.each(that.suggestions, function (i, suggestion) {
                if (groupBy) {
                    html += formatGroup(suggestion, value, i);
                }
                html += '<div class="' + className + '" data-index="' + i + '">' + formatResult(suggestion, value, i) + '</div>';
            });
            this.adjustContainerWidth();
            noSuggestionsContainer.detach();
            container.html(html);
            if ($.isFunction(beforeRender)) {
                beforeRender.call(that.element, container, that.suggestions);
            }
            that.fixPosition();
            container.show();
            if (options.autoSelectFirst) {
                that.selectedIndex = 0;
                container.scrollTop(0);
                container.children('.' + className).first().addClass(classSelected);
            }
            that.visible = true;
            that.findBestHint();
        },
        noSuggestions: function () {
            var that = this, beforeRender = that.options.beforeRender, container = $(that.suggestionsContainer), noSuggestionsContainer = $(that.noSuggestionsContainer);
            this.adjustContainerWidth();
            noSuggestionsContainer.detach();
            container.empty();
            container.append(noSuggestionsContainer);
            if ($.isFunction(beforeRender)) {
                beforeRender.call(that.element, container, that.suggestions);
            }
            that.fixPosition();
            container.show();
            that.visible = true;
        },
        adjustContainerWidth: function () {
            var that = this, options = that.options, width, container = $(that.suggestionsContainer);
            if (options.width === 'auto') {
                width = that.el.outerWidth();
                container.css('width', width > 0 ? width : 300);
            }
            else if (options.width === 'flex') {
                container.css('width', '');
            }
        },
        findBestHint: function () {
            var that = this, value = that.el.val().toLowerCase(), bestMatch = null;
            if (!value) {
                return;
            }
            $.each(that.suggestions, function (i, suggestion) {
                var foundMatch = suggestion.value.toLowerCase().indexOf(value) === 0;
                if (foundMatch) {
                    bestMatch = suggestion;
                }
                return !foundMatch;
            });
            that.signalHint(bestMatch);
        },
        signalHint: function (suggestion) {
            var hintValue = '', that = this;
            if (suggestion) {
                hintValue = that.currentValue + suggestion.value.substr(that.currentValue.length);
            }
            if (that.hintValue !== hintValue) {
                that.hintValue = hintValue;
                that.hint = suggestion;
                (this.options.onHint || $.noop)(hintValue);
            }
        },
        verifySuggestionsFormat: function (suggestions) {
            if (suggestions.length && typeof suggestions[0] === 'string') {
                return $.map(suggestions, function (value) {
                    return { value: value, data: null };
                });
            }
            return suggestions;
        },
        validateOrientation: function (orientation, fallback) {
            orientation = $.trim(orientation || '').toLowerCase();
            if ($.inArray(orientation, ['auto', 'bottom', 'top']) === -1) {
                orientation = fallback;
            }
            return orientation;
        },
        processResponse: function (result, originalQuery, cacheKey) {
            var that = this, options = that.options;
            result.suggestions = that.verifySuggestionsFormat(result.suggestions);
            if (!options.noCache) {
                that.cachedResponse[cacheKey] = result;
                if (options.preventBadQueries && !result.suggestions.length) {
                    that.badQueries.push(originalQuery);
                }
            }
            if (originalQuery !== that.getQuery(that.currentValue)) {
                return;
            }
            that.suggestions = result.suggestions;
            that.suggest();
        },
        activate: function (index) {
            var that = this, activeItem, selected = that.classes.selected, container = $(that.suggestionsContainer), children = container.find('.' + that.classes.suggestion);
            container.find('.' + selected).removeClass(selected);
            that.selectedIndex = index;
            if (that.selectedIndex !== -1 && children.length > that.selectedIndex) {
                activeItem = children.get(that.selectedIndex);
                $(activeItem).addClass(selected);
                return activeItem;
            }
            return null;
        },
        selectHint: function () {
            var that = this, i = $.inArray(that.hint, that.suggestions);
            that.select(i);
        },
        select: function (i) {
            var that = this;
            that.hide();
            that.onSelect(i);
        },
        moveUp: function () {
            var that = this;
            if (that.selectedIndex === -1) {
                return;
            }
            if (that.selectedIndex === 0) {
                $(that.suggestionsContainer).children('.' + that.classes.suggestion).first().removeClass(that.classes.selected);
                that.selectedIndex = -1;
                that.ignoreValueChange = false;
                that.el.val(that.currentValue);
                that.findBestHint();
                return;
            }
            that.adjustScroll(that.selectedIndex - 1);
        },
        moveDown: function () {
            var that = this;
            if (that.selectedIndex === (that.suggestions.length - 1)) {
                return;
            }
            that.adjustScroll(that.selectedIndex + 1);
        },
        adjustScroll: function (index) {
            var that = this, activeItem = that.activate(index);
            if (!activeItem) {
                return;
            }
            var offsetTop, upperBound, lowerBound, heightDelta = $(activeItem).outerHeight();
            offsetTop = activeItem.offsetTop;
            upperBound = $(that.suggestionsContainer).scrollTop();
            lowerBound = upperBound + that.options.maxHeight - heightDelta;
            if (offsetTop < upperBound) {
                $(that.suggestionsContainer).scrollTop(offsetTop);
            }
            else if (offsetTop > lowerBound) {
                $(that.suggestionsContainer).scrollTop(offsetTop - that.options.maxHeight + heightDelta);
            }
            if (!that.options.preserveInput) {
                that.ignoreValueChange = true;
                that.el.val(that.getValue(that.suggestions[index].value));
            }
            that.signalHint(null);
        },
        onSelect: function (index) {
            var that = this, onSelectCallback = that.options.onSelect, suggestion = that.suggestions[index];
            that.currentValue = that.getValue(suggestion.value);
            if (that.currentValue !== that.el.val() && !that.options.preserveInput) {
                that.el.val(that.currentValue);
            }
            that.signalHint(null);
            that.suggestions = [];
            that.selection = suggestion;
            if ($.isFunction(onSelectCallback)) {
                onSelectCallback.call(that.element, suggestion);
            }
        },
        getValue: function (value) {
            var that = this, delimiter = that.options.delimiter, currentValue, parts;
            if (!delimiter) {
                return value;
            }
            currentValue = that.currentValue;
            parts = currentValue.split(delimiter);
            if (parts.length === 1) {
                return value;
            }
            return currentValue.substr(0, currentValue.length - parts[parts.length - 1].length) + value;
        },
        dispose: function () {
            var that = this;
            that.el.off('.autocomplete').removeData('autocomplete');
            $(window).off('resize.autocomplete', that.fixPositionCapture);
            $(that.suggestionsContainer).remove();
        }
    };
    $.fn.devbridgeAutocomplete = function (options, args) {
        var dataKey = 'autocomplete';
        if (!arguments.length) {
            return this.first().data(dataKey);
        }
        return this.each(function () {
            var inputElement = $(this), instance = inputElement.data(dataKey);
            if (typeof options === 'string') {
                if (instance && typeof instance[options] === 'function') {
                    instance[options](args);
                }
            }
            else {
                if (instance && instance.dispose) {
                    instance.dispose();
                }
                instance = new Autocomplete(this, options);
                inputElement.data(dataKey, instance);
            }
        });
    };
    if (!$.fn.autocomplete) {
        $.fn.autocomplete = $.fn.devbridgeAutocomplete;
    }
}));
!function (e, t) { "object" == typeof exports && "undefined" != typeof module ? module.exports = t() : "function" == typeof define && define.amd ? define(t) : e.flatpickr = t(); }(this, function () {
    "use strict";
    var $ = function (e) { return ("0" + e).slice(-2); }, q = function (e) { return !0 === e ? 1 : 0; };
    function z(n, a, i) { var o; return void 0 === i && (i = !1), function () { var e = this, t = arguments; null !== o && clearTimeout(o), o = window.setTimeout(function () { o = null, i || n.apply(e, t); }, a), i && !o && n.apply(e, t); }; }
    var G = function (e) { return e instanceof Array ? e : [e]; }, e = function () { }, V = function (e, t, n) { return n.months[t ? "shorthand" : "longhand"][e]; }, D = { D: e, F: function (e, t, n) { e.setMonth(n.months.longhand.indexOf(t)); }, G: function (e, t) { e.setHours(parseFloat(t)); }, H: function (e, t) { e.setHours(parseFloat(t)); }, J: function (e, t) { e.setDate(parseFloat(t)); }, K: function (e, t, n) { e.setHours(e.getHours() % 12 + 12 * q(new RegExp(n.amPM[1], "i").test(t))); }, M: function (e, t, n) { e.setMonth(n.months.shorthand.indexOf(t)); }, S: function (e, t) { e.setSeconds(parseFloat(t)); }, U: function (e, t) { return new Date(1e3 * parseFloat(t)); }, W: function (e, t) { var n = parseInt(t); return new Date(e.getFullYear(), 0, 2 + 7 * (n - 1), 0, 0, 0, 0); }, Y: function (e, t) { e.setFullYear(parseFloat(t)); }, Z: function (e, t) { return new Date(t); }, d: function (e, t) { e.setDate(parseFloat(t)); }, h: function (e, t) { e.setHours(parseFloat(t)); }, i: function (e, t) { e.setMinutes(parseFloat(t)); }, j: function (e, t) { e.setDate(parseFloat(t)); }, l: e, m: function (e, t) { e.setMonth(parseFloat(t) - 1); }, n: function (e, t) { e.setMonth(parseFloat(t) - 1); }, s: function (e, t) { e.setSeconds(parseFloat(t)); }, w: e, y: function (e, t) { e.setFullYear(2e3 + parseFloat(t)); } }, Z = { D: "(\\w+)", F: "(\\w+)", G: "(\\d\\d|\\d)", H: "(\\d\\d|\\d)", J: "(\\d\\d|\\d)\\w+", K: "", M: "(\\w+)", S: "(\\d\\d|\\d)", U: "(.+)", W: "(\\d\\d|\\d)", Y: "(\\d{4})", Z: "(.+)", d: "(\\d\\d|\\d)", h: "(\\d\\d|\\d)", i: "(\\d\\d|\\d)", j: "(\\d\\d|\\d)", l: "(\\w+)", m: "(\\d\\d|\\d)", n: "(\\d\\d|\\d)", s: "(\\d\\d|\\d)", w: "(\\d\\d|\\d)", y: "(\\d{2})" }, l = { Z: function (e) { return e.toISOString(); }, D: function (e, t, n) { return t.weekdays.shorthand[l.w(e, t, n)]; }, F: function (e, t, n) { return V(l.n(e, t, n) - 1, !1, t); }, G: function (e, t, n) { return $(l.h(e, t, n)); }, H: function (e) { return $(e.getHours()); }, J: function (e, t) { return void 0 !== t.ordinal ? e.getDate() + t.ordinal(e.getDate()) : e.getDate(); }, K: function (e, t) { return t.amPM[q(11 < e.getHours())]; }, M: function (e, t) { return V(e.getMonth(), !0, t); }, S: function (e) { return $(e.getSeconds()); }, U: function (e) { return e.getTime() / 1e3; }, W: function (e, t, n) { return n.getWeek(e); }, Y: function (e) { return e.getFullYear(); }, d: function (e) { return $(e.getDate()); }, h: function (e) { return e.getHours() % 12 ? e.getHours() % 12 : 12; }, i: function (e) { return $(e.getMinutes()); }, j: function (e) { return e.getDate(); }, l: function (e, t) { return t.weekdays.longhand[e.getDay()]; }, m: function (e) { return $(e.getMonth() + 1); }, n: function (e) { return e.getMonth() + 1; }, s: function (e) { return e.getSeconds(); }, w: function (e) { return e.getDay(); }, y: function (e) { return String(e.getFullYear()).substring(2); } }, Q = { weekdays: { shorthand: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"], longhand: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"] }, months: { shorthand: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"], longhand: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"] }, daysInMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31], firstDayOfWeek: 0, ordinal: function (e) { var t = e % 100; if (3 < t && t < 21)
            return "th"; switch (t % 10) {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        } }, rangeSeparator: " to ", weekAbbreviation: "Wk", scrollTitle: "Scroll to increment", toggleTitle: "Click to toggle", amPM: ["AM", "PM"], yearAriaLabel: "Year" }, t = function (e) { var t = e.config, o = void 0 === t ? b : t, n = e.l10n, r = void 0 === n ? Q : n; return function (a, e, t) { if (void 0 !== o.formatDate)
        return o.formatDate(a, e); var i = t || r; return e.split("").map(function (e, t, n) { return l[e] && "\\" !== n[t - 1] ? l[e](a, i, o) : "\\" !== e ? e : ""; }).join(""); }; }, X = function (e) { var t = e.config, h = void 0 === t ? b : t, n = e.l10n, v = void 0 === n ? Q : n; return function (e, t, n) { if (0 === e || e) {
        var a, i = e;
        if (e instanceof Date)
            a = new Date(e.getTime());
        else if ("string" != typeof e && void 0 !== e.toFixed)
            a = new Date(e);
        else if ("string" == typeof e) {
            var o = t || (h || b).dateFormat, r = String(e).trim();
            if ("today" === r)
                a = new Date, n = !0;
            else if (/Z$/.test(r) || /GMT$/.test(r))
                a = new Date(e);
            else if (h && h.parseDate)
                a = h.parseDate(e, o);
            else {
                a = h && h.noCalendar ? new Date((new Date).setHours(0, 0, 0, 0)) : new Date((new Date).getFullYear(), 0, 1, 0, 0, 0, 0);
                for (var l, c = [], d = 0, s = 0, u = ""; d < o.length; d++) {
                    var f = o[d], m = "\\" === f, g = "\\" === o[d - 1] || m;
                    if (Z[f] && !g) {
                        u += Z[f];
                        var p = new RegExp(u).exec(e);
                        p && (l = !0) && c["Y" !== f ? "push" : "unshift"]({ fn: D[f], val: p[++s] });
                    }
                    else
                        m || (u += ".");
                    c.forEach(function (e) { var t = e.fn, n = e.val; return a = t(a, n, v) || a; });
                }
                a = l ? a : void 0;
            }
        }
        if (a instanceof Date)
            return !0 === n && a.setHours(0, 0, 0, 0), a;
        h.errorHandler(new Error("Invalid date provided: " + i));
    } }; };
    function ee(e, t, n) { return void 0 === n && (n = !0), !1 !== n ? new Date(e.getTime()).setHours(0, 0, 0, 0) - new Date(t.getTime()).setHours(0, 0, 0, 0) : e.getTime() - t.getTime(); }
    var te = function (e, t, n) { return e > Math.min(t, n) && e < Math.max(t, n); }, ne = { DAY: 864e5 }, b = { _disable: [], _enable: [], allowInput: !1, altFormat: "F j, Y", altInput: !1, altInputClass: "form-control input", animate: "object" == typeof window && -1 === window.navigator.userAgent.indexOf("MSIE"), ariaDateFormat: "F j, Y", clickOpens: !0, closeOnSelect: !0, conjunction: ", ", dateFormat: "Y-m-d", defaultHour: 12, defaultMinute: 0, defaultSeconds: 0, disable: [], disableMobile: !1, enable: [], enableSeconds: !1, enableTime: !1, errorHandler: console.warn, getWeek: function (e) { var t = new Date(e.getTime()); t.setHours(0, 0, 0, 0), t.setDate(t.getDate() + 3 - (t.getDay() + 6) % 7); var n = new Date(t.getFullYear(), 0, 4); return 1 + Math.round(((t.getTime() - n.getTime()) / 864e5 - 3 + (n.getDay() + 6) % 7) / 7); }, hourIncrement: 1, ignoredFocusElements: [], inline: !1, locale: "default", minuteIncrement: 5, mode: "single", nextArrow: "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 17 17'><g></g><path d='M13.207 8.472l-7.854 7.854-0.707-0.707 7.146-7.146-7.146-7.148 0.707-0.707 7.854 7.854z' /></svg>", noCalendar: !1, now: new Date, onChange: [], onClose: [], onDayCreate: [], onDestroy: [], onKeyDown: [], onMonthChange: [], onOpen: [], onParseConfig: [], onReady: [], onValueUpdate: [], onYearChange: [], onPreCalendarPosition: [], plugins: [], position: "auto", positionElement: void 0, prevArrow: "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 17 17'><g></g><path d='M5.207 8.471l7.146 7.147-0.707 0.707-7.853-7.854 7.854-7.853 0.707 0.707-7.147 7.146z' /></svg>", shorthandCurrentMonth: !1, showMonths: 1, static: !1, time_24hr: !1, weekNumbers: !1, wrap: !1 };
    function ae(e, t, n) { if (!0 === n)
        return e.classList.add(t); e.classList.remove(t); }
    function ie(e, t, n) { var a = window.document.createElement(e); return t = t || "", n = n || "", a.className = t, void 0 !== n && (a.textContent = n), a; }
    function oe(e) { for (; e.firstChild;)
        e.removeChild(e.firstChild); }
    function re(e, t) { var n = ie("div", "numInputWrapper"), a = ie("input", "numInput " + e), i = ie("span", "arrowUp"), o = ie("span", "arrowDown"); if (a.type = "text", a.pattern = "\\d*", void 0 !== t)
        for (var r in t)
            a.setAttribute(r, t[r]); return n.appendChild(a), n.appendChild(i), n.appendChild(o), n; }
    "function" != typeof Object.assign && (Object.assign = function (n) { if (!n)
        throw TypeError("Cannot convert undefined or null to object"); for (var e = arguments.length, t = new Array(1 < e ? e - 1 : 0), a = 1; a < e; a++)
        t[a - 1] = arguments[a]; for (var i = function (t) { t && Object.keys(t).forEach(function (e) { return n[e] = t[e]; }); }, o = 0; o < t.length; o++) {
        i(t[o]);
    } return n; });
    var le = 300;
    function r(s, u) { var D = { config: Object.assign({}, ce.defaultConfig), l10n: Q }; function f(e) { return e.bind(D); } function d(e) { 0 !== D.selectedDates.length && (!function (e) { e.preventDefault(); var t = "keydown" === e.type, n = e.target; void 0 !== D.amPM && e.target === D.amPM && (D.amPM.textContent = D.l10n.amPM[q(D.amPM.textContent === D.l10n.amPM[0])]); var a = parseFloat(n.getAttribute("data-min")), i = parseFloat(n.getAttribute("data-max")), o = parseFloat(n.getAttribute("data-step")), r = parseInt(n.value, 10), l = e.delta || (t ? 38 === e.which ? 1 : -1 : 0), c = r + o * l; if (void 0 !== n.value && 2 === n.value.length) {
        var d = n === D.hourElement, s = n === D.minuteElement;
        c < a ? (c = i + c + q(!d) + (q(d) && q(!D.amPM)), s && h(void 0, -1, D.hourElement)) : i < c && (c = n === D.hourElement ? c - i - q(!D.amPM) : a, s && h(void 0, 1, D.hourElement)), D.amPM && d && (1 === o ? c + r === 23 : Math.abs(c - r) > o) && (D.amPM.textContent = D.l10n.amPM[q(D.amPM.textContent === D.l10n.amPM[0])]), n.value = $(c);
    } }(e), "input" !== e.type ? (m(), B()) : setTimeout(function () { m(), B(); }, le)); } function m() { if (void 0 !== D.hourElement && void 0 !== D.minuteElement) {
        var e, t, n = (parseInt(D.hourElement.value.slice(-2), 10) || 0) % 24, a = (parseInt(D.minuteElement.value, 10) || 0) % 60, i = void 0 !== D.secondElement ? (parseInt(D.secondElement.value, 10) || 0) % 60 : 0;
        void 0 !== D.amPM && (e = n, t = D.amPM.textContent, n = e % 12 + 12 * q(t === D.l10n.amPM[1]));
        var o = void 0 !== D.config.minTime || D.config.minDate && D.minDateHasTime && D.latestSelectedDateObj && 0 === ee(D.latestSelectedDateObj, D.config.minDate, !0);
        if (void 0 !== D.config.maxTime || D.config.maxDate && D.maxDateHasTime && D.latestSelectedDateObj && 0 === ee(D.latestSelectedDateObj, D.config.maxDate, !0)) {
            var r = void 0 !== D.config.maxTime ? D.config.maxTime : D.config.maxDate;
            (n = Math.min(n, r.getHours())) === r.getHours() && (a = Math.min(a, r.getMinutes())), a === r.getMinutes() && (i = Math.min(i, r.getSeconds()));
        }
        if (o) {
            var l = void 0 !== D.config.minTime ? D.config.minTime : D.config.minDate;
            (n = Math.max(n, l.getHours())) === l.getHours() && (a = Math.max(a, l.getMinutes())), a === l.getMinutes() && (i = Math.max(i, l.getSeconds()));
        }
        c(n, a, i);
    } } function g(e) { var t = e || D.latestSelectedDateObj; t && c(t.getHours(), t.getMinutes(), t.getSeconds()); } function c(e, t, n) { void 0 !== D.latestSelectedDateObj && D.latestSelectedDateObj.setHours(e % 24, t, n || 0, 0), D.hourElement && D.minuteElement && !D.isMobile && (D.hourElement.value = $(D.config.time_24hr ? e : (12 + e) % 12 + 12 * q(e % 12 == 0)), D.minuteElement.value = $(t), void 0 !== D.amPM && (D.amPM.textContent = D.l10n.amPM[q(12 <= e)]), void 0 !== D.secondElement && (D.secondElement.value = $(n))); } function n(e) { var t = parseInt(e.target.value) + (e.delta || 0); 4 !== t.toString().length && "Enter" !== e.key || (e.target.blur(), /[^\d]/.test(t.toString()) || E(t)); } function o(t, n, a, i) { return n instanceof Array ? n.forEach(function (e) { return o(t, e, a, i); }) : t instanceof Array ? t.forEach(function (e) { return o(e, n, a, i); }) : (t.addEventListener(n, a, i), void D._handlers.push({ element: t, event: n, handler: a })); } function a(t) { return function (e) { 1 === e.which && t(e); }; } function p() { W("onChange"); } function i(e) { var t = void 0 !== e ? D.parseDate(e) : D.latestSelectedDateObj || (D.config.minDate && D.config.minDate > D.now ? D.config.minDate : D.config.maxDate && D.config.maxDate < D.now ? D.config.maxDate : D.now); try {
        void 0 !== t && (D.currentYear = t.getFullYear(), D.currentMonth = t.getMonth());
    }
    catch (e) {
        e.message = "Invalid date supplied: " + t, D.config.errorHandler(e);
    } D.redraw(); } function r(e) { ~e.target.className.indexOf("arrow") && h(e, e.target.classList.contains("arrowUp") ? 1 : -1); } function h(e, t, n) { var a = e && e.target, i = n || a && a.parentNode && a.parentNode.firstChild, o = R("increment"); o.delta = t, i && i.dispatchEvent(o); } function v(e, t, n, a) { var i, o = k(t, !0), r = ie("span", "flatpickr-day " + e, t.getDate().toString()); return r.dateObj = t, r.$i = a, r.setAttribute("aria-label", D.formatDate(t, D.config.ariaDateFormat)), 0 === ee(t, D.now) && (D.todayDateElem = r).classList.add("today"), o ? (r.tabIndex = -1, J(t) && (r.classList.add("selected"), D.selectedDateElem = r, "range" === D.config.mode && (ae(r, "startRange", D.selectedDates[0] && 0 === ee(t, D.selectedDates[0], !0)), ae(r, "endRange", D.selectedDates[1] && 0 === ee(t, D.selectedDates[1], !0)), "nextMonthDay" === e && r.classList.add("inRange")))) : r.classList.add("disabled"), "range" === D.config.mode && (i = t, !("range" !== D.config.mode || D.selectedDates.length < 2) && 0 <= ee(i, D.selectedDates[0]) && ee(i, D.selectedDates[1]) <= 0 && !J(t) && r.classList.add("inRange")), D.weekNumbers && 1 === D.config.showMonths && "prevMonthDay" !== e && n % 7 == 1 && D.weekNumbers.insertAdjacentHTML("beforeend", "<span class='flatpickr-day'>" + D.config.getWeek(t) + "</span>"), W("onDayCreate", r), r; } function b(e, t) { var n = ((void 0 !== e ? e : document.activeElement.$i) || 0) + t || 0, a = Array.prototype.find.call(D.days.children, function (e, t) { return n <= t && -1 === e.className.indexOf("MonthDay") && k(e.dateObj); }); void 0 !== a && (a.focus(), "range" === D.config.mode && S(a)); } function l(e, t) { for (var n = (new Date(e, t, 1).getDay() - D.l10n.firstDayOfWeek + 7) % 7, a = D.utils.getDaysInMonth((t - 1 + 12) % 12), i = D.utils.getDaysInMonth(t), o = window.document.createDocumentFragment(), r = a + 1 - n, l = 0; r <= a; r++, l++)
        o.appendChild(v("prevMonthDay", new Date(e, t - 1, r), r, l)); for (r = 1; r <= i; r++, l++)
        o.appendChild(v("", new Date(e, t, r), r, l)); for (var c = i + 1; c <= 42 - n && (1 === D.config.showMonths || l % 7 != 0); c++, l++)
        o.appendChild(v("nextMonthDay", new Date(e, t + 1, c % i), c, l)); var d = ie("div", "dayContainer"); return d.appendChild(o), d; } function w() { if (void 0 !== D.daysContainer) {
        oe(D.daysContainer), D.weekNumbers && oe(D.weekNumbers);
        for (var e = document.createDocumentFragment(), t = 0; t < D.config.showMonths; t++) {
            var n = new Date(D.currentYear, D.currentMonth, 1);
            n.setMonth(D.currentMonth + t), e.appendChild(l(n.getFullYear(), n.getMonth()));
        }
        D.daysContainer.appendChild(e), D.days = D.daysContainer.firstChild;
    } } function C() { var e = ie("div", "flatpickr-month"), t = window.document.createDocumentFragment(), n = ie("span", "cur-month"); n.title = D.l10n.scrollTitle; var a = re("cur-year", { tabindex: "-1" }), i = a.childNodes[0]; i.title = D.l10n.scrollTitle, i.setAttribute("aria-label", D.l10n.yearAriaLabel), D.config.minDate && i.setAttribute("data-min", D.config.minDate.getFullYear().toString()), D.config.maxDate && (i.setAttribute("data-max", D.config.maxDate.getFullYear().toString()), i.disabled = !!D.config.minDate && D.config.minDate.getFullYear() === D.config.maxDate.getFullYear()); var o = ie("div", "flatpickr-current-month"); return o.appendChild(n), o.appendChild(a), t.appendChild(o), e.appendChild(t), { container: e, yearElement: i, monthElement: n }; } function M() { var e = D.l10n.firstDayOfWeek, t = D.l10n.weekdays.shorthand.concat(); 0 < e && e < t.length && (t = t.splice(e, t.length).concat(t.splice(0, e))); for (var n = D.config.showMonths; n--;)
        D.weekdayContainer.children[n].innerHTML = "\n      <span class=flatpickr-weekday>\n        " + t.join("</span><span class=flatpickr-weekday>") + "\n      </span>\n      "; } function y(e, t, n) { void 0 === t && (t = !0), void 0 === n && (n = !1); var a = t ? e : e - D.currentMonth; a < 0 && !0 === D._hidePrevMonthArrow || 0 < a && !0 === D._hideNextMonthArrow || (D.currentMonth += a, (D.currentMonth < 0 || 11 < D.currentMonth) && (D.currentYear += 11 < D.currentMonth ? 1 : -1, D.currentMonth = (D.currentMonth + 12) % 12, W("onYearChange")), w(), W("onMonthChange"), K(), !0 === n && b(void 0, 0)); } function x(e) { return !(!D.config.appendTo || !D.config.appendTo.contains(e)) || D.calendarContainer.contains(e); } function T(t) { if (D.isOpen && !D.config.inline) {
        var e = x(t.target), n = t.target === D.input || t.target === D.altInput || D.element.contains(t.target) || t.path && t.path.indexOf && (~t.path.indexOf(D.input) || ~t.path.indexOf(D.altInput)), a = "blur" === t.type ? n && t.relatedTarget && !x(t.relatedTarget) : !n && !e, i = !D.config.ignoredFocusElements.some(function (e) { return e.contains(t.target); });
        a && i && (D.close(), "range" === D.config.mode && 1 === D.selectedDates.length && (D.clear(!1), D.redraw()));
    } } function E(e) { if (!(!e || D.config.minDate && e < D.config.minDate.getFullYear() || D.config.maxDate && e > D.config.maxDate.getFullYear())) {
        var t = e, n = D.currentYear !== t;
        D.currentYear = t || D.currentYear, D.config.maxDate && D.currentYear === D.config.maxDate.getFullYear() ? D.currentMonth = Math.min(D.config.maxDate.getMonth(), D.currentMonth) : D.config.minDate && D.currentYear === D.config.minDate.getFullYear() && (D.currentMonth = Math.max(D.config.minDate.getMonth(), D.currentMonth)), n && (D.redraw(), W("onYearChange"));
    } } function k(e, t) { void 0 === t && (t = !0); var n = D.parseDate(e, void 0, t); if (D.config.minDate && n && ee(n, D.config.minDate, void 0 !== t ? t : !D.minDateHasTime) < 0 || D.config.maxDate && n && 0 < ee(n, D.config.maxDate, void 0 !== t ? t : !D.maxDateHasTime))
        return !1; if (0 === D.config.enable.length && 0 === D.config.disable.length)
        return !0; if (void 0 === n)
        return !1; for (var a, i = 0 < D.config.enable.length, o = i ? D.config.enable : D.config.disable, r = 0; r < o.length; r++) {
        if ("function" == typeof (a = o[r]) && a(n))
            return i;
        if (a instanceof Date && void 0 !== n && a.getTime() === n.getTime())
            return i;
        if ("string" == typeof a && void 0 !== n) {
            var l = D.parseDate(a, void 0, !0);
            return l && l.getTime() === n.getTime() ? i : !i;
        }
        if ("object" == typeof a && void 0 !== n && a.from && a.to && n.getTime() >= a.from.getTime() && n.getTime() <= a.to.getTime())
            return i;
    } return !i; } function I(e) { var t = e.target === D._input, n = x(e.target), a = D.config.allowInput, i = D.isOpen && (!a || !t), o = D.config.inline && t && !a; if (13 === e.keyCode && t) {
        if (a)
            return D.setDate(D._input.value, !0, e.target === D.altInput ? D.config.altFormat : D.config.dateFormat), e.target.blur();
        D.open();
    }
    else if (n || i || o) {
        var r = !!D.timeContainer && D.timeContainer.contains(e.target);
        switch (e.keyCode) {
            case 13:
                r ? B() : j(e);
                break;
            case 27:
                e.preventDefault(), Y();
                break;
            case 8:
            case 46:
                t && !D.config.allowInput && (e.preventDefault(), D.clear());
                break;
            case 37:
            case 39:
                if (r)
                    D.hourElement && D.hourElement.focus();
                else if (e.preventDefault(), D.daysContainer) {
                    var l = t ? 0 : 39 === e.keyCode ? 1 : -1;
                    e.ctrlKey ? y(l, !0, !0) : b(void 0, l);
                }
                break;
            case 38:
            case 40:
                e.preventDefault();
                var c = 40 === e.keyCode ? 1 : -1;
                D.daysContainer && void 0 !== e.target.$i ? e.ctrlKey ? (E(D.currentYear - c), b(e.target.$i, 0)) : r || b(e.target.$i, 7 * c) : D.config.enableTime && (!r && D.hourElement && D.hourElement.focus(), d(e), D._debouncedChange());
                break;
            case 9: e.target === D.hourElement ? (e.preventDefault(), D.minuteElement.select()) : e.target === D.minuteElement && (D.secondElement || D.amPM) ? (e.preventDefault(), void 0 !== D.secondElement ? D.secondElement.focus() : void 0 !== D.amPM && D.amPM.focus()) : e.target === D.secondElement && D.amPM && (e.preventDefault(), D.amPM.focus());
        }
        switch (e.key) {
            case D.l10n.amPM[0].charAt(0):
            case D.l10n.amPM[0].charAt(0).toLowerCase():
                void 0 !== D.amPM && e.target === D.amPM && (D.amPM.textContent = D.l10n.amPM[0], m(), B());
                break;
            case D.l10n.amPM[1].charAt(0):
            case D.l10n.amPM[1].charAt(0).toLowerCase(): void 0 !== D.amPM && e.target === D.amPM && (D.amPM.textContent = D.l10n.amPM[1], m(), B());
        }
        W("onKeyDown", e);
    } } function S(o) { if (1 === D.selectedDates.length && o.classList.contains("flatpickr-day") && !o.classList.contains("disabled")) {
        for (var r = o.dateObj.getTime(), l = D.parseDate(D.selectedDates[0], void 0, !0).getTime(), e = Math.min(r, D.selectedDates[0].getTime()), t = Math.max(r, D.selectedDates[0].getTime()), n = D.daysContainer.children, a = n[0].children[0].dateObj.getTime(), i = n[n.length - 1].lastChild.dateObj.getTime(), c = !1, d = 0, s = 0, u = a; u < i; u += ne.DAY)
            k(new Date(u), !0) || (c = c || e < u && u < t, u < l && (!d || d < u) ? d = u : l < u && (!s || u < s) && (s = u));
        for (var f = 0; f < D.config.showMonths; f++)
            for (var m = D.daysContainer.children[f], g = D.daysContainer.children[f - 1], p = function (e, t) { var n = m.children[e], a = n.dateObj.getTime(), i = 0 < d && a < d || 0 < s && s < a; return i ? (n.classList.add("notAllowed"), ["inRange", "startRange", "endRange"].forEach(function (e) { n.classList.remove(e); }), "continue") : c && !i ? "continue" : (["startRange", "inRange", "endRange", "notAllowed"].forEach(function (e) { n.classList.remove(e); }), o.classList.add(r < D.selectedDates[0].getTime() ? "startRange" : "endRange"), void (!m.contains(o) && 0 < f && g && g.lastChild.dateObj.getTime() >= a || (l < r && a === l ? n.classList.add("startRange") : r < l && a === l && n.classList.add("endRange"), d <= a && (0 === s || a <= s) && te(a, l, r) && n.classList.add("inRange")))); }, h = 0, v = m.children.length; h < v; h++)
                p(h);
    } } function O() { !D.isOpen || D.config.static || D.config.inline || P(); } function _(a) { return function (e) { var t = D.config["_" + a + "Date"] = D.parseDate(e, D.config.dateFormat), n = D.config["_" + ("min" === a ? "max" : "min") + "Date"]; void 0 !== t && (D["min" === a ? "minDateHasTime" : "maxDateHasTime"] = 0 < t.getHours() || 0 < t.getMinutes() || 0 < t.getSeconds()), D.selectedDates && (D.selectedDates = D.selectedDates.filter(function (e) { return k(e); }), D.selectedDates.length || "min" !== a || g(t), B()), D.daysContainer && (A(), void 0 !== t ? D.currentYearElement[a] = t.getFullYear().toString() : D.currentYearElement.removeAttribute(a), D.currentYearElement.disabled = !!n && void 0 !== t && n.getFullYear() === t.getFullYear()); }; } function F() { "object" != typeof D.config.locale && void 0 === ce.l10ns[D.config.locale] && D.config.errorHandler(new Error("flatpickr: invalid locale " + D.config.locale)), D.l10n = Object.assign({}, ce.l10ns.default, "object" == typeof D.config.locale ? D.config.locale : "default" !== D.config.locale ? ce.l10ns[D.config.locale] : void 0), Z.K = "(" + D.l10n.amPM[0] + "|" + D.l10n.amPM[1] + "|" + D.l10n.amPM[0].toLowerCase() + "|" + D.l10n.amPM[1].toLowerCase() + ")", D.formatDate = t(D); } function P(e) { if (void 0 !== D.calendarContainer) {
        W("onPreCalendarPosition");
        var t = e || D._positionElement, n = Array.prototype.reduce.call(D.calendarContainer.children, function (e, t) { return e + t.offsetHeight; }, 0), a = D.calendarContainer.offsetWidth, i = D.config.position, o = t.getBoundingClientRect(), r = window.innerHeight - o.bottom, l = "above" === i || "below" !== i && r < n && o.top > n, c = window.pageYOffset + o.top + (l ? -n - 2 : t.offsetHeight + 2);
        if (ae(D.calendarContainer, "arrowTop", !l), ae(D.calendarContainer, "arrowBottom", l), !D.config.inline) {
            var d = window.pageXOffset + o.left, s = window.document.body.offsetWidth - o.right, u = d + a > window.document.body.offsetWidth;
            ae(D.calendarContainer, "rightMost", u), D.config.static || (D.calendarContainer.style.top = c + "px", u ? (D.calendarContainer.style.left = "auto", D.calendarContainer.style.right = s + "px") : (D.calendarContainer.style.left = d + "px", D.calendarContainer.style.right = "auto"));
        }
    } } function A() { D.config.noCalendar || D.isMobile || (M(), K(), w()); } function Y() { D._input.focus(), -1 !== window.navigator.userAgent.indexOf("MSIE") || void 0 !== navigator.msMaxTouchPoints ? setTimeout(D.close, 0) : D.close(); } function j(e) { e.preventDefault(), e.stopPropagation(); var t = function e(t, n) { return n(t) ? t : t.parentNode ? e(t.parentNode, n) : void 0; }(e.target, function (e) { return e.classList && e.classList.contains("flatpickr-day") && !e.classList.contains("disabled") && !e.classList.contains("notAllowed"); }); if (void 0 !== t) {
        var n = t, a = D.latestSelectedDateObj = new Date(n.dateObj.getTime()), i = (a.getMonth() < D.currentMonth || a.getMonth() > D.currentMonth + D.config.showMonths - 1) && "range" !== D.config.mode;
        if (D.selectedDateElem = n, "single" === D.config.mode)
            D.selectedDates = [a];
        else if ("multiple" === D.config.mode) {
            var o = J(a);
            o ? D.selectedDates.splice(parseInt(o), 1) : D.selectedDates.push(a);
        }
        else
            "range" === D.config.mode && (2 === D.selectedDates.length && D.clear(!1), D.selectedDates.push(a), 0 !== ee(a, D.selectedDates[0], !0) && D.selectedDates.sort(function (e, t) { return e.getTime() - t.getTime(); }));
        if (m(), i) {
            var r = D.currentYear !== a.getFullYear();
            D.currentYear = a.getFullYear(), D.currentMonth = a.getMonth(), r && W("onYearChange"), W("onMonthChange");
        }
        if (K(), w(), D.config.minDate && D.minDateHasTime && D.config.enableTime && 0 === ee(a, D.config.minDate) && g(D.config.minDate), B(), D.config.enableTime && setTimeout(function () { return D.showTimeInput = !0; }, 50), "range" === D.config.mode && (1 === D.selectedDates.length ? S(n) : K()), i || "range" === D.config.mode || 1 !== D.config.showMonths ? D.selectedDateElem && D.selectedDateElem.focus() : b(n.$i, 0), void 0 !== D.hourElement && setTimeout(function () { return void 0 !== D.hourElement && D.hourElement.select(); }, 451), D.config.closeOnSelect) {
            var l = "single" === D.config.mode && !D.config.enableTime, c = "range" === D.config.mode && 2 === D.selectedDates.length && !D.config.enableTime;
            (l || c) && Y();
        }
        p();
    } } D.parseDate = X({ config: D.config, l10n: D.l10n }), D._handlers = [], D._bind = o, D._setHoursFromDate = g, D.changeMonth = y, D.changeYear = E, D.clear = function (e) { void 0 === e && (e = !0); D.input.value = "", void 0 !== D.altInput && (D.altInput.value = ""); void 0 !== D.mobileInput && (D.mobileInput.value = ""); D.selectedDates = [], D.latestSelectedDateObj = void 0, !(D.showTimeInput = !1) === D.config.enableTime && (void 0 !== D.config.minDate ? g(D.config.minDate) : c(D.config.defaultHour, D.config.defaultMinute, D.config.defaultSeconds)); D.redraw(), e && W("onChange"); }, D.close = function () { D.isOpen = !1, D.isMobile || (D.calendarContainer.classList.remove("open"), D._input.classList.remove("active")); W("onClose"); }, D._createElement = ie, D.destroy = function () { void 0 !== D.config && W("onDestroy"); for (var e = D._handlers.length; e--;) {
        var t = D._handlers[e];
        t.element.removeEventListener(t.event, t.handler);
    } D._handlers = [], D.mobileInput ? (D.mobileInput.parentNode && D.mobileInput.parentNode.removeChild(D.mobileInput), D.mobileInput = void 0) : D.calendarContainer && D.calendarContainer.parentNode && D.calendarContainer.parentNode.removeChild(D.calendarContainer); D.altInput && (D.input.type = "text", D.altInput.parentNode && D.altInput.parentNode.removeChild(D.altInput), delete D.altInput); D.input && (D.input.type = D.input._type, D.input.classList.remove("flatpickr-input"), D.input.removeAttribute("readonly"), D.input.value = ""); ["_showTimeInput", "latestSelectedDateObj", "_hideNextMonthArrow", "_hidePrevMonthArrow", "__hideNextMonthArrow", "__hidePrevMonthArrow", "isMobile", "isOpen", "selectedDateElem", "minDateHasTime", "maxDateHasTime", "days", "daysContainer", "_input", "_positionElement", "innerContainer", "rContainer", "monthNav", "todayDateElem", "calendarContainer", "weekdayContainer", "prevMonthNav", "nextMonthNav", "currentMonthElement", "currentYearElement", "navigationCurrentMonth", "selectedDateElem", "config"].forEach(function (e) { try {
        delete D[e];
    }
    catch (e) { } }); }, D.isEnabled = k, D.jumpToDate = i, D.open = function (e, t) { void 0 === t && (t = D._input); if (!0 === D.isMobile)
        return e && (e.preventDefault(), e.target && e.target.blur()), setTimeout(function () { void 0 !== D.mobileInput && D.mobileInput.click(); }, 0), void W("onOpen"); if (D._input.disabled || D.config.inline)
        return; var n = D.isOpen; D.isOpen = !0, n || (D.calendarContainer.classList.add("open"), D._input.classList.add("active"), W("onOpen"), P(t)); !0 === D.config.enableTime && !0 === D.config.noCalendar && (0 === D.selectedDates.length && (D.setDate(void 0 !== D.config.minDate ? new Date(D.config.minDate.getTime()) : (new Date).setHours(D.config.defaultHour, D.config.defaultMinute, D.config.defaultSeconds, 0), !1), m(), B()), setTimeout(function () { return D.hourElement.select(); }, 50)); }, D.redraw = A, D.set = function (e, t) { null !== e && "object" == typeof e ? Object.assign(D.config, e) : (D.config[e] = t, void 0 !== N[e] && N[e].forEach(function (e) { return e(); })); D.redraw(), i(); }, D.setDate = function (e, t, n) { void 0 === t && (t = !1); void 0 === n && (n = D.config.dateFormat); if (0 !== e && !e)
        return D.clear(t); H(e, n), D.showTimeInput = 0 < D.selectedDates.length, D.latestSelectedDateObj = D.selectedDates[0], D.redraw(), i(), g(), B(t), t && W("onChange"); }, D.toggle = function () { if (D.isOpen)
        return D.close(); D.open(); }; var N = { locale: [F] }; function H(e, t) { var n = []; if (e instanceof Array)
        n = e.map(function (e) { return D.parseDate(e, t); });
    else if (e instanceof Date || "number" == typeof e)
        n = [D.parseDate(e, t)];
    else if ("string" == typeof e)
        switch (D.config.mode) {
            case "single":
                n = [D.parseDate(e, t)];
                break;
            case "multiple":
                n = e.split(D.config.conjunction).map(function (e) { return D.parseDate(e, t); });
                break;
            case "range": n = e.split(D.l10n.rangeSeparator).map(function (e) { return D.parseDate(e, t); });
        }
    else
        D.config.errorHandler(new Error("Invalid date supplied: " + JSON.stringify(e))); D.selectedDates = n.filter(function (e) { return e instanceof Date && k(e, !1); }), "range" === D.config.mode && D.selectedDates.sort(function (e, t) { return e.getTime() - t.getTime(); }); } function L(e) { return e.map(function (e) { return "string" == typeof e || "number" == typeof e || e instanceof Date ? D.parseDate(e, void 0, !0) : e && "object" == typeof e && e.from && e.to ? { from: D.parseDate(e.from, void 0), to: D.parseDate(e.to, void 0) } : e; }).filter(function (e) { return e; }); } function W(e, t) { var n = D.config[e]; if (void 0 !== n && 0 < n.length)
        for (var a = 0; n[a] && a < n.length; a++)
            n[a](D.selectedDates, D.input.value, D, t); "onChange" === e && (D.input.dispatchEvent(R("change")), D.input.dispatchEvent(R("input"))); } function R(e) { var t = document.createEvent("Event"); return t.initEvent(e, !0, !0), t; } function J(e) { for (var t = 0; t < D.selectedDates.length; t++)
        if (0 === ee(D.selectedDates[t], e))
            return "" + t; return !1; } function K() { D.config.noCalendar || D.isMobile || !D.monthNav || (D.yearElements.forEach(function (e, t) { var n = new Date(D.currentYear, D.currentMonth, 1); n.setMonth(D.currentMonth + t), D.monthElements[t].textContent = V(n.getMonth(), D.config.shorthandCurrentMonth, D.l10n) + " ", e.value = n.getFullYear().toString(); }), D._hidePrevMonthArrow = void 0 !== D.config.minDate && (D.currentYear === D.config.minDate.getFullYear() ? D.currentMonth <= D.config.minDate.getMonth() : D.currentYear < D.config.minDate.getFullYear()), D._hideNextMonthArrow = void 0 !== D.config.maxDate && (D.currentYear === D.config.maxDate.getFullYear() ? D.currentMonth + 1 > D.config.maxDate.getMonth() : D.currentYear > D.config.maxDate.getFullYear())); } function B(e) { if (void 0 === e && (e = !0), 0 === D.selectedDates.length)
        return D.clear(e); void 0 !== D.mobileInput && D.mobileFormatStr && (D.mobileInput.value = void 0 !== D.latestSelectedDateObj ? D.formatDate(D.latestSelectedDateObj, D.mobileFormatStr) : ""); var t = "range" !== D.config.mode ? D.config.conjunction : D.l10n.rangeSeparator; D.input.value = D.selectedDates.map(function (e) { return D.formatDate(e, D.config.dateFormat); }).join(t), void 0 !== D.altInput && (D.altInput.value = D.selectedDates.map(function (e) { return D.formatDate(e, D.config.altFormat); }).join(t)), !1 !== e && W("onValueUpdate"); } function U(e) { var t = D.prevMonthNav.contains(e.target), n = D.nextMonthNav.contains(e.target); t || n ? y(t ? -1 : 1) : 0 <= D.yearElements.indexOf(e.target) ? (e.preventDefault(), e.target.select()) : e.target.classList.contains("arrowUp") ? D.changeYear(D.currentYear + 1) : e.target.classList.contains("arrowDown") && D.changeYear(D.currentYear - 1); } return function () { if (D.element = D.input = s, D.isOpen = !1, function () { var e = ["wrap", "weekNumbers", "allowInput", "clickOpens", "time_24hr", "enableTime", "noCalendar", "altInput", "shorthandCurrentMonth", "inline", "static", "enableSeconds", "disableMobile"], t = ["onChange", "onClose", "onDayCreate", "onDestroy", "onKeyDown", "onMonthChange", "onOpen", "onParseConfig", "onReady", "onValueUpdate", "onYearChange", "onPreCalendarPosition"], n = Object.assign({}, u, JSON.parse(JSON.stringify(s.dataset || {}))), a = {}; D.config.parseDate = n.parseDate, D.config.formatDate = n.formatDate, Object.defineProperty(D.config, "enable", { get: function () { return D.config._enable; }, set: function (e) { D.config._enable = L(e); } }), Object.defineProperty(D.config, "disable", { get: function () { return D.config._disable; }, set: function (e) { D.config._disable = L(e); } }), !n.dateFormat && n.enableTime && (a.dateFormat = n.noCalendar ? "H:i" + (n.enableSeconds ? ":S" : "") : ce.defaultConfig.dateFormat + " H:i" + (n.enableSeconds ? ":S" : "")), n.altInput && n.enableTime && !n.altFormat && (a.altFormat = n.noCalendar ? "h:i" + (n.enableSeconds ? ":S K" : " K") : ce.defaultConfig.altFormat + " h:i" + (n.enableSeconds ? ":S" : "") + " K"), Object.defineProperty(D.config, "minDate", { get: function () { return D.config._minDate; }, set: _("min") }), Object.defineProperty(D.config, "maxDate", { get: function () { return D.config._maxDate; }, set: _("max") }); var i = function (t) { return function (e) { D.config["min" === t ? "_minTime" : "_maxTime"] = D.parseDate(e, "H:i"); }; }; Object.defineProperty(D.config, "minTime", { get: function () { return D.config._minTime; }, set: i("min") }), Object.defineProperty(D.config, "maxTime", { get: function () { return D.config._maxTime; }, set: i("max") }), Object.assign(D.config, a, n); for (var o = 0; o < e.length; o++)
        D.config[e[o]] = !0 === D.config[e[o]] || "true" === D.config[e[o]]; for (var r = t.length; r--;)
        void 0 !== D.config[t[r]] && (D.config[t[r]] = G(D.config[t[r]] || []).map(f)); "time" === D.config.mode && (D.config.noCalendar = !0, D.config.enableTime = !0); for (var l = 0; l < D.config.plugins.length; l++) {
        var c = D.config.plugins[l](D) || {};
        for (var d in c)
            ~t.indexOf(d) ? D.config[d] = G(c[d]).map(f).concat(D.config[d]) : void 0 === n[d] && (D.config[d] = c[d]);
    } D.isMobile = !D.config.disableMobile && !D.config.inline && "single" === D.config.mode && !D.config.disable.length && !D.config.enable.length && !D.config.weekNumbers && /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent), W("onParseConfig"); }(), F(), function () { if (D.input = D.config.wrap ? s.querySelector("[data-input]") : s, !D.input)
        return D.config.errorHandler(new Error("Invalid input element specified")); D.input._type = D.input.type, D.input.type = "text", D.input.classList.add("flatpickr-input"), D._input = D.input, D.config.altInput && (D.altInput = ie(D.input.nodeName, D.input.className + " " + D.config.altInputClass), D._input = D.altInput, D.altInput.placeholder = D.input.placeholder, D.altInput.disabled = D.input.disabled, D.altInput.required = D.input.required, D.altInput.tabIndex = D.input.tabIndex, D.altInput.type = "text", D.input.type = "hidden", !D.config.static && D.input.parentNode && D.input.parentNode.insertBefore(D.altInput, D.input.nextSibling)), D.config.allowInput || D._input.setAttribute("readonly", "readonly"), D._positionElement = D.config.positionElement || D._input; }(), function () { D.selectedDates = [], D.now = D.parseDate(D.config.now) || new Date; var e = D.config.defaultDate || D.input.value; e && H(e, D.config.dateFormat); var t = 0 < D.selectedDates.length ? D.selectedDates[0] : D.config.minDate && D.config.minDate.getTime() > D.now.getTime() ? D.config.minDate : D.config.maxDate && D.config.maxDate.getTime() < D.now.getTime() ? D.config.maxDate : D.now; D.currentYear = t.getFullYear(), D.currentMonth = t.getMonth(), 0 < D.selectedDates.length && (D.latestSelectedDateObj = D.selectedDates[0]), void 0 !== D.config.minTime && (D.config.minTime = D.parseDate(D.config.minTime, "H:i")), void 0 !== D.config.maxTime && (D.config.maxTime = D.parseDate(D.config.maxTime, "H:i")), D.minDateHasTime = !!D.config.minDate && (0 < D.config.minDate.getHours() || 0 < D.config.minDate.getMinutes() || 0 < D.config.minDate.getSeconds()), D.maxDateHasTime = !!D.config.maxDate && (0 < D.config.maxDate.getHours() || 0 < D.config.maxDate.getMinutes() || 0 < D.config.maxDate.getSeconds()), Object.defineProperty(D, "showTimeInput", { get: function () { return D._showTimeInput; }, set: function (e) { D._showTimeInput = e, D.calendarContainer && ae(D.calendarContainer, "showTimeInput", e), D.isOpen && P(); } }); }(), D.utils = { getDaysInMonth: function (e, t) { return void 0 === e && (e = D.currentMonth), void 0 === t && (t = D.currentYear), 1 === e && (t % 4 == 0 && t % 100 != 0 || t % 400 == 0) ? 29 : D.l10n.daysInMonth[e]; } }, D.isMobile || function () { var e = window.document.createDocumentFragment(); if (D.calendarContainer = ie("div", "flatpickr-calendar"), D.calendarContainer.tabIndex = -1, !D.config.noCalendar) {
        if (e.appendChild(function () { D.monthNav = ie("div", "flatpickr-months"), D.yearElements = [], D.monthElements = [], D.prevMonthNav = ie("span", "flatpickr-prev-month"), D.prevMonthNav.innerHTML = D.config.prevArrow, D.nextMonthNav = ie("span", "flatpickr-next-month"), D.nextMonthNav.innerHTML = D.config.nextArrow, D.monthNav.appendChild(D.prevMonthNav); for (var e = D.config.showMonths; e--;) {
            var t = C();
            D.yearElements.push(t.yearElement), D.monthElements.push(t.monthElement), D.monthNav.appendChild(t.container);
        } return D.monthNav.appendChild(D.nextMonthNav), Object.defineProperty(D, "_hidePrevMonthArrow", { get: function () { return D.__hidePrevMonthArrow; }, set: function (e) { D.__hidePrevMonthArrow !== e && (ae(D.prevMonthNav, "disabled", e), D.__hidePrevMonthArrow = e); } }), Object.defineProperty(D, "_hideNextMonthArrow", { get: function () { return D.__hideNextMonthArrow; }, set: function (e) { D.__hideNextMonthArrow !== e && (ae(D.nextMonthNav, "disabled", e), D.__hideNextMonthArrow = e); } }), D.currentYearElement = D.yearElements[0], K(), D.monthNav; }()), D.innerContainer = ie("div", "flatpickr-innerContainer"), D.config.weekNumbers) {
            var t = function () { D.calendarContainer.classList.add("hasWeeks"); var e = ie("div", "flatpickr-weekwrapper"); e.appendChild(ie("span", "flatpickr-weekday", D.l10n.weekAbbreviation)); var t = ie("div", "flatpickr-weeks"); return e.appendChild(t), { weekWrapper: e, weekNumbers: t }; }(), n = t.weekWrapper, a = t.weekNumbers;
            D.innerContainer.appendChild(n), D.weekNumbers = a, D.weekWrapper = n;
        }
        D.rContainer = ie("div", "flatpickr-rContainer"), D.rContainer.appendChild(function () { D.weekdayContainer || (D.weekdayContainer = ie("div", "flatpickr-weekdays")); for (var e = D.config.showMonths; e--;) {
            var t = ie("div", "flatpickr-weekdaycontainer");
            D.weekdayContainer.appendChild(t);
        } return M(), D.weekdayContainer; }()), D.daysContainer || (D.daysContainer = ie("div", "flatpickr-days"), D.daysContainer.tabIndex = -1), w(), D.rContainer.appendChild(D.daysContainer), D.innerContainer.appendChild(D.rContainer), e.appendChild(D.innerContainer);
    } D.config.enableTime && e.appendChild(function () { D.calendarContainer.classList.add("hasTime"), D.config.noCalendar && D.calendarContainer.classList.add("noCalendar"), D.timeContainer = ie("div", "flatpickr-time"), D.timeContainer.tabIndex = -1; var e = ie("span", "flatpickr-time-separator", ":"), t = re("flatpickr-hour"); D.hourElement = t.childNodes[0]; var n = re("flatpickr-minute"); if (D.minuteElement = n.childNodes[0], D.hourElement.tabIndex = D.minuteElement.tabIndex = -1, D.hourElement.value = $(D.latestSelectedDateObj ? D.latestSelectedDateObj.getHours() : D.config.time_24hr ? D.config.defaultHour : function (e) { switch (e % 24) {
        case 0:
        case 12: return 12;
        default: return e % 12;
    } }(D.config.defaultHour)), D.minuteElement.value = $(D.latestSelectedDateObj ? D.latestSelectedDateObj.getMinutes() : D.config.defaultMinute), D.hourElement.setAttribute("data-step", D.config.hourIncrement.toString()), D.minuteElement.setAttribute("data-step", D.config.minuteIncrement.toString()), D.hourElement.setAttribute("data-min", D.config.time_24hr ? "0" : "1"), D.hourElement.setAttribute("data-max", D.config.time_24hr ? "23" : "12"), D.minuteElement.setAttribute("data-min", "0"), D.minuteElement.setAttribute("data-max", "59"), D.timeContainer.appendChild(t), D.timeContainer.appendChild(e), D.timeContainer.appendChild(n), D.config.time_24hr && D.timeContainer.classList.add("time24hr"), D.config.enableSeconds) {
        D.timeContainer.classList.add("hasSeconds");
        var a = re("flatpickr-second");
        D.secondElement = a.childNodes[0], D.secondElement.value = $(D.latestSelectedDateObj ? D.latestSelectedDateObj.getSeconds() : D.config.defaultSeconds), D.secondElement.setAttribute("data-step", D.minuteElement.getAttribute("data-step")), D.secondElement.setAttribute("data-min", D.minuteElement.getAttribute("data-min")), D.secondElement.setAttribute("data-max", D.minuteElement.getAttribute("data-max")), D.timeContainer.appendChild(ie("span", "flatpickr-time-separator", ":")), D.timeContainer.appendChild(a);
    } return D.config.time_24hr || (D.amPM = ie("span", "flatpickr-am-pm", D.l10n.amPM[q(11 < (D.latestSelectedDateObj ? D.hourElement.value : D.config.defaultHour))]), D.amPM.title = D.l10n.toggleTitle, D.amPM.tabIndex = -1, D.timeContainer.appendChild(D.amPM)), D.timeContainer; }()), ae(D.calendarContainer, "rangeMode", "range" === D.config.mode), ae(D.calendarContainer, "animate", !0 === D.config.animate), ae(D.calendarContainer, "multiMonth", 1 < D.config.showMonths), D.calendarContainer.appendChild(e); var i = void 0 !== D.config.appendTo && void 0 !== D.config.appendTo.nodeType; if ((D.config.inline || D.config.static) && (D.calendarContainer.classList.add(D.config.inline ? "inline" : "static"), D.config.inline && (!i && D.element.parentNode ? D.element.parentNode.insertBefore(D.calendarContainer, D._input.nextSibling) : void 0 !== D.config.appendTo && D.config.appendTo.appendChild(D.calendarContainer)), D.config.static)) {
        var o = ie("div", "flatpickr-wrapper");
        D.element.parentNode && D.element.parentNode.insertBefore(o, D.element), o.appendChild(D.element), D.altInput && o.appendChild(D.altInput), o.appendChild(D.calendarContainer);
    } D.config.static || D.config.inline || (void 0 !== D.config.appendTo ? D.config.appendTo : window.document.body).appendChild(D.calendarContainer); }(), function () { if (D.config.wrap && ["open", "close", "toggle", "clear"].forEach(function (t) { Array.prototype.forEach.call(D.element.querySelectorAll("[data-" + t + "]"), function (e) { return o(e, "click", D[t]); }); }), D.isMobile)
        return function () { var e = D.config.enableTime ? D.config.noCalendar ? "time" : "datetime-local" : "date"; D.mobileInput = ie("input", D.input.className + " flatpickr-mobile"), D.mobileInput.step = D.input.getAttribute("step") || "any", D.mobileInput.tabIndex = 1, D.mobileInput.type = e, D.mobileInput.disabled = D.input.disabled, D.mobileInput.required = D.input.required, D.mobileInput.placeholder = D.input.placeholder, D.mobileFormatStr = "datetime-local" === e ? "Y-m-d\\TH:i:S" : "date" === e ? "Y-m-d" : "H:i:S", 0 < D.selectedDates.length && (D.mobileInput.defaultValue = D.mobileInput.value = D.formatDate(D.selectedDates[0], D.mobileFormatStr)), D.config.minDate && (D.mobileInput.min = D.formatDate(D.config.minDate, "Y-m-d")), D.config.maxDate && (D.mobileInput.max = D.formatDate(D.config.maxDate, "Y-m-d")), D.input.type = "hidden", void 0 !== D.altInput && (D.altInput.type = "hidden"); try {
            D.input.parentNode && D.input.parentNode.insertBefore(D.mobileInput, D.input.nextSibling);
        }
        catch (e) { } o(D.mobileInput, "change", function (e) { D.setDate(e.target.value, !1, D.mobileFormatStr), W("onChange"), W("onClose"); }); }(); var e = z(O, 50); D._debouncedChange = z(p, le), D.daysContainer && !/iPhone|iPad|iPod/i.test(navigator.userAgent) && o(D.daysContainer, "mouseover", function (e) { "range" === D.config.mode && S(e.target); }), o(window.document.body, "keydown", I), D.config.static || o(D._input, "keydown", I), D.config.inline || D.config.static || o(window, "resize", e), void 0 !== window.ontouchstart && o(window.document, "touchstart", T), o(window.document, "mousedown", a(T)), o(window.document, "focus", T, { capture: !0 }), !0 === D.config.clickOpens && (o(D._input, "focus", D.open), o(D._input, "mousedown", a(D.open))), void 0 !== D.daysContainer && (o(D.monthNav, "mousedown", a(U)), o(D.monthNav, ["keyup", "increment"], n), o(D.daysContainer, "mousedown", a(j))), void 0 !== D.timeContainer && void 0 !== D.minuteElement && void 0 !== D.hourElement && (o(D.timeContainer, ["input", "increment"], d), o(D.timeContainer, "mousedown", a(r)), o(D.timeContainer, ["input", "increment"], D._debouncedChange, { passive: !0 }), o([D.hourElement, D.minuteElement], ["focus", "click"], function (e) { return e.target.select(); }), void 0 !== D.secondElement && o(D.secondElement, "focus", function () { return D.secondElement && D.secondElement.select(); }), void 0 !== D.amPM && o(D.amPM, "mousedown", a(function (e) { d(e), p(); }))); }(), (D.selectedDates.length || D.config.noCalendar) && (D.config.enableTime && g(D.config.noCalendar ? D.latestSelectedDateObj || D.config.minDate : void 0), B(!1)), D.showTimeInput = 0 < D.selectedDates.length || D.config.noCalendar, void 0 !== D.daysContainer) {
        D.calendarContainer.style.visibility = "hidden", D.calendarContainer.style.display = "block";
        var e = (D.daysContainer.offsetWidth + 1) * D.config.showMonths;
        D.daysContainer.style.width = e + "px", D.calendarContainer.style.width = e + "px", void 0 !== D.weekWrapper && (D.calendarContainer.style.width = e + D.weekWrapper.offsetWidth + "px"), D.calendarContainer.style.visibility = "visible", D.calendarContainer.style.display = null;
    } var t = /^((?!chrome|android).)*safari/i.test(navigator.userAgent); !D.isMobile && t && P(), W("onReady"); }(), D; }
    function n(e, t) { for (var n = Array.prototype.slice.call(e), a = [], i = 0; i < n.length; i++) {
        var o = n[i];
        try {
            if (null !== o.getAttribute("data-fp-omit"))
                continue;
            void 0 !== o._flatpickr && (o._flatpickr.destroy(), o._flatpickr = void 0), o._flatpickr = r(o, t || {}), a.push(o._flatpickr);
        }
        catch (e) {
            console.error(e);
        }
    } return 1 === a.length ? a[0] : a; }
    "undefined" != typeof HTMLElement && (HTMLCollection.prototype.flatpickr = NodeList.prototype.flatpickr = function (e) { return n(this, e); }, HTMLElement.prototype.flatpickr = function (e) { return n([this], e); });
    var ce = function (e, t) { return e instanceof NodeList ? n(e, t) : n("string" == typeof e ? window.document.querySelectorAll(e) : [e], t); };
    return ce.defaultConfig = b, ce.l10ns = { en: Object.assign({}, Q), default: Object.assign({}, Q) }, ce.localize = function (e) { ce.l10ns.default = Object.assign({}, ce.l10ns.default, e); }, ce.setDefaults = function (e) { ce.defaultConfig = Object.assign({}, ce.defaultConfig, e); }, ce.parseDate = X({}), ce.formatDate = t({}), ce.compareDates = ee, "undefined" != typeof jQuery && (jQuery.fn.flatpickr = function (e) { return n(this, e); }), Date.prototype.fp_incr = function (e) { return new Date(this.getFullYear(), this.getMonth(), this.getDate() + ("string" == typeof e ? parseInt(e, 10) : e)); }, ce;
});
(function (root, factory) {
    'use strict';
    var libName = 'Taggle';
    if (typeof define === 'function' && define.amd) {
        define([], function () {
            var module = factory();
            root[libName] = module;
            return module;
        });
    }
    else if (typeof module === 'object' && module.exports) {
        module.exports = root[libName] = factory();
    }
    else {
        root[libName] = factory();
    }
}(this, function () {
    'use strict';
    var noop = function () { };
    var retTrue = function () {
        return true;
    };
    var BACKSPACE = 8;
    var COMMA = 188;
    var TAB = 9;
    var ENTER = 13;
    var DEFAULTS = {
        additionalTagClasses: '',
        allowDuplicates: false,
        saveOnBlur: false,
        clearOnBlur: true,
        duplicateTagClass: '',
        containerFocusClass: 'active',
        focusInputOnContainerClick: true,
        hiddenInputName: 'taggles[]',
        tags: [],
        delimeter: ',',
        delimiter: '',
        attachTagId: false,
        allowedTags: [],
        disallowedTags: [],
        maxTags: null,
        tabIndex: 1,
        placeholder: 'Enter tags...',
        submitKeys: [COMMA, TAB, ENTER],
        preserveCase: false,
        inputFormatter: noop,
        tagFormatter: noop,
        onBeforeTagAdd: noop,
        onTagAdd: noop,
        onBeforeTagRemove: retTrue,
        onTagRemove: noop
    };
    function _extend() {
        var master = arguments[0];
        for (var i = 1, l = arguments.length; i < l; i++) {
            var object = arguments[i];
            for (var key in object) {
                if (object.hasOwnProperty(key)) {
                    master[key] = object[key];
                }
            }
        }
        return master;
    }
    function _isArray(arr) {
        if (Array.isArray) {
            return Array.isArray(arr);
        }
        return Object.prototype.toString.call(arr) === '[object Array]';
    }
    function _on(element, eventName, handler) {
        if (element.addEventListener) {
            element.addEventListener(eventName, handler, false);
        }
        else if (element.attachEvent) {
            element.attachEvent('on' + eventName, handler);
        }
        else {
            element['on' + eventName] = handler;
        }
    }
    function _off(element, eventName, handler) {
        if (element.removeEventListener) {
            element.removeEventListener(eventName, handler, false);
        }
        else if (element.detachEvent) {
            element.detachEvent('on' + eventName, handler);
        }
        else {
            element['on' + eventName] = null;
        }
    }
    function _trim(str) {
        return str.replace(/^\s+|\s+$/g, '');
    }
    function _setText(el, text) {
        if (window.attachEvent && !window.addEventListener) {
            el.innerText = text;
        }
        else {
            el.textContent = text;
        }
    }
    var Taggle = function (el, options) {
        this.settings = _extend({}, DEFAULTS, options);
        this.measurements = {
            container: {
                rect: null,
                style: null,
                padding: null
            }
        };
        this.container = el;
        this.tag = {
            values: [],
            elements: []
        };
        this.list = document.createElement('ul');
        this.inputLi = document.createElement('li');
        this.input = document.createElement('input');
        this.sizer = document.createElement('div');
        this.pasting = false;
        this.placeholder = null;
        this.data = null;
        if (this.settings.placeholder) {
            this.placeholder = document.createElement('span');
        }
        if (typeof el === 'string') {
            this.container = document.getElementById(el);
        }
        this._id = 0;
        this._closeEvents = [];
        this._closeButtons = [];
        this._setMeasurements();
        this._setupTextarea();
        this._attachEvents();
    };
    Taggle.prototype._setMeasurements = function () {
        this.measurements.container.rect = this.container.getBoundingClientRect();
        this.measurements.container.style = window.getComputedStyle(this.container);
        var style = this.measurements.container.style;
        var lpad = parseInt(style['padding-left'] || style.paddingLeft, 10);
        var rpad = parseInt(style['padding-right'] || style.paddingRight, 10);
        var lborder = parseInt(style['border-left-width'] || style.borderLeftWidth, 10);
        var rborder = parseInt(style['border-right-width'] || style.borderRightWidth, 10);
        this.measurements.container.padding = lpad + rpad + lborder + rborder;
    };
    Taggle.prototype._setupTextarea = function () {
        var fontSize;
        this.list.className = 'taggle_list';
        this.input.type = 'text';
        this.input.style.paddingLeft = 0;
        this.input.style.paddingRight = 0;
        this.input.className = 'taggle_input';
        this.input.tabIndex = this.settings.tabIndex;
        this.sizer.className = 'taggle_sizer';
        if (this.settings.tags.length) {
            for (var i = 0, len = this.settings.tags.length; i < len; i++) {
                var taggle = this._createTag(this.settings.tags[i]);
                this.list.appendChild(taggle);
            }
        }
        if (this.placeholder) {
            this.placeholder.style.opacity = 0;
            this.placeholder.classList.add('taggle_placeholder');
            this.container.appendChild(this.placeholder);
            _setText(this.placeholder, this.settings.placeholder);
            if (!this.settings.tags.length) {
                this._showPlaceholder();
            }
        }
        var formattedInput = this.settings.inputFormatter(this.input);
        if (formattedInput) {
            this.input = formattedInput;
        }
        this.inputLi.appendChild(this.input);
        this.list.appendChild(this.inputLi);
        this.container.appendChild(this.list);
        this.container.appendChild(this.sizer);
        fontSize = window.getComputedStyle(this.input).fontSize;
        this.sizer.style.fontSize = fontSize;
    };
    Taggle.prototype._attachEvents = function () {
        var self = this;
        if (this._eventsAttached) {
            return false;
        }
        this._eventsAttached = true;
        function containerClick() {
            self.input.focus();
        }
        if (this.settings.focusInputOnContainerClick) {
            this._handleContainerClick = containerClick.bind(this);
            _on(this.container, 'click', this._handleContainerClick);
        }
        this._handleFocus = this._focusInput.bind(this);
        this._handleBlur = this._blurEvent.bind(this);
        this._handleKeydown = this._keydownEvents.bind(this);
        this._handleKeyup = this._keyupEvents.bind(this);
        _on(this.input, 'focus', this._handleFocus);
        _on(this.input, 'blur', this._handleBlur);
        _on(this.input, 'keydown', this._handleKeydown);
        _on(this.input, 'keyup', this._handleKeyup);
        return true;
    };
    Taggle.prototype._detachEvents = function () {
        if (!this._eventsAttached) {
            return false;
        }
        var self = this;
        this._eventsAttached = false;
        _off(this.container, 'click', this._handleContainerClick);
        _off(this.input, 'focus', this._handleFocus);
        _off(this.input, 'blur', this._handleBlur);
        _off(this.input, 'keydown', this._handleKeydown);
        _off(this.input, 'keyup', this._handleKeyup);
        this._closeButtons.forEach(function (button, i) {
            var eventFn = self._closeEvents[i];
            _off(button, 'click', eventFn);
        });
        return true;
    };
    Taggle.prototype._fixInputWidth = function () {
        var width;
        var inputRect;
        var rect;
        var leftPos;
        var padding;
        this._setMeasurements();
        this._setInputWidth();
        inputRect = this.input.getBoundingClientRect();
        rect = this.measurements.container.rect;
        width = rect.width;
        if (!width) {
            width = rect.right - rect.left;
        }
        leftPos = inputRect.left - rect.left;
        padding = this.measurements.container.padding;
        this._setInputWidth(Math.floor(width - leftPos - padding));
    };
    Taggle.prototype._canAdd = function (e, text) {
        if (!text) {
            return false;
        }
        var limit = this.settings.maxTags;
        if (limit !== null && limit <= this.getTagValues().length) {
            return false;
        }
        if (this.settings.onBeforeTagAdd(e, text) === false) {
            return false;
        }
        if (!this.settings.allowDuplicates && this._hasDupes(text)) {
            return false;
        }
        var sensitive = this.settings.preserveCase;
        var allowed = this.settings.allowedTags;
        if (allowed.length && !this._tagIsInArray(text, allowed, sensitive)) {
            return false;
        }
        var disallowed = this.settings.disallowedTags;
        if (disallowed.length && this._tagIsInArray(text, disallowed, sensitive)) {
            return false;
        }
        return true;
    };
    Taggle.prototype._tagIsInArray = function (text, arr, caseSensitive) {
        if (caseSensitive) {
            return arr.indexOf(text) !== -1;
        }
        var lowercased = [].slice.apply(arr).map(function (str) {
            return str.toLowerCase();
        });
        return lowercased.indexOf(text) !== -1;
    };
    Taggle.prototype._add = function (e, text) {
        var self = this;
        var values = text || '';
        if (typeof text !== 'string') {
            values = _trim(this.input.value);
        }
        var delimiter = this.settings.delimiter || this.settings.delimeter;
        values.split(delimiter).map(function (val) {
            return self._formatTag(val);
        }).forEach(function (val) {
            if (!self._canAdd(e, val)) {
                return;
            }
            var li = self._createTag(val);
            var lis = self.list.children;
            var lastLi = lis[lis.length - 1];
            self.list.insertBefore(li, lastLi);
            val = self.tag.values[self.tag.values.length - 1];
            self.settings.onTagAdd(e, val);
            self.input.value = '';
            self._fixInputWidth();
            self._focusInput();
        });
    };
    Taggle.prototype._checkLastTag = function (e) {
        e = e || window.event;
        var taggles = this.container.querySelectorAll('.taggle');
        var lastTaggle = taggles[taggles.length - 1];
        var hotClass = 'taggle_hot';
        var heldDown = this.input.classList.contains('taggle_back');
        if (this.input.value === '' && e.keyCode === BACKSPACE && !heldDown) {
            if (lastTaggle.classList.contains(hotClass)) {
                this.input.classList.add('taggle_back');
                this._remove(lastTaggle, e);
                this._fixInputWidth();
                this._focusInput();
            }
            else {
                lastTaggle.classList.add(hotClass);
            }
        }
        else if (lastTaggle.classList.contains(hotClass)) {
            lastTaggle.classList.remove(hotClass);
        }
    };
    Taggle.prototype._setInputWidth = function (width) {
        this.input.style.width = (width || 10) + 'px';
    };
    Taggle.prototype._hasDupes = function (text) {
        var needle = this.tag.values.indexOf(text);
        var tagglelist = this.container.querySelector('.taggle_list');
        var dupes;
        if (this.settings.duplicateTagClass) {
            dupes = tagglelist.querySelectorAll('.' + this.settings.duplicateTagClass);
            for (var i = 0, len = dupes.length; i < len; i++) {
                dupes[i].classList.remove(this.settings.duplicateTagClass);
            }
        }
        if (needle > -1) {
            if (this.settings.duplicateTagClass) {
                tagglelist.childNodes[needle].classList.add(this.settings.duplicateTagClass);
            }
            return true;
        }
        return false;
    };
    Taggle.prototype._isConfirmKey = function (key) {
        var confirmKey = false;
        if (this.settings.submitKeys.indexOf(key) > -1) {
            confirmKey = true;
        }
        return confirmKey;
    };
    Taggle.prototype._focusInput = function () {
        this._fixInputWidth();
        if (!this.container.classList.contains(this.settings.containerFocusClass)) {
            this.container.classList.add(this.settings.containerFocusClass);
        }
        if (this.placeholder) {
            this.placeholder.style.opacity = 0;
        }
    };
    Taggle.prototype._blurEvent = function (e) {
        if (this.container.classList.contains(this.settings.containerFocusClass)) {
            this.container.classList.remove(this.settings.containerFocusClass);
        }
        if (this.settings.saveOnBlur) {
            e = e || window.event;
            this._listenForEndOfContainer();
            if (this.input.value !== '') {
                this._confirmValidTagEvent(e);
                return;
            }
            if (this.tag.values.length) {
                this._checkLastTag(e);
            }
        }
        else if (this.settings.clearOnBlur) {
            this.input.value = '';
            this._setInputWidth();
        }
        if (!this.tag.values.length && !this.input.value) {
            this._showPlaceholder();
        }
    };
    Taggle.prototype._keydownEvents = function (e) {
        e = e || window.event;
        var key = e.keyCode;
        this.pasting = false;
        this._listenForEndOfContainer();
        if (key === 86 && e.metaKey) {
            this.pasting = true;
        }
        if (this._isConfirmKey(key) && this.input.value !== '') {
            this._confirmValidTagEvent(e);
            return;
        }
        if (this.tag.values.length) {
            this._checkLastTag(e);
        }
    };
    Taggle.prototype._keyupEvents = function (e) {
        e = e || window.event;
        this.input.classList.remove('taggle_back');
        _setText(this.sizer, this.input.value);
        if (this.pasting && this.input.value !== '') {
            this._add(e);
            this.pasting = false;
        }
    };
    Taggle.prototype._confirmValidTagEvent = function (e) {
        e = e || window.event;
        if (e.preventDefault) {
            e.preventDefault();
        }
        else {
            e.returnValue = false;
        }
        this._add(e);
    };
    Taggle.prototype._listenForEndOfContainer = function () {
        var width = this.sizer.getBoundingClientRect().width;
        var max = this.measurements.container.rect.width - this.measurements.container.padding;
        var size = parseInt(this.sizer.style.fontSize, 10);
        if (width + (size * 1.5) > parseInt(this.input.style.width, 10)) {
            this.input.style.width = max + 'px';
        }
    };
    Taggle.prototype._createTag = function (text) {
        var li = document.createElement('li');
        var close = document.createElement('button');
        var hidden = document.createElement('input');
        var span = document.createElement('span');
        text = this._formatTag(text);
        close.innerHTML = '&times;';
        close.className = 'close';
        close.type = 'button';
        var eventFn = this._remove.bind(this, close);
        _on(close, 'click', eventFn);
        _setText(span, text);
        span.className = 'taggle_text';
        li.className = 'taggle ' + this.settings.additionalTagClasses;
        hidden.type = 'hidden';
        hidden.value = text;
        hidden.name = this.settings.hiddenInputName;
        li.appendChild(span);
        li.appendChild(close);
        li.appendChild(hidden);
        var formatted = this.settings.tagFormatter(li);
        if (typeof formatted !== 'undefined') {
            li = formatted;
        }
        if (!(li instanceof HTMLElement) || li.tagName !== 'LI') {
            throw new Error('tagFormatter must return an li element');
        }
        if (this.settings.attachTagId) {
            this._id += 1;
            text = {
                text: text,
                id: this._id
            };
        }
        this.tag.values.push(text);
        this.tag.elements.push(li);
        this._closeEvents.push(eventFn);
        this._closeButtons.push(close);
        return li;
    };
    Taggle.prototype._showPlaceholder = function () {
        if (this.placeholder) {
            this.placeholder.style.opacity = 1;
        }
    };
    Taggle.prototype._remove = function (li, e) {
        var self = this;
        var text;
        var elem;
        var index;
        if (li.tagName.toLowerCase() !== 'li') {
            li = li.parentNode;
        }
        elem = (li.tagName.toLowerCase() === 'a') ? li.parentNode : li;
        index = this.tag.elements.indexOf(elem);
        text = this.tag.values[index];
        function done(error) {
            if (error) {
                return;
            }
            var eventFn = self._closeEvents[index];
            var button = self._closeButtons[index];
            _off(button, 'click', eventFn);
            li.parentNode.removeChild(li);
            self.tag.elements.splice(index, 1);
            self.tag.values.splice(index, 1);
            self.settings.onTagRemove(e, text);
            self._focusInput();
        }
        var ret = this.settings.onBeforeTagRemove(e, text, done);
        if (!ret) {
            return;
        }
        done();
    };
    Taggle.prototype._formatTag = function (text) {
        return this.settings.preserveCase ? text : text.toLowerCase();
    };
    Taggle.prototype.getTags = function () {
        return {
            elements: this.getTagElements(),
            values: this.getTagValues()
        };
    };
    Taggle.prototype.getTagElements = function () {
        return this.tag.elements;
    };
    Taggle.prototype.getTagValues = function () {
        return [].slice.apply(this.tag.values);
    };
    Taggle.prototype.getInput = function () {
        return this.input;
    };
    Taggle.prototype.getContainer = function () {
        return this.container;
    };
    Taggle.prototype.add = function (text) {
        var isArr = _isArray(text);
        if (isArr) {
            for (var i = 0, len = text.length; i < len; i++) {
                if (typeof text[i] === 'string') {
                    this._add(null, text[i]);
                }
            }
        }
        else {
            this._add(null, text);
        }
        return this;
    };
    Taggle.prototype.remove = function (text, all) {
        var len = this.tag.values.length - 1;
        var found = false;
        while (len > -1) {
            var tagText = this.tag.values[len];
            if (this.settings.attachTagId) {
                tagText = tagText.text;
            }
            if (tagText === text) {
                found = true;
                this._remove(this.tag.elements[len]);
            }
            if (found && !all) {
                break;
            }
            len--;
        }
        return this;
    };
    Taggle.prototype.removeAll = function () {
        for (var i = this.tag.values.length - 1; i >= 0; i--) {
            this._remove(this.tag.elements[i]);
        }
        this._showPlaceholder();
        return this;
    };
    Taggle.prototype.setOptions = function (options) {
        this.settings = _extend({}, this.settings, options || {});
        return this;
    };
    Taggle.prototype.enable = function () {
        var buttons = [].slice.call(this.container.querySelectorAll('button'));
        var inputs = [].slice.call(this.container.querySelectorAll('input'));
        buttons.concat(inputs).forEach(function (el) {
            el.removeAttribute('disabled');
        });
        return this;
    };
    Taggle.prototype.disable = function () {
        var buttons = [].slice.call(this.container.querySelectorAll('button'));
        var inputs = [].slice.call(this.container.querySelectorAll('input'));
        buttons.concat(inputs).forEach(function (el) {
            el.setAttribute('disabled', '');
        });
        return this;
    };
    Taggle.prototype.setData = function (data) {
        this.data = data;
        return this;
    };
    Taggle.prototype.getData = function () {
        return this.data;
    };
    Taggle.prototype.attachEvents = function () {
        var self = this;
        var attached = this._attachEvents();
        if (attached) {
            this._closeButtons.forEach(function (button, i) {
                var eventFn = self._closeEvents[i];
                _on(button, 'click', eventFn);
            });
        }
        return this;
    };
    Taggle.prototype.removeEvents = function () {
        this._detachEvents();
        return this;
    };
    return Taggle;
}));
(function (window, document, $, ZMBA, U) {
    $.ajaxSetup({ cache: false });
    U.pages = {};
    ZMBA.extendType(U, {
        loginCookie: document.cookies.getCookieObject("uinfo"),
        getRouteMetadata: function (route, cb) {
            $.ajax({
                cache: false,
                type: "GET",
                url: 'webapi/metadata?route=' + encodeURIComponent(route),
                success: function (data) {
                    if (data.result) {
                        cb(data.result);
                    }
                }
            });
        },
        buildAjaxRequestFromInputs: (function () {
            var rgxCSVSplit = /\s*,\s*/;
            var rgxTrimUri = /^(\s|\?|\/|&)+|(\s|\?|\/|&)+$/;
            var request, querystring, path;
            function setTargetValue(target, name, value, jsType, isCollection) {
                if (isCollection) {
                    target[name] = String(value).split(rgxCSVSplit);
                }
                else {
                    target[name] = value;
                }
            }
            function setParam(name, value, source, jsType, isCollection) {
                if (ZMBA.isNullOrWhitespace(value)) {
                    return;
                }
                if (source === "QueryString" || source === "Url") {
                    querystring += name + "=" + encodeURIComponent(value) + "&";
                }
                else if (name.indexOf('.') === -1) {
                    setTargetValue(request.data, name, value, jsType, isCollection);
                }
                else {
                    var target = request.data;
                    var parts = name.split('.');
                    while (parts.length > 0) {
                        var part = parts.shift();
                        if (parts.length === 0) {
                            setTargetValue(target, part, value, jsType, isCollection);
                        }
                        else {
                            if (!target[part]) {
                                target[part] = {};
                            }
                            target = target[part];
                        }
                    }
                }
            }
            return function buildAjaxRequestFromInputs(inputs, oRequest) {
                request = oRequest || {};
                request.data = request.data || {};
                path = request.url || "";
                querystring = "?";
                if (inputs.length) {
                    for (var i = 0, len = inputs.length; i < len; i++) {
                        var input = inputs[i];
                        if ('getAttribute' in input) {
                            setParam(input.getAttribute("param"), input.value, input.getAttribute("source"), input.getAttribute("jsType"), input.getAttribute("isCollection") === "true");
                        }
                        else {
                            setParam(input.param, input.value, input.source, input.jsType, input.isCollection);
                        }
                    }
                }
                else {
                    var names = Object.getOwnPropertyNames(inputs);
                    for (var i = 0, len = names.length; i < len; i++) {
                        var name_1 = names[i];
                        var input = inputs[name_1];
                        setParam(name_1, input.value, input.source, input.jsType, input.isCollection);
                    }
                }
                request.url = (path + querystring).replace(rgxTrimUri, '');
                return request;
            };
        }()),
        setPageMessage: function (type, message, timeout) {
            U.setNotification(document.getElementById('divPageMessage'), type, message, timeout);
        },
        setNotification: (function () {
            var classmap = { 0: 'success', 1: 'info', 2: 'alert', 3: 'error' };
            return function (elem, type, message, timeout) {
                if (elem.name !== 'notification_message') {
                    var sel = '[name=notification_message]';
                    elem = elem.querySelector(sel) || elem.closest(sel) || elem;
                }
                elem.className = classmap[type] || type;
                elem.innerHTML = "";
                if (message instanceof Node) {
                    elem.appendChild(message);
                }
                else {
                    elem.innerHTML = message;
                }
            };
        }()),
        highlightRequiredInputs: function (bool) {
            document.body.classList.toggle('required-highlight', bool);
        },
        LocationAutoComplete: (function () {
            LocAutoComplete.defaults = {
                search: null,
                city: "[param='Location.Locality']",
                state: "[param='Location.AdminDistrict']",
                zip: "[param='Location.PostalCode']",
                country: "[param='Location.CountryRegion']"
            };
            function LocAutoComplete(options) {
                var cfg = this.cfg = Object.assign({}, LocAutoComplete.defaults, options);
                this.$search = $(cfg.search);
                this.$city = $(cfg.city);
                this.$state = $(cfg.state);
                this.$zip = $(cfg.zip);
                this.$country = $(cfg.country);
                this.$els = $().add(this.$city).add(this.$state).add(this.$zip).add(this.$country);
                var self = this;
                var settings = {
                    ajaxSettings: {
                        cache: true,
                        dataType: "json"
                    },
                    showNoSuggestionNotice: true,
                    paramName: 'query',
                    transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.Formatted, data: x }; }) }; },
                    formatResult: function (suggestion) { return suggestion.data.Formatted; },
                    onSearchStart: function (query) {
                        query.query = self.$els.map(function () { return this.value; }).get().join(' ');
                    }
                };
                this.autoSearch = this.$search.length > 0 && this.$search.autocomplete(Object.assign({}, settings, {
                    serviceUrl: 'webapi/autocomplete/locations',
                    onSearchStart: function () { },
                    onSelect: function (suggestion) {
                        var data = suggestion.data;
                        if (data) {
                            if (data.City) {
                                self.$city.val(data.City);
                            }
                            if (data.State) {
                                self.$state.val(data.State);
                            }
                            if (data.Zip) {
                                self.$zip.val(data.Zip);
                            }
                            if (data.Country) {
                                self.$country.val(data.Country);
                            }
                        }
                    }
                })).autocomplete();
                this.autoCity = this.$city.length > 0 && this.$city.autocomplete(Object.assign({}, settings, {
                    serviceUrl: 'webapi/autocomplete/cities',
                    transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.City, data: x }; }) }; }
                })).autocomplete();
                this.autoState = this.$state.length > 0 && this.$state.autocomplete(Object.assign({}, settings, {
                    serviceUrl: 'webapi/autocomplete/states',
                    formatResult: function (suggestion) { return suggestion.data.State + ", " + suggestion.data.Country; },
                    transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.State, data: x }; }) }; }
                })).autocomplete();
                this.autoZip = this.$zip.length > 0 && this.$zip.autocomplete(Object.assign({}, settings, {
                    serviceUrl: 'webapi/autocomplete/postalcodes',
                    transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.Zip, data: x }; }) }; }
                })).autocomplete();
                this.autoCountry = this.$country.length > 0 && this.$country.autocomplete(Object.assign({}, settings, {
                    serviceUrl: 'webapi/autocomplete/countries'
                })).autocomplete();
            }
            return LocAutoComplete;
        }()),
        Modal: (function () {
            function open(ev) {
                this.btnOpen.enable = false;
                this.el.style.display = 'block';
                var bounds = document.getElementsByClassName('body-content')[0].firstElementChild.getBoundingClientRect();
                this.content.style.top = '60px';
                this.content.style.width = (bounds.width * .8) + 'px';
            }
            function close(ev) {
                if (ev) {
                    if (ev.target == this.el || ev.target == this.btnClose) {
                        this.close();
                    }
                    return;
                }
                this.btnOpen.enable = true;
                this.el.style.display = 'none';
            }
            function Modal(el, btnOpen) {
                this.el = $(el)[0];
                this.btnOpen = $(btnOpen)[0];
                this.btnClose = this.el.querySelector('.close');
                this.content = this.el.querySelector('.modal-content');
                this.header = this.content.querySelector('.modal-header');
                this.body = this.content.querySelector('.modal-body');
                this.footer = this.content.querySelector('.modal-footer');
                this.open = open.bind(this);
                this.close = close.bind(this);
                this.btnOpen.addEventListener('click', this.open);
                this.btnClose.addEventListener('click', this.close);
                window.addEventListener('click', this.close);
            }
            Modal.prototype = {};
            return Modal;
        }())
    }, { override: false, merge: true });
    ZMBA.onDocumentReady(function () {
        document.querySelectorAll("time").forEach(function (el) {
            if (!el.innerText) {
                var datetime = el.getAttribute('dateTime');
                el.innerText = (new Date(datetime)).toLocaleString();
            }
        });
    });
}(window, window.document, window.jQuery, window.ZMBA, window.U = window.U || {}));
U.pages.Account = (function (window, document, $, U, ZMBA) {
    return function AccountPage() {
        function handleFailure(ev) { U.setPageMessage('error', ev.message); }
        document.querySelectorAll(".btnLogoutRecord").forEach(function (el) {
            el.addEventListener('click', function () {
                var tr = el.closest('tr');
                var key = tr.querySelector('.apikey').innerHTML;
                $.ajax({
                    type: "GET",
                    url: "webapi/account/logout?username=" + encodeURIComponent(U.loginCookie.UserName) + "&apikeyorpassword=" + encodeURIComponent(key),
                    error: handleFailure,
                    success: function (ev) {
                        if (!ev.success) {
                            return handleFailure(ev);
                        }
                        U.setPageMessage('success', ev.message);
                        tr.remove();
                        if (tr.className === "current") {
                            window.setTimeout(function () {
                                location.pathname = "";
                            }, 1000);
                        }
                    }
                });
            });
        });
        document.querySelector('#btnLogoutAll').addEventListener('click', function () {
            $.ajax({
                type: "GET",
                url: "webapi/account/logout?username=" + encodeURIComponent(U.loginCookie.UserName),
                error: handleFailure,
                success: function (ev) {
                    if (!ev.success) {
                        return handleFailure(ev);
                    }
                    U.setPageMessage('success', ev.message);
                    window.setTimeout(function () {
                        location.pathname = "";
                    }, 1000);
                }
            });
        });
        document.querySelectorAll('.sendEmailVerification').forEach(function (el) {
            el.addEventListener('click', function () {
                $.ajax({
                    type: "GET",
                    url: "webapi/account/sendverificationemail?email=" + el.name,
                    error: handleFailure,
                    success: function (ev) {
                        if (!ev.success) {
                            return handleFailure(ev);
                        }
                        U.setPageMessage('success', ev.message);
                    }
                });
            });
        });
    };
}(window, window.document, window.jQuery, window.U, window.ZMBA));
U.pages.ApiTest = (function (window, document, $, U, ZMBA) {
    return function ApiTestPage() {
        $.ajax('webapi/metadata').done(function (data) {
            U.dictMetaData = data.result;
            var webMethodSelect = document.getElementById('webMethodSelect');
            webMethodSelect.addEventListener('change', handleWebMethodSelected);
            for (var route in U.dictMetaData) {
                var option = document.createElement('option');
                option.value = route;
                option.innerText = route;
                webMethodSelect.appendChild(option);
            }
        });
        var metadata;
        var btnClear = document.getElementById('btnClear');
        var btnExecute = document.getElementById('btnExecute');
        var currentHttpType = document.getElementById('httpType');
        var route = document.getElementById('route');
        var postBody = document.getElementById('postBody');
        var inputParams = document.getElementById('InputParams');
        var resultJson = document.getElementById('resultJson');
        var milliseconds = document.getElementById('millitime');
        function handleWebMethodSelected(ev) {
            btnClear.disabled = true;
            btnExecute.enable = false;
            while (inputParams.firstElementChild) {
                inputParams.removeChild(inputParams.firstElementChild);
            }
            var option = this.options[this.selectedIndex];
            metadata = U.dictMetaData[option.value];
            if (metadata) {
                btnExecute.disabled = false;
                currentHttpType.value = metadata.httpMethod.toUpperCase();
                route.value = metadata.path;
                if (!metadata.params) {
                    metadata.params = [];
                    metadata.input.forEach(function (param) {
                        param.elem = Element.From("<li class=\"inputparam\"><span>(" + param.typeName + ")</span><label>" + param.name + ":</label><input type=\"text\" param=\"" + param.name + "\" source=\"" + param.source + "\" jsType=\"" + param.jsType + "\" isCollection=\"" + param.isCollection + "\"/></li>");
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
        function handleParamChanage(ev) {
            if (ev) {
                ev.stopPropagation();
            }
            btnClear.disabled = false;
            var oData = U.buildAjaxRequestFromInputs(metadata.params, { url: metadata.path });
            route.value = oData.url;
            postBody.value = JSON.stringify(oData.data, null, '\t');
            if (metadata.path.includes('autocomplete')) {
                executeRequest();
            }
        }
        btnClear.addEventListener('click', function () {
            for (var i = 0, len = metadata.params.length; i < len; i++) {
                metadata.params[i].value = '';
            }
        });
        var start;
        function logStart() { start = performance.now(); }
        function executeRequest() {
            $.ajax({
                type: currentHttpType.value,
                url: route.value,
                data: postBody.value && JSON.parse(postBody.value),
                beforeSend: logStart
            }).done(function (data) {
                milliseconds.innerText = performance.now() - start;
                resultJson.value = JSON.stringify(data, null, '\t');
            });
        }
        btnExecute.addEventListener('click', executeRequest);
        U.executeRequest = executeRequest;
    };
}(window, window.document, window.jQuery, window.U, window.ZMBA));
U.pages.CreateEvent = (function (window, document, $, U, ZMBA, flatpickr) {
    function initAutoCompletion() {
        U.locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });
        var autocompleteSettings = {
            ajaxSettings: {
                cache: true,
                dataType: "json"
            },
            showNoSuggestionNotice: true,
            paramName: 'query',
            transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.name, data: x }; }) }; },
            formatResult: function (suggestion) { return suggestion.data.name + ((suggestion.data.description && " - " + suggestion.data.description) || ''); }
        };
        U.eventType = $("#eventType").autocomplete(Object.assign({}, autocompleteSettings, {
            serviceUrl: 'webapi/autocomplete/eventtypes',
            minChars: 0
        })).autocomplete();
        U.eventTags = new Taggle(document.querySelector('.event_tags_input'), {
            allowDuplicates: false,
            submitKeys: [],
            tagFormatter: function (elem) {
                var el = elem.firstElementChild;
                var tagtext = el.innerText;
                var data = U.eventTags.cache[tagtext];
                if (data) {
                    el.title = data.data.description;
                }
                return elem;
            }
        });
        U.eventTags.cache = {};
        U.eventTags.autocomplete = $(U.eventTags.getInput()).autocomplete(Object.assign({}, autocompleteSettings, {
            serviceUrl: 'webapi/autocomplete/tags',
            minChars: 0,
            onSelect: function (data) {
                U.eventTags.add(data);
            },
            transformResult: function (response) {
                var data = autocompleteSettings.transformResult(response);
                for (var i = 0, len = data.suggestions.length; i < len; i++) {
                    U.eventTags.cache[data.suggestions[i].value.toLowerCase()] = data.suggestions[i];
                }
                return data;
            }
        })).autocomplete();
    }
    function initDatePickers() {
        flatpickr.setDefaults({
            enableTime: true,
            inline: true,
            dateFormat: "Z",
            onReady: function (dObj, dStr, fp, elem) {
                fp.showTimeInput = true;
            }
        });
        var startPicker, endPicker;
        startPicker = flatpickr('[param="DateStart"]', {
            minDate: "today",
            onChange: function (selectedDates, dateStr, fp) {
                var startDate = new Date(startPicker.input.value);
                var endDate = new Date(endPicker.input.value);
                if (!endPicker.input.value || endDate < startDate) {
                    endPicker.setDate(startDate);
                }
                if (startPicker.input.value) {
                    endPicker.config.minDate = startDate;
                    endPicker.config.minTime = startDate;
                }
            }
        });
        endPicker = flatpickr('[param="DateEnd"]', {
            minDate: "today",
            onChange: function (selectedDates, dateStr, fp) {
                var startDate = new Date(startPicker.input.value);
                var endDate = new Date(endPicker.input.value);
                if (!startPicker.input.value) {
                    startPicker.setDate(endDate);
                }
                if (startPicker.input.value) {
                    startPicker.config.maxDate = endDate;
                    startPicker.config.maxTime = endDate;
                }
            }
        });
        U.dateStart = startPicker;
        U.dateEnd = endPicker;
    }
    function initEventCreation() {
        var InputParams = document.getElementById("InputParams");
        var EventCreationLabel = document.getElementById("EventCreationLabel");
        var EventCreationButton = document.getElementById("EventCreationButton");
        EventCreationButton.addEventListener("click", function () {
            EventCreationButton.disabled = true;
            var oRequest = U.buildAjaxRequestFromInputs(InputParams.querySelectorAll("[param]"), { type: "POST", url: "webapi/events/create " });
            if (!U.eventType.selection) {
                U.setPageMessage('error', 'Select an event type');
                U.eventType.element.parentElement.classList.add('required-highlight');
                EventCreationButton.disabled = false;
                return;
            }
            oRequest.data.EventTypeID = U.eventType.selection.data.eventTypeID;
            oRequest.data.Tags = U.eventTags.getTagValues();
            if (oRequest.data.Tags == null || oRequest.data.Tags.length === 0) {
                U.setPageMessage('error', 'Add at least one tag');
                U.eventTags.container.parentElement.classList.add('required-highlight');
                EventCreationButton.disabled = false;
                return;
            }
            function handleFailure(ev) {
                console.log(oRequest, ev);
                EventCreationLabel.value = "Failed!";
                InputParams.class = "Failed";
                U.setPageMessage('error', ev.message);
                U.highlightRequiredInputs(true);
                EventCreationButton.disabled = false;
            }
            $.ajax(oRequest)
                .fail(handleFailure)
                .done(function (ev) {
                if (ev.success) {
                    console.log(oRequest, ev);
                    InputParams.class = "Success";
                    U.setPageMessage('success', 'Success!  Go to the <a href="/Index">Event feed to see events </a>');
                }
                else {
                    handleFailure(ev);
                }
                window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
            });
        });
    }
    function initModals() {
        U.modalCreateEventType = (function () {
            var modal = new U.Modal('#modalCreateEventType', '#btnCreateEventType');
            modal.footer.addEventListener('click', function () {
                var oRequest = U.buildAjaxRequestFromInputs(modal.body.querySelectorAll("[param]"), { type: "POST", url: "webapi/eventtypes/create" });
                function handleFailure(ev) {
                    console.log(oRequest, ev);
                    U.setNotification(modal.el, 'error', ev.message);
                }
                $.ajax(oRequest)
                    .fail(handleFailure)
                    .done(function (ev) {
                    if (ev.success) {
                        U.setNotification(modal.el, 'success', 'Success! EventType Created!');
                        U.eventType.cachedResponse = {};
                        U.eventType.suggestions = [{ value: ev.result.name, data: ev.result }];
                        U.eventType.select(0);
                        window.setTimeout(modal.close, 2000);
                    }
                    else {
                        handleFailure(ev);
                    }
                });
            });
            return modal;
        }());
        U.modalCreateTag = (function () {
            var modal = new U.Modal('#modalCreateTag', '#btnCreateTag');
            modal.footer.addEventListener('click', function () {
                var oRequest = U.buildAjaxRequestFromInputs(modal.body.querySelectorAll("[param]"), { type: "POST", url: "webapi/tags/create" });
                function handleFailure(ev) {
                    console.log(oRequest, ev);
                    U.setNotification(modal.el, 'error', ev.message);
                }
                $.ajax(oRequest)
                    .fail(handleFailure)
                    .done(function (ev) {
                    if (ev.success) {
                        U.setNotification(modal.el, 'success', 'Success! Tag Created!');
                        U.eventTags.add(ev.result.name);
                        window.setTimeout(modal.close, 2000);
                    }
                    else {
                        handleFailure(ev);
                    }
                });
            });
            return modal;
        }());
    }
    return function CreateEventPage() {
        initAutoCompletion();
        initDatePickers();
        initEventCreation();
        initModals();
    };
}(window, window.document, window.jQuery, window.U, window.ZMBA, window.flatpickr));
U.pages.Index = (function (window, document, $, U, ZMBA) {
    function initEventDataFields(target, data, dataFields) {
        for (var i = 0, len = dataFields.length; i < len; i++) {
            var key = dataFields[i];
            target[key] = target.el.getElementsByClassName(key)[0].getElementsByClassName('ef_value')[0];
            if (target[key].tagName === 'TIME') {
                target[key].dateTime = data[key];
                target[key].innerText = (new Date(data[key])).toLocaleString();
            }
            else {
                target[key].innerText = data[key];
            }
        }
        target.event_type = target.el.getElementsByClassName('event_type')[0];
        target.event_type.appendChild(Element.From("<span title=\"" + data.event_type.description + "\">" + data.event_type.name + "</span>"));
        target.tags = target.el.getElementsByClassName('tags')[0].getElementsByClassName('ef_value')[0];
        while (target.tags.firstElementChild) {
            target.tags.firstElementChild.remove();
        }
        if (data.tags) {
            for (var i_1 = 0, len_1 = data.tags.length; i_1 < len_1; i_1++) {
                var tag = data.tags[i_1];
                var eltag = Element.From("<span class='tag' title=\"" + tag.description + "\">" + tag.name + "</span>");
                target.tags.appendChild(eltag);
            }
        }
    }
    var EventModal = (function () {
        var dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "rsvp_later", "rsvp_no", "time_start", "time_end", "description"];
        function rsvpToEventClickHandler(ev) {
            var self = this;
            var name = ev.target.name;
            var req = {
                url: 'webapi/rsvp/toevent?eventid=' + this.data.id + '&rsvpName=' + encodeURIComponent(name),
                type: 'GET',
                dataType: 'json',
                error: function (data) {
                    console.log(req, data);
                    U.setNotification(self.el, 'error', data.message);
                },
                success: function (data) {
                    if (data.success) {
                        self._setActiveRSVP(name);
                    }
                    else {
                        req.error(data);
                    }
                }
            };
            $.ajax(req);
        }
        function EventModal(feedItem) {
            U.Modal.call(this, document.getElementById('Template_EventDetailsModal').cloneNode(true), feedItem.el);
            this.feedItem = feedItem;
            this.el.id = 'efm_' + feedItem.data.id;
            initEventDataFields(this, this.data, dataFields);
            this._requestEventDescription();
            this.rsvpButtons = this.el.getElementsByClassName("set_event_rsvp");
            this._requestUserRSVPStatus();
            var rsvpToEvendHandler = rsvpToEventClickHandler.bind(this);
            for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
                this.rsvpButtons[i].addEventListener('click', rsvpToEvendHandler);
            }
            document.getElementById('EventModalContent').appendChild(this.el);
        }
        EventModal.prototype = U.Modal;
        ZMBA.extendType(EventModal.prototype, {
            get data() { return this.feedItem.data; },
            _requestEventDescription: function () {
                var self = this;
                var oRequest = {
                    url: 'webapi/events/getdescription?id=' + self.data.id,
                    type: 'GET',
                    dataType: 'json',
                    error: function (ev) {
                        console.log(oRequest, ev);
                        U.setNotification(self.el, 'error', ev.message);
                    },
                    success: function (ev) {
                        if (ev.success) {
                            self.description.innerHTML = ev.result;
                        }
                        else {
                            oRequest.error(ev);
                        }
                    }
                };
                $.ajax(oRequest);
            },
            _requestUserRSVPStatus: function () {
                var self = this;
                $.ajax({
                    url: 'webapi/rsvp/getrsvp?eventid=' + self.data.id,
                    type: 'GET',
                    dataType: 'json',
                    success: function (ev) {
                        if (ev.success) {
                            self._setActiveRSVP(ev.result.name);
                        }
                    }
                });
            },
            _setActiveRSVP: function (name) {
                name = name.toLowerCase();
                for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
                    var btn = this.rsvpButtons[i];
                    btn.classList.toggle('selected', btn.name.toLowerCase() == name);
                }
            },
            _rsvpToEventClickHandler: function (ev) {
            }
        });
        return EventModal;
    }());
    var FeedItem = (function () {
        var dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "time_start", "time_end"];
        function _handleClick(ev) {
            ev.stopPropagation();
            if (!this.modal) {
                this.modal = new EventModal(this);
                this.modal.open();
            }
            else {
                this.el.removeEventListener('click', this.handleClick);
            }
        }
        function FeedItem(data) {
            this.modal = null;
            this.data = data;
            this.el = document.getElementById('Template_FeedItem').cloneNode(true);
            this.el.id = "efi_" + this.data.id;
            initEventDataFields(this, data, dataFields);
            this.rsvp_stopby.innerText = data.rsvp_stopby + data.rsvp_later;
            this.handleClick = _handleClick.bind(this);
            this.enableListeners();
        }
        ZMBA.extendType(FeedItem.prototype, {
            enableListeners: function () {
                this.el.addEventListener('click', this.handleClick);
            }
        });
        return FeedItem;
    }());
    var EventFeed = (function () {
        function EventFeed(id) {
            this.loading_spinner = document.querySelector('.loading_spinner');
            this.el = document.getElementById(id);
            this.ul = this.el.querySelector('ul');
            this.feedItems = [];
            var existingItems = this.el.querySelectorAll('.ef_item');
            for (var i = 0, len = existingItems.length; i < len; i++) {
                this.feedItems.push(new FeedItem(existingItems[i]));
            }
        }
        EventFeed.prototype = {
            requestEvents: function (dateFrom, dateTo) {
                this.loading_spinner.style.opacity = '1';
                this.loading_spinner.firstElementChild.style.display = 'block';
                this.loading_spinner.classList.remove('fadeOut');
                var self = this;
                function handleFailure(ev) {
                    console.log(ev);
                    U.setPageMessage('error', ev.message);
                }
                function handleSuccess(ev) {
                    if (ev.success) {
                        self.addEvents(ev.result);
                    }
                    else {
                        handleFailure(ev);
                    }
                }
                var oRequest = {
                    url: 'webapi/events/search',
                    type: 'GET',
                    dataType: 'json',
                    error: handleFailure,
                    success: handleSuccess
                };
                $.ajax(oRequest)
                    .done(function () {
                    setTimeout(function () {
                        self.loading_spinner.style.opacity = '0';
                        self.loading_spinner.firstElementChild.style.display = 'none';
                    }, 1900);
                    self.loading_spinner.classList.add('fadeOut');
                });
            },
            addEvents: function (events) {
                var ul = document.createElement('ul');
                for (var i = 0; i < events.length; i++) {
                    var item = new FeedItem(events[i]);
                    var li = document.createElement('li');
                    li.appendChild(item.el);
                    ul.appendChild(li);
                    this.feedItems.push(item);
                }
                this.el.appendChild(ul);
            }
        };
        return EventFeed;
    }());
    U.EventModal = EventModal;
    U.FeedItem = FeedItem;
    U.EventFeed = EventFeed;
    return function IndexPage() {
        var feed = new U.EventFeed("EventFeed");
        feed.requestEvents();
    };
}(window, window.document, window.jQuery, window.U, window.ZMBA));
U.pages.Login = (function (window, document, $, U, ZMBA) {
    return function LoginPage() {
        var btnLogin = document.getElementById("btnLogin");
        btnLogin.addEventListener("click", handleLogin);
        document.addEventListener('keyup', function (ev) {
            if (ev.keyCode === 13) {
                handleLogin();
            }
        });
        function handleLogin() {
            var oRequest = U.buildAjaxRequestFromInputs(document.querySelectorAll("input[param]"), { type: "GET", url: 'webapi/account/login' });
            function handleError(ev) {
                console.log(ev, oRequest);
                U.setPageMessage('error', ev.message);
                U.highlightRequiredInputs(true);
            }
            $.ajax(oRequest)
                .fail(handleError)
                .done(function (ev) {
                if (ev.success) {
                    U.setPageMessage('success', "Welcome " + ev.result.userName);
                    window.setTimeout(function () {
                        location.pathname = "";
                    }, 1000);
                }
                else {
                    handleError(ev);
                }
            });
        }
    };
}(window, window.document, window.jQuery, window.U, window.ZMBA));
U.pages.SignUp = (function (window, document, $, U) {
    return function SignUpPage() {
        var InputParams = document.getElementById("InputParams");
        var signUpLabel = document.getElementById("signUpLabel");
        var signUpButton = document.getElementById("signUpButton");
        signUpButton.addEventListener("click", function () {
            var oRequest = U.buildAjaxRequestFromInputs(InputParams.querySelectorAll("input[param]"), { type: "POST", url: "webapi/account/createuser" });
            function handleFailure(ev) {
                console.log(oRequest, ev);
                signUpLabel.value = "Failed!";
                InputParams.class = "Failed";
                U.setPageMessage('error', ev.message);
                U.highlightRequiredInputs(true);
            }
            $.ajax(oRequest)
                .fail(handleFailure)
                .done(function (ev) {
                if (ev.success) {
                    signUpButton.disabled = true;
                    console.log(oRequest, ev);
                    signUpLabel.value = "Success!";
                    InputParams.class = "Success";
                    U.setPageMessage('success', 'Success!  Go to the <a href="/Login">Login Page to Login!</a>');
                }
                else {
                    handleFailure(ev);
                }
            });
        });
        U._locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });
    };
}(window, window.document, window.jQuery, window.U));
