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

using ZMBA;


namespace UniEvents.Managers {

	public class TagManager {
      private const int ExpireSeconds = 10*60;

      private readonly Factory Ctx;

      private readonly ConcurrentBag<DBTag> _allTags = new ConcurrentBag<DBTag>();

      private readonly ConcurrentDictionary<long, DBTag> _byId = new ConcurrentDictionary<long, DBTag>();
      private readonly ConcurrentDictionary<string, DBTag> _byName = new ConcurrentDictionary<string, DBTag>();

      private readonly ConcurrentDictionary<long, DateTime> _idMisses = new ConcurrentDictionary<long, DateTime>();       
      private readonly ConcurrentDictionary<string, DateTime> _nameMisses = new ConcurrentDictionary<string, DateTime>();

      private readonly ConcurrentDictionary<string, DBTag[]> _searches = new ConcurrentDictionary<string, DBTag[]>();
      private readonly ConcurrentDictionary<string, DateTime> _searchMisses = new ConcurrentDictionary<string, DateTime>();

      internal TagManager(Factory ctx) {
         this.Ctx = ctx;
		}

      private void Add(DBTag tag) {
         _byId[tag.TagID] = tag;
         _byName[tag.Name.ToAlphaNumericLower()] = tag;
         _allTags.Add(tag);
      }

      public DBTag this[long id] {
         get {
            if (id <= 0) { return null; }
            DBTag value = null;
            if (_byId.TryGetValue(id, out value)) {
               return value;
            }
            //Limit the amount of times we check the database for a record if this id has already failed to produce a record recently
            if(_idMisses.TryGetValue(id, out DateTime time) && time < DateTime.Now.AddSeconds(-ExpireSeconds) ) {
               return null;
            }

            try {
               value = DBTag.SP_Tags_GetOne(Ctx, TagID: id);
            } catch { if (Ctx.Config.IsDebugMode) { throw; } }

            if (value == null) {
               _idMisses[id] = DateTime.Now;
            } else {
               Add(value);
            }
            return value;
         }
      }

      public DBTag this[string name] {
         get {
            if (String.IsNullOrWhiteSpace(name)) { return null; }
            string normalized = name.ToAlphaNumericLower();
            DBTag value = null;
            if (_byName.TryGetValue(normalized, out value)) {
               return value;
            }
            //Limit the amount of times we check the database for a record if this id has already failed to produce a record recently
            if (_nameMisses.TryGetValue(normalized, out DateTime time) && time < DateTime.Now.AddSeconds(-ExpireSeconds)) {
               return null;
            }

            try {
               value = DBTag.SP_Tags_GetOne(Ctx, Name: name);
            } catch { if (Ctx.Config.IsDebugMode) { throw; } }

            if (value == null) {
               _nameMisses[normalized] = DateTime.Now;
            } else {
               Add(value);
            }
            return value;
         }
      }


      //public DBTag[] Search(string str) {
      //   if(String.IsNullOrWhiteSpace(str)) { return null; }
      //   string normalized = str.ToAlphaNumericLower();
      //   if(_searches.TryGetValue(normalized, out var arr)) {
      //      return arr;
      //   }
      //   if (_searchMisses.TryGetValue(normalized, out DateTime time) && time < DateTime.Now.AddSeconds(-ExpireSeconds)) {
      //      return null;
      //   }

      //   IEnumerable<DBTag> tags = null;
      //   try {
      //      tags = DBTag.SP_Tags_Query(Ctx, str);
      //   } catch (Exception ex) { if (Ctx.Config.IsDebugMode) { throw; } }

      //   if(tags != null) {
      //      List<DBTag> candidates = ListCache<DBTag>.Take(64);
      //      foreach(var tag in tags) {
      //         yield return tag;
      //      }
      //   }

      //   return null;
      //}

   }
}
