
/// <reference path="ZMBA.js" />

/*! UniEvents.js */
(function (window, document, ZMBA, U) {


   ZMBA.extendType(U, {
      rgxTrimUri: /^(\s|\?|\/|&)+|(\s|\?|\/|&)+$/,
      UserAccount: (function () {
         return {
            LoginCookie: document.cookies.getCookieObject("userlogin"),
            get userName() { return this.LoginCookie && this.LoginCookie.userName; },
            get apiKey() { return this.LoginCookie && this.LoginCookie.apiKey; },
            
            TryLogin: function (username, password, onSuccess, onFailure) {
               function handleFailure(ev) { if (onFailure) { onFailure(ev); } }
               $.ajax({ type: "GET", url: `webapi/account/login?UserName=${encodeURIComponent(username)}&Password=${encodeURIComponent(password)}` })
                  .fail(handleFailure)
                  .done(function (ev) {
                     if (ev.success) {
                        this.LoginCookie = ev.result;
                        if (onSuccess) { onSuccess(ev); }
                     } else {
                        handleFailure(ev);
                     }
                  });
            },
            TryLogout: function (onSuccess, onFailure) {
               function handleFailure(ev) { if (onFailure) { onFailure(ev); } }

               $.ajax({ type: "GET", url: `webapi/account/logout?username=${encodeURIComponent(this.userName)}&apikeyorpassword=${encodeURIComponent(this.apiKey)}` })
                  .fail(handleFailure)
                  .done(function (ev) {
                     if (ev.success) {
                        if (onSuccess) { onSuccess(ev); }
                     } else {
                        handleFailure(ev);
                     }
                  })
                  .always(function (ev) {
                     this.LoginCookie = null;
                  });
            }
         };
      }()),
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
         const rgxCSVSplit = /\s*,\s*/;
         var request, querystring, path;

         function setTargetValue(target, name, value, jsType, isCollection) {
            if (isCollection) {
               target[name] = String(value).split(rgxCSVSplit);
            } else {
               target[name] = value;
            }          
         }
         function setParam(name, value, source, jsType, isCollection) {
            if (ZMBA.isNullOrWhitespace(value)) { return; }
            if (source === "QueryString" || source === "Url") {
               querystring += name + "=" + encodeURIComponent(value) + "&";
            } else if (name.indexOf('.') === -1) {
               setTargetValue(request.data, name, value, jsType, isCollection);
            } else {
               var target = request.data;
               var parts = name.split('.');
               while (parts.length > 0) {
                  var part = parts.shift();
                  if (parts.length === 0) {
                     setTargetValue(target, part, value, jsType, isCollection);
                  } else {
                     if (!target[part]) { target[part] = {}; }
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
               for (let i = 0, len = inputs.length; i < len; i++) {
                  let input = inputs[i];
                  if ('getAttribute' in input) {
                     setParam(input.getAttribute("param"), input.value, input.getAttribute("source"), input.getAttribute("jsType"), input.getAttribute("isCollection") === "true" );
                  } else {
                     setParam(input.param, input.value, input.source, input.jsType, input.isCollection);
                  }                  
               }
            } else {
               var names = Object.getOwnPropertyNames(inputs);
               for (let i = 0, len = names.length; i < len; i++) {
                  let name = names[i];
                  let input = inputs[name];
                  setParam(name, input.value, input.source, input.jsType, input.isCollection);
               }
            }

            request.url = (path + querystring).replace(U.rgxTrimUri, '');
            return request;
         }
      }()),
      setPageMessage: (function () {
         const classmap = { 0: 'success', 1: 'info', 2: 'alert', 3: 'error' };
         return function (type, message, timeout) {
            var div = document.getElementById('divPageMessage');
            div.className = classmap[type] || type;
            div.innerHTML = "";
            if (message instanceof Node) {
               div.appendChild(message);
            } else {
               div.innerHTML = message;
            }
         }
      }()),
      highlightRequiredInputs: function (bool) {
         document.body.classList.toggle('highlightRequiredInputs', bool);
      },

      LocationAutoComplete: (function () {

         LocAutoComplete.defaults = {
            search: null,
            city: "[param='Location.Locality']",
            state: "[param='Location.AdminDistrict']",
            zip: "[param='Location.PostalCode']",
            country: "[param='Location.CountryRegion']"
         }

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
               transformResult: (response) => { return { suggestions: response.result.map(x => { return { value: x.Formatted, data: x } }) } },
               formatResult: (suggestion) => suggestion.data.Formatted,
               onSearchStart: function (query) {
                  query.query = self.$els.map(function () { return this.value; }).get().join(' ');
               }
            }

            this.autoSearch = this.$search.length > 0 && this.$search.autocomplete(Object.assign({}, settings, {
               serviceUrl: 'webapi/autocomplete/locations',
               onSearchStart: () => { },
               onSelect: (suggestion) => {
                  var data = suggestion.data;
                  if(data) {
                     if (data.City) { self.$city.val(data.City); }
                     if (data.State) { self.$state.val(data.State); }
                     if (data.Zip) { self.$zip.val(data.Zip); }
                     if (data.Country) { self.$country.val(data.Country); }
                  }
               }
            })).autocomplete();

            this.autoCity = this.$city.length > 0 && this.$city.autocomplete(Object.assign({}, settings, {
               serviceUrl: 'webapi/autocomplete/cities',
               transformResult: (response) => { return { suggestions: response.result.map(x => { return { value: x.City, data: x } }) } }
            })).autocomplete();

            this.autoState = this.$state.length > 0 && this.$state.autocomplete(Object.assign({}, settings, {
               serviceUrl: 'webapi/autocomplete/states',
               formatResult: (suggestion) => suggestion.data.State + ", " + suggestion.data.Country,
               transformResult: (response) => { return { suggestions: response.result.map(x => { return { value: x.State, data: x } }) } }
            })).autocomplete();

            this.autoZip = this.$zip.length > 0 && this.$zip.autocomplete(Object.assign({}, settings, {
               serviceUrl: 'webapi/autocomplete/postalcodes',
               transformResult: (response) => { return { suggestions: response.result.map(x => { return { value: x.Zip, data: x } }) } }
            })).autocomplete();

            this.autoCountry = this.$country.length > 0 && this.$country.autocomplete(Object.assign({}, settings, {
               serviceUrl: 'webapi/autocomplete/countries'
            })).autocomplete();


         }

         return LocAutoComplete;
      }())

   }, { override: false, merge: true });

 
   ZMBA.onDocumentReady(() => {        
      document.querySelectorAll("time").forEach(function (el) {
         if (!el.innerText) {
            el.innerText = (new Date(el.dateTime)).toLocaleString();
         }
      });
   });

   $.ajaxSetup({ cache: false });
   
}(window, window.document, window.ZMBA, window.U = window.U || {}));






