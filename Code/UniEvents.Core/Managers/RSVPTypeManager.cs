﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using static ZMBA.Common;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using ApiModels = UniEvents.Models.ApiModels;
using DBModels = UniEvents.Models.DBModels;


namespace UniEvents.Managers {

   public class RSVPTypeManager {
      private readonly CoreContext Ctx;

      public IList<RSVPType> RSVPTypes { get; private set; }

      public RSVPType No { get; private set; }
      public RSVPType Maybe { get; private set; }
      public RSVPType StopBy { get; private set; }
      public RSVPType Later { get; private set; }
      public RSVPType Attending { get; private set; }

      internal RSVPTypeManager(CoreContext ctx) {
         this.Ctx = ctx;
         this.RSVPTypes = DBModels.DBRSVPType.SP_RSVPTypes_Get(ctx).Select(x => {
            var y = new RSVPType(x);
            if (y.Name.EqIgCaseSym(nameof(No))) {
               this.No = y;
            } else if (y.Name.EqIgCaseSym(nameof(Maybe))) {
               this.Maybe = y;
            } else if (y.Name.EqIgCaseSym(nameof(StopBy))) {
               this.StopBy = y;
            } else if (y.Name.EqIgCaseSym(nameof(Later))) {
               this.Later = y;
            } else if (y.Name.EqIgCaseSym(nameof(Attending))) {
               this.Attending = y;
            }
            return y;
         }).ToList().AsReadOnly();
      }


   }
}