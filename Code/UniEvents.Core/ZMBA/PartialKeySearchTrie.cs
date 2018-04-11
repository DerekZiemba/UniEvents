using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZMBA {


   public class PartialKeySearchTrie<T> {
     
      private CharNode RootNode = new CharNode('\n');
      public int Count { get; private set; }
      public int NodeCount { get; private set; }

      public void Optimize() {
         Stack<CharNode> stack = StackCache<CharNode>.Take();
         stack.Push(RootNode);
         while (stack.Count > 0) {
            var current = stack.Pop();
            if (current.Children != null) {
               current.Children = new Dictionary<char, CharNode>(current.Children);
               foreach (var kvp in current.Children) { stack.Push(kvp.Value); }
            }
            if (current.Items != null) {
               current.Items.TrimExcess();
            }
         }
         StackCache<CharNode>.Return(ref stack);
      }

      public void AddExact(string key, T value) {
         var str = Normalize(key);
         if (str.Length == 0) { return; }
         CharNode current = RootNode;
         for (int idx = 0; idx < str.Length; idx++) {
            char ch = str[idx];
            if(current.Children == null) { current.Children = new Dictionary<char, CharNode>(); }
            current = current.Children.TryGetValue(ch, out CharNode next) ? next : AddNodeChild(current, ch);
         }
         AddNodeItem(current, str, value);
      }

      public void Add(string key, T value) {
         List<string> words = GetNormalizedWords(key);
         if(words == null || words.Count == 0) { return; }
         for(var i=0; i<words.Count; i++) {
            string word = words[i];
            CharNode current = RootNode;
            for (int idx = 0; idx < word.Length; idx++) {
               char ch = word[idx];
               if (current.Children == null) { current.Children = new Dictionary<char, CharNode>(); }
               current = current.Children.TryGetValue(ch, out CharNode next) ? next : AddNodeChild(current, ch);
            }
            AddNodeItem(current, word, value);
         }
         ListCache<string>.Return(ref words);
      }

      private CharNode AddNodeChild(CharNode target, char ch) {
         if (target.Children == null) {
            target.Children = new Dictionary<char, CharNode>();
         }
         CharNode child = new CharNode(ch);
         target.Children.Add(ch, child);
         NodeCount++;
         return child;
      }

      private void AddNodeItem(CharNode target, string key, T item) {
         NodeItem kvp = new NodeItem(key, item);
         if (target.Items == null) {
            target.Items = new HashSet<NodeItem>(new NodeItem.Comparer()) { kvp };
         } else {
            if (target.Items.Contains(kvp)) { return; }
            target.Items.Add(kvp);
         }
         Count++;
      }



      public IEnumerable<T> FindMatches(string query, int maxCount)=> FindMatches<T>(query, maxCount);

      public IEnumerable<TSelect> FindMatches<TSelect>(string query, int maxCount) where TSelect : T {
         List<string> lsWords = GetNormalizedWords(query);
         Dictionary<NodeItem, Matched> matchLookup = new Dictionary<NodeItem, Matched>( Math.Min(Count, 64), NodeItem.NodeCmp);
         HashSet<NodeItem> unique =  new HashSet<NodeItem>(NodeItem.NodeCmp);
         Stack<CharNode> stack = StackCache<CharNode>.Take();

         matchLookup.Clear();
         unique.Clear();

         if (lsWords == null || lsWords.Count == 0) {            
            stack.Push(RootNode);
            TraverseNodes<TSelect>(stack, unique, matchLookup, null, maxCount);
            unique.Clear();
         } else {         
            for (int i = 0; i < lsWords.Count; i++) {              
               stack.Push(GetBestMatchChildNode(RootNode, lsWords[i]));
               TraverseNodes<TSelect>(stack, unique, matchLookup, lsWords[i], -1);
               unique.Clear();
            }            
         }

         if (lsWords != null) { ListCache<string>.Return(ref lsWords); }
         StackCache<CharNode>.Return(ref stack);     

         foreach (Matched match in matchLookup.Values.OrderByDescending(GetMatchRank)) {
            if ((maxCount--) >= 0) {
               yield return (TSelect)match.Value.Item;
            } else {
               matchLookup.Clear();             
               yield break;
            }
         }
      }


      #region "Helper Methods"

      private static int GetMatchRank(Matched match) => match.Rank;

      private static CharNode GetBestMatchChildNode(CharNode current, string word) {
         for (int idx = 0; idx < word.Length; idx++) {
            if (current.Children.TryGetValue(word[idx], out CharNode next)) { current = next; } else { break; }
         }
         return current;
      }

      private static void TraverseNodes<TSelect>(Stack<CharNode> stack, HashSet<NodeItem> unique, Dictionary<NodeItem, Matched> matchLookup, string word, int maxCount) {              
         while (stack.Count > 0) {
            CharNode current = stack.Pop();
            if (current.Items != null) {
               foreach (NodeItem item in current.Items) {
                  if (item.Item is TSelect) {
                     bool bExact = StringComparer.OrdinalIgnoreCase.Equals(item.Key, word);
                     Matched match;
                     if (unique.Add(item)) {
                        if (!matchLookup.TryGetValue(item, out match)) {
                           match = new Matched() { Value = item };
                           maxCount--;
                        }
                        match.Rank += bExact ? 2 : 1;
                        match.bExact = bExact;
                        matchLookup[item] = match;                  
                     } else if (bExact && matchLookup.TryGetValue(item, out match) && !match.bExact) {
                        match.Rank++;
                        match.bExact = true;
                        matchLookup[item] = match;
                     }                  
                  }
                  if (maxCount == 0) { return; }
               }
            }
            if (current.Children != null) {
               foreach (var child in current.Children) { stack.Push(child.Value); }
            }
         }
      }


      private static string Normalize(string input) {
         if (String.IsNullOrWhiteSpace(input)) { return ""; }

         string str = input.Normalize(NormalizationForm.FormD);
         StringBuilder sb = StringBuilderCache.Take(str.Length);
         char ch;
         for (var i = 0; i < str.Length; i++) {
            ch = str[i];

            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            switch (cat) {
               case UnicodeCategory.TitlecaseLetter:
               case UnicodeCategory.UppercaseLetter: sb.Append(char.ToLower(ch)); break;
               case UnicodeCategory.LowercaseLetter: sb.Append(ch); break;
               case UnicodeCategory.DecimalDigitNumber: sb.Append(ch); break;
               case UnicodeCategory.DashPunctuation:
               case UnicodeCategory.ConnectorPunctuation:
                  if (sb.Length > 0 && sb[sb.Length - 1] != '-') {
                     sb.Append('-');
                  }
                  break;
               case UnicodeCategory.SpaceSeparator:
               case UnicodeCategory.LineSeparator:
               case UnicodeCategory.ParagraphSeparator:
               case UnicodeCategory.OtherPunctuation:
                  if (sb.Length > 0 && sb[sb.Length - 1] != ' ') {
                     sb.Append(' ');
                  }
                  break;
            }
         }
         while (sb.Length > 0 && (sb[sb.Length - 1] == ' ' || sb[sb.Length - 1] == '-')) {
            sb.Length = sb.Length - 1;
         }

         return StringBuilderCache.Release(ref sb);
      }

      private static List<string> GetNormalizedWords(string input) {
         if (String.IsNullOrWhiteSpace(input)) { return null; }

         string str = input.Normalize(NormalizationForm.FormD);
         List<string> ls = ListCache<string>.Take();
         StringBuilder sb = StringBuilderCache.Take();
         char ch;
         for (var i = 0; i < str.Length; i++) {
            ch = str[i];
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            switch (cat) {
               case UnicodeCategory.TitlecaseLetter:
               case UnicodeCategory.UppercaseLetter: sb.Append(char.ToLower(ch)); break;
               case UnicodeCategory.LowercaseLetter: sb.Append(ch); break;
               case UnicodeCategory.DecimalDigitNumber: sb.Append(ch); break;
               case UnicodeCategory.DashPunctuation:
               case UnicodeCategory.ConnectorPunctuation:
                  if (sb.Length > 0 && sb[sb.Length - 1] != '-') {
                     sb.Append('-');
                  }
                  break;
               case UnicodeCategory.SpaceSeparator:
               case UnicodeCategory.LineSeparator:
               case UnicodeCategory.ParagraphSeparator:
               case UnicodeCategory.OtherPunctuation:
                  if (sb.Length > 0) {
                     ls.Add(sb.ToString());
                     sb.Clear();
                  }
                  break;
            }
         }
         if (sb.Length > 0) {
            ls.Add(sb.ToString());
         }

         StringBuilderCache.Return(ref sb);
         return ls;
      }


      #endregion


      #region "Helper Classes"

      private struct Matched {
         public int Rank;
         public bool bExact;
         public NodeItem Value;
      }

      private struct NodeItem {
         public static Comparer NodeCmp = new Comparer();
         public string Key;
         public T Item;

         public NodeItem(string key, T item) { Key = key; Item = item; }
         public override int GetHashCode() => Item.GetHashCode();
         public override bool Equals(object obj) => this.Equals((NodeItem)obj);
         public bool Equals(NodeItem obj) => Item.Equals(obj.Item);

         public struct Comparer : IEqualityComparer<NodeItem> { //So NodeItem isn't boxed into object everytime.
            public bool Equals(NodeItem x, NodeItem y) => x.Equals(y);
            public int GetHashCode(NodeItem obj) => obj.GetHashCode();
         }
      }

      private class CharNode {
         public readonly char Char;
         public Dictionary<char, CharNode> Children;
         public HashSet<NodeItem> Items;

         public CharNode(char ch) { Char = ch; }

         public override bool Equals(object obj) => this.Equals(obj as CharNode);
         public bool Equals(CharNode obj) => obj != null && this.Char == obj.Char;
         public override int GetHashCode() => this.Char.GetHashCode();
      }

      #endregion


   }

}
