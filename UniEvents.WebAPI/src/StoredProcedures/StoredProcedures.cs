using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UniEvents.Core;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;


using UniEvents.Core.DBModels;
using static ZMBA.Common;


namespace UniEvents.WebAPI {

	internal static partial class StoredProcedures {
		private const int MAX_POOL_SIZE = 128;
		private static ConcurrentBag<SqlConnection> _dbUniHangoutsConnPool = new ConcurrentBag<SqlConnection>();

		private static SqlConnection TakeDbUniHangoutsConn() {
			SqlConnection conn = null;
			if(!_dbUniHangoutsConnPool.TryTake(out conn)) {
				conn = new SqlConnection(Settings.SqlDbUniHangoutsConnStr);
			}
			return conn;
		}

		private static void ReturnDbUniHangoutsConn(SqlConnection conn) {
			if(_dbUniHangoutsConnPool.Count >= 128) {
				conn.Dispose();
			} else {
				_dbUniHangoutsConnPool.Add(conn);
			}
		}

	}
}
