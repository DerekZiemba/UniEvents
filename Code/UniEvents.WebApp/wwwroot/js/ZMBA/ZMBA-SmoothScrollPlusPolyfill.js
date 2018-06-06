///<reference path="ZMBA.js"/>

(function SmoothScrollPlusPolyfill(window, document, ZMBA, Element, performance) {
   if ('scrollBehavior' in document.documentElement.style) {
      return; //Native browser support;
   }

   const SCROLL_TIME = 468;
   const ROUNDING_TOLERANCE = ZMBA.isMicrosoftBrowser ? 1 : 0;

   var nativeWinScroll = window.scroll || window.scrollTo;
   var nativeWinScrollBy = window.scrollBy;
   var nativeElemScroll = Element.prototype.scroll || Element.prototype.scrollTo || function (x, y) { this.scrollLeft = x; this.scrollTop = y; }
   var nativeElemScrollBy = Element.prototype.scrollBy || nativeElemScroll;
   var nativeElemScrollIntoView = Element.prototype.scrollIntoView;
   

   window.scroll = window.scrollTo = function myScrollTo(argx, argy) {
      if (argx == null) { return; }
      if (typeof argx !== 'object') {
         nativeWinScroll.call(this, argx, argy);
      } else if (argx.behavior == null || argx.behavior === 'auto' || argx.behavior === 'instant') {
         nativeWinScroll.call(this, argx.left != null ? argx.left : this.scrollX || this.pageXOffset, argx.top != null ? argx.top : this.scrollY || this.pageYOffset);
      } else if (argx.behavior === 'smooth') {
         var ssa = new SmoothScrollAnimation(document.body, argx.left != null ? ~~argx.left : this.scrollX || this.pageXOffset, argx.top != null ? ~~argx.top : this.scrollY || this.pageYOffset);
      } else {
         throw new TypeError("Invalid Scroll Option");
      }
   }



   window.scrollBy = function (argx, argy) {
      if (argx == null) { return; }
      if (typeof argx !== 'object') {
         nativeWinScrollBy.call(this, argx, argy);
      } else if (argx.behavior == null || argx.behavior === 'auto' || argx.behavior === 'instant') {
         nativeWinScrollBy.call(this, argx.left || 0, argx.top || 0);
      } else if (argx.behavior === 'smooth') {
         var ssa = new SmoothScrollAnimation(document.body, ~~argx.left + (this.scrollX || this.pageXOffset), ~~argx.top + (this.scrollY || this.pageYOffset));
      } else {
         throw new TypeError("Invalid Scroll Option");
      }
   };


   Element.prototype.scroll = Element.prototype.scrollTo = function (argx, argy) {
      if (argx == null) { return; }
      if (typeof argx !== 'object') {
         nativeElemScroll.call(this, argx, argy);
      } else if (argx.behavior == null || argx.behavior === 'auto' || argx.behavior === 'instant') {
         nativeElemScroll.call(this, argx.left != null ? ~~argx.left : this.scrollLeft, argx.top != null ? ~~argx.top : this.scrollTop);
      } else if (argx.behavior === 'smooth') {
         var ssa = new SmoothScrollAnimation(this, argx.left != null ? ~~argx.left : this.scrollLeft, argx.top != null ? ~~argx.top : this.scrollTop);
      } else {
         throw new TypeError("Invalid Scroll Option");
      }
   };


   Element.prototype.scrollBy = function (argx, argy) {
      if (argx == null) { return; }
      if (typeof argx !== 'object') {
         nativeElemScrollBy.call(this, argx, argy);
      } else if (argx.behavior == null || argx.behavior === 'auto' || argx.behavior === 'instant') {
         nativeElemScrollBy.call(this, ~~argx.left + this.scrollLeft, ~~argx.top + this.scrollTop);
      } else if (argx.behavior === 'smooth') {
         var ssa = new SmoothScrollAnimation(this, ~~argx.left + this.scrollLeft, ~~argx.top + this.scrollTop);
      } else {
         throw new TypeError("Invalid Scroll Option");
      }
   };



   Element.prototype.scrollIntoView = function (options) {
      if (options == null) { return; }
      if (options === true) { options = { block: 'start', inline: 'nearest' } }
      else if (options === false) { options = { block: 'end', inline: 'nearest' } };


      // avoid smooth behavior if not required
      if (shouldBailOut(arguments[0]) === true) {
         original.scrollIntoView.call(
            this,
            arguments[0] === undefined ? true : arguments[0]
         );

         return;
      }

      // LET THE SMOOTHNESS BEGIN!
      var scrollableParent = this;
      do { scrollableParent = scrollableParent.parentNode; } while (scrollableParent !== document.body && isScrollable(scrollableParent) === false);

      var parentRects = scrollableParent.getBoundingClientRect();
      var clientRects = this.getBoundingClientRect();

      if (scrollableParent !== d.body) {
         // reveal element inside parent
         smoothScroll.call(
            this,
            scrollableParent,
            scrollableParent.scrollLeft + clientRects.left - parentRects.left,
            scrollableParent.scrollTop + clientRects.top - parentRects.top
         );

         // reveal parent in viewport unless is fixed
         if (w.getComputedStyle(scrollableParent).position !== 'fixed') {
            w.scrollBy({
               left: parentRects.left,
               top: parentRects.top,
               behavior: 'smooth'
            });
         }
      } else {
         // reveal element in viewport
         w.scrollBy({
            left: clientRects.left,
            top: clientRects.top,
            behavior: 'smooth'
         });
      }
   };



   function SmoothScrollAnimation(el, x, y) {
      this.startTime = performance.now();
      this.x = x;
      this.y = y;
      if (el === document.body) {         
         this.startX = window.scrollX || window.pageXOffset;
         this.startY = window.scrollY || window.pageYOffset;
         this.method = nativeWinScroll.bind(window);
      } else {       
         this.startX = el.scrollLeft;
         this.startY = el.scrollTop;
         this.method = nativeElemScroll.bind(el);
      }
      
      this.step = this._step.bind(this);
   }
   SmoothScrollAnimation.prototype = {
      startTime: 0.0,
      x: 0,
      y: 0,
      startX: 0,
      startY: 0,
      _step: function () {
         var easing = 0.5 * (1 - Math.cos(Math.PI * Math.max((performance.now() - this.startTime) / SCROLL_TIME, 1)));
         var cx = this.startX + (this.x - this.startX) * easing;
         var cy = this.startY + (this.y - this.startY) * easing;
         this.method.call(cx, cy);
         if (cx !== this.x || cy !== this.y) {
            window.requestAnimationFrame(this.step);
         }
      }
   }






   function shouldBailOut(firstArg) {
      if (firstArg === null || typeof firstArg !== 'object' || firstArg.behavior == null || firstArg.behavior === 'auto' || firstArg.behavior === 'instant') {
         return true;
      } else if (firstArg.behavior === 'smooth') {
         return false;
      }

      throw new TypeError('behavior member of ScrollOptions ' + firstArg.behavior + ' is not a valid value for enumeration ScrollBehavior.');
   }



   function isScrollable(el) {
      var sty, ovf;
      if (el.clientHeight + ROUNDING_TOLERANCE < el.scrollHeight) {
         sty = window.getComputedStyle(el); ovf = sty['overflowY'];
         if (ovf === 'auto' || ovf === 'scroll') { return true; }
      }
      if (el.clientWidth + ROUNDING_TOLERANCE < el.scrollWidth) {
         sty = sty || window.getComputedStyle(el); ovf = sty['overflowX'];
         if (ovf === 'auto' || ovf === 'scroll') { return true; }
      }
      return false;
   }





}(window, window.document, ZMBA, window.HTMLElement || window.Element, window.performance));