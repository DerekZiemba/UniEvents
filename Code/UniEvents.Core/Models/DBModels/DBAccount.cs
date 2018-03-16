using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBAccount {

      [DBCol("AccountID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 AccountID { get; set; }

      [DBCol("LocationID", SqlDbType.BigInt, 1, true)]
      public Int64? LocationID { get; set; }

      [DBCol("PasswordHash", SqlDbType.Binary, 256, true)]
      public byte[] PasswordHash { get; set; }

      [DBCol("Salt", SqlDbType.VarChar, 20, true)]
      public string Salt { get; set; }

      [DBCol("UserName", SqlDbType.VarChar, 20, false)]
      public string UserName { get; set; }

      [DBCol("DisplayName", SqlDbType.NVarChar, 50, true)]
      public string DisplayName { get; set; }

      [DBCol("FirstName", SqlDbType.NVarChar, 50, true)]
      public string FirstName { get; set; }

      [DBCol("LastName", SqlDbType.NVarChar, 50, true)]
      public string LastName { get; set; }

      [DBCol("SchoolEmail", SqlDbType.NVarChar, 50, true)]
      public string SchoolEmail { get; set; }

      [DBCol("ContactEmail", SqlDbType.VarChar, 50, true)]
      public string ContactEmail { get; set; }

      [DBCol("PhoneNumber", SqlDbType.VarChar, 20, true)]
      public string PhoneNumber { get; set; }

      [DBCol("IsGroup", SqlDbType.Bit, 1, false)]
      public bool IsGroup { get; set; }

      [DBCol("VerifiedSchoolEmail", SqlDbType.Bit, 1, true)]
      public bool? VerifiedSchoolEmail { get; set; }

      [DBCol("VerifiedContactEmail", SqlDbType.Bit, 1, true)]
      public bool? VerifiedContactEmail { get; set; }

      public DBAccount() { }


      private DBAccount(IDataReader reader) {
         AccountID = reader.GetInt64(nameof(AccountID));
         LocationID = reader.GetInt64(nameof(LocationID));
         PasswordHash = reader.GetBytes(nameof(PasswordHash), 32);
         Salt = reader.GetString(nameof(Salt));
         UserName = reader.GetString(nameof(UserName));
         DisplayName = reader.GetString(nameof(DisplayName));
         FirstName = reader.GetString(nameof(FirstName));
         LastName = reader.GetString(nameof(LastName));
         SchoolEmail = reader.GetString(nameof(SchoolEmail));
         ContactEmail = reader.GetString(nameof(ContactEmail));
         IsGroup = reader.GetBoolean(nameof(IsGroup));
         VerifiedSchoolEmail = reader.GetNBoolean(nameof(VerifiedSchoolEmail));
         VerifiedContactEmail = reader.GetNBoolean(nameof(VerifiedContactEmail));
      }


      public static async Task<DBAccount> SP_Account_GetAsync(CoreContext ctx, long AccountID, string UserName = null) {
         if(AccountID <= 0 && UserName.IsNullOrWhitespace()) { throw new ArgumentNullException("AccountID or UserName must be specified."); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Get]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@AccountID), AccountID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@UserName), @UserName);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  return new DBAccount(reader);
               }
            }
            return null;
         }
      }

      public static bool SP_Account_Create(CoreContext ctx, DBAccount model) {
         if (model == null) { throw new ArgumentNullException("DBAccount_Null"); }
         if (model.IsGroup) { throw new ArgumentException("Is a Group not a User."); }
         if (model.UserName.IsNullOrWhitespace()) { throw new ArgumentNullException("UserName_Invalid"); }
         if (model.PasswordHash.IsEmpty() || model.Salt.IsEmpty()) { throw new ArgumentException("PasswordHash or Salt invalid."); }

         //TODO: Match params to properties
         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter AccountID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(AccountID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(LocationID), model.LocationID);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Binary, nameof(PasswordHash), model.PasswordHash);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Salt), model.Salt);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(UserName), model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(DisplayName), model.DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(FirstName), model.FirstName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(LastName), model.LastName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(SchoolEmail), model.SchoolEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(ContactEmail), model.ContactEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(PhoneNumber), model.PhoneNumber);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(IsGroup), model.IsGroup);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedSchoolEmail), model.VerifiedSchoolEmail);
            //cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedContactEmail), model.VerifiedContactEmail);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            int rowsAffected = cmd.ExecuteNonQuery();

            model.AccountID = (long)AccountID.Value;
            return model.AccountID > 0;
         }
      }

      public static async Task<bool> SP_Group_CreateAsync(CoreContext ctx, DBAccount model, long @GroupOwnerAccountID) {
         if (model == null) { throw new ArgumentNullException("DBAccount_Null"); }
         if (!model.IsGroup) { throw new ArgumentException("Is a User not a Group."); }
         if (@GroupOwnerAccountID <= 0) { throw new ArgumentNullException("GroupOwnerAccountID_Invalid"); }
         if (model.UserName.IsNullOrWhitespace()) { throw new ArgumentNullException("UserName_Invalid"); }
         if (!model.PasswordHash.IsEmpty() || !model.Salt.IsEmpty()) { throw new ArgumentException("Groups don't have Passwords"); }
         if (!model.FirstName.IsNullOrWhitespace()) { throw new ArgumentException("Groups don't have FirstNames"); }
         if (!model.LastName.IsNullOrWhitespace()) { throw new ArgumentException("Groups don't have LastNames"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Group_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter @GroupID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(@GroupID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, "GroupName", model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), model.DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@ContactEmail), model.ContactEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), model.PhoneNumber);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), model.LocationID);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            model.AccountID = (long)@GroupID.Value;
            return model.AccountID > 0;
         }
      }


      public static async Task<List<DBAccount>> SP_Account_SearchAsync(CoreContext ctx,
                                                                        string UserName = null,
                                                                        string DisplayName = null,
                                                                        string FirstName = null,
                                                                        string LastName = null,
                                                                        string Email = null,
                                                                        string PhoneNumber = null) {

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@UserName), @UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), @DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@FirstName), @FirstName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@LastName), @LastName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Email), @Email);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), @PhoneNumber);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }

            var ls = new List<DBAccount>();
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
               while (await reader.ReadAsync().ConfigureAwait(false)) {
                  ls.Add(new DBAccount(reader));
               }
            }
            return ls;
         }
      }

      public static IEnumerable<DBAccount> SP_Account_Search(CoreContext ctx,
                                                               string UserName = null,
                                                               string DisplayName = null,
                                                               string FirstName = null,
                                                               string LastName = null,
                                                               string Email = null,
                                                               string PhoneNumber = null) {

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@UserName), @UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), @DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@FirstName), @FirstName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@LastName), @LastName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@Email), @Email);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), @PhoneNumber);

            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
            using (SqlDataReader reader = cmd.ExecuteReader()) {
               while (reader.Read()) {
                  yield return new DBAccount(reader);
               }
            }
         }
      }

   }
}
