﻿///<reference path="unievents.js"/>


U.pages.Index = (function (window, document, $, U, ZMBA) {

   function initEventDataFields(target, data, dataFields) {
      for (var i = 0, len = dataFields.length; i < len; i++) {
         var key = dataFields[i];
         target[key] = target.el.getElementsByClassName(key)[0].getElementsByClassName('ef_value')[0];
         if (target[key].tagName === 'TIME') {
            target[key].dateTime = data[key];
            target[key].innerText = (new Date(data[key])).toLocaleString();
         } else {
            target[key].innerText = data[key];
         }

      }

      target.event_type = target.el.getElementsByClassName('event_type')[0].getElementsByClassName('ef_value')[0];
      while (target.event_type.firstElementChild) { target.event_type.firstElementChild.remove(); }
      target.event_type.appendChild(Element.From(`<span title="${data.event_type.description}">${data.event_type.name}</span>`));

      target.tags = target.el.getElementsByClassName('tags')[0].getElementsByClassName('ef_value')[0];
      while (target.tags.firstElementChild) { target.tags.firstElementChild.remove(); }
      if (data.tags) {
         for (let i = 0, len = data.tags.length; i < len; i++) {
            var tag = data.tags[i];
            var eltag = Element.From(`<span class='tag' title="${tag.description}">${tag.name}</span>`);
            target.tags.appendChild(eltag);
         }
      }
   }


   const EventModal = (function () { //Build a type.  Think of it as buildign a C# class at runtime. 

      const dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "rsvp_later", "rsvp_no", "time_start", "time_end", "details"];

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
                  self.doFullRefresh();
                  //self._setActiveRSVP(name);
               } else {
                  req.error(data);
               }
            }
         }
         $.ajax(req);
      }


      function handleSubmitEdit(ev) {
         var self = this;
         var oRequest = {
            url: 'webapi/events/update?EventID=' + self.data.id,
            type: 'POST',
            dataType: 'json',
            data: {
               Title: self.title.innerText,
               Caption: self.caption.innerText,
               Description: self._detailsEditor.value,
               DateStart: self.time_start.innerText,
               DateEnd: self.time_end.innerText
            },
            error: function (ev) {
               console.log(oRequest, ev);
               U.setNotification(self.el, 'error', ev.message);
            },
            success: function (ev) {
               if (ev.success) {
                  U.setNotification(self.el, 'success', "Success! " + ev.message);
                  handleCancelEdit.call(self);
                  self.doFullRefresh();
               } else {
                  oRequest.error(ev);
               }
            }
         }
         $.ajax(oRequest);
      }

      function handleCancelEdit(ev) {
         this._isediting = false;
         this.btnStartEdit.firstElementChild.innerText = "EDIT";    

         this.header.querySelector('h2').innerText = "Event Information";
         while (this.footer.firstElementChild) { this.footer.firstElementChild.remove(); }
         for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
            this.footer.appendChild(this.rsvpButtons[i]);
         }

         this.details.innerHTML = this._detailsEditor.value;
         this._detailsEditor.remove();
         this._detailsParent.appendChild(this.details);

         this.el.querySelectorAll('.editable').forEach(elem => {
            elem.contentEditable = 'false';
            elem.classList.remove('editing');
         });

      }

      function handleEditClick() {
         if (this._isediting) {
            this.editorCancel();
         } else {
            this._isediting = true;
            this.btnStartEdit.firstElementChild.innerText = "CANCEL EDIT";

            this.header.querySelector('h2').innerText = "Event Quick Editor";
            for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
               this.rsvpButtons[i].remove();
            }

            this.btnCancelEdit = Element.From(`<button class="btn btnCancelEdit">Cancel Edit</button>`);
            this.btnCancelEdit.addEventListener('click', this.editorCancel);
            this.btnConfirmEdit = Element.From(`<button class="btn btnConfirmEdit">Save Changes</button>`);
            this.btnConfirmEdit.addEventListener('click', this.editorSubmit);
            this.footer.appendChild(this.btnConfirmEdit);
            this.footer.appendChild(this.btnCancelEdit);

            var rect = this.details.getBoundingClientRect();
            this._detailsParent = this.details.parentElement;
            this._detailsEditor = Element.From(`<textarea class="editable" contenteditable="true" style="height:${Math.max(150, Math.min(window.innerWidth * .7, rect.height))}px"></textarea>`);
            this._detailsEditor.innerHTML = this.details.innerHTML;
            this.details.remove();
            this._detailsParent.appendChild(this._detailsEditor);

            this.el.querySelectorAll('.editable').forEach(elem => {
               elem.contentEditable = 'true';
               elem.classList.add('editing');
            });
         }
      }

      function EventModal(feedItem) {
         U.Modal.call(this, document.getElementById('Template_EventDetailsModal').cloneNode(true), feedItem.el);        
         this.feedItem = feedItem;
         this.el.id = 'efm_' + feedItem.data.id;
         this.rsvpButtons = this.footer.querySelectorAll(".set_event_rsvp");
         this.spinner = this.el.querySelector('.loading_spinner');
         this.btnStartEdit = this.header.querySelector('.btnStartEdit');
         this.updateFields(this.data);

         this.editorOpen = handleEditClick.bind(this);
         this.editorCancel = handleCancelEdit.bind(this);
         this.editorSubmit = handleSubmitEdit.bind(this);
       
         var rsvpToEvendHandler = rsvpToEventClickHandler.bind(this);
         for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
            this.rsvpButtons[i].addEventListener('click', rsvpToEvendHandler);
         }

         document.getElementById('EventModalContent').appendChild(this.el);
         this.doFullRefresh();

         this.btnStartEdit.addEventListener('click', this.editorOpen);

      }

      EventModal.prototype = U.Modal;

      ZMBA.extendType(EventModal.prototype, {
         get data() { return this.feedItem.data; },
         updateFields: function (data) {
            initEventDataFields(this, data, dataFields);;
            this.details.innerHTML = data.details;
            if (data.user_rsvp_status) {
               var rsvpname = data.user_rsvp_status.toLowerCase();
               for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
                  var btn = this.rsvpButtons[i];
                  btn.classList.toggle('selected', btn.name.toLowerCase() === rsvpname);
               }
            }
            this.spinner.classList.remove('details_loading');
            this.spinner.classList.add('details_loaded');
            this.btnStartEdit.style.display = data.can_edit_event ? 'block' : 'none';
         },
         doFullRefresh: function () {
            this.spinner.classList.remove('details_loaded');
            this.spinner.classList.add('details_loading');
            var self = this;
            var oRequest = {
               url: 'webapi/events/getbyidwithuserview?EventID=' + self.data.id,
               type: 'GET',
               dataType: 'json',
               error: function (ev) {
                  console.log(oRequest, ev);
                  U.setNotification(self.el, 'error', ev.message);
               },
               success: function (ev) {
                  if (ev.success) {
                     self.updateFields(ev.result);
                     self.feedItem.updateFields(ev.result);
                  } else {
                     oRequest.error(ev);
                  }
               }
            }
            $.ajax(oRequest);
         },
         _setActiveRSVP: function(name) {
            name = name.toLowerCase();
            for (var i = 0, len = this.rsvpButtons.length; i < len; i++) {
               var btn = this.rsvpButtons[i];
               btn.classList.toggle('selected', btn.name.toLowerCase() == name);
            }

         }
      });


      return EventModal;
   }());


   const FeedItem = (function () { //Build a type
      const dataFields = ["title", "caption", "host", "location", "address", "rsvp_attending", "rsvp_stopby", "rsvp_maybe", "time_start", "time_end"];

      function _handleClick(ev) {
         ev.stopPropagation();
         if (!this.modal) {
            this.modal = new EventModal(this);
            this.modal.open();
         } else {
            this.el.removeEventListener('click', this.handleClick);
         }
      }

      function FeedItem(data) {
         this.modal = null;
         this.data = data;
         this.el = document.getElementById('Template_FeedItem').cloneNode(true);
         this.el.id = "efi_" + this.data.id;
         this.el.dataset.id = this.data.id;

         this.updateFields(data);       

         this.handleClick = _handleClick.bind(this);
         this.el.addEventListener('click', this.handleClick);

         var start = new Date(data.time_start);
         var end = new Date(data.time_end);
         var now = Date.Current;
         if (now < start) {
            this.el.classList.add('event-future');
         } else if (end < now) {
            this.el.classList.add('event-past');
         } else if (now < end && now > start) {
            this.el.classList.add('event-ongoing');
         }
      }

      ZMBA.extendType(FeedItem.prototype, {
         updateFields: function (data) {
            initEventDataFields(this, data, dataFields);;
            this.rsvp_stopby.innerText = data.rsvp_stopby + data.rsvp_later;
            this.data = data;
         }
      });

      return FeedItem;
   }());


   const EventFeed = (function () { //Build a type

      function EventFeed(id) {
         this.loading_spinner = document.querySelector('.loading_spinner.feed_loading');
         this.el = document.getElementById(id);
         this.ul = this.el.querySelector('ul');
         this.feedItems = {};
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
                  ev.result.dateFrom = dateFrom;
                  ev.result.dateTo = dateTo;
                  self.addEvents(ev.result);
               } else {
                  handleFailure(ev);
               }
            }
            var oRequest = {
               url: 'webapi/events/search',
               type: 'GET',
               dataType: 'json',
               error: handleFailure,
               success: handleSuccess
            }
            oRequest = U.buildAjaxRequestFromInputs({
               DateFrom: { source: "QueryString", value: dateFrom },
               DateTo: { source: "QueryString", value: dateTo }
            }, oRequest);
            $.ajax(oRequest)
               .done(() => {
                  setTimeout(() => {
                     self.loading_spinner.style.opacity = '0';
                     self.loading_spinner.firstElementChild.style.display = 'none';
                  }, 1900);
                  self.loading_spinner.classList.add('fadeOut');
               });

         },
         addEvents: function (events) {
            for (var i = 0; i < events.length; i++) {
               var data = events[i];
               if (!this.feedItems[data.id]) {
                  var item = new FeedItem(data);
                  this.feedItems[data.id] = item;
                  this.ul.appendChild(item.el);
               }
            }
            this._sortEvents();
         },
         _sortEvents: (function () {
            function cmp(el_a, el_b) {
               var a = this.feedItems[el_a.dataset.id];
               var b = this.feedItems[el_b.dataset.id];
               var astart = a.data.time_start;
               var bstart = b.data.time_start;
               var aend = a.data.time_end;
               var bend = b.data.time_end;
               return (astart < bstart ? -1 : (astart > bstart ? 1 : 0)) || (aend < bend ? -1 : (aend > bend ? 1 : 0));
            }
            return function () {
               var lis = this.ul.querySelectorAll("li.efi").ToArray().sort(cmp.bind(this));
               for (var i = 0, len = lis.length; i < len; i++) {
                  this.ul.appendChild(lis[i]);
               }
            };
         }())
           
      };

      return EventFeed;
   }());


   U.EventModal = EventModal;
   U.FeedItem = FeedItem;
   U.EventFeed = EventFeed;


   return function IndexPage() {
      U.eventFeed = new U.EventFeed("EventFeed");
      U.eventFeed.requestEvents(Date.Current.toUTCString());


      var btnLoadOlderEvents = document.getElementById("btnLoadOlderEvents");
      btnLoadOlderEvents.addEventListener("click", function () {
         U.eventFeed.requestEvents(null, Date.Current.toUTCString());
         btnLoadOlderEvents.style.display = 'none';
      });
   }

}(window, window.document, window.jQuery, window.U, window.ZMBA));

