using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBEventFeedItem : DBModel {


      [DBCol("EventID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 EventID { get; set; }

      [DBCol("EventTypeID", SqlDbType.BigInt, 1, false)]
      public long EventTypeID { get; set; }

      [DBCol("DateStart", SqlDbType.SmallDateTime, 1, false)]
      public DateTime DateStart { get; set; }

      [DBCol("DateEnd", SqlDbType.SmallDateTime, 1, false)]
      public DateTime DateEnd { get; set; }

      [DBCol("AccountID", SqlDbType.BigInt, 1, false)]
      public Int64 AccountID { get; set; }

      [DBCol("LocationID", SqlDbType.BigInt, 1, false)]
      public Int64 LocationID { get; set; }

      [DBCol("Title", SqlDbType.VarChar, 80, false)]
      public string Title { get; set; }

      [DBCol("Caption", SqlDbType.NVarChar, 160, false)]
      public string Caption { get; set; }

      public long[] TagIds { get; set; }

      public int RSVP_Attending { get; set; }
      public int RSVP_Later { get; set; }
      public int RSVP_StopBy { get; set; }
      public int RSVP_Maybe { get; set; }
      public int RSVP_No { get; set; }

      public string UserDisplayName { get; set; }
      public string UserName { get; set; }

      public string LocationName { get; set; }
      public string AddressLine { get; set; }
      public string Locality { get; set; }
      public string PostalCode { get; set; }
      public string AdminDistrict { get; set; }
      public string CountryRegion { get; set; }

      public DBEventFeedItem() { }

      public DBEventFeedItem(IDataReader reader) {
         EventID = reader.GetInt64(nameof(EventID));
         EventTypeID = reader.GetInt64(nameof(EventTypeID));
         DateStart = reader.GetDateTime(nameof(DateStart));
         DateEnd = reader.GetDateTime(nameof(DateEnd));
         AccountID = reader.GetInt64(nameof(AccountID));
         LocationID = reader.GetInt64(nameof(LocationID));
         Title = reader.GetString(nameof(Title));
         Caption = reader.GetString(nameof(Caption));
         RSVP_Attending = reader.GetInt32(nameof(RSVP_Attending));
         RSVP_Later = reader.GetInt32(nameof(RSVP_Later));
         RSVP_StopBy = reader.GetInt32(nameof(RSVP_StopBy));
         RSVP_Maybe = reader.GetInt32(nameof(RSVP_Maybe));
         RSVP_No = reader.GetInt32(nameof(RSVP_No));
         TagIds = reader.GetString(nameof(TagIds))?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
         UserDisplayName = reader.GetString(nameof(UserDisplayName));
         UserName = reader.GetString(nameof(UserName));
         LocationName = reader.GetString(nameof(LocationName));
         AddressLine = reader.GetString(nameof(AddressLine));
         Locality = reader.GetString(nameof(Locality));
         PostalCode = reader.GetString(nameof(PostalCode));
         AdminDistrict = reader.GetString(nameof(AdminDistrict));
         CountryRegion = reader.GetString(nameof(CountryRegion));
      }

   }


   public class DBEventFeedItemExtended : DBEventFeedItem {
      public Int64? ParentLocationID { get; set; }
      public string Details { get; set; }

      public DBEventFeedItemExtended() { }

      public DBEventFeedItemExtended(IDataReader reader) : base(reader) {
         ParentLocationID = reader.GetNInt64(nameof(ParentLocationID));
         Details = reader.GetString(nameof(Details));
      }
   }

}
