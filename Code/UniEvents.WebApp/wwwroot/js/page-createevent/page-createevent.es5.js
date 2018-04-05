"use strict";
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
            }
            else {
                handleFailure(ev);
            }
        });
    });
    U._locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });
    flatpickr.setDefaults({
        enableTime: true,
        inline: true,
        dateFormat: "Z",
        onReady: function (dObj, dStr, fp, elem) {
            fp.showTimeInput = true;
        }
    });
    var startPicker = flatpickr('[param="DateStart"]', {
        minDate: "today",
        onChange: function (selectedDates, dateStr, fp) {
            var startDate = new Date(startPicker.input.value);
            var endDate = new Date(endPicker.input.value);
            if (!endPicker.input.value || endDate < startDate) {
                endPicker.setDate(startDate);
            }
            if (startPicker.input.value) {
                endPicker.config.minDate = startDate;
                endPicker.config.minTime = startDate;
            }
        }
    });
    var endPicker = flatpickr('[param="DateEnd"]', {
        minDate: "today",
        onChange: function (selectedDates, dateStr, fp) {
            var startDate = new Date(startPicker.input.value);
            var endDate = new Date(endPicker.input.value);
            if (!startPicker.input.value) {
                startPicker.setDate(endDate);
            }
            if (startPicker.input.value) {
                startPicker.config.maxDate = endDate;
                startPicker.config.maxTime = endDate;
            }
        }
    });
    U._startDatePicker = startPicker;
    U._endDatePicker = endPicker;
    var autocompleteSettings = {
        ajaxSettings: {
            cache: true,
            dataType: "json"
        },
        showNoSuggestionNotice: true,
        paramName: 'query',
        transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.name, data: x }; }) }; },
        formatResult: function (suggestion) { return suggestion.data.name + (suggestion.data.description && " - " + suggestion.data.description); }
    };
    U._autoEventType = $("#eventType").autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/eventtypes'
    })).autocomplete();
    U._autoTags = $("#eventTags").autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/tags',
    })).autocomplete();
}());
