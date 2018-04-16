
///<reference path="unievents.js"/>

U.pages.Login = (function (window, document, $, U, ZMBA) {

   return function LoginPage() {

      var btnLogin = document.getElementById("btnLogin");

      btnLogin.addEventListener("click", handleLogin);
      document.addEventListener('keyup', function (ev) {
         if (ev.keyCode === 13) { //Enter key
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
                     location.pathname = "";//Go to Homepage
                  }, 1000);
               } else {
                  handleError(ev);
               }
            });

      }

   }

}(window, window.document, window.jQuery, window.U, window.ZMBA));