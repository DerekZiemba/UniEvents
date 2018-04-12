using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


using UniEvents.WebApp;
using UniEvents.Core;
using UniEvents.Models;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI.Controllers {

   [Produces("application/json")]
   [ApiExplorerSettings(IgnoreApi = false)]
   public class EventController : WebAPIController {


      [HttpGet, Route("webapi/events/search/{EventID?}/{EventTypeID?}/{AccountID?}/{LocationID?}/{DateFrom?}/{DateTo?}/{Title?}/{Caption?}/")]
      public async Task<ApiResult<List<EventInfo>>> EventSearch(long? EventID = null,
         long? EventTypeID = null,
         long? AccountID = null,
         long? LocationID = null,
         DateTime? DateFrom = null,
         DateTime? DateTo = null,
         string Title = null,
         string Caption = null) {

         var apiresult = new ApiResult<List<EventInfo>>();
         try {
            return apiresult.Success(await Factory.EventFeedManager.EventSearch());
         } catch (Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpPost, Route("webapi/events/create")]
      public ApiResult<EventInfo> EventCreate(EventInput input) {
         var apiresult = new ApiResult<EventInfo>();
         if (input == null) { return apiresult.Failure("Bad Post. Input is null."); }
         if (input.Location == null) { return apiresult.Failure("Location Invalid"); }
         //TODO sanitize Title, Caption, and Description to be free of javascript
         if (input.Title.CountAlphaNumeric() <= 5) { return apiresult.Failure("Title to short."); }
         if (input.Caption.CountAlphaNumeric() <= 8) { return apiresult.Failure("Caption to short."); }       
         if (input.DateStart.ToUniversalTime() < DateTime.UtcNow) { return apiresult.Failure("DateStart in the past."); }
         if (input.DateEnd.ToUniversalTime() < input.DateStart.ToUniversalTime()) { return apiresult.Failure("DateEnd is before DateStart"); }
         if (input.DateStart.AddDays(14).ToUniversalTime() < input.DateEnd.ToUniversalTime()) { return apiresult.Failure("Events cannot last longer than 2 weeks."); }

         if (UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if (!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         DBEventType eventType = Factory.EventTypeManager[input.EventTypeID];
         if(eventType == null) { return apiresult.Failure("EventType does not exist."); }

         if (input.Tags == null || input.Tags.Length == 0) { return apiresult.Failure("Include at least one EventTag."); }
         DBTag[] eventTags = new DBTag[input.Tags.Length];
         for(int i = 0; i < input.Tags.Length; i++) {
            DBTag tag = Factory.TagManager[input.Tags[i]];
            if(tag == null) { return apiresult.Failure("Invalid Tag: " + input.Tags[i].ToString()); }
            eventTags[i] = tag;
         }

         LocationNode.CountryRegionNode oCountry = Factory.LocationManager.QueryCachedCountries(input.Location.CountryRegion).FirstOrDefault();
         if (oCountry == null) { return apiresult.Failure("Invalid Country"); }

         LocationNode.AdminDistrictNode oState = Factory.LocationManager.QueryCachedStates(input.Location.AdminDistrict).FirstOrDefault();
         if (oState == null) { return apiresult.Failure("Invalid State"); }

         if (input.Location.PostalCode.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid PostalCode"); }     
         if (oCountry.Abbreviation == "USA") {
            LocationNode.PostalCodeNode oZip = Factory.LocationManager.QueryCachedPostalCodes(input.Location.PostalCode).FirstOrDefault();
            if (oZip == null) { return apiresult.Failure("Invalid PostalCode"); }
         }

         if (input.Location.Locality.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid City"); }

        
         try {
            StreetAddress address = new StreetAddress(Factory.LocationManager.GetOrCreateDBLocation(input.Location));
            DBEventFeedItem dbEventItem = DBEventFeedItem.SP_Event_CreateOrUpdate(Factory, eventType.EventTypeID, input.DateStart, input.DateEnd, UserContext.AccountID, address.LocationID.UnBox(), input.Title, input.Caption, input.Description);
            EventInfo info = new EventInfo(){
               EventID = dbEventItem.EventID,
               DateStart = dbEventItem.DateStart,
               DateEnd = dbEventItem.DateEnd,
               Title=dbEventItem.Title,
               Caption = dbEventItem.Title,
               EventTypeID = dbEventItem.EventTypeID,
               EventType = eventType,
               LocationID = dbEventItem.LocationID,
               LocationName = address.Name,
               AddressLine = Helpers.FormatAddress(null, address.AddressLine, address.Locality, address.AdminDistrict, address.PostalCode, address.CountryRegion),
               AccountID = dbEventItem.AccountID,
               Host = String.IsNullOrWhiteSpace(UserContext.UserDisplayName) ? UserContext.UserName : UserContext.UserDisplayName,
               Tags = eventTags
            };

            for (int i = 0; i < eventTags.Length; i++) {
               DBTag tag = eventTags[i];
               Factory.TagManager.LinkTagToEvent(info.EventID, tag.TagID);
            }

            return apiresult.Success(info);

         } catch (Exception ex) {
            return apiresult.Failure(ex);
         }


      }


      public EventController(IHttpContextAccessor accessor): base(accessor) { }
   }
}