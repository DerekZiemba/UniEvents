using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class EventController : WebAppController {


      [HttpGet, Route("webapi/events/search/{EventID?}/{EventTypeID?}/{AccountID?}/{LocationID?}/{DateFrom?}/{DateTo?}/{Title?}/{Caption?}/")]
      public async Task<ApiResult<EventInfo[]>> EventSearch(long? EventID = null,
         long? EventTypeID = null,
         long? AccountID = null,
         long? LocationID = null,
         DateTime? DateFrom = null,
         DateTime? DateTo = null,
         string Title = null,
         string Caption = null) {


         var apiresult = new ApiResult<EventInfo[]>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         return apiresult.Failure("TODO");
         try {
            //This is why we need to implement Managers with caching abilities or do the whole thing in SQL, because this will quickly cripple the server. 
           var feedItems = DBModels.DBEventFeedItem.SP_Event_Search(WebAppContext.Factory, EventID, EventTypeID, AccountID, LocationID, DateFrom, DateTo, Title, Caption);
            foreach(var item in feedItems) {

            }

            return apiresult;
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpPost, Route("webapi/events/create")]
      public ApiResult<EventInfo> EventCreate(EventCreatInput info) {
         var apiresult = new ApiResult<EventInfo>();
         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         return apiresult.Failure("TODO");

         try {

         } catch (Exception ex) { return apiresult.Failure(ex); }

      }

      public class EventCreatInput {
         public Int64 EventID { get; set; }
         public int EventTypeID { get; set; }
         public DateTime DateStart { get; set; }
         public DateTime DateEnd { get; set; }
         public Int64 AccountID { get; set; }
         public Int64 LocationID { get; set; }
         public string Title { get; set; }
         public string Caption { get; set; }

         public Int64[] TagIds { get; set; }
      }


      public EventController(IHttpContextAccessor accessor): base(accessor) { }
   }
}