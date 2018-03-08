using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;

using static UniEvents.Core.Extensions;

namespace UniEvents.Core.DBModels {

	public abstract class DBModelBase {

		
	}

	public abstract class DBModelBase<T> : DBModelBase where T : DBModelBase {
		private delegate void FastCopyFromDelegate(ref T location, T src);

		private static FastCopyFromDelegate _fastCopyFrom = null;
	
		static DBModelBase(){
			//For performance and so we don't have to manually copy every field, we will generate a copy method at runtime using assembly. 
			Type type = typeof(T);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public |  BindingFlags.NonPublic);
			DynamicMethod dynmethod = new System.Reflection.Emit.DynamicMethod("AsmFastShallowFieldCopy", typeof(void), new Type[2]{ type.MakeByRefType(), type }, true);
			ILGenerator gen = dynmethod.GetILGenerator();
			foreach (FieldInfo field in fields) {
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Ldfld, field);
				gen.Emit(OpCodes.Stfld, field);
			}
			gen.Emit(OpCodes.Ret);
			_fastCopyFrom = (FastCopyFromDelegate)dynmethod.CreateDelegate(typeof(FastCopyFromDelegate));
		}

		protected DBModelBase() {

		}
		protected DBModelBase(T copyfrom) {
			T me = (T)(object)this;
			_fastCopyFrom(ref me, copyfrom);
		}
	}

}
