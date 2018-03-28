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


namespace UniEvents.Managers {

   public class TagManager {
      private readonly Factory Ctx;

      private Task _initTask;
      private CachedDBTag[] _allTags;

      private readonly ConcurrentDictionary<long, CachedDBTag> _byId = new ConcurrentDictionary<long, CachedDBTag>();
      private readonly ConcurrentDictionary<string, CachedDBTag> _byName = new ConcurrentDictionary<string, CachedDBTag>();

      internal TagManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run(Init);
      }

      private async Task Init() {
         using (var cmd = DBTag.GetSqlCommandForSP_Tags_Search(this.Ctx)) {
            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  var tag = new CachedDBTag(new DBTag(reader));
                  _byId[tag.Tag.TagID] = tag;
                  _byName[tag.NormName] = tag;
               }
            }
            _allTags = _byId.Values.ToArray();
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

      public DBTag this[long id] {
         get {
            if (id <= 0) { return null; }
            BlockUntilInit();
            return _byId.TryGetValue(id, out var cached) ? cached.Tag : null;
         }
      }

      public DBTag this[string name] {
         get {
            if (String.IsNullOrWhiteSpace(name)) { return null; }
            name = name.ToAlphaNumericLower();
            BlockUntilInit();
            return _byName.TryGetValue(name, out var cached) ? cached.Tag : null;
         }
      }

      public IEnumerable<DBTag> Search(string name, string description) {
         string normName = name.ToAlphaNumericLower();
         string normDesc = description.ToAlphaNumericLower();
         bool bHasName = normName != null && normName.Length > 0;
         bool bHasDesc = normName != null && normName.Length > 1;
         BlockUntilInit();

         if (!bHasName && !bHasDesc) {
            for (var i = 0; i < _allTags.Length; i++) { yield return _allTags[i].Tag; }
         } else {          
            if (bHasName && _byName.TryGetValue(normName, out var outval)) {
               yield return outval.Tag;
            } else {
               List<DBTag> Ctier = ListCache<DBTag>.Take();
               if (bHasName && bHasDesc) {
                  List<DBTag> Btier = ListCache<DBTag>.Take();
                  for (var i = 0; i < _allTags.Length; i++) {
                     var tag = _allTags[i];
                     int idxName = tag.NormName.IndexOf(normName, StringComparison.Ordinal);
                     int idxDesc = tag.NormDesc.IndexOf(normDesc, StringComparison.Ordinal);
                     if (idxName == 0 && idxDesc >= 0) {
                        yield return tag.Tag;
                     } else if (idxName >= 0 && idxDesc >= 0) {
                        Btier.Add(tag.Tag);
                     } else if (idxName >= 0 || idxDesc >= 0) {
                        Ctier.Add(tag.Tag);
                     }
                  }
                  for (var i = 0; i < Btier.Count; i++) { yield return Btier[i]; }
                  ListCache<DBTag>.Return(Btier);
               } else {
                  string value = bHasName ? normName : normDesc;
                  for (var i = 0; i < _allTags.Length; i++) {
                     var tag = _allTags[i];
                     int idx = (bHasName ? tag.NormName : tag.NormDesc).IndexOf(value, StringComparison.Ordinal);
                     if (idx == 0) {        //Matches at the start
                        yield return tag.Tag;
                     } else if (idx > 0) {  //Matches some where in the middle
                        Ctier.Add(tag.Tag);
                     }
                  }
               }
               for (var i = 0; i < Ctier.Count; i++) { yield return Ctier[i]; }
               ListCache<DBTag>.Return(Ctier);
            }
         }
      }

      public IEnumerable<DBTag> Query(string query) {
         string norm = query.ToAlphaNumericLower();
         BlockUntilInit();

         if (String.IsNullOrEmpty(norm)) {
            for (var i = 0; i < _allTags.Length; i++) { yield return _allTags[i].Tag; }
         } else {         
            if (_byName.TryGetValue(norm, out var outval)) {
               yield return outval.Tag;
            } else {
               List<DBTag> Ctier = ListCache<DBTag>.Take();
               List<DBTag> Btier = ListCache<DBTag>.Take();
               for (var i = 0; i < _allTags.Length; i++) {
                  var tag = _allTags[i];
                  int idxName = tag.NormName.IndexOf(norm, StringComparison.Ordinal);
                  int idxDesc = tag.NormDesc.IndexOf(norm, StringComparison.Ordinal);
                  if (idxName == 0 && idxDesc >= 0) {
                     yield return tag.Tag;
                  } else if (idxName >= 0 && idxDesc >= 0) {
                     Btier.Add(tag.Tag);
                  } else if (idxName >= 0 || idxDesc >= 0) {
                     Ctier.Add(tag.Tag);
                  }
               }
               for (var i = 0; i < Btier.Count; i++) { yield return Btier[i]; }
               ListCache<DBTag>.Return(Btier);
               for (var i = 0; i < Ctier.Count; i++) { yield return Ctier[i]; }
               ListCache<DBTag>.Return(Ctier);
            }
         }
      }

      private class CachedDBTag {
         public DBTag Tag;
         public string NormName;
         public string NormDesc;
         public CachedDBTag(DBTag tag) {
            Tag = tag;
            NormName = tag.Name.ToAlphaNumericLower();
            NormDesc = tag.Description.ToAlphaNumericLower();
         }
      }

   }
}
