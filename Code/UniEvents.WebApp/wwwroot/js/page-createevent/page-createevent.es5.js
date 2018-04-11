"use strict";
(function () {
    U._locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });
    var autocompleteSettings = {
        ajaxSettings: {
            cache: true,
            dataType: "json"
        },
        showNoSuggestionNotice: true,
        paramName: 'query',
        transformResult: function (response) { return { suggestions: response.result.map(function (x) { return { value: x.name, data: x }; }) }; },
        formatResult: function (suggestion) { return suggestion.data.name + ((suggestion.data.description && " - " + suggestion.data.description) || ''); }
    };
    U._autoEventType = $("#eventType").autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/eventtypes',
        minChars: 0
    })).autocomplete();
    var eventTags = U._eventTags = new Taggle(document.querySelector('.event_tags_input'), {
        allowDuplicates: false,
        submitKeys: [],
        onBeforeTagAdd: function (ev, tag) {
            var x = 0;
        },
        onTagAdd: function (ev, tag) {
            var x = 0;
        },
        tagFormatter: function (el) {
            var x = 0;
        }
    });
    var tagsInput = eventTags.getInput();
    U._autoTags = $(tagsInput).autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/tags',
        minChars: 0,
        onSelect: function (data) {
            eventTags.add(data);
        }
    })).autocomplete();
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
    var InputParams = document.getElementById("InputParams");
    var EventCreationLabel = document.getElementById("EventCreationLabel");
    var EventCreationButton = document.getElementById("EventCreationButton");
    EventCreationButton.addEventListener("click", function () {
        var oRequest = U.buildAjaxRequestFromInputs(InputParams.querySelectorAll("[param]"), { type: "POST", url: "webapi/events/create " });
        if (!U._autoEventType.selection) {
            U.setPageMessage('error', 'Select an event type');
            U.highlightRequiredInputs(true);
            return;
        }
        oRequest.data.EventTypeID = U._autoEventType.selection.data.eventTypeID;
        oRequest.data.Tags = U._eventTags.getTagValues();
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
                console.log(oRequest, ev);
                InputParams.class = "Success";
                U.setPageMessage('success', 'Success!  Go to the <a href="/Index">Event feed to see events </a>');
            }
            else {
                handleFailure(ev);
            }
        });
    });
}());
