using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA {

   public class AsciiAutoCompleteTrie<T> {
      private object _mutLock = new object();
      private Node[] Roots;


      //public AsciiAutoCompleteTrie(IEnumerable<string> sourcestrings) {
      //   Roots = new Node[36];
      //   for (byte i = 0; i < 36; i++) { Roots[i] = new Node() { KeyCode = i }; }
      //   if(sourcestrings != null) {
      //      IEnumerable<string> srcs = sourcestrings.Select(x=>x?.Trim()).Where(Common.IsNotWhitespace).OrderBy(x=>x).Distinct();
      //      foreach (string str in srcs) {
      //         AddEntryInternal(str);
      //      }
      //   }
      //}

      public AsciiAutoCompleteTrie() {
         Roots = new Node[36];
         for (byte i = 0; i < 36; i++) { Roots[i] = new Node() { KeyCode = i }; }
      }


      internal void AddEntryInternal(string key, T value) {
         byte code = MapChar(key[0]);
         Node current = Roots[code];

         for (int idx = 1; idx < key.Length; idx++) {
            code = MapChar(key[idx]);
            if (code != 255) {
               if (current.Children == null) {
                  current.Children = new List<Node>() { new Node() { KeyCode = code } };
               }
               for (var i = 0; i < current.Children.Count; i++) {
                  if (current.Children[i].KeyCode == code) {
                     current = current.Children[i];
                     goto LoopNextChar;
                  }
               }
               Node nextNode = new Node(){KeyCode=code};
               current.Children.Add(nextNode);
               current = nextNode;
            }
LoopNextChar:;
         }
         if(current.Items == null) { current.Items = new List<T>(); }
         if (!current.Items.Contains(value)) {
            current.Items.Add(value);
         }
      }


      public void AddEntry(string entry, T value) {
         lock (_mutLock) {
            AddEntryInternal(entry, value);
         }
      }


      public IEnumerable<T> GetSuggestions(string query) {
         if (!String.IsNullOrEmpty(query)) {
            int idx = 0;
            byte code = 255;
            for (; code == 255 && idx < query.Length; idx++) { code = MapChar(query[idx]); }

            if (code != 255 && idx < query.Length) {
               Node current = Roots[code];

               for (; idx < query.Length; idx++) {
                  if (current.Children == null || current.Children.Count == 0) { break; }

                  code = MapChar(query[idx]);
                  if (code != 255) {
                     for (var i = 0; i < current.Children.Count; i++) {
                        if (current.Children[i].KeyCode == code) {
                           current = current.Children[i];
                           goto LoopNextChar;
                        }
                     }
                  }
LoopNextChar:;
               }

               if(current.Children == null || current.Children.Count == 0) {
                  if(current.Items != null) {
                     for (idx = 0; idx < current.Items.Count; idx++) { yield return current.Items[idx]; }                
                  }                 
               } else {
                  Stack<Node> stack = StackCache<Node>.Take();
                  stack.Push(current);
                  while (stack.Count > 0) {
                     current = stack.Pop();
                     if (current.Children != null) {
                        for (idx = current.Children.Count - 1; idx >= 0; idx--) { stack.Push(current.Children[idx]); }
                     }
                     if (current.Items != null) {
                        for (idx = 0; idx < current.Items.Count; idx++) { yield return current.Items[idx]; }
                     }
                  }
                  StackCache<Node>.Return(ref stack);
               }
            }
         }
      }

      private static string Normalize(string input) {
         if (String.IsNullOrWhiteSpace(input)) { return ""; }
         var sb = StringBuilderCache.Take(input.Length);
         bool bSpace = false;
         for (var i = 0; i < input.Length; i++) {
            char ch = input[i];
            if (Char.IsLetter(ch)) {
               sb.Append(char.ToLower(ch));
               bSpace = false;
            } else if (char.IsNumber(ch)) {
               sb.Append(ch);
               bSpace = false;
            } else if (!bSpace) {
               if (char.IsWhiteSpace(ch)) {
                  sb.Append(' ');
                  bSpace = true;
               }
            }
         }
         if (sb[sb.Length - 1] == ' ') { sb.Length = sb.Length - 1; }
         return StringBuilderCache.Release(ref sb);
      }


      private static byte MapChar(char ch) {
         if (ch >= 'a' && ch <= 'z') {
            return (byte)(ch - 'a');
         }
         if (ch >= 'A' && ch <= 'Z') {
            return (byte)(ch - 'A');
         }
         if (ch >= '0' && ch <= '9') {
            return (byte)(ch - '0' + 26);
         }
         return 255;
      }


      private class Node {
         public byte KeyCode;
         public List<Node> Children;
         public List<T> Items;
      }

   }

}
