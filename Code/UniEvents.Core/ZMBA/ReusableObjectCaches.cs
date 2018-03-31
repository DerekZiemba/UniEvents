using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ZMBA {

   internal static class StringBuilderCache {
      public const int MAX_ITEM_CAPACITY = 320;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static StringBuilder _local;
      private static StringBuilder _globalA;
      private static StringBuilder _globalB;
      private static StringBuilder _globalC;

      public static StringBuilder Take(int capacity = 32) {
         if (capacity > MAX_ITEM_CAPACITY) { return new StringBuilder(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         StringBuilder value = null;
         value = _local;
         if (value != null) { _local = null; return value.Clear(); }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value.Clear(); }
         value = _globalB;
         if (value != null && Interlocked.CompareExchange(ref _globalB, null, value) == value) { return value.Clear(); }
         value = _globalC;
         if (value != null && Interlocked.CompareExchange(ref _globalC, null, value) == value) { return value.Clear(); }
         return new StringBuilder(capacity);
      }

      public static void Return(ref StringBuilder item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            if (_globalA == null) {
               Interlocked.CompareExchange(ref _globalA, value, null);
               return;
            }
            if (_globalB == null) {
               Interlocked.CompareExchange(ref _globalB, value, null);
               return;
            }
            if (_local == null) {
               _local = value;
               return;
            }
            if (_globalC == null) {
               Interlocked.CompareExchange(ref _globalC, value, null);
               return;
            }
         }
      }

      public static string Release(ref StringBuilder value) {
         string str = value.ToString();
         Return(ref value);
         return str;
      }
   }


   internal static class ListCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 512;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static List<TItem> _localA;
      private static List<TItem>  _globalA;
      private static ConcurrentBag<List<TItem>> _bag = new ConcurrentBag<List<TItem>>();

      public static List<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new List<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         List<TItem> value = null;
         value = _localA;
         if (value != null) { _localA = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         if(_bag.TryTake(out value)){
            return value;
         }
         return new List<TItem>(capacity);
      }

      public static void Return(ref List<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
ReturnStart:
            if (_globalA == null) {
               if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
               Interlocked.CompareExchange(ref _globalA, value, null);
               return;
            }
            if (_localA == null) {
               if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case one of the _globals became free during the clear operation
               _localA = value;
               return;
            }
            _bag.Add(item);
         }
      }
   }

   internal static class HashSetCache<TItem> {
      [ThreadStatic] private static HashSet<TItem> _local;
      private static HashSet<TItem>  _globalA;
      private static ConcurrentBag<HashSet<TItem>> _bag = new ConcurrentBag<HashSet<TItem>>();

      public static HashSet<TItem> Take() {
         HashSet<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         if (_bag.TryTake(out value)) {
            return value;
         }
         return new HashSet<TItem>();
      }

      public static void Return(ref HashSet<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
ReturnStart:
         if (_globalA == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalA, value, null);
            return;
         }
         if (_local == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case one of the _globals became free during the clear operation
            _local = value;
            return;
         }
         _bag.Add(item);
      }
   }


   internal static class StackCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 256;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static Stack<TItem> _local;
      private static Stack<TItem>  _globalA;
      private static ConcurrentBag<Stack<TItem>> _bag = new ConcurrentBag<Stack<TItem>>();

      public static Stack<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new Stack<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         Stack<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         if (_bag.TryTake(out value)) {
            return value;
         }
         return new Stack<TItem>(capacity);
      }

      public static void Return(ref Stack<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
ReturnStart:
         if (_globalA == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalA, value, null);
            return;
         }
         if (_local == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case one of the _globals became free during the clear operation
            _local = value;
            return;
         }
         _bag.Add(item);
      }
   }


   internal static class QueueCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 256;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static Queue<TItem> _local;
      private static Queue<TItem>  _globalA;
      private static ConcurrentBag<Queue<TItem>> _bag = new ConcurrentBag<Queue<TItem>>();

      public static Queue<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new Queue<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         Queue<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         if (_bag.TryTake(out value)) {
            return value;
         }
         return new Queue<TItem>(capacity);
      }

      public static void Return(ref Queue<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
ReturnStart:
         if (_globalA == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalA, value, null);
            return;
         }
         if (_local == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case one of the _globals became free during the clear operation
            _local = value;
            return;
         }
         _bag.Add(item);
      }
   }


}