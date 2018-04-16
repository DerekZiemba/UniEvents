"use strict";
(function (window, document, $, U, ZMBA, factory) {
    factory(window, document, $, U, ZMBA);
    var feed = new U.EventFeed("EventFeed");
    feed.requestEvents();
}(window, window.document, window.jQuery, window.U, window.ZMBA, function Factory(window, document, $, U, ZMBA) {
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
        var divEventModalContent = document.getElementById('EventModalContent');
        var divTemplate = document.getElementById('Template_EventDetailsModal');
        var dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "rsvp_later", "rsvp_no", "time_start", "time_end", "description"];
        function EventModal(feedItem) {
            U.Modal.call(this, divTemplate.cloneNode(true), feedItem.el);
            this.feedItem = feedItem;
            this.el.id = 'efm_' + feedItem.data.id;
            initEventDataFields(this, this.data, dataFields);
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
            divEventModalContent.appendChild(this.el);
        }
        EventModal.prototype = U.Modal;
        ZMBA.extendType(EventModal.prototype, {
            get data() { return this.feedItem.data; }
        });
        return EventModal;
    }());
    var FeedItem = (function () {
        var divTemplate = document.getElementById('Template_FeedItem');
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
            this.el = divTemplate.cloneNode(true);
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
        ZMBA.extendType(EventFeed.prototype, {
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
        });
        return EventFeed;
    }());
    U.EventModal = EventModal;
    U.FeedItem = FeedItem;
    U.EventFeed = EventFeed;
}));
