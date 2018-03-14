using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBAccount {

      [DBCol("AccountID", SqlDbType.BigInt, 1, false, isAutoValue: true)]
      public Int64 AccountID { get; set; }

      [DBCol("LocationID", SqlDbType.BigInt, 1, true)]
      public Int64 LocationID { get; set; }

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
      public bool VerifiedSchoolEmail { get; set; }

      [DBCol("VerifiedContactEmail", SqlDbType.Bit, 1, true)]
      public bool VerifiedContactEmail { get; set; }

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
         VerifiedSchoolEmail = reader.GetBoolean(nameof(VerifiedSchoolEmail));
         VerifiedContactEmail = reader.GetBoolean(nameof(VerifiedContactEmail));
      }


      public static async Task<DBAccount> SP_Account_GetAsync(CoreContext ctx, long AccountID, string UserName = null) {
         Contract.Requires<ArgumentException>((AccountID > 0) || !UserName.IsNullOrWhitespace(), "AccountID or UserName must be specified.");

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsRead))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Account_Search]", conn) { CommandType = CommandType.StoredProcedure }) {
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

      public static async Task<bool> SP_Account_CreateAsync(CoreContext ctx, DBAccount model) {
         Contract.Requires<ArgumentNullException>(model != null, "DBAccount_Null");
         Contract.Requires<ArgumentException>(model.IsGroup, "Is a Group not a User.");
         Contract.Requires<ArgumentNullException>(!model.UserName.IsNullOrWhitespace(), "UserName_Invalid");
         Contract.Requires<ArgumentNullException>(!model.PasswordHash.IsEmpty(), "PasswordHash_Invalid");
         Contract.Requires<ArgumentNullException>(!model.Salt.IsEmpty(), "Salt_Invalid");

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
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(IsGroup), model.IsGroup);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedSchoolEmail), model.VerifiedSchoolEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.Bit, nameof(VerifiedContactEmail), model.VerifiedContactEmail);

            if (cmd.Connection.State != ConnectionState.Open) { await cmd.Connection.OpenAsync().ConfigureAwait(false); }
            int rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            model.AccountID = (long)AccountID.Value;
            return rowsAffected == 1;
         }
      }

      public static async Task<bool> SP_Group_CreateAsync(CoreContext ctx, DBAccount model, long @GroupOwnerAccountID) {
         Contract.Requires<ArgumentNullException>(model != null, "DBAccount_Null");
         Contract.Requires<ArgumentException>(!model.IsGroup, "Is a User not a Group.");
         Contract.Requires<ArgumentNullException>(@GroupOwnerAccountID > 0, "GroupOwnerAccountID_Invalid");
         Contract.Requires<ArgumentNullException>(!model.UserName.IsNullOrWhitespace(), "UserName_Invalid");
         Contract.Requires<ArgumentException>(model.PasswordHash.IsEmpty(), "Groups don't have Passwords");
         Contract.Requires<ArgumentException>(model.Salt.IsEmpty(), "Groups don't have Passwords");
         Contract.Requires<ArgumentException>(model.FirstName.IsNullOrWhitespace(), "Groups don't have FirstNames");
         Contract.Requires<ArgumentException>(model.LastName.IsNullOrWhitespace(), "Groups don't have LastNames");

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
            return rowsAffected == 1;
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
