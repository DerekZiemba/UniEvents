﻿using System;
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

      private readonly ConcurrentDictionary<long, CachedEntry> _byId = new ConcurrentDictionary<long, CachedEntry>();
      private readonly ConcurrentDictionary<string, CachedEntry> _byName = new ConcurrentDictionary<string, CachedEntry>();
      private readonly PartialKeySearchTrie<CachedEntry> QueryAutoComplete = new PartialKeySearchTrie<CachedEntry>();

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
                  QueryAutoComplete.Add(tag.Item.Name, tag);
                  QueryAutoComplete.Add(tag.Item.Description, tag);
               }
            }
         }
         QueryAutoComplete.Optimize();
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
            QueryAutoComplete.Add(tag.Item.Name, tag);
            QueryAutoComplete.Add(tag.Item.Description, tag);
            return tag;
         }
         return null;
      }


      public IEnumerable<DBEventType> QueryCached(string query) {
         Helpers.BlockUntilFinished(ref _initTask);
         foreach (var cached in QueryAutoComplete.FindMatches(query, 15)) {
            yield return cached.Item;
         }
      }



      public DBEventType Create(string name, string description) {
         return AddTag(DBEventType.SP_EventType_Create(Ctx, name, description))?.Item;
      }



      private class CachedEntry {
         public DBEventType Item;
         public string NormName;

         public CachedEntry(DBEventType tag) {
            Item = tag;
            NormName = tag.Name.ToAlphaNumericLower();
         }

         public override bool Equals(object x) => this.Equals((CachedEntry)x);
         public bool Equals(CachedEntry other) {
            if (other == null) { return false; }
            if (this.Item.EventTypeID != other.Item.EventTypeID) { return false; }
            return true;
         }

         public override int GetHashCode() => this.Item.EventTypeID.GetHashCode();

      }

   }
}
