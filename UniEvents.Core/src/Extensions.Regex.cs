using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace UniEvents.Core {

	public static partial class Extensions {


		public static IEnumerable<Match> GetMatches(this Regex rgx, string input) {
			foreach (Match match in rgx.Matches(input)) {
				if (match.Index >= 0 && match.Length > 0) {
					yield return match;
				}
			}
		}

		public static IEnumerable<string> GetMatchedValues(this Regex rgx, string input) {
			foreach (Match match in rgx.GetMatches(input)) {
				yield return match.Value;
			}
		}

		public static IEnumerable<Group> FindNamedGroups(this Regex rgx, string input, string name) {
			foreach (Match match in rgx.GetMatches(input)) {
				if (match.Index >= 0 && match.Length > 0) {
					Group group = match.Groups[name];
					if (group.Index >= 0 && group.Length > 0) {
						yield return group;
					}
				}
			}
		}

		public static IEnumerable<string> FindNamedGroupValues(this Regex rgx, string input, string name) {
			foreach (Group group in rgx.FindNamedGroups(input, name)) {
				yield return group.Value;
			}
		}

		[MethodImpl(AggressiveInlining)]
		public static string FindGroupValue(this Match match, string name) {
			Group group = match.Groups[name];
			return group.Index >= 0 && group.Length > 0 ? group.Value : null;
		}


		public static string ReplaceNamedGroup(this Regex rgx, string sInput, string groupName, string newValue) {
			if (!string.IsNullOrWhiteSpace(sInput)) {
				StringBuilder sb = new StringBuilder(sInput.Length + newValue.Length);
				int idx = 0;
				foreach (Group group in rgx.FindNamedGroups(sInput, groupName)) {
					if (group.Index >= idx) {
						sb.Append(sInput.Substring(idx, group.Index - idx));
						sb.Append(newValue);
						idx = group.Index + group.Length;
					}
				}
				if (idx < sInput.Length) {
					sb.Append(sInput.Substring(idx));
				}
				return sb.ToString();
			}
			return sInput;
		}



	}

}
