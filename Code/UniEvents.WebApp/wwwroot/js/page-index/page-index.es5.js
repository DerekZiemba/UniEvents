"use strict";
(function (window, document, $, U, ZMBA, factory) {
    factory(window, document, $, U, ZMBA);
    var feed = new U.EventFeed("EventFeed");
    document.getElementById("TenMoreButton").addEventListener("click", function () {
        feed.loadMoreEvents(10);
    });
}(window, window.document, window.jQuery, window.U, window.ZMBA, function Factory(window, document, $, U, ZMBA) {
    function GetLastChildOrSelf(el) {
        return el && el.lastElementChild ? el.lastElementChild : el;
    }
    var EventModal = (function () {
        var divTemplate = document.getElementById('Template_EventDetailsModal');
        var divEventModalContent = document.getElementById('EventModalContent');
        EventModal.elemClassNames = ["title", "caption", "host", "location", "address", "rsvp_attending", "description", "close", "join_event"];
        EventModal.elemClassNames.forEach(function (key) {
            Object.defineProperty(EventModal.prototype, "el_" + key, { get: function () { return GetLastChildOrSelf(this.el.getElementsByClassName(key)[0]); } });
        });
        function _handleCloseClick(ev) {
            ev.stopPropagation();
            this.toggle(false);
        }
        function _handleModalClick(ev) {
            ev.stopPropagation();
        }
        function _handleWindowClick(ev) {
            this.toggle(false);
        }
        function EventModal(feedItem) {
            this.feedItem = feedItem;
            this.el = divTemplate.cloneNode(true);
            this.handleWindowClick = _handleWindowClick.bind(this);
            this.handleModalClick = _handleModalClick.bind(this);
            this.handleCloseClick = _handleCloseClick.bind(this);
            this.el_description.innerText = feedItem.description;
            this.el.addEventListener('click', this.handleModalClick);
            this.el_close.addEventListener('click', this.handleCloseClick);
            divEventModalContent.appendChild(this.el);
        }
        ZMBA.extendType(EventModal.prototype, {
            toggle: function (bool) {
                if (arguments.length === 0) {
                    bool = this.el.style.display === 'block' ? false : true;
                }
                if (bool) {
                    window.addEventListener('click', this.handleWindowClick);
                    this.el.style.display = 'block';
                }
                else {
                    window.removeEventListener('click', this.handleWindowClick);
                    this.el.style.display = 'none';
                }
            }
        });
        return EventModal;
    }());
    var FeedItem = (function () {
        var divTemplate = document.getElementById('Template_FeedItem');
        FeedItem.dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "time_start", "time_end"];
        FeedItem.dataFields.forEach(function (key) {
            Object.defineProperty(FeedItem.prototype, "el_" + key, { get: function () { return GetLastChildOrSelf(this.el.getElementsByClassName(key)[0]); } });
        });
        function _handleClick(ev) {
            ev.stopPropagation();
            if (!this.modal) {
                this.modal = new EventModal(this);
            }
            this.modal.toggle(true);
        }
        function FeedItem(elOrData) {
            if (elOrData instanceof HTMLElement) {
                this.el = elOrData;
                this.data = {};
                this.data.id = this.el.id;
                this.data.startTime = this.el_time_start.dateTime;
                this.data.endTime = this.el_time_end.dateTime;
                for (var i = 0, len = FeedItem.dataFields.length; i < len; i++) {
                    var key = FeedItem.dataFields[i];
                    this.data[key] = this["el_" + key].innerText;
                }
            }
            else {
                this.el = divTemplate.cloneNode(true);
                this.data = elOrData;
                this.el.id = this.data.id;
                this.el_time_start.dateTime = this.data.startTime;
                this.el_time_end.dateTime = this.data.endTime;
                for (var i = 0, len = FeedItem.dataFields.length; i < len; i++) {
                    var key = FeedItem.dataFields[i];
                    this["el_" + key].innerText = this.data[key];
                }
            }
            this.modal = null;
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
            this.el = document.getElementById(id);
            this.ul = this.el.querySelector('ul');
            this.feedItems = [];
            var existingItems = this.el.querySelectorAll('.ef_item');
            for (var i = 0, len = existingItems.length; i < len; i++) {
                this.feedItems.push(new FeedItem(existingItems[i]));
            }
        }
        ZMBA.extendType(EventFeed.prototype, {
            loadMoreEvents: function (count) {
                for (var i = 0; i < count; i++) {
                    var item = new FeedItem({
                        id: "efi_" + this.feedItems.length,
                        title: "Newly added event",
                        caption: "Added through loadMoreEvents",
                        description: "My parents are going out",
                        host: "You are",
                        location: "wherever u want bby",
                        address: "u kno where",
                        time_start: new Date(),
                        time_end: (new Date()).AddHours(1),
                        rsvp_attending: "u + me"
                    });
                    var li = document.createElement('li');
                    li.appendChild(item.el);
                    this.ul.appendChild(li);
                    this.feedItems.push(item);
                }
            }
        });
        return EventFeed;
    }());
    U.EventModal = EventModal;
    U.FeedItem = FeedItem;
    U.EventFeed = EventFeed;
}));
