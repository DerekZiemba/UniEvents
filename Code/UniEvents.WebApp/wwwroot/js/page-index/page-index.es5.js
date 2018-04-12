"use strict";
(function (window, document, $, U, ZMBA, factory) {
    factory(window, document, $, U, ZMBA);
    var feed = new U.EventFeed("EventFeed");
    feed.requestEvents();
}(window, window.document, window.jQuery, window.U, window.ZMBA, function Factory(window, document, $, U, ZMBA) {
    function GetTarget(el) {
        return el && (el.getElementsByClassName('ef_value')[0] || el);
    }
    var EventModal = (function () {
        var divTemplate = document.getElementById('Template_EventDetailsModal');
        var divEventModalContent = document.getElementById('EventModalContent');
        EventModal.elemClassNames = ["title", "caption", "host", "location", "address", "rsvp_attending", "description", "close", "join_event"];
        EventModal.elemClassNames.forEach(function (key) {
            Object.defineProperty(EventModal.prototype, "el_" + key, { get: function () { return GetTarget(this.el.getElementsByClassName(key)[0]); } });
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
        FeedItem.dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "time_start", "time_end"];
        FeedItem.dataFields.forEach(function (key) {
            Object.defineProperty(FeedItem.prototype, "el_" + key, { get: function () { return GetTarget(this.el.getElementsByClassName(key)[0]); } });
        });
        function _handleClick(ev) {
            ev.stopPropagation();
            if (!this.modal) {
                this.modal = new EventModal(this);
            }
            this.modal.toggle(true);
        }
        function FeedItem(data) {
            this.el = divTemplate.cloneNode(true);
            this.data = data;
            this.el.id = "efi_" + this.data.id;
            for (var i = 0, len = FeedItem.dataFields.length; i < len; i++) {
                var key = FeedItem.dataFields[i];
                this["el_" + key].innerText = data[key];
            }
            this.el_time_start.dateTime = data.time_start;
            this.el_time_end.dateTime = data.time_end;
            this.el_time_start.innerText = (new Date(data.time_start)).toLocaleString();
            this.el_time_end.innerText = (new Date(data.time_end)).toLocaleString();
            this.modal = null;
            this.el_event_type = this.el.querySelector('.event_type');
            if (data.event_type) {
                this.el_event_type.appendChild(Element.From("<span title=\"" + data.event_type.description + "\">" + data.event_type.name + "</span>"));
            }
            this.el_tags = this.el.querySelector('.tags');
            if (data.tags) {
                for (var i = 0, len = data.tags.length; i < len; i++) {
                    var tag = data.tags[i];
                    var eltag = Element.From("<span class='tag' title=\"" + tag.description + "\">" + tag.name + "</span>");
                    this.el_tags.appendChild(eltag);
                }
            }
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
        ZMBA.extendType(EventFeed.prototype, {
            requestEvents: function (dateFrom, dateTo) {
                this.loading_spinner.style.opacity = '1';
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
                    setTimeout(function () { self.loading_spinner.style.opacity = '0'; }, 2000);
                    self.loading_spinner.classList.add('fadeOut');
                });
            },
            addEvents: function (events) {
                var ul = document.createElement('ul');
                for (var i = 0; i < events.length; i++) {
                    var event = events[i];
                    var item = new FeedItem({
                        id: event.eventID,
                        title: event.title,
                        caption: event.caption,
                        host: event.host,
                        location: event.locationName,
                        address: event.addressLine,
                        time_start: event.dateStart,
                        time_end: event.dateEnd,
                        rsvp_attending: event.rsvp_attending,
                        rsvp_stopby: event.rsvp_stopby + event.rsvp_later,
                        rsvp_maybe: event.rsvp_maybe,
                        rsvp_no: event.rsvp_no,
                        event_type: event.eventType,
                        tags: event.tags
                    });
                    var li = document.createElement('li');
                    li.appendChild(item.el);
                    ul.appendChild(li);
                    this.feedItems.push(item);
                }
                this.el.appendChild(ul);
            }
        });
        return EventFeed;
    }());
    U.EventModal = EventModal;
    U.FeedItem = FeedItem;
    U.EventFeed = EventFeed;
}));
