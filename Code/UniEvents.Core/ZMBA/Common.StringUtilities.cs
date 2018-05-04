//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Dynamic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Data;
//using System.Data.SqlClient;
//using System.Threading.Tasks;
//using System.Globalization;
//using System.IO;
//using System.IO.Compression;
//using System.Runtime.CompilerServices;
//using System.Reflection;
//using Newtonsoft.Json;
//using CmpOp = System.Globalization.CompareOptions;
//using static System.Runtime.CompilerServices.MethodImplOptions;


//namespace ZMBA {

//   [Flags]
//   public enum SubstrOptions {
//      Default = 0,
//      /// <summary> Whether the sequence is included in the returned substring </summary>
//      IncludeSeq = 1 << 0,
//      /// <summary> OrdinalIgnoreCase </summary>
//      IgnoreCase = 1 << 1,
//      /// <summary> If operation fails, return the original input string. </summary>
//      RetInput = 1 << 2
//   }

//   public static partial class Common {
//      private static CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentCulture;
//      private static readonly CompareInfo InvCmpInfo =CultureInfo.InvariantCulture.CompareInfo;
//      private static readonly CompareOptions VbCmp = CmpOp.IgnoreWidth | CmpOp.IgnoreNonSpace | CmpOp.IgnoreKanaType; //Compare like VisualBasic's default compare


//      #region ************************************** String ******************************************************

//      [MethodImpl(AggressiveInlining)] public static bool IsNullOrEmpty(this string str) => String.IsNullOrEmpty(str);

//      [MethodImpl(AggressiveInlining)] public static bool IsNotWhitespace(this string str) => !String.IsNullOrWhiteSpace(str);

//      [MethodImpl(AggressiveInlining)] public static bool Eq(this string str, string other) => String.Equals(str, other, StringComparison.Ordinal);

//      [MethodImpl(AggressiveInlining)] public static bool EqIgCase(this string str, string other) => String.Equals(str, other, StringComparison.OrdinalIgnoreCase);

//      [MethodImpl(AggressiveInlining)] public static bool EqAlphaNum(this string str, string other) => 0 == InvCmpInfo.Compare(str, other, VbCmp | CmpOp.IgnoreSymbols);

//      [MethodImpl(AggressiveInlining)] public static bool EqAlphaNumIgCase(this string str, string other) => 0 == InvCmpInfo.Compare(str, other, VbCmp | CmpOp.IgnoreSymbols | CmpOp.IgnoreCase);


//      public static int CountAlphaNumeric(this string str) {
//         int count = 0;
//         if (!str.IsNullOrEmpty()) {
//            for (var i = 0; i < str.Length; i++) { if (Char.IsLetter(str[i]) || char.IsNumber(str[i])) { count++; } }
//         }
//         return count;
//      }

//      public static string ToAlphaNumeric(this string str) {
//         if (str.IsNullOrEmpty()) { return str; }
//         var sb = StringBuilderCache.Take(str.Length);
//         for (var i = 0; i < str.Length; i++) { if (Char.IsLetter(str[i]) || char.IsNumber(str[i])) { sb.Append(str[i]); } }
//         return StringBuilderCache.Release(ref sb);
//      }
//      public static string ToAlphaNumericLower(this string str) {
//         if (str.IsNullOrEmpty()) { return str; }
//         var sb = StringBuilderCache.Take(str.Length);
//         for (var i = 0; i < str.Length; i++) { if (Char.IsLetter(str[i]) || char.IsNumber(str[i])) { sb.Append(char.ToLower(str[i])); } }
//         return StringBuilderCache.Release(ref sb);
//      }

//      public static string ReplaceIgCase(this string sInput, string oldValue, string newValue) {
//         if (!string.IsNullOrEmpty(sInput) && !string.IsNullOrEmpty(oldValue)) {
//            int idxLeft = sInput.IndexOf(oldValue, 0, StringComparison.OrdinalIgnoreCase);
//            //Don't build a new string if it doesn't even contain the value
//            if (idxLeft >= 0) {
//               if (newValue == null)
//                  newValue = string.Empty;
//               var sb = StringBuilderCache.Take(sInput.Length + Math.Max(0, newValue.Length - oldValue.Length) + 16);
//               int pos = 0;
//               while (pos < sInput.Length) {
//                  if (idxLeft == -1) {
//                     sb.Append(sInput.Substring(pos));
//                     break;
//                  } else {
//                     sb.Append(sInput.Substring(pos, idxLeft - pos));
//                     sb.Append(newValue);
//                     pos = idxLeft + oldValue.Length + 1;
//                  }
//                  if (pos < sInput.Length) {
//                     idxLeft = sInput.IndexOf(oldValue, pos, StringComparison.OrdinalIgnoreCase);
//                  }
//               }
//               return StringBuilderCache.Release(ref sb);
//            }
//         }
//         return sInput;
//      }


