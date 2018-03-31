using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA {

   public class PartialNameSearchTrie<T> : IEnumerable<T> where T : Interfaces.IKeyName  {
      private Node[] Roots;

      public PartialNameSearchTrie() {
         Roots = new Node[36];
         for (byte i = 0; i < 36; i++) { Roots[i] = new Node() { KeyCode = i }; }
      }

      public int Count { get; private set; }
      public int NodeCount { get; private set; }

      public void AddItem(T value) {
         AddItem(value.Key, value);
      }

      public void AddItem(string key, T value) {
         if (!String.IsNullOrEmpty(key)) {
            int idx = 0;
            byte code = 255;
            for (; code == 255 && idx < key.Length; idx++) { code = MapChar(key[idx]); }
            if (code == 255 || idx >= key.Length) { return; }

            Node current = Roots[code];

            for (; idx < key.Length; idx++) {
               code = MapChar(key[idx]);
               if (code == 255) { continue; }
               if (current.Children == null) {
                  current = AddNodeChild(current, new Node() { KeyCode = code });
               } else {
                  for (var i = 0; i < current.Children.Count; i++) {
                     if (current.Children[i].KeyCode == code) {
                        current = current.Children[i];
                        goto LoopNextChar;
                     }
                  }
                  current = AddNodeChild(current, new Node() { KeyCode = code });
               }
LoopNextChar:;
            }
            AddNodeItem(current, key, value);
         }
      }


      public IEnumerable<T> FindMatches(string query, bool bIncludeSimilar) {
         if (!String.IsNullOrEmpty(query)) {
            int idx = 0;
            byte code = 255;
            for (; code == 255 && idx < query.Length; idx++) { code = MapChar(query[idx]); }
            if (code == 255 || idx >= query.Length) { yield break; }

            Node current = Roots[code];

            for (; idx < query.Length; idx++) {
               if (current.Children == null) { break; }
               code = MapChar(query[idx]);
               if (code == 255) { continue; }
               for (var i = 0; i < current.Children.Count; i++) {
                  if (current.Children[i].KeyCode == code) {
                     current = current.Children[i];
                     goto LoopNextChar;
                  }
               }
               if (!bIncludeSimilar) { break; }
LoopNextChar:;
            }

            if(current.Children == null) {
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

      private static byte MapChar(char ch) {
         if (ch >= 'a' && ch <= 'z') {
            return (byte)(ch - 'a' + 10);
         }
         if (ch >= 'A' && ch <= 'Z') {
            return (byte)(ch - 'A' + 10);
         }
         if (ch >= '0' && ch <= '9') {
            return (byte)(ch - '0');
         }
         return 255;
      }

      
      public IEnumerator<T> GetEnumerator() {
         Stack<Node> stack = StackCache<Node>.Take();
         for (var i = 0; i < Roots.Length; i++) {
            stack.Push(Roots[i]);
            while (stack.Count > 0) {
               var current = stack.Pop();
               if (current.Children != null) {
                  for (var idx = current.Children.Count - 1; idx >= 0; idx--) { stack.Push(current.Children[idx]); }
               }
               if (current.Items != null) {
                  for (var idx = 0; idx < current.Items.Count; idx++) { yield return current.Items[idx]; }
               }
            }
         }
         StackCache<Node>.Return(ref stack);
      }

      IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

      public void TrimExcess() {
         Stack<Node> stack = StackCache<Node>.Take();
         for (var i = 0; i < Roots.Length; i++) {
            stack.Push(Roots[i]);
            while (stack.Count > 0) {
               var current = stack.Pop();
               if (current.Children != null) {
                  current.Children.TrimExcess();
                  current.Children.Sort((x, y) => x.KeyCode.CompareTo(y.KeyCode));
                  for (var idx = current.Children.Count - 1; idx >= 0; idx--) {  stack.Push(current.Children[idx]);  }
               }
               if (current.Items != null) {
                  current.Items.TrimExcess();
                  current.Items.Sort((x, y) => StringComparer.Ordinal.Compare(x.Key, y.Key));
               }
            }
         }
         StackCache<Node>.Return(ref stack);
      }



      private Node AddNodeChild(Node target, Node child) {
         if (target.Children == null) {
            target.Children = new List<Node>() { child };
         } else {
            target.Children.Add(child);
         }
         NodeCount++;
         return child;
      }

      private void AddNodeItem(Node target, string key, T item) {
         if (target.Items == null) {
            target.Items = new List<T>() { item };
         } else {
            if (target.Items.Contains(item)) { return; }
            target.Items.Add(item);
         }
         Count++;
      }

      private class Node {
         public byte KeyCode;
         public List<Node> Children;
         public List<T> Items;
      }

   }

}
