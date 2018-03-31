using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using static ZMBA.Common;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;
using System.Data;
using System.Data.SqlClient;

namespace UniEvents.Core.Managers {

   public class RSVPTypeManager {
      private readonly Factory Ctx;

      private Task _initTask;
      private IList<RSVPType> _RSVPTypes;
      private RSVPType _No;
      private RSVPType _Maybe ;
      private RSVPType _StopBy;
      private RSVPType _Later ;
      private RSVPType _Attending;

      public IList<RSVPType> RSVPTypes => Helpers.BlockUntilFinished(ref _initTask, ref _RSVPTypes);
      public RSVPType No => Helpers.BlockUntilFinished(ref _initTask, ref _No);
      public RSVPType Maybe => Helpers.BlockUntilFinished(ref _initTask, ref _Maybe);
      public RSVPType StopBy => Helpers.BlockUntilFinished(ref _initTask, ref _StopBy);
      public RSVPType Later => Helpers.BlockUntilFinished(ref _initTask, ref _Later);
      public RSVPType Attending => Helpers.BlockUntilFinished(ref _initTask, ref _Attending);


      internal RSVPTypeManager(Factory ctx) {
         this.Ctx = ctx;
         _initTask = Task.Run(Init);
      }


      private async Task<bool> Init() {
         using (var cmd = new SqlCommand("[dbo].[sp_RSVPTypes_Get]", new SqlConnection(Ctx.Config.dbUniHangoutsConfiguration)) { CommandType = CommandType.StoredProcedure }) {
            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               var ls = new List<RSVPType>();
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  var y = new RSVPType(){
                     RSVPTypeID = reader.GetInt16("RSVPTypeID"),
                     Name = reader.GetString("Name"),
                     Description = reader.GetString("Description")
                  };
                  if (y.Name.EqAlphaNumIgCase(nameof(No))) {
                     this._No = y;
                  } else if (y.Name.EqAlphaNumIgCase(nameof(Maybe))) {
                     this._Maybe = y;
                  } else if (y.Name.EqAlphaNumIgCase(nameof(StopBy))) {
                     this._StopBy = y;
                  } else if (y.Name.EqAlphaNumIgCase(nameof(Later))) {
                     this._Later = y;
                  } else if (y.Name.EqAlphaNumIgCase(nameof(Attending))) {
                     this._Attending = y;
                  }
                  ls.Add(y);
               }
               _RSVPTypes = ls.AsReadOnly();
            }           
         }
         return true;
      }


   }
}
