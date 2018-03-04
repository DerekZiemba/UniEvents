using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace UniEvents.Core {
	public static partial class Extensions {

		[MethodImpl(AggressiveInlining)] public static bool IsNumericType(this Type oType) => (uint)(Type.GetTypeCode(oType) - 5) <= 10U;

		[MethodImpl(AggressiveInlining)] public static bool IsDefault<T>(this T input) => EqualityComparer<T>.Default.Equals(input, default(T));

		[MethodImpl(AggressiveInlining)] public static T UnBox<T>(this T? input, T @default = default(T)) where T : struct => input ?? @default;


	}
}
