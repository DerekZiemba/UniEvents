﻿
///<reference path="unievents.js"/>

U.pages.Account = (function (window, document, $, U, ZMBA) {

   return function AccountPage() {

      function handleFailure(ev) { U.setPageMessage('error', ev.message); }

      document.querySelectorAll(".btnLogoutRecord").forEach(function (el) {
         el.addEventListener('click', function () {
            var tr = el.closest('tr');
            var key = tr.querySelector('.apikey').innerHTML;
            $.ajax({
               type: "GET",
               url: "webapi/account/logout?username=@HttpUtility.UrlEncode(Model.UserContext.UserName)&apikeyorpassword=" + encodeURIComponent(key),
               error: handleFailure,
               success: function (ev) {
                  if (!ev.success) { return handleFailure(ev); }
                  U.setPageMessage('success', ev.message);
                  tr.remove();
                  if (tr.className === "current") {
                     window.setTimeout(function () {
                        location.pathname = "";//Go to Homepage
                     }, 1000);
                  }
               }
            });

         });
      });

      document.querySelector('#btnLogoutAll').addEventListener('click', function () {
         $.ajax({
            type: "GET",
            url: "webapi/account/logout?username=@HttpUtility.UrlEncode(Model.UserContext.UserName)",
            error: handleFailure,
            success: function (ev) {
               if (!ev.success) { return handleFailure(ev); }
               U.setPageMessage('success', ev.message);
               window.setTimeout(function () {
                  location.pathname = "";//Go to Homepage
               }, 1000);
            }
         });
      });

   }

}(window, window.document, window.jQuery, window.U, window.ZMBA));