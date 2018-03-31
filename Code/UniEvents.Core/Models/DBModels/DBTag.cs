﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using UniEvents.Core;
using ZMBA;



namespace UniEvents.Models.DBModels {
   public class DBTag : DBModel {

      [DBCol("TagID", SqlDbType.BigInt, 1, false, true)]
      public Int64 TagID { get; set; }

      [DBCol("Name", SqlDbType.VarChar, 30, false, false)]
      public string Name { get; set; }

      [DBCol("Description", SqlDbType.NVarChar, 160, false, false)]
      public string Description { get; set; }

      public DBTag() { }

      public DBTag(IDataReader reader) {
         TagID = reader.GetInt64(nameof(TagID));
         Name = reader.GetString(nameof(Name));
         Description = reader.GetString(nameof(Description));
      }


      internal static SqlCommand GetSqlCommandForSP_Search(Factory ctx, long? TagID = null, string Name = null, string Description = null) {
         SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Search]", new SqlConnection(ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure };
         cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(TagID), TagID);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);
         return cmd;
      }
      internal static SqlCommand GetSqlCommandForSP_Query(Factory ctx, string Query) {
         SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Query]", new SqlConnection(ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure };
         cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Query), Query);
         return cmd;
      }
      internal static SqlCommand GetSqlCommandForSP_GetOne(Factory ctx, long? TagID = null, string Name = null) {
         SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_GetOne]", new SqlConnection(ctx.Config.dbUniHangoutsRead)) { CommandType = CommandType.StoredProcedure };
         cmd.AddParam(ParameterDirection.Input, SqlDbType.BigInt, nameof(TagID), TagID);
         cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
         return cmd;
      }

      internal static DBTag SP_Create(Factory ctx, string Name, string Description) {
         using (SqlCommand cmd = new SqlCommand("[dbo].[sp_Tags_Create]", new SqlConnection(ctx.Config.dbUniHangoutsWrite)) { CommandType = CommandType.StoredProcedure }) {
            var tagidParam = cmd.AddParam(ParameterDirection.Output, SqlDbType.BigInt, nameof(TagID), null);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.VarChar, nameof(Name), Name);
            cmd.AddParam(ParameterDirection.Input, SqlDbType.NVarChar, nameof(Description), Description);

            int rowsAffected = cmd.ExecuteProcedure();
            long id = (Int64)tagidParam.Value;

            if(id > 0) {
               DBTag result = new DBTag(){
                  TagID = id,
                  Name = Name,
                  Description = Description
               };
               return result;
            }
         }
         return null;
      }

    }
}