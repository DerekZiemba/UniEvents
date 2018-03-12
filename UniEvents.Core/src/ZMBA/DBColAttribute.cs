using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA {

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class DBColAttribute : Attribute {

		public readonly string Name;
		public readonly SqlDbType DBType;
		public readonly ParameterDirection Direction;
		public readonly int Size;

		public DBColAttribute(string name, SqlDbType dbType, int size, ParameterDirection direction = ParameterDirection.InputOutput) {
			this.Name = name;
			this.DBType = dbType;
			this.Direction = direction;
			this.Size = size;
		}
	}

}