//      public static string[] ToStringArray<T>(this IEnumerable<T> ienum, Func<T, string> converter = null) {
//         if (ienum == null) { return new string[0]; }
//         if (converter == null) {
//            if (ienum is IDictionary || typeof(T).TryGetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
//               converter = DynamicKVP;
//            } else {
//               converter = GenericConverter;
//            }
//         }
//         return ienum.Select(converter).ToArray();

//         string DynamicKVP(T value) {
//            dynamic obj = value;
//            return String.Format(CurrentCulture, "{0}={1}", obj.Key, obj.Value);
//         }
//         string GenericConverter(T ob) { return string.Format(CurrentCulture, "{0}", ob); }
//      }


//      public static string ToStringJoin<T>(this IEnumerable<T> ienum, string separator = ", ", Func<T, string> converter = null) {
//         if (ienum == null) { return ""; }
//         if (converter == null) {
//            if (ienum is IDictionary || typeof(T).GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
//               converter = DynamicKVP;
//            } else {
//               converter = GenericConverter;
//            }
//         }
//         return (from x in ienum select converter(x)).ToStringJoin(separator);

//         string DynamicKVP(T value) { dynamic obj = value; return String.Format(CurrentCulture, "{0}={1}", obj.Key, obj.Value); }
//         string GenericConverter(T ob) { return string.Format(CurrentCulture, "{0}", ob); }
//      }


//      public static string ToStringJoin(this IEnumerable<string> ienum, string separator = ", ") {
//         if(ienum == null) { return ""; }
//         if(separator == null) { separator = ""; }
//         var ls = ListCache<string>.Take();
//         var size = 0;
//         foreach(string item in ienum) {
//            if (!string.IsNullOrWhiteSpace(item)) {
//               var str = item.Trim();
//               size += str.Length + separator.Length;
//               ls.Add(str);
//            }
//         }
//         var sb = StringBuilderCache.Take(size);
//         for(var i = 0; i < ls.Count; i++) {
//            sb.Append(ls[i]);
//            if (i < ls.Count - 1) { sb.Append(separator); }
//         }
//         ListCache<string>.Return(ref ls);
//         return StringBuilderCache.Release(ref sb);
//      }

//      public static string ToStringJoin(this string[] arr, string separator = ", ") {
//         if (arr == null || arr.Length == 0) { return ""; }
//         if (separator == null) { separator = ""; }
//         var sb = StringBuilderCache.Take(Math.Min(StringBuilderCache.MAX_ITEM_CAPACITY, arr.Length * 16));
//         bool bSep = false;
//         for (var i = 0; i < arr.Length; i++) {
//            var item = arr[i];
//            if (!string.IsNullOrWhiteSpace(item)) {
//               if (bSep) { sb.Append(separator); }
//               sb.Append(item.Trim());
//               bSep = true;
//            }
//         }
//         return StringBuilderCache.Release(ref sb);
//      }

//      #endregion


//      #region ************************************** StringBuilder ******************************************************

//      public static StringBuilder Reverse(this StringBuilder sb) {
//         int end = sb.Length - 1;
//         int start = 0;
//         while(end-start > 0) {
//            char ch = sb[end];
//            sb[end] = sb[start];
//            sb[start] = ch;
//            start++;
//            end--;
//         }
//         return sb;
//      }

//      #endregion


//      #region ************************************** Substring ******************************************************

