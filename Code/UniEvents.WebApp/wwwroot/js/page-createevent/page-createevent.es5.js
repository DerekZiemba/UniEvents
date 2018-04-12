"use strict";
(function wireupAutoCompletion() {
    U.locationAutoComplete = new U.LocationAutoComplete({ search: "#searchLocations" });
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
    U.eventType = $("#eventType").autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/eventtypes',
        minChars: 0
    })).autocomplete();
    U.eventTags = new Taggle(document.querySelector('.event_tags_input'), {
        allowDuplicates: false,
        submitKeys: []
    });
    $(U.eventTags.getInput()).autocomplete(Object.assign({}, autocompleteSettings, {
        serviceUrl: 'webapi/autocomplete/tags',
        minChars: 0,
        onSelect: function (data) {
            U.eventTags.add(data);
        }
    }));
}());
(function wireupDatePickers() {
    flatpickr.setDefaults({
        enableTime: true,
        inline: true,
        dateFormat: "Z",
        onReady: function (dObj, dStr, fp, elem) {
            fp.showTimeInput = true;
        }
    });
    var startPicker, endPicker;
    startPicker = flatpickr('[param="DateStart"]', {
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
    endPicker = flatpickr('[param="DateEnd"]', {
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
    U.dateStart = startPicker;
    U.dateEnd = endPicker;
}());
(function wireupCreateEvent() {
    var InputParams = document.getElementById("InputParams");
    var EventCreationLabel = document.getElementById("EventCreationLabel");
    var EventCreationButton = document.getElementById("EventCreationButton");
    EventCreationButton.addEventListener("click", function () {
        var oRequest = U.buildAjaxRequestFromInputs(InputParams.querySelectorAll("[param]"), { type: "POST", url: "webapi/events/create " });
        if (!U.eventType.selection) {
            U.setPageMessage('error', 'Select an event type');
            U.eventType.element.parentElement.classList.add('required-highlight');
            return;
        }
        oRequest.data.EventTypeID = U.eventType.selection.data.eventTypeID;
        oRequest.data.Tags = U.eventTags.getTagValues();
        if (oRequest.data.Tags == null || oRequest.data.Tags.length === 0) {
            U.setPageMessage('error', 'Add at least one tag');
            U.eventTags.container.parentElement.classList.add('required-highlight');
            return;
        }
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
(function wireupModals() {
    U.modalCreateEventType = (function () {
        var modal = document.getElementById('modalCreateEventType');
        var btnOpen = document.getElementById('btnCreateEventType');
        var btnClose = modal.querySelector('.close');
        function open() {
            btnOpen.enable = false;
            modal.style.display = "block";
        }
        function close() {
            btnOpen.enable = true;
            modal.style.display = "none";
        }
        btnOpen.addEventListener('click', open);
        btnClose.addEventListener('click', close);
        window.addEventListener('click', function (ev) {
            if (ev.target === modal) {
                close();
            }
        });
        return { modal: modal, btnOpen: btnOpen, btnClose: btnClose, open: open, close: close };
    }());
}());
