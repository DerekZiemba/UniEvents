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
            return apiresult.Success(await Factory.EventManager.EventSearch(EventID, EventTypeID, AccountID, LocationID, DateFrom, DateTo, Title, Caption).ConfigureAwait(false));
         } catch(Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpGet, Route("webapi/events/remove/{EventID?}")]
      public async Task<ApiResult> RemoveEvent(long EventID) {
         var apiresult = new ApiResult();
         if(UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if(!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         try {
            await Factory.EventManager.RemoveEventAsync(EventID, UserContext.AccountID).ConfigureAwait(false);
            return apiresult.Success("Success");
         } catch(Exception ex) { return apiresult.Failure(ex); }
      }


      [HttpGet, Route("webapi/events/getbyidwithuserview/{EventID?}")]
      public ApiResult<EventInfo> GetEventInfoWithUserView(long EventID) {
         var apiresult = new ApiResult<EventInfo>();

         if(UserContext == null || !UserContext.IsVerifiedLogin) {
            try {
               DBEventFeedItemExtended item =  Factory.EventManager.EventGetByID(EventID);
               return apiresult.Success(new EventInfoUserView(Factory, item));
            } catch(Exception ex) { return apiresult.Failure(ex); }
         } else {
            try {
               DBEventFeedItemExtended item =  Factory.EventManager.EventGetByIDWithUserView(EventID, UserContext.AccountID);
               return apiresult.Success(new EventInfoUserView(Factory, item) { CanEditEvent = (item.AccountID == UserContext.AccountID || UserContext.IsAdmin) });
            } catch(Exception ex) { return apiresult.Failure(ex); }
         }

      }



      [HttpPost, Route("webapi/events/create")]
      public ApiResult<EventInfo> EventCreate(EventInput input) {
         var apiresult = new ApiResult<EventInfo>();
         if(UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if(!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         if(input == null) { return apiresult.Failure("Bad Post. Input is null."); }
         if(input.Location == null) { return apiresult.Failure("Location Invalid"); }
         //TODO sanitize Title, Caption, and Description to be free of javascript
         if(input.Title.CountAlphaNumeric() <= 5) { return apiresult.Failure("Title to short."); }
         if(input.Caption.CountAlphaNumeric() <= 8) { return apiresult.Failure("Caption to short."); }
         if(input.DateStart.ToUniversalTime() < DateTime.UtcNow) { return apiresult.Failure("DateStart in the past."); }
         if(input.DateEnd.ToUniversalTime() < input.DateStart.ToUniversalTime()) { return apiresult.Failure("DateEnd is before DateStart"); }
         if(input.DateStart.AddDays(14).ToUniversalTime() < input.DateEnd.ToUniversalTime()) { return apiresult.Failure("Events cannot last longer than 2 weeks."); }



         DBEventType eventType = Factory.EventTypeManager[input.EventTypeID];
         if(eventType == null) { return apiresult.Failure("EventType does not exist."); }

         if(input.Tags == null || input.Tags.Length == 0) { return apiresult.Failure("Include at least one EventTag."); }
         DBTag[] eventTags = new DBTag[input.Tags.Length];
         for(int i = 0; i < input.Tags.Length; i++) {
            DBTag tag = Factory.TagManager[input.Tags[i]];
            if(tag == null) { return apiresult.Failure("Invalid Tag: " + input.Tags[i].ToString()); }
            eventTags[i] = tag;
         }

         LocationNode.CountryRegionNode oCountry = Factory.LocationManager.QueryCachedCountries(input.Location.CountryRegion).FirstOrDefault();
         if(oCountry == null) { return apiresult.Failure("Invalid Country"); }

         LocationNode.AdminDistrictNode oState = Factory.LocationManager.QueryCachedStates(input.Location.AdminDistrict).FirstOrDefault();
         if(oState == null) { return apiresult.Failure("Invalid State"); }

         if(input.Location.PostalCode.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid PostalCode"); }
         if(oCountry.Abbreviation == "USA") {
            LocationNode.PostalCodeNode oZip = Factory.LocationManager.QueryCachedPostalCodes(input.Location.PostalCode).FirstOrDefault();
            if(oZip == null) { return apiresult.Failure("Invalid PostalCode"); }
         }

         if(input.Location.Locality.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid City"); }


         try {
            StreetAddress address = new StreetAddress(Factory.LocationManager.GetOrCreateDBLocation(input.Location));
            DBEventFeedItem dbEventItem = Factory.EventManager.CreateEvent(eventType.EventTypeID, input.DateStart, input.DateEnd, UserContext.AccountID, address.LocationID.UnBox(), input.Title, input.Caption, input.Description);
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
               Tags = eventTags,
               Details = input.Description
            };

            for(int i = 0; i < eventTags.Length; i++) {
               DBTag tag = eventTags[i];
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
               Factory.TagManager.LinkTagToEvent(info.EventID, tag.TagID);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return apiresult.Success(info);

         } catch(Exception ex) {
            return apiresult.Failure(ex);
         }

      }


      [HttpPost, Route("webapi/events/update/{EventID?}")]
      public ApiResult<EventInfo> EventUpdate(long EventID, EventInput input) {
         //TODO: Verify Event belongs to user if updating. Right now anyone can update any event. 
         var apiresult = new ApiResult<EventInfo>();
         if(UserContext == null) { return apiresult.Failure("Must be logged in."); }
         if(!UserContext.IsVerifiedLogin) { return apiresult.Failure("Insufficient account permissions."); }

         if(EventID <= 0) { return apiresult.Failure("Invalid ID"); }
         DBEventFeedItemExtended existing = null;
         try {
            existing = Factory.EventManager.EventGetByID(EventID);
            if(existing == null) { return apiresult.Failure("Event Does not exist."); }
         } catch(Exception ex) { return apiresult.Failure(ex); }


         if(input == null) { return apiresult.Failure("Input is null."); }
         if(input.Title != null && existing.Title != input.Title ) { existing.Title = input.Title; }
         if(input.Caption != null && existing.Caption != input.Caption) { existing.Caption = input.Caption; }
         if(input.Description != null && existing.Details != input.Description) { existing.Details = input.Description; }
         if(input.EventTypeID > 0 && existing.EventTypeID != input.EventTypeID) { existing.EventTypeID = input.EventTypeID; }
         if(input.DateStart != default  && existing.DateStart != input.DateStart) { existing.DateStart = input.DateStart; }
         if(input.DateEnd != default && existing.DateEnd != input.DateEnd) { existing.DateEnd = input.DateEnd; }

         //TODO sanitize Title, Caption, and Description to be free of javascript
         if(existing.Title.CountAlphaNumeric() <= 5) { return apiresult.Failure("Title to short."); }
         if(existing.Caption.CountAlphaNumeric() <= 8) { return apiresult.Failure("Caption to short."); }
         if(existing.DateStart.ToUniversalTime() < DateTime.UtcNow) {
            apiresult.Failure("DateStart in the past.");
            if(UserContext.IsAdmin) { apiresult.AppendMessage("(AdminOverride)"); } else { return apiresult; }
         }
         if(existing.DateEnd.ToUniversalTime() < input.DateStart.ToUniversalTime()) { return apiresult.Failure("DateEnd is before DateStart"); }
         if(existing.DateStart.AddDays(14).ToUniversalTime() < input.DateEnd.ToUniversalTime()) { return apiresult.Failure("Events cannot last longer than 2 weeks."); }


         DBEventType eventType = Factory.EventTypeManager[existing.EventTypeID];
         if(eventType == null) { return apiresult.Failure("EventType does not exist."); }


         List<DBTag> newTags = null;
         List<DBTag> removedTags = null;
         DBTag[] eventTags = null;

         if(input.Tags != null && input.Tags.Length > 0) {
            DBTag[] previousTags = existing.TagIds.Select(x=>Factory.TagManager[x]).ToArray();
            existing.TagIds = new long[input.Tags.Length];
            eventTags = new DBTag[input.Tags.Length];
            newTags = new List<DBTag>();
            removedTags = new List<DBTag>();

            for(int i = 0; i < input.Tags.Length; i++) {
               DBTag tag = eventTags[i] = Factory.TagManager[input.Tags[i]];
               if(tag == null) { return apiresult.Failure("Invalid Tag: " + input.Tags[i].ToString()); }
               existing.TagIds[i] = tag.TagID;
               if(Array.IndexOf(previousTags, tag) == -1) {
                  newTags.Add(tag);
               }
            }
            for(int i = 0; i < previousTags.Length; i++) {
               DBTag tag = previousTags[i];
               if(Array.IndexOf(eventTags, tag) == -1) {
                  removedTags.Add(tag);
               }
            }

         } else {
            eventTags = new DBTag[existing.TagIds.Length];
            for(int i = 0; i < existing.TagIds.Length; i++) {
               DBTag tag = Factory.TagManager[existing.TagIds[i]];
               if(tag == null) { return apiresult.Failure("Invalid Tag: " + input.Tags[i].ToString()); }
               eventTags[i] = tag;
            }
         }



         bool bLocChange = false;
         if(input.Location != null) {
            var loc = input.Location;
            if(loc.Name != null && existing.LocationName != loc.Name) { existing.LocationName = loc.Name; bLocChange = true; }
            if(loc.AddressLine != null && existing.AddressLine != loc.AddressLine) { existing.AddressLine = loc.AddressLine; bLocChange = true; }
            if(loc.Locality != null && existing.Locality != loc.Name) { existing.Locality = loc.Locality; bLocChange = true; }
            if(loc.PostalCode != null && existing.PostalCode != loc.PostalCode) { existing.PostalCode = loc.PostalCode; bLocChange = true; }
            if(loc.AdminDistrict != null && existing.AdminDistrict != loc.Name) { existing.AdminDistrict = loc.AdminDistrict; bLocChange = true; }
            if(loc.CountryRegion != null && existing.CountryRegion != loc.CountryRegion) { existing.CountryRegion = loc.CountryRegion; bLocChange = true; }

            if(bLocChange) {
               LocationNode.CountryRegionNode oCountry = Factory.LocationManager.QueryCachedCountries(existing.CountryRegion).FirstOrDefault();
               if(oCountry == null) { return apiresult.Failure("Invalid Country"); }

               LocationNode.AdminDistrictNode oState = Factory.LocationManager.QueryCachedStates(existing.AdminDistrict).FirstOrDefault();
               if(oState == null) { return apiresult.Failure("Invalid State"); }

               if(existing.PostalCode.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid PostalCode"); }
               if(oCountry.Abbreviation == "USA") {
                  LocationNode.PostalCodeNode oZip = Factory.LocationManager.QueryCachedPostalCodes(existing.PostalCode).FirstOrDefault();
                  if(oZip == null) { return apiresult.Failure("Invalid PostalCode"); }
               }

               if(existing.Locality.CountAlphaNumeric() < 3) { return apiresult.Failure("Invalid City"); }
            }
         }

         StreetAddress address = new StreetAddress(){
            ParentLocationID = existing.ParentLocationID,
            LocationID = existing.LocationID,
            Name = existing.LocationName,
            AddressLine = existing.AddressLine,
            Locality = existing.Locality,
            AdminDistrict = existing.AdminDistrict,
            PostalCode = existing.PostalCode,
            CountryRegion = existing.CountryRegion
         };

         if(bLocChange) {
            try {
               address = new StreetAddress(Factory.LocationManager.GetOrCreateDBLocation(address));
            } catch(Exception ex) { return apiresult.Failure(ex); }
         }

         try {
            Factory.EventManager.UpdateEvent(EventID, existing.EventTypeID, existing.DateStart, existing.DateEnd, UserContext.AccountID, existing.LocationID, existing.Title, existing.Caption, existing.Details);
            EventInfo info = new EventInfo(){
               EventID = existing.EventID,
               DateStart = existing.DateStart,
               DateEnd = existing.DateEnd,
               Title=existing.Title,
               Caption = existing.Title,
               EventTypeID = existing.EventTypeID,
               EventType = eventType,
               LocationID = existing.LocationID,
               LocationName = address.Name,
               AddressLine = Helpers.FormatAddress(null, address.AddressLine, address.Locality, address.AdminDistrict, address.PostalCode, address.CountryRegion),
               AccountID = existing.AccountID,
               Host = String.IsNullOrWhiteSpace(UserContext.UserDisplayName) ? UserContext.UserName : UserContext.UserDisplayName,
               Tags = eventTags,
               Details = existing.Details
            };

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if(newTags != null) {
               for(int i = 0; i < newTags.Count; i++) {
                  Factory.TagManager.LinkTagToEvent(info.EventID, newTags[i].TagID);
               }
            }
            if(removedTags != null) {
               for(int i = 0; i < removedTags.Count; i++) {
                  Factory.TagManager.RemoveTagFromEvent(info.EventID, removedTags[i].TagID);
               }
            }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return apiresult.Success(info);

         } catch(Exception ex) {
            return apiresult.Failure(ex);
         }

      }



      [HttpGet, Route("webapi/events/getdescription/{id?}")]
      public ApiResult<string> GetEventDescription(long id) {
         var apiresult = new ApiResult<string>();
         try {
            return apiresult.Success("", Factory.EventManager.GetEventDescription(id));
         } catch(Exception ex) { return apiresult.Failure(ex); }
      }

      public EventController(IHttpContextAccessor accessor) : base(accessor) { }
   }
}