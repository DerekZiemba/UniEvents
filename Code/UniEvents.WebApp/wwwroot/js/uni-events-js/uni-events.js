
/// <reference path="ZMBA.js" />

/*! UniEvents.js */
(function (window, document, $, ZMBA, factory) {

	window.U = factory(window, document, $, ZMBA); //Export as U

}(window, window.document, window.jQuery, window.ZMBA,
function UniEventFactory(window, document, $, ZMBA) {

   const UniEvents = {
      rgxTrimUri: /^(\s|\?|\/|&)+|(\s|\?|\/|&)+$/,
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
      buildAjaxRequestFromInputs: function (inputs, oRequest) {
         oRequest = oRequest || {};
         oRequest.data = oRequest.data || {};

         var querystring = "?";
         var path = oRequest.url || "";

         for (var i = 0, len = inputs.length; i < len; i++) {
            var input = inputs[i];
            var value = input.value;
            if (value) {
               var name = input.getAttribute("param");
               var source = input.getAttribute("source");
               if (source === "QueryString" || source === "Url") {
                  querystring += name + "=" + encodeURI(value) + "&";
               } else {
                  if (name.indexOf('.') >= 0) {
                     var target = oRequest.data;
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
                  } else {
                     oRequest.data[name] = value;
                  }
               }
            }
         }
         oRequest.url = (path + querystring).replace(UniEvents.rgxTrimUri, '');
         return oRequest;
      }
   };



   return UniEvents;

}));






