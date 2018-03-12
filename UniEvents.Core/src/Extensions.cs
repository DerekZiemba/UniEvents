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

		[MethodImpl(AggressiveInlining)] public static bool IsNullOrWhitespace(this string str) => String.IsNullOrWhiteSpace(str);


		[MethodImpl(AggressiveInlining)] public static bool IsEmpty(this string str) => String.IsNullOrEmpty(str);
		[MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this T[] arr) => arr is null || arr.Length == 0;
		[MethodImpl(AggressiveInlining)] public static bool IsEmpty<T>(this ICollection<T> arr) => arr is null || arr.Count == 0;


	}
}
