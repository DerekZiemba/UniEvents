
(function (factory) {

   factory(window, window.document, window.jQuery, window.U, window.ZMBA);

}(function Factory(window, document, $, U, ZMBA) {

   $.ajax('webapi/metadata').done(data => {
      U.dictMetaData = data.result;
      var webMethodSelect = document.getElementById('webMethodSelect');
      webMethodSelect.addEventListener('change', handleWebMethodSelected);
      for (var route in U.dictMetaData) {
         var option = document.createElement('option');
         option.value = route;
         option.innerText = route;
         webMethodSelect.appendChild(option);
      }
   });


   var metadata;
   var btnClear = document.getElementById('btnClear');
   var btnExecute = document.getElementById('btnExecute');
   var currentHttpType = document.getElementById('httpType');
   var route = document.getElementById('route');
   var postBody = document.getElementById('postBody');
   var inputParams = document.getElementById('InputParams');
   var resultJson = document.getElementById('resultJson');
   var milliseconds = document.getElementById('millitime');

   function handleWebMethodSelected(ev) {
      btnClear.disabled = true;
      btnExecute.enable = false;
      while (inputParams.firstElementChild) { inputParams.removeChild(inputParams.firstElementChild); }

      var option = this.options[this.selectedIndex];
      metadata = U.dictMetaData[option.value];
      if (metadata) {
         btnExecute.disabled = false;
         currentHttpType.value = metadata.httpMethod.toUpperCase();
         route.value = metadata.path;

         if (!metadata.params) {      
            metadata.params = [];
            metadata.input.forEach(param => {
               param.elem = Element.From(`<li class="inputparam"><span>(${param.typeName})</span><label>${param.name}:</label><input type="text" param="${param.name}" source="${param.source}" jsType="${param.jsType}" isCollection="${param.isCollection}"/></li>`);
               param.elemInput = param.elem.getElementsByTagName('input')[0];
               param.elemInput.addEventListener('change', handleParamChanage);
               param.elemInput.addEventListener('keyup', handleParamChanage);
               metadata.params.push(param.elemInput);
            });
         }

         metadata.input.forEach(param => {
            inputParams.appendChild(param.elem);
         });

         handleParamChanage();
      } else {
         btnExecute.disabled = true;
      }
   }

   function handleParamChanage(ev) {
      if (ev) { ev.stopPropagation(); }
      btnClear.disabled = false;
      var oData = U.buildAjaxRequestFromInputs(metadata.params, { url: metadata.path });
      route.value = oData.url;
      postBody.value = JSON.stringify(oData.data, null, '\t');  
      if (metadata.path.includes('autocomplete')) {
         executeRequest();
      }
   }

   btnClear.addEventListener('click', () => {
      for (var i = 0, len = metadata.params.length; i < len; i++) {
         metadata.params[i].value = '';
      }
   });

   var start;
   function logStart() { start = performance.now(); }
   function executeRequest() {
      $.ajax({
         type: currentHttpType.value,
         url: route.value,
         data: postBody.value && JSON.parse(postBody.value),
         beforeSend: logStart
      }).done(data => {
         milliseconds.innerText = performance.now() - start;
         resultJson.value = JSON.stringify(data, null, '\t');
      });
   }

   btnExecute.addEventListener('click', executeRequest);

   U.executeRequest = executeRequest;
}));

