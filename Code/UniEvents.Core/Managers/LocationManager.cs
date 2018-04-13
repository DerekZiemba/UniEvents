using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UniEvents.Core;
using UniEvents.Models;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;

using ZMBA;



namespace UniEvents.Core.Managers {

   public class LocationManager {
      private Factory Ctx;

      private Task _initTask;
      private ConcurrentDictionary<long, DBLocation> _dbLocationCache = new ConcurrentDictionary<long, DBLocation>();

      private Dictionary<string, AbbreviationEntry> DirectionAbbreviations;
      private Dictionary<string, AbbreviationEntry> AddressLineAbbreviations;
    
      private PartialKeySearchTrie<LocationNode> QueryAutoComplete = new PartialKeySearchTrie<LocationNode>();

      internal LocationManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run((Func<bool>)Init);
      }

      private bool Init() {
         var _usStates = new Dictionary<string, LocationNode.AdminDistrictNode>(200, StringComparer.OrdinalIgnoreCase);

         try {
            JSONAddresses jsabbrevs;
            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\Addresses.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var jsreader = new JsonTextReader(new StreamReader(fs, Encoding.UTF8))) {
               jsabbrevs = Common.CompactJsonSerializer.Deserialize<JSONAddresses>(jsreader);
            }

            foreach (KeyValuePair<string, JSONAddresses.JSONCountry> kvpCountry in jsabbrevs.Countries) {
               var countryNode = new LocationNode.CountryRegionNode(kvpCountry.Key, kvpCountry.Value.Variations);
               QueryAutoComplete.AddExact(countryNode.Key, countryNode);

               foreach (var abbrev in countryNode.Variations) { QueryAutoComplete.AddExact(abbrev, countryNode); }

               var bIsUSA = countryNode.Variations.Includes("USA");
               foreach (KeyValuePair<string, string[]> kvp in kvpCountry.Value.States) {
                  var stateNode = new LocationNode.AdminDistrictNode(kvp.Key, kvp.Value, countryNode);
                  countryNode.Children.Add(stateNode.Key, stateNode);
                  QueryAutoComplete.AddExact(stateNode.Key, stateNode);

                  foreach (var abbrev in stateNode.Variations) {
                     countryNode.Children.Add(abbrev, stateNode);
                     QueryAutoComplete.AddExact(abbrev, stateNode);
                     if (bIsUSA) {
                        _usStates.Add(abbrev, stateNode);
                     }
                  }
               }
            }

            Dictionary<string, AbbreviationEntry> dict = new Dictionary<string, AbbreviationEntry>(500, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string[]> kvp in jsabbrevs.Cardinal) {
               var entry = new AbbreviationEntry(){FullName = kvp.Key, Variations = kvp.Value };
               dict.Add(entry.FullName, entry);
               SetPreferedAbbreviation(entry, true);
            }
            DirectionAbbreviations = new Dictionary<string, AbbreviationEntry>(dict, StringComparer.OrdinalIgnoreCase);

            dict.Clear();
            foreach (KeyValuePair<string, string[]> kvp in jsabbrevs.AddressLine) {
               var entry = new AbbreviationEntry(){FullName = kvp.Key, Variations = kvp.Value };
               dict.Add(entry.FullName, entry);
               SetPreferedAbbreviation(entry, false);
            }
            AddressLineAbbreviations = new Dictionary<string, AbbreviationEntry>(dict, StringComparer.OrdinalIgnoreCase);


            void SetPreferedAbbreviation(AbbreviationEntry entry, bool bUseFirst) {
               for (var i = 0; i < entry.Variations.Length; i++) {
                  if (entry.Variations[i][0] == '$') {
                     entry.Variations[i] = entry.Variations[i].Substring(1);
                     entry.Preferred = entry.Variations[i];
                  }
                  dict.Add(entry.Variations[i], entry);
               }

               if (entry.Preferred == null) {
                  entry.Preferred = bUseFirst ? entry.Variations[0] : entry.FullName;
               }
            }
         } catch (Exception ex) { throw; }

