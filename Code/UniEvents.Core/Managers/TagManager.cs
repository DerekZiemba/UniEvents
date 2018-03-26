using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;

using static ZMBA.Common;


namespace UniEvents.Managers {

	public class TagManager {
      private const int SIZE = 128;
      private const int EXPIRESECONDS = 60;

      private readonly CoreContext Ctx;
      private readonly Dictionary<long, DBTag> _byId = new Dictionary<long, DBTag>(SIZE);
      private readonly Dictionary<string, DBTag> _byName = new Dictionary<string, DBTag>(SIZE);

      private readonly TimeRecord<long>[] _idMisses = new TimeRecord<long>[SIZE];
      private readonly TimeRecord<string>[] _nameMisses = new TimeRecord<string>[SIZE];

      private int _idLen = 0;
      private int _nameLen = 0;

      internal TagManager(CoreContext ctx) {
         this.Ctx = ctx;
		}

      //public unsafe DBTag this[long id] {
      //   get {
      //      DBTag value = null;
      //      if(_byId.TryGetValue(id, out value)) {
      //         return value;
      //      }
      //      int exp = -1;
      //      DateTime cutoff = DateTime.Now.AddSeconds(EXPIRESECONDS);
      //      for (int i = 0; i < _idLen; i++) {
      //         if (_idMisses[i].Key == id && _idMisses[i].Time != default && _idMisses[i].Time > cutoff) { return null; }
      //         if (exp < 0 && _idMisses[i].Time < cutoff) { exp = i; }
      //      }
      //      try {

      //      } catch (Exception ex) { if (System.Diagnostics.Debugger.IsAttached) { throw; } }

      //      DBTag.SP_Tags_Search
      //      return null;
      //   }
      //}

      //public  DBTag this[string name] {
      //   get {
      //      return null;
      //   }
      //}





   }
}
