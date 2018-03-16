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
		public readonly int Size;
		public readonly bool AllowNull;
		public readonly bool IsAutoValue;
		public readonly ParameterDirection Direction;
		

		public DBColAttribute(string name, SqlDbType dbType, int size, bool allowNull,
									bool isAutoValue = false, ParameterDirection direction = ParameterDirection.InputOutput) {
			this.Name = name;
			this.DBType = dbType;
			this.Size = size;
			this.AllowNull = allowNull;
			this.IsAutoValue = isAutoValue;
			this.Direction = direction;			
		}
	}

}
