using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;

using ZMBA;


namespace UniEvents.Core.Managers {

   public class TagManager {
      private const int MissExpireSecs = 5 * 60;

      private readonly Factory Ctx;

      private Task _initTask;

      private readonly ConcurrentDictionary<long, CachedEntry> _byId = new ConcurrentDictionary<long, CachedEntry>();
      private readonly ConcurrentDictionary<string, CachedEntry> _byName = new ConcurrentDictionary<string, CachedEntry>();
      private readonly PartialKeySearchTrie<CachedEntry> QueryAutoComplete = new PartialKeySearchTrie<CachedEntry>();

      private readonly ConcurrentDictionary<long, DateTime> _idMisses = new ConcurrentDictionary<long, DateTime>();
      private readonly ConcurrentDictionary<string, DateTime> _nameMisses = new ConcurrentDictionary<string, DateTime>();

      internal TagManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run(Init);
      }

      private async Task Init() {
         using (var cmd = DBTag.GetSqlCommandForSP_Search(this.Ctx)) {
            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  var tag = new CachedEntry(new DBTag(reader));
                  _byId[tag.Item.TagID] = tag;
                  _byName[tag.NormName] = tag;
                  QueryAutoComplete.Add(tag.Item.Name, tag);
                  QueryAutoComplete.Add(tag.Item.Description, tag);
               }
            }
         }
         QueryAutoComplete.Optimize();
      }

      public DBTag this[long id] {
         get {
            if (id <= 0) { return null; }
            Helpers.BlockUntilFinished(ref _initTask);
            if (_byId.TryGetValue(id, out var cached)) {
               return cached.Item;
            }
            DateTime lastMiss = _idMisses.GetValueOrDefault(id);
            if (lastMiss < DateTime.Now.AddSeconds(-MissExpireSecs)) {
               using (var cmd = DBTag.GetSqlCommandForSP_GetOne(Ctx, id)) {
                  CachedEntry tag = AddTag(cmd.ExecuteReader_GetOne<DBTag>());
                  if (tag != null) {
                     return tag.Item;
                  }
               }
               _idMisses[id] = DateTime.Now;
            }
            return null;
         }
      }

      public DBTag this[string name] {
         get {
            if (String.IsNullOrWhiteSpace(name)) { return null; }
            name = name.ToAlphaNumericLower();
            Helpers.BlockUntilFinished(ref _initTask);
            if (_byName.TryGetValue(name, out var cached)){
               return cached.Item;
            }
            DateTime lastMiss = _nameMisses.GetValueOrDefault(name);
            if(lastMiss < DateTime.Now.AddSeconds(-MissExpireSecs)) {
               using (var cmd = DBTag.GetSqlCommandForSP_GetOne(Ctx, null, name)) {
                  CachedEntry tag = AddTag(cmd.ExecuteReader_GetOne<DBTag>());
                  if(tag != null) {
                     return tag.Item;
                  }
               }
               _nameMisses[name] = DateTime.Now;
            }           
            return null;
         }
      }

      private CachedEntry AddTag(DBTag dbtag) {
         if (dbtag != null) {
            var tag = new CachedEntry(dbtag);
            _byId[tag.Item.TagID] = tag;
            _byName[tag.NormName] = tag;
            QueryAutoComplete.Add(tag.Item.Name, tag);
            QueryAutoComplete.Add(tag.Item.Description, tag);
            return tag;
         }
         return null;
      }

      public IEnumerable<DBTag> QueryCached(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         foreach (var cached in QueryAutoComplete.FindMatches(query, 15)) {
            yield return cached.Item;
         }
      }


      public DBTag Create(string name, string description) {
         return AddTag(DBTag.SP_Create(Ctx, name, description))?.Item;
      }

      public async Task LinkTagToEvent(long EventID, long TagID) {
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Event_TagAdd]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(EventID), EventID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(TagID), TagID);
            await cmd.ExecuteProcedureAsync().ConfigureAwait(false);
         }
      }


      private class CachedEntry {
         public DBTag Item;
         public string NormName;

         public CachedEntry(DBTag tag) {
            Item = tag;
            NormName = tag.Name.ToAlphaNumericLower();
         }

         public override bool Equals(object x) => this.Equals((CachedEntry)x);
         public bool Equals(CachedEntry other) {
            if (other == null) { return false; }
            if (this.Item.TagID != other.Item.TagID) { return false; }
            return true;
         }

         public override int GetHashCode() => this.Item.TagID.GetHashCode();

      }

   }
}
