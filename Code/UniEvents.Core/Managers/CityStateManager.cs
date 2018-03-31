using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniEvents.Models;
using ZMBA;


namespace UniEvents.Core.Managers {

  
   public class CityStateManager {
      private readonly Factory Ctx;

      private Task _initTask;

      private Dictionary<string, AbbreviationEntry> DirectionAbbreviations;
      private Dictionary<string, AbbreviationEntry> AddressLineAbbreviations;

      private PartialNameSearchTrie<LocationNode> QueryAutoComplete = new PartialNameSearchTrie<LocationNode>();

      internal CityStateManager(Factory ctx) {
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
               var countryNode = new LocationNode.CountryRegionNode() {Name=kvpCountry.Key, Variations = kvpCountry.Value.Variations };
               SetPreferedName(countryNode, (cbstr) => {
                  //CountryAutoComplete.AddItem(cbstr, countryNode);
                  QueryAutoComplete.AddItem(cbstr, countryNode);
               });
               var bIsUSA = countryNode.Variations.Includes("USA");
               foreach (KeyValuePair<string, string[]> kvp in kvpCountry.Value.States) {
                  var stateNode = new LocationNode.AdminDistrictNode() {Name=kvp.Key, Variations=kvp.Value};
                  stateNode.Parent = countryNode;
                  SetPreferedName(stateNode, (cbstr) => {
                     countryNode.Children.Add(cbstr, stateNode);
                     QueryAutoComplete.AddItem(cbstr, stateNode);
                     if (bIsUSA) {
                        _usStates.Add(cbstr, stateNode);
                     }
                  });
               }
            }
            void SetPreferedName(LocationNode.LocationNodeWithAbbreviations entry, Action<string> callback) {
               callback(entry.Name);
               for (var i = 0; i < entry.Variations.Length; i++) {
                  if (entry.Variations[i][0] == '$') {
                     entry.Variations[i] = entry.Variations[i].Substring(1);
                     entry.PreferredName = entry.Variations[i];
                  }
                  callback(entry.Variations[i]);
               }
               if (entry.PreferredName == null) {
                  entry.PreferredName = entry.Variations[0];
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
                              city = new LocationNode.LocalityNode();
                              city.Parent = state;
                              city.Name = cityname;
                              state.Children.Add(cityname, city);
                              QueryAutoComplete.AddItem(city);
                              QueryAutoComplete.AddItem(statecode, city);                              
                              //QueryAutoComplete.AddItem(state.Name, city);
                           }
                           var zip = (LocationNode.PostalCodeNode)city.Children.GetValueOrDefault(zipcode);
                           if (zip == null) {
                              zip = new LocationNode.PostalCodeNode();
                              zip.Parent = city;
                              zip.Name = zipcode;
                              zip.Latitude = lat;
                              zip.Longitude = lon;
                              city.Children.Add(zipcode, zip);
                              QueryAutoComplete.AddItem(zip);
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

         return true;
      }


      public IEnumerable<LocationNode> QueryLocations(string query) {
         const int MaxYield = 25;
         int yieldcount = 0;
         if (String.IsNullOrEmpty(query)) { yield break; }
         Helpers.BlockUntilFinished(ref _initTask);

         string[] words = query.Split(new []{' ', ','}, StringSplitOptions.RemoveEmptyEntries);

         HashSet<LocationNode> hsAll = HashSetCache<LocationNode>.Take();
         HashSet<LocationNode.SubdivisionNode> hsPrecise = HashSetCache<LocationNode.SubdivisionNode>.Take();

         HashSet<LocationNode>[] arrWordNodes = new HashSet<LocationNode>[words.Length];
         for (var i = 0; i < words.Length; i++) {
            arrWordNodes[i] = HashSetCache<LocationNode>.Take();
            var matches = QueryAutoComplete.FindMatches(words[i], true);
            foreach (var entity in matches) {
               if (arrWordNodes[i].Add(entity)) {
                  if (entity is LocationNode.SubdivisionNode) {
                     hsPrecise.Add((LocationNode.SubdivisionNode)entity);
                  } else {
                     hsAll.Add(entity);
                  }
               }
            }
         }

         Queue<LocationNode>[] prospects = new Queue<LocationNode>[words.Length];
         foreach (var entity in hsPrecise) {
            int count = 1;
            for (var i = 0; i < arrWordNodes.Length; i++) {
               if (arrWordNodes[i].Contains(entity.CityNode)) { count++; }
            }
            if (count >= arrWordNodes.Length) {
               yieldcount++;
               yield return entity;
               if (yieldcount >= MaxYield) { break; }
            } else {
               if (prospects[count] == null) { prospects[count] = QueueCache<LocationNode>.Take(); }
               prospects[count].Enqueue(entity);
            }
         }
         HashSetCache<LocationNode.SubdivisionNode>.Return(ref hsPrecise);

         foreach (var entity in hsAll) {
            int count = 0;
            for (var i = 0; i < arrWordNodes.Length; i++) {
               if (arrWordNodes[i].Contains(entity)) { count++; }
            }
            if (count == 0) { continue; }

            if (count == arrWordNodes.Length) {
               yieldcount++;
               yield return entity;
               if(yieldcount >= MaxYield) { break; }
            } else {
               if (prospects[count] == null) { prospects[count] = QueueCache<LocationNode>.Take(); }
               prospects[count].Enqueue(entity);
            }
         }

         HashSetCache<LocationNode>.Return(ref hsAll);

         for (var i = prospects.Length - 1; i >= 0; i--) {
            while (yieldcount < MaxYield && prospects[i] != null && prospects[i].Count > 0) {
               yieldcount++;
               yield return prospects[i].Dequeue();
            }
            if (prospects[i] != null) { QueueCache<LocationNode>.Return(ref prospects[i]); }
         }
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
