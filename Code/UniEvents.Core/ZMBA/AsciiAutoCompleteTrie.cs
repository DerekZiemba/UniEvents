using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA {

   public class AsciiAutoCompleteTrie {
      private object _mutLock = new object();
      private Node[] Roots;


      public AsciiAutoCompleteTrie(IEnumerable<string> sourcestrings) {
         Roots = new Node[36];
         for (byte i = 0; i < 36; i++) { Roots[i] = new Node() { Key = i }; }

         IEnumerable<string> srcs = sourcestrings.Select(x=>x?.Trim()).Where(Common.IsNotWhitespace).OrderBy(x=>x).Distinct();
         foreach (string str in srcs) {
            AddEntryInternal(str);
         }
      }


      private void AddEntryInternal(string entry) {
         byte key = MapChar(entry[0]);
         Node current = Roots[key];

         for (int idx = 1; idx < entry.Length; idx++) {
            key = MapChar(entry[idx]);
            if (key != 255) {
               if (current.Children == null) {
                  current.Children = new List<Node>() { new Node() { Key = key } };
               }
               for (var i = 0; i < current.Children.Count; i++) {
                  if (current.Children[i].Key == key) {
                     current = current.Children[i];
                     goto LoopNextChar;
                  }
               }
               Node nextNode = new Node(){Key=key};
               current.Children.Add(nextNode);
               current = nextNode;
            }
LoopNextChar:;
         }
         current.FullString = entry;
      }


      public void AddEntry(string entry) {
         lock (_mutLock) {
            AddEntryInternal(entry);
         }
      }


      public IEnumerable<string> GetSuggestions(string query) {
         if (!String.IsNullOrEmpty(query)) {
            int idx = 0;
            byte key = 255;
            for (; key == 255 && idx < query.Length; idx++) { key = MapChar(query[idx]); }

            if (key != 255 && idx < query.Length) {
               Node current = Roots[key];

               for (; idx < query.Length; idx++) {
                  if (current.Children == null || current.Children.Count == 0) { break; }

                  key = MapChar(query[idx]);
                  if (key != 255) {
                     for (var i = 0; i < current.Children.Count; i++) {
                        if (current.Children[i].Key == key) {
                           current = current.Children[i];
                           goto LoopNextChar;
                        }
                     }
                  }
LoopNextChar:;
               }

               if(current.Children == null || current.Children.Count == 0) {
                  if(current.FullString != null) {
                     yield return current.FullString;
                  }                 
               } else {
                  Stack<Node> stack = StackCache<Node>.Take();
                  stack.Push(current);
                  while (stack.Count > 0) {
                     current = stack.Pop();
                     if (current.Children != null) {
                        for (idx = current.Children.Count - 1; idx >= 0; idx--) { stack.Push(current.Children[idx]); }
                     }
                     if (current.FullString != null) {
                        yield return current.FullString;
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
         public byte Key;
         public List<Node> Children;
         public string FullString;
      }

   }

}
