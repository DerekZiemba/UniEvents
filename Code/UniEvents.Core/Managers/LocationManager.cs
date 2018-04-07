using System;
using System.Collections.Generic;
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



      public async Task<ApiResult<StreetAddress>> CreateLocation(StreetAddress address) {
         var result = new ApiResult<StreetAddress>();
         try {
            DBLocation dbLocation = new DBLocation(address);
            result.bSuccess = await DBLocation.SP_Locations_CreateOneAsync(Ctx, dbLocation).ConfigureAwait(false);
            result.Result = new StreetAddress(dbLocation);
            if (!result.bSuccess) {
               result.sMessage = "Failed for Unknown Reason";
            }
         } catch (Exception ex) {
            result.sMessage = ex.Message;
         }
         return result;
      }

      public async Task<StreetAddress> GetStreetAddressOrCreate(StreetAddress address) {
         DBLocation existing;
         using (SqlCommand cmd = DBLocation.GetSqlCommandForSP_Locations_Search(Ctx, Name:address.Name, AddressLine:address.AddressLine, Locality:address.Locality, AdminDistrict:address.AdminDistrict, PostalCode:address.PostalCode)) {
            existing = await cmd.ExecuteReader_GetOneAsync<DBLocation>().ConfigureAwait(false);
         }
         if (existing != null) {
            return new StreetAddress(existing);
         } else {
            DBLocation dbLocation = new DBLocation(address);
            if (await DBLocation.SP_Locations_CreateOneAsync(Ctx, dbLocation).ConfigureAwait(false)) {
               return new StreetAddress(dbLocation);
            }           
         }
         return null;
      }



      public IEnumerable<LocationNode> QueryCachedLocations(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         return QueryAutoComplete.FindMatches(query, 20);
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
