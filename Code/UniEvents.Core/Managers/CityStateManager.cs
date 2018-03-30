using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZMBA;


namespace UniEvents.Core.Managers {

   public class CityStateManager {
      private readonly Factory Ctx;

      private Task _initTask;

      private Dictionary<string, AbbreviationEntry> StateAbbreviations;
      private Dictionary<string, AbbreviationEntry> DirectionAbbreviations;
      private Dictionary<string, AbbreviationEntry> AddressLineAbbreviations;

      private List<CityEntry> AllCities;
      private AsciiAutoCompleteTrie<CityEntry> CitiesAutocomplete = new AsciiAutoCompleteTrie<CityEntry>();

      internal CityStateManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run(Init);
      }

      private async Task<bool> Init() {
         Task<bool> taskAbbrevs = Task.Run((Func<bool>)LoadAbbreviations);
         Task<bool> taskCities = Task.Run((Func<bool>)LoadCities);

         await Task.WhenAll(taskAbbrevs, taskCities).ConfigureAwait(false);

         for (var i = 0; i < AllCities.Count; i++) {
            CityEntry entry = AllCities[i];
            entry.StateAbbrv = StateAbbreviations.GetItemOrDefault(entry.StateCode);
         }

         return true;

         bool LoadAbbreviations() {
            JSONAbbreviations jsabbrevs = null;
            
            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\Abbreviations.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            using (var jsreader = new JsonTextReader(sr)) {
               jsabbrevs = Common.JsonSerializer.Deserialize<JSONAbbreviations>(jsreader);
            }

            Dictionary<string, AbbreviationEntry> dict = new Dictionary<string, AbbreviationEntry>(600, StringComparer.OrdinalIgnoreCase);

            foreach(KeyValuePair<string, string[]> kvp in jsabbrevs.States) {
               var entry = new AbbreviationEntry(){FullName = kvp.Key, Variations = kvp.Value };
               dict.Add(entry.FullName, entry);
               SetPreferedAbbreviation(dict, entry, true);
            }
            StateAbbreviations = new Dictionary<string, AbbreviationEntry>(dict, StringComparer.OrdinalIgnoreCase);
            dict.Clear();

            foreach (KeyValuePair<string, string[]> kvp in jsabbrevs.Cardinal) {
               var entry = new AbbreviationEntry(){FullName = kvp.Key, Variations = kvp.Value };
               dict.Add(entry.FullName, entry);
               SetPreferedAbbreviation(dict, entry, true);
            }
            DirectionAbbreviations = new Dictionary<string, AbbreviationEntry>(dict, StringComparer.OrdinalIgnoreCase);
            dict.Clear();

            foreach (KeyValuePair<string, string[]> kvp in jsabbrevs.AddressLine) {
               var entry = new AbbreviationEntry(){FullName = kvp.Key, Variations = kvp.Value };
               dict.Add(entry.FullName, entry);
               SetPreferedAbbreviation(dict, entry, false);
            }
            AddressLineAbbreviations = new Dictionary<string, AbbreviationEntry>(dict, StringComparer.OrdinalIgnoreCase);
            return true;

         }


         bool LoadCities() {
            AllCities = new List<CityEntry>(42000);
            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\USCities.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            using (var jsreader = new JsonTextReader(sr)) {
               bool bSubArr = false;
               while (jsreader.Read()) {
                  if(jsreader.TokenType == JsonToken.StartArray) {
                     if (bSubArr) {
                        JArray arr = JArray.Load(jsreader);
                        CityEntry city = new CityEntry(){
                           City=arr[0].ToString(),
                           StateCode=arr[1].ToString(),
                           Zip=arr[2].ToString(),
                           Latitude=double.Parse(arr[3].ToString()),
                           Longitude=double.Parse(arr[4].ToString())
                        };
                        AllCities.Add(city);
                     }
                     bSubArr = true;
                  }
               }
            }

            for(var i = 0; i < AllCities.Count; i++) {
               CityEntry entry = AllCities[i];
               CitiesAutocomplete.AddEntryInternal(entry.City, entry);
            }

            return true;
         }


         void SetPreferedAbbreviation(Dictionary<string, AbbreviationEntry> dict, AbbreviationEntry entry, bool bUseFirst) {
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

      }

      private void BlockUntilInit() {
         if (_initTask == null) { return; }
         if (!_initTask.IsCompleted) {
            if (_initTask.IsFaulted) { throw _initTask.Exception; }
            _initTask.ConfigureAwait(false).GetAwaiter().GetResult();
         }
         _initTask.Dispose();
         _initTask = null;
      }


      public IEnumerable<CityEntry> QueryCity(string query) {
         BlockUntilInit();
         return CitiesAutocomplete.GetSuggestions(query);
      }


      public class CityEntry {
         public string City;
         public string StateCode;
         public string Zip;
         public double Latitude;
         public double Longitude;
         public AbbreviationEntry StateAbbrv;
      }

      public class AbbreviationEntry {
         public string FullName;
         public string Preferred;
         public string[] Variations;
      }

      private class JSONAbbreviations {
         public Dictionary<string, string[]> States;
         public Dictionary<string, string[]> Cardinal;
         public Dictionary<string, string[]> AddressLine;
      }


   }
}
