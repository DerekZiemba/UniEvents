using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;


namespace UniEvents.Models.DBModels {

   public class DBAccount : DBModel {

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
      public bool VerifiedSchoolEmail { get; set; }

      [DBCol("VerifiedContactEmail", SqlDbType.Bit, 1, true)]
      public bool VerifiedContactEmail { get; set; }

      public bool IsAdmin { get; set; }

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
         VerifiedSchoolEmail = reader.GetNBoolean(nameof(VerifiedSchoolEmail)).UnBox();
         VerifiedContactEmail = reader.GetNBoolean(nameof(VerifiedContactEmail)).UnBox();
         IsAdmin = reader.GetNBoolean(nameof(IsAdmin)).UnBox();
      }

      public static async Task<bool> SP_Account_CreateAsync(Factory ctx, DBAccount model) {
         if (model == null) { throw new ArgumentNullException("DBAccount_Null"); }
         if (model.IsGroup) { throw new ArgumentException("Is a Group not a User."); }
         if (String.IsNullOrWhiteSpace(model.UserName)) { throw new ArgumentNullException("UserName_Invalid"); }
         if (model.PasswordHash.IsEmpty() || model.Salt.IsNullOrEmpty()) { throw new ArgumentException("PasswordHash or Salt invalid."); }

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

            int rowsAffected = await cmd.ExecuteProcedureAsync().ConfigureAwait(false);

            model.AccountID = (long)AccountID.Value;
            return model.AccountID > 0;
         }
      }

      public static async Task<bool> SP_Group_CreateAsync(Factory ctx, DBAccount model, long @GroupOwnerAccountID) {
         if (model == null) { throw new ArgumentNullException("DBAccount_Null"); }
         if (!model.IsGroup) { throw new ArgumentException("Is a User not a Group."); }
         if (@GroupOwnerAccountID <= 0) { throw new ArgumentNullException("GroupOwnerAccountID_Invalid"); }
         if (String.IsNullOrWhiteSpace(model.UserName)) { throw new ArgumentNullException("UserName_Invalid"); }
         if (!model.PasswordHash.IsEmpty() || !model.Salt.IsNullOrEmpty()) { throw new ArgumentException("Groups don't have Passwords"); }
         if (model.FirstName.IsNotWhitespace()) { throw new ArgumentException("Groups don't have FirstNames"); }
         if (model.LastName.IsNotWhitespace()) { throw new ArgumentException("Groups don't have LastNames"); }

         using (SqlConnection conn = new SqlConnection(ctx.Config.dbUniHangoutsWrite))
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Group_Create]", conn) { CommandType = CommandType.StoredProcedure }) {
            SqlParameter @GroupID = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(@GroupID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, "GroupName", model.UserName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(@DisplayName), model.DisplayName);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@ContactEmail), model.ContactEmail);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(@PhoneNumber), model.PhoneNumber);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(@LocationID), model.LocationID);

            int rowsAffected = await cmd.ExecuteProcedureAsync().ConfigureAwait(false);

            model.AccountID = (long)@GroupID.Value;
            return model.AccountID > 0;
         }
      }


      public static async Task<List<DBAccount>> SP_Account_SearchAsync(Factory ctx,
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

            return await cmd.ExecuteReader_GetManyAsync<DBAccount>().ConfigureAwait(false);
         }
      }

      public static IEnumerable<DBAccount> SP_Account_Search(Factory ctx,
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

            foreach (var item in cmd.ExecuteReader_GetManyRecords()) { yield return new DBAccount(item); }
         }
      }

   }
}