         try {
            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\USCities.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var jsreader = new JsonTextReader(new StreamReader(fs, Encoding.UTF8))) {
               bool bSubArr = false;
               while (jsreader.Read()) {
                  if (jsreader.TokenType == JsonToken.StartArray) {
                     if (bSubArr) {
                        JArray arr = JArray.Load(jsreader);
                        string cityname = arr[0].ToString();
                        string statecode = arr[1].ToString();
                        string zipcode = arr[2].ToString();
                        double lat =double.Parse(arr[3].ToString());
                        double lon = double.Parse(arr[4].ToString());
                        var state = _usStates[statecode];

                        try {
                           var city = (LocationNode.LocalityNode)state.Children.GetValueOrDefault(cityname);
                           if (city == null) {
                              city = new LocationNode.LocalityNode(cityname, state);
                              state.Children.Add(cityname, city);
                              QueryAutoComplete.AddExact(city.CityName, city);
                              QueryAutoComplete.Add(city.StateName, city);
                              QueryAutoComplete.Add(city.Formatted, city);
                           }
                           var zip = (LocationNode.PostalCodeNode)city.Children.GetValueOrDefault(zipcode);
                           if (zip == null) {
                              zip = new LocationNode.PostalCodeNode(zipcode, city);
                              zip.Latitude = lat;
                              zip.Longitude = lon;
                              city.Children.Add(zipcode, zip);
                              QueryAutoComplete.AddExact(zipcode, zip);
                              QueryAutoComplete.Add(zip.StateName, zip);
                              QueryAutoComplete.Add(zip.Formatted, zip);
                           } else {
                              throw new Exception(zipcode);
                           }
                        } catch (Exception ex) { throw; }
                     }
                     bSubArr = true;
                  }
               }
            }
         } catch (Exception ex) { throw; }

         QueryAutoComplete.Optimize();
         return true;
      }

      private void AddLocationToQueryAutoComplete(DBLocation model) {
         bool bHasName = !String.IsNullOrWhiteSpace(model.Name);
         bool bHasAddress = !String.IsNullOrWhiteSpace(model.AddressLine);
         bool bHasZip = !String.IsNullOrWhiteSpace(model.PostalCode);
         bool bHasCity = !String.IsNullOrWhiteSpace(model.Locality);
         bool bHasState = !String.IsNullOrWhiteSpace(model.AdminDistrict);
         bool bHasCountry = !String.IsNullOrWhiteSpace(model.CountryRegion);

         Helpers.BlockUntilFinished(ref _initTask);
         LocationNode node;
         //TODO:
      }

      public DBLocation CreateDBLocation(StreetAddress address) {
         if (String.IsNullOrWhiteSpace(address.CountryRegion)) { throw new ArgumentException("Country Required"); }
         if (String.IsNullOrWhiteSpace(address.AdminDistrict)) { throw new ArgumentException("State Required"); }
         if (String.IsNullOrWhiteSpace(address.Locality)) { throw new ArgumentException("City Required"); }

         DBLocation model = new DBLocation(address);

         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_CreateOne]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter @LocationID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(model.@LocationID), null);
            SqlParameter @ParentLocationID = cmd.AddParam(ParameterDirection.InputOutput, SqlDbType.BigInt, nameof(model.@ParentLocationID), model.ParentLocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@Name), model.@Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@AddressLine), model.@AddressLine);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@Locality), model.@Locality);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@AdminDistrict), model.@AdminDistrict);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@PostalCode), model.@PostalCode);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@CountryRegion), model.CountryRegion);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.@Description), model.@Description);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(model.@Latitude6x), model.@Latitude6x);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Real, nameof(model.@Longitude6x), model.@Longitude6x);

            int rowsAffected = cmd.ExecuteProcedure();
            model.LocationID = (long)@LocationID.Value;
            model.ParentLocationID = (long?)ParentLocationID.Value;

            if(model.LocationID <= 0) {
               throw new Exception("Failed for Unknown Reason");
            } 
         }

         AddLocationToQueryAutoComplete(model);
         return model;
      }


      public List<DBLocation> SearchDBLocations(StreetAddress address) {
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_Search]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(address.@LocationID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(address.@ParentLocationID), address.ParentLocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Name), address.@Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@AddressLine), address.@AddressLine);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Locality), address.@Locality);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@AdminDistrict), address.@AdminDistrict);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@PostalCode), address.@PostalCode);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@CountryRegion), address.CountryRegion);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Description), address.@Description);

            return cmd.ExecuteReader_GetMany<DBLocation>().ToList();
         }
      }

      public DBLocation GetOrCreateDBLocation(StreetAddress address) {
         DBLocation existing;
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_Search]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(address.@LocationID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(address.@ParentLocationID), address.ParentLocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Name), address.@Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@AddressLine), address.@AddressLine);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Locality), address.@Locality);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@AdminDistrict), address.@AdminDistrict);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@PostalCode), address.@PostalCode);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@CountryRegion), address.CountryRegion);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(address.@Description), address.@Description);
            existing = cmd.ExecuteReader_GetOne<DBLocation>();
         }
      
         if (existing != null) {
            return existing;
         } else {
            return CreateDBLocation(address);
         }
      }

      public DBLocation GetDBLocationByID(long LocationID) {
         DBLocation location = _dbLocationCache.GetValueOrDefault(LocationID);
         if(location == null || location.RetrievedOn > DateTime.UtcNow.AddHours(-1)) {
            using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Locations_GetOne]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
               cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), LocationID);
               location = cmd.ExecuteReader_GetOne<DBLocation>();
            }
            if(location != null) {
               _dbLocationCache[LocationID] = location;
            }
         }
         return location;
      }


      public IEnumerable<LocationNode> QueryCachedLocations(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches<LocationNode>(query, 20, LocationQualityEvaluator);

         int LocationQualityEvaluator(string key, string term, LocationNode item) {
            if (term == null) { return 1; }
            int len = Math.Min(key.Length, term.Length);
            if (StringComparer.Ordinal.Equals(key, term)) {
               if (item is LocationNode.PostalCodeNode) {
                  return 6;
               }
               return 5;
            }
            int count = 0;
            for (int i = 0; i < len; i++) {
               if (key[i] == term[i]) {
                  count++;
               } else {
                  break;
               }
            }
            return 1 + (count / 5);
         }
      }
      public IEnumerable<LocationNode.CountryRegionNode> QueryCachedCountries(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches<LocationNode.CountryRegionNode>(query, 5);
      }
      public IEnumerable<LocationNode.AdminDistrictNode> QueryCachedStates(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches<LocationNode.AdminDistrictNode>(query, 20);
      }
      public IEnumerable<LocationNode.LocalityNode> QueryCachedCities(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches<LocationNode.LocalityNode>(query, 20);
      }
      public IEnumerable<LocationNode.PostalCodeNode> QueryCachedPostalCodes(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches<LocationNode.PostalCodeNode>(query, 20);
      }


      private class AbbreviationEntry {
         public string FullName;
         public string Preferred;
         public string[] Variations;
      }

      private class JSONAddresses {
         public Dictionary<string, JSONCountry> Countries;
         public Dictionary<string, string[]> Cardinal;
         public Dictionary<string, string[]> AddressLine;

         public class JSONCountry {
            public string[] Variations;
            public Dictionary<string, string[]> States;
         }
      }

   }
}
