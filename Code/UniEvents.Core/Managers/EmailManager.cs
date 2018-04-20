using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;
using ZMBA;
using UniEvents.Core;
using UniEvents.Models.ApiModels;
using UniEvents.Models.DBModels;


namespace UniEvents.Core.Managers {

   public class EmailManager {
      private readonly Factory Ctx;

      internal EmailManager(Factory ctx) {
         this.Ctx = ctx;
      }


      private DBEmailVerification GetDBEmailVerification(long AccountID, string VerificationKey, string Email) {
         using(SqlCommand cmd = new SqlCommand("[dbo].[sp_EmailVerification_GetOne]", new SqlConnection(Ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBEmailVerification.@AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBEmailVerification.@VerificationKey), @VerificationKey);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBEmailVerification.Email), Email);
            var model = cmd.ExecuteReader_GetOne<DBEmailVerification>();
            return model;
         }
      }

      public void SendVerificationEmail(long AccountID, string UserName, string Email, string spath) {
         DBEmailVerification model = GetDBEmailVerification(AccountID, null, Email);
         if(model != null && model.IsVerified) {
            throw new InvalidOperationException("Already Verified");
         } 

         SmtpClient smtp = new SmtpClient("smtp.gmail.com");
         smtp.EnableSsl = true;
         smtp.Port = 587;
         smtp.Credentials = new NetworkCredential(Ctx.Config.sEmailVerifyAccountName, Ctx.Config.sEmailVerifyAccountPassword);

         byte[] VerificationHash = null;
         string VerificationKey = null;

         if(model == null) {
            (VerificationHash, VerificationKey) = HashUtils.CreateAPIKey256(AccountID + "#" + Email);
         } else {
            VerificationHash = model.VerificationHash;
            VerificationKey = model.VerificationKey;
         }
         
         smtp.Send("noreply@UniEvents.com", Email, "UniEvents: Verify Email", $@"
UserName: {UserName}
Email: {Email}

<a href='{spath}?id={AccountID}&key={WebUtility.UrlEncode(VerificationKey)}'>Click here to verify email with UniEvents!</a>
");

         if(model == null) {
            using(SqlCommand cmd = new SqlCommand("[dbo].[sp_EmailVerification_Add]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
               cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBEmailVerification.AccountID), AccountID);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBEmailVerification.@VerificationKey), @VerificationKey);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(DBEmailVerification.@VerificationHash), @VerificationHash);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBEmailVerification.Email), Email);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.SmallDateTime, nameof(DBEmailVerification.Date), DateTime.UtcNow);
               cmd.ExecuteProcedure();
            }
         }

      }

      public ApiResult<DBEmailVerification> VerifyEmail(long AccountID, string VerificationKey) {
         ApiResult<DBEmailVerification> apiresult = new ApiResult<DBEmailVerification>();
         DBEmailVerification model = GetDBEmailVerification(AccountID, VerificationKey, null);

         if(model is null) {
            return apiresult.SetResult(model).Failure("Verification email was never sent.");
         }

         if(model.IsVerified) {
            return apiresult.Success("Already Verified", model);
         }

         model.IsVerified = HashUtils.VerifyHashMatch256(VerificationKey, AccountID + "#" + model.Email, model.VerificationHash);

         if(model.IsVerified) {
            using(SqlCommand cmd = new SqlCommand("[dbo].[sp_EmailVerification_SetVerified]", new SqlConnection(Ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
               cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(DBEmailVerification.@AccountID), AccountID);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(DBEmailVerification.@VerificationKey), @VerificationKey);
               cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(DBEmailVerification.IsVerified), model.IsVerified);
               cmd.ExecuteProcedureAsync().ConfigureAwait(false);
            }
            return apiresult.Success(model);
         }
       
         return apiresult.SetResult(model).Failure("Invalid Verification Key"); 
      }


   }
}
