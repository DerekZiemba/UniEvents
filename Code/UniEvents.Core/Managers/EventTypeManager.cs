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

using ZMBA;


namespace UniEvents.Core.Managers {

   public class EventTypeManager {
      private const int MissExpireSecs = 5 * 60;
      private readonly Factory Ctx;

      private Task _initTask;
      private List<CachedEntry> _allEntries;

      private readonly ConcurrentDictionary<long, CachedEntry> _byId = new ConcurrentDictionary<long, CachedEntry>();
      private readonly ConcurrentDictionary<string, CachedEntry> _byName = new ConcurrentDictionary<string, CachedEntry>();

      private readonly ConcurrentDictionary<long, DateTime> _idMisses = new ConcurrentDictionary<long, DateTime>();
      private readonly ConcurrentDictionary<string, DateTime> _nameMisses = new ConcurrentDictionary<string, DateTime>();

      internal EventTypeManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run(Init);
      }

      private async Task Init() {
         using (var cmd = DBEventType.GetSqlCommandForSP_Search(this.Ctx)) {
            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  var tag = new CachedEntry(new DBEventType(reader));
                  _byId[tag.Item.EventTypeID] = tag;
                  _byName[tag.NormName] = tag;
               }
            }
            _allEntries = _byId.Values.ToList();
         }
      }

      public DBEventType this[long id] {
         get {
            if (id <= 0) { return null; }
            Helpers.BlockUntilFinished(ref _initTask);
            if (_byId.TryGetValue(id, out var cached)) {
               return cached.Item;
            }
            DateTime lastMiss = _idMisses.GetValueOrDefault(id);
            if (lastMiss < DateTime.Now.AddSeconds(-MissExpireSecs)) {
               using (var cmd = DBEventType.GetSqlCommandForSP_GetOne(Ctx, id)) {
                  CachedEntry tag = AddTag(cmd.ExecuteReader_GetOne<DBEventType>());
                  if (tag != null) {
                     return tag.Item;
                  }
               }
               _idMisses[id] = DateTime.Now;
            }
            return null;
         }
      }

      public DBEventType this[string name] {
         get {
            if (String.IsNullOrWhiteSpace(name)) { return null; }
            name = name.ToAlphaNumericLower();
            Helpers.BlockUntilFinished(ref _initTask);
            if (_byName.TryGetValue(name, out var cached)) {
               return cached.Item;
            }
            DateTime lastMiss = _nameMisses.GetValueOrDefault(name);
            if (lastMiss < DateTime.Now.AddSeconds(-MissExpireSecs)) {
               using (var cmd = DBEventType.GetSqlCommandForSP_GetOne(Ctx, null, name)) {
                  CachedEntry tag = AddTag(cmd.ExecuteReader_GetOne<DBEventType>());
                  if (tag != null) {
                     return tag.Item;
                  }
               }
               _nameMisses[name] = DateTime.Now;
            }
            return null;
         }
      }

      private CachedEntry AddTag(DBEventType dbtag) {
         if (dbtag != null) {
            var tag = new CachedEntry(dbtag);
            _byId[tag.Item.EventTypeID] = tag;
            _byName[tag.NormName] = tag;
            _allEntries.Add(tag);
            return tag;
         }
         return null;
      }

      public IEnumerable<DBEventType> SearchCached(string name, string description) {
         string normName = name.ToAlphaNumericLower();
         string normDesc = description.ToAlphaNumericLower();
         bool bHasName = normName != null && normName.Length > 0;
         bool bHasDesc = normName != null && normName.Length > 1;
         Helpers.BlockUntilFinished(ref _initTask);

         if (!bHasName && !bHasDesc) {
            for (var i = 0; i < _allEntries.Count; i++) { yield return _allEntries[i].Item; }
         } else {
            if (bHasName && _byName.TryGetValue(normName, out var outval)) {
               yield return outval.Item;
            } else {
               List<DBEventType> Ctier = ListCache<DBEventType>.Take();
               if (bHasName && bHasDesc) {
                  List<DBEventType> Btier = ListCache<DBEventType>.Take();
                  for (var i = 0; i < _allEntries.Count; i++) {
                     var tag = _allEntries[i];
                     int idxName = tag.NormName.IndexOf(normName, StringComparison.Ordinal);
                     int idxDesc = tag.NormDesc?.IndexOf(normDesc, StringComparison.Ordinal)??-1;
                     if (idxName == 0 && idxDesc >= 0) {
                        yield return tag.Item;
                     } else if (idxName >= 0 && idxDesc >= 0) {
                        Btier.Add(tag.Item);
                     } else if (idxName >= 0 || idxDesc >= 0) {
                        Ctier.Add(tag.Item);
                     }
                  }
                  for (var i = 0; i < Btier.Count; i++) { yield return Btier[i]; }
                  ListCache<DBEventType>.Return(ref Btier);
               } else {
                  string value = bHasName ? normName : normDesc;
                  for (var i = 0; i < _allEntries.Count; i++) {
                     var tag = _allEntries[i];
                     int idx = (bHasName ? tag.NormName : tag.NormDesc).IndexOf(value, StringComparison.Ordinal);
                     if (idx == 0) {        //Matches at the start
                        yield return tag.Item;
                     } else if (idx > 0) {  //Matches some where in the middle
                        Ctier.Add(tag.Item);
                     }
                  }
               }
               for (var i = 0; i < Ctier.Count; i++) { yield return Ctier[i]; }
               ListCache<DBEventType>.Return(ref Ctier);
            }
         }
      }

      public IEnumerable<DBEventType> QueryCached(string query) {
         string norm = query.ToAlphaNumericLower();
         Helpers.BlockUntilFinished(ref _initTask);

         if (String.IsNullOrEmpty(norm)) {
            for (var i = 0; i < _allEntries.Count; i++) { yield return _allEntries[i].Item; }
         } else {
            if (_byName.TryGetValue(norm, out var outval)) {
               yield return outval.Item;
            } else {
               List<DBEventType> Ctier = ListCache<DBEventType>.Take();
               List<DBEventType> Btier = ListCache<DBEventType>.Take();
               for (var i = 0; i < _allEntries.Count; i++) {
                  var tag = _allEntries[i];
                  int idxName = tag.NormName.IndexOf(norm, StringComparison.Ordinal);
                  int idxDesc = tag.NormDesc?.IndexOf(norm, StringComparison.Ordinal) ?? -1;
                  if (idxName == 0 && idxDesc >= 0) {
                     yield return tag.Item;
                  } else if (idxName >= 0 && idxDesc >= 0) {
                     Btier.Add(tag.Item);
                  } else if (idxName >= 0 || idxDesc >= 0) {
                     Ctier.Add(tag.Item);
                  }
               }
               for (var i = 0; i < Btier.Count; i++) { yield return Btier[i]; }
               ListCache<DBEventType>.Return(ref Btier);
               for (var i = 0; i < Ctier.Count; i++) { yield return Ctier[i]; }
               ListCache<DBEventType>.Return(ref Ctier);
            }
         }
      }



      public DBEventType Create(string name, string description) {
         return AddTag(DBEventType.SP_EventType_Create(Ctx, name, description))?.Item;
      }



      private class CachedEntry {
         public DBEventType Item;
         public string NormName;
         public string NormDesc;
         public CachedEntry(DBEventType tag) {
            Item = tag;
            NormName = tag.Name.ToAlphaNumericLower();
            NormDesc = tag.Description.ToAlphaNumericLower();
         }
      }

   }
}
