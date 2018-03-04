using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;


namespace UniEvents.Core {
	public static partial class Extensions {


		[MethodImpl(AggressiveInlining)] public static bool Eq(this string str, string other) => String.Equals(str, other, StringComparison.Ordinal);

		[MethodImpl(AggressiveInlining)] public static bool EqIgCase(this string str, string other) => String.Equals(str, other, StringComparison.OrdinalIgnoreCase);

		[MethodImpl(AggressiveInlining)] public static bool EqIgCaseSym(this string str, string other) {
			CompareOptions cmp = CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreKanaType;
			return CultureInfo.InvariantCulture.CompareInfo.Compare(str, other, cmp) == 0;
		}

		[MethodImpl(AggressiveInlining)] public static bool EqIgSym(this string str, string other, bool ignorecase = false) {
			CompareOptions cmp = CompareOptions.IgnoreWidth | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreKanaType;
			return CultureInfo.InvariantCulture.CompareInfo.Compare(str, other, cmp) == 0;
		}


		public static string ReplaceIgCase(this string sInput, string oldValue, string newValue) {
			if (!string.IsNullOrEmpty(sInput) && !string.IsNullOrEmpty(oldValue)) {
				int idxLeft = sInput.IndexOf(oldValue, 0, StringComparison.OrdinalIgnoreCase);
				//Don't build a new string if it doesn't even contain the value
				if (idxLeft >= 0) {
					if (newValue == null)
						newValue = string.Empty;
					var sb = new StringBuilder(sInput.Length + Math.Max(0, newValue.Length - oldValue.Length) + 16);
					int pos = 0;
					while (pos < sInput.Length) {
						if (idxLeft == -1) {
							sb.Append(sInput.Substring(pos));
							break;
						} else {
							sb.Append(sInput.Substring(pos, idxLeft - pos));
							sb.Append(newValue);
							pos = idxLeft + oldValue.Length + 1;
						}
						if (pos < sInput.Length) {
							idxLeft = sInput.IndexOf(oldValue, pos, StringComparison.OrdinalIgnoreCase);
						}
					}
					return sb.ToString();
				}
			}
			return sInput;
		}

		public static string ToStringJoin(this IEnumerable<string> ienum, string separator = ", ") {
			return String.Join(separator, from string x in ienum where !string.IsNullOrWhiteSpace(x) select x.Trim());
		}


	}

}