//      public static string SubstrBefore(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && seq?.Length > 0) {
//            int index = input.IndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//            if (index >= 0) {
//               if ((opts & SubstrOptions.IncludeSeq) > 0) { index += seq.Length; }
//               return input.Substring(0, index);
//            }
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrBeforeLast(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && seq?.Length > 0) {
//            int index = input.LastIndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//            if (index >= 0) {
//               if ((opts & SubstrOptions.IncludeSeq) > 0) { index += seq.Length; }
//               return input.Substring(0, index);
//            }
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrAfter(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && seq?.Length > 0) {
//            int index = input.IndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//            if (index >= 0) {
//               if ((opts & SubstrOptions.IncludeSeq) == 0) { index += seq.Length; }
//               return input.Substring(index);
//            }
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrAfterLast(this string input, string seq, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && seq?.Length > 0) {
//            int index = input.LastIndexOf(seq, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//            if (index >= 0) {
//               if ((opts & SubstrOptions.IncludeSeq) == 0) { index += seq.Length; }
//               return input.Substring(index);
//            }
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }


//      public static string SubstrBefore(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && sequences?.Length > 0) {
//            int idx = input.Length;
//            for (int i = 0; i < sequences.Length; i++) {
//               string seq = sequences[i];
//               if (seq?.Length > 0) {
//                  int pos = input.IndexOf(seq, 0, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//                  if (pos >= 0 && pos <= idx) {
//                     if ((opts & SubstrOptions.IncludeSeq) > 0) { pos += seq.Length; }
//                     idx = pos;
//                  }
//               }
//            }
//            return input.Substring(0, idx);
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrBeforeLast(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && sequences?.Length > 0) {
//            int idx = input.Length;
//            for (int i = 0; i < sequences.Length; i++) {
//               string seq = sequences[i];
//               if (seq?.Length > 0) {
//                  int pos = input.LastIndexOf(seq, idx, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//                  if (pos >= 0 && pos <= idx) {
//                     if ((opts & SubstrOptions.IncludeSeq) > 0) { pos += seq.Length; }
//                     idx = pos;
//                  }
//               }
//            }
//            return input.Substring(0, idx);
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrAfter(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && sequences?.Length > 0) {
//            int idx = 0;
//            for (int i = 0; i < sequences.Length; i++) {
//               string seq = sequences[i];
//               if (seq?.Length > 0) {
//                  int pos = input.IndexOf(seq, idx, input.Length - idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//                  if (pos >= idx && pos <= input.Length) {
//                     if ((opts & SubstrOptions.IncludeSeq) == 0) { pos += seq.Length; }
//                     idx = pos;
//                  }
//               }
//            }
//            return input.Substring(idx);
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }
//      public static string SubstrAfterLast(this string input, string[] sequences, SubstrOptions opts = SubstrOptions.Default) {
//         if (input?.Length > 0 && sequences?.Length > 0) {
//            int idx = 0;
//            for (int i = 0; i < sequences.Length; i++) {
//               string seq = sequences[i];
//               if (seq?.Length > 0) {
//                  int pos = input.LastIndexOf(seq, idx, idx, (opts & SubstrOptions.IgnoreCase) > 0 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
//                  if (pos >= idx && pos <= input.Length) {
//                     if ((opts & SubstrOptions.IncludeSeq) == 0) { pos += seq.Length; }
//                     idx = pos;
//                  }
//               }
//            }
//            return input.Substring(idx);
//         }
//         return (opts & SubstrOptions.RetInput) > 0 ? input : null;
//      }


//      #endregion



//      #region ************************************** REGEX ******************************************************

//      public static IEnumerable<Match> GetMatches(this Regex rgx, string input) {
//         foreach (Match match in rgx.Matches(input)) {
//            if (match.Index >= 0 && match.Length > 0) {
//               yield return match;
//            }
//         }
//      }

//      public static IEnumerable<string> GetMatchedValues(this Regex rgx, string input) {
//         foreach (Match match in rgx.GetMatches(input)) {
//            yield return match.Value;
//         }
//      }

//      public static IEnumerable<Group> FindNamedGroups(this Regex rgx, string input, string name) {
//         foreach (Match match in rgx.GetMatches(input)) {
//            if (match.Index >= 0 && match.Length > 0) {
//               Group group = match.Groups[name];
//               if (group.Index >= 0 && group.Length > 0) {
//                  yield return group;
//               }
//            }
//         }
//      }

//      public static IEnumerable<string> FindNamedGroupValues(this Regex rgx, string input, string name) {
//         foreach (Group group in rgx.FindNamedGroups(input, name)) {
//            yield return group.Value;
//         }
//      }

//      [MethodImpl(AggressiveInlining)]
//      public static string FindGroupValue(this Match match, string name) {
//         Group group = match.Groups[name];
//         return group.Index >= 0 && group.Length > 0 ? group.Value : null;
//      }


//      public static string ReplaceNamedGroup(this Regex rgx, string sInput, string groupName, string newValue) {
//         if (!string.IsNullOrWhiteSpace(sInput)) {
//            StringBuilder sb = new StringBuilder(sInput.Length + newValue.Length);
//            int idx = 0;
//            foreach (Group group in rgx.FindNamedGroups(sInput, groupName)) {
//               if (group.Index >= idx) {
//                  sb.Append(sInput.Substring(idx, group.Index - idx));
//                  sb.Append(newValue);
//                  idx = group.Index + group.Length;
//               }
//            }
//            if (idx < sInput.Length) {
//               sb.Append(sInput.Substring(idx));
//            }
//            return sb.ToString();
//         }
//         return sInput;
//      }


//      #endregion





//   }

//}
