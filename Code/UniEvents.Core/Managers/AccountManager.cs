using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using ZMBA;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;


namespace UniEvents.Core.Managers {

   public class AccountManager {
      private readonly Factory Ctx;

      internal AccountManager(Factory ctx) {
         this.Ctx = ctx;
      }

      public async Task<bool> CheckPrivilege(string username, string apikey) {
         if (username.IsNotWhitespace() && apikey.IsNotWhitespace()) {
            try {
               DBLogin dbLogin = await DBLogin.SP_Account_Login_GetAsync(Ctx, username, apikey).ConfigureAwait(false);
               return dbLogin != null && HashUtils.VerifyHashMatch256(apikey, dbLogin.UserName, dbLogin.APIKeyHash);
            } catch { if (Ctx.Config.IsDebugMode) { throw; } }
         }
         return false;
      }


      public async Task<DBAccount> GetAccount(long AccountID, string UserName = null) {
         if(AccountID <= 0 && String.IsNullOrWhiteSpace(UserName)) { throw new ArgumentNullException("AccountID or UserName must be specified."); }

         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_GetOne]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@UserName), @UserName);

             return await cmd.ExecuteReader_GetOneAsync<DBAccount>().ConfigureAwait(false);
         }
      }



      public void CreateAccount(DBAccount model) {
         if (model == null) { throw new ArgumentNullException("DBAccount_Null"); }
         if(model.IsGroup) { throw new ArgumentException("Is a Group not a User."); }
         if(String.IsNullOrWhiteSpace(model.UserName)) { throw new ArgumentNullException("UserName_Invalid"); }
         if(model.PasswordHash.IsEmpty() || model.Salt.IsNullOrEmpty()) { throw new ArgumentException("PasswordHash or Salt invalid."); }


         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Create]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter AccountID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(model.AccountID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(model.LocationID), model.LocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(model.PasswordHash), model.PasswordHash);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.Salt), model.Salt);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.UserName), model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(model.DisplayName), model.DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(model.FirstName), model.FirstName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(model.LastName), model.LastName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.SchoolEmail), model.SchoolEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.ContactEmail), model.ContactEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(model.PhoneNumber), model.PhoneNumber);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(IsGroup), model.IsGroup);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedSchoolEmail), model.VerifiedSchoolEmail);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedContactEmail), model.VerifiedContactEmail);

            int rowsAffected = cmd.ExecuteProcedure();
            model.AccountID = (long)AccountID.Value;
         }
         if(model.AccountID <= 0) {
            throw new DataException("AccountID was not set to positive integer. Something in the Database went wrong.");
         }

      }



   }
}
