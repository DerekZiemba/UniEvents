

(function () {

   var InputParams = document.getElementById("InputParams");
   var EventCreationLabel = document.getElementById("EventCreationLabel");
   var EventCreationButton = document.getElementById("EventCreationButton");

   EventCreationButton.addEventListener("click", function () {
      var oRequest = U.buildAjaxRequestFromInputs(InputParams.querySelectorAll("input[param]"), { type: "POST", url: "webapi/TODO " });

      function handleFailure(ev) {
         console.log(oRequest, ev);
         EventCreationLabel.value = "Failed!";
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
               U.setPageMessage('success', 'Success!  Go to the <a href="/Index">Event feed to see events </a>');
            } else {
               handleFailure(ev);
            }
         });

   });

   U._locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });

   var datepickerOptions = {
      minDate: "today",
      enableTime: true,
      inline: true,
      onReady: function (arg0, arg1, inst) {
         console.log(inst):
         inst.showTimeInput = true;
      }
   }
   U._startDatePicker = flatpickr('[param="DateStart"]', datepickerOptions);
   U._endDatePicker = flatpickr('[param="DateEnd"]', datepickerOptions);


}());

