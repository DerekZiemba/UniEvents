
(function (factory) {

   factory(window, window.document, window.jQuery, window.U, window.ZMBA);

}(function Factory(window, document, $, U, ZMBA) {

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
            } else {
               handleFailure(ev);
            }
         });
   });

   U._locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });


}));

