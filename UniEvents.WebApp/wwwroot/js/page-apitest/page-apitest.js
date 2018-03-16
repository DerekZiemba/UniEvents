
(function (factory) {

   factory(window, window.document, window.jQuery, window.U, window.ZMBA);

}(function Factory(window, document, $, U, ZMBA) {

   $.ajax({
      cache: false,
      url: 'webapi/metadata',
      success: function (data) {
         U.dictMetaData = data.result;
         var webMethodSelect = document.getElementById('webMethodSelect');
         webMethodSelect.addEventListener('change', handleWebMethodSelected);
         for (var route in U.dictMetaData) {
            var option = document.createElement('option');
            option.value = route;
            option.innerText = route;
            webMethodSelect.appendChild(option);
         }
      }
   });


   var metadata;
   var oBody = {};

   var btnClear = document.getElementById('btnClear');
   var btnExecute = document.getElementById('btnExecute');
   var routeTemplate = document.getElementById('routeTemplate'); 
   var currentHttpType = document.getElementById('httpType');
   var route = document.getElementById('route');
   var postBody = document.getElementById('postBody');
   var inputParams = document.getElementById('InputParams');
   var resultJson = document.getElementById('resultJson');

   function handleWebMethodSelected(ev) {
      btnClear.disabled = true;
      btnExecute.enable = false;
      while (inputParams.firstElementChild) { inputParams.removeChild(inputParams.firstElementChild); }

      var option = this.selectedOptions[0];
      metadata = U.dictMetaData[option.value];
      if (metadata) {
         btnExecute.disabled = false;
         routeTemplate.value = metadata.route;
         currentHttpType.value = metadata.httpMethod.toUpperCase();
         route.value = metadata.path;

         if (!metadata.params) {
            metadata.params = [];
            metadata.input.forEach(param => {
               param.elem = Element.From(`<li class="inputparam" name="${param.name}"> 
                                          <span>(${param.typeName})</span>
                                          <label>${param.name}:</label>
                                          <input type="text"/>
                                       </li>`);
               param.elemInput = param.elem.getElementsByTagName('input')[0];
               param.elemInput.addEventListener('change', handleParamChanage);
               param.elemInput.addEventListener('keyup', handleParamChanage);
               metadata.params.push(param);
            });
         }

         metadata.params.forEach(param => {
            inputParams.appendChild(param.elem);
         });
      } else {
         btnExecute.disabled = true;
      }

   }

   function handleParamChanage() {
      btnClear.disabled = false;
      oBody = {};

      var params = metadata.params;
      var path = metadata.path;
      var querystring = "?";

      for (var i = 0, len = params.length; i < len; i++) {
         var param = params[i];
         if (param.elemInput.value) {
            if (param.source === 'Url') {
               path = path.trim('/') + '/' + param.elemInput.value;
            } else if (param.source === "QueryString") {
               querystring += param.name + "=" + param.elemInput.value + "&";
            } else {
               if (param.name.indexOf('.') >= 0) {
                  var target = oBody;
                  var parts = param.name.split('.');
                  while (parts.length > 0) {
                     var part = parts.shift();
                     if (parts.length === 0) {
                        target[part] = param.elemInput.value;
                     } else {
                        if (!target[part]) { target[part] = {}; }
                        target = target[part];
                     }                    
                  }
               } else {
                  oBody[param.name] = param.elemInput.value;
               }             
            }
         }
      }

      route.value = (path + querystring.trim("&")).trim("?");
      postBody.value = JSON.stringify(oBody, null, '\t');
   }

   btnClear.addEventListener('click', () => {
      for (var i = 0, len = metadata.params.length; i < len; i++) {
         metadata.params[i].elemInput.value = '';
      }
   });

   btnExecute.addEventListener('click', () => {
      function callback(data) {
         resultJson.value = JSON.stringify(data, null, '\t');
      }
      var request = {
         cache: false,
         type: currentHttpType.value,
         url: route.value,
         success: callback,
         error: callback
      }
      if (postBody.value) {
         request.data = JSON.parse(postBody.value);
      }

      $.ajax(request);
   });


}));

