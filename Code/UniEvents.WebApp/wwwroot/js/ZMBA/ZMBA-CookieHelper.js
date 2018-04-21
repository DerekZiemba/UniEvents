///<reference path="ZMBA.js"/>

(function CookieHelperModule(window, document, ZMBA) {
   const _rgxVerifyKey = /^(?:expires|max\-age|path|domain|secure)$/i;

   function CookieHelper(document) {
      this.document = document;
   }

   CookieHelper.prototype = {
      getCookie: function (sKey) {
         if (!sKey) { return null; }
         var rgx = new RegExp("(?:(?:^|.*;)\\s*" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=\\s*([^;]*).*$)|^.*$");
         return decodeURIComponent(this.document.cookie.replace(rgx, "$1")) || null;
      },
      getCookieObject: function (sKey) {
         var str = this.getCookie(sKey);
         return !str ? null : JSON.parse(str);
      },
      setCookie: function (sKey, sValue, expires, sPath, sDomain, bSecure) {
         if (!sKey || _rgxVerifyKey.test(sKey)) { return false; }
         var sExpires = "";
         if (expires) {
            switch (expires.constructor) {
               case Number: sExpires = expires === Infinity ? "; expires=Fri, 31 Dec 9999 23:59:59 GMT" : "; max-age=" + expires; break;
               case String: sExpires = "; expires=" + expires; break;
               case Date: sExpires = "; expires=" + expires.toUTCString(); break;
            }
         }
         if (typeof sValue !== 'string') {
            sValue = JSON.stringify(sValue);
         }
         this.document.cookie = encodeURIComponent(sKey) + "=" + encodeURIComponent(sValue) + sExpires + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "") + (bSecure ? "; secure" : "");
         return true;
      },
      removeCookie: function (sKey, sPath, sDomain) {
         if (!this.hasCookie(sKey)) { return false; }
         this.document.cookie = encodeURIComponent(sKey) + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT" + (sDomain ? "; domain=" + sDomain : "") + (sPath ? "; path=" + sPath : "");
         return true;
      },
      hasCookie: function (sKey) {
         if (!sKey || _rgxVerifyKey.test(sKey)) { return false; }
         return (new RegExp("(?:^|;\\s*)" + encodeURIComponent(sKey).replace(/[\-\.\+\*]/g, "\\$&") + "\\s*\\=")).test(this.document.cookie);
      },
      get keys() {
         var aKeys = this.document.cookie.replace(/((?:^|\s*;)[^\=]+)(?=;|$)|^\s*|\s*(?:\=[^;]*)?(?:\1|$)/g, "").split(/\s*(?:\=[^;]*)?;\s*/);
         for (var nLen = aKeys.length, nIdx = 0; nIdx < nLen; nIdx++) { aKeys[nIdx] = decodeURIComponent(aKeys[nIdx]); }
         return aKeys;
      }
   }

   ZMBA.extendType(Document.prototype, {
      get cookies() { return new CookieHelper(this); }
   });

}(window, window.document, ZMBA));