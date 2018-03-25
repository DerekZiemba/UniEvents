
/// <reference path="ZMBA.js" />

/*! UniEvents.js */
(function (window, document, $, ZMBA, U) {

   $.ajaxSetup({ cache: false });

   ZMBA.extendType(U, {
      rgxTrimUri: /^(\s|\?|\/|&)+|(\s|\?|\/|&)+$/,
      UserAccount: (function () {
         return {
            LoginCookie: document.cookies.getCookieObject("userlogin"),
            get userName() { return this.LoginCookie && this.LoginCookie.userName; },
            get apiKey() { return this.LoginCookie && this.LoginCookie.apiKey; },
            
            TryLogin: function (username, password, onSuccess, onFailure) {
               function handleFailure(ev) { if (onFailure) { onFailure(ev); } }

               $.ajax({ type: "GET", url: `webapi/account/login?UserName=${encodeURI(username)}&Password=${encodeURI(password)}` })
                  .fail(handleFailure)
                  .done(function (ev) {
                     if (ev.success) {
                        this.LoginCookie = ev.result;
                        document.cookies.setCookie("userlogin", ev.result, Date.Current.AddDays(14));
                        if (onSuccess) { onSuccess(ev); }
                     } else {
                        handleFailure(ev);
                     }
                  });
            },
            TryLogout: function (onSuccess, onFailure) {
               function handleFailure(ev) { if (onFailure) { onFailure(ev); } }

               $.ajax({ type: "GET", url: `webapi/account/logout?UserName=${encodeURI(this.userName)}&keypass=${encodeURI(this.apiKey)}` })
                  .fail(handleFailure)
                  .done(function (ev) {
                     if (ev.success) {
                        if (onSuccess) { onSuccess(ev); }
                     } else {
                        handleFailure(ev);
                     }
                  })
                  .always(function (ev) {
                     document.cookies.removeCookie("userlogin");
                     this.LoginCookie = null;
                  });
            }
         };
      }()),
      getRouteMetadata: function (route, cb) {
         $.ajax({
            cache: false,
            type: "GET",
            url: 'webapi/metadata?route=' + encodeURI(route),
            success: function (data) {
               if (data.result) {
                  cb(data.result);
               }
            }
         });
      },
      buildAjaxRequestFromInputs: (function () {
         var request, querystring, path;

         function setParam(name, value, source) {
            if (ZMBA.IsNullOrWhitespace(value)) { return; }
            if (source === "QueryString" || source === "Url") {
               querystring += name + "=" + encodeURI(value) + "&";
            } else if (name.indexOf('.') === -1) {
               request.data[name] = value;
            } else {
               var target = request.data;
               var parts = name.split('.');
               while (parts.length > 0) {
                  var part = parts.shift();
                  if (parts.length === 0) {
                     target[part] = value;
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
                  let bHasGetAttrib = 'getAttribute' in input;
                  setParam(bHasGetAttrib ? input.getAttribute("param") : input.param, input.value, bHasGetAttrib ? input.getAttribute("source") : input.source);
               }
            } else {
               var names = Object.getOwnPropertyNames(inputs);
               for (let i = 0, len = names.length; i < len; i++) {
                  let name = names[i];
                  setParam(name, inputs[name].value, inputs[name].source);
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
      }
   }, { override: false, merge: true });



}(window, window.document, window.jQuery, window.ZMBA, window.U = window.U || {}));






