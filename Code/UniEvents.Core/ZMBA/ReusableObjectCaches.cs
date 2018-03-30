using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ZMBA {

   public static class StringBuilderCache {
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


   public static class ListCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 128;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static List<TItem> _local;
      private static List<TItem>  _globalA;
      private static List<TItem>  _globalB;
      private static List<TItem>  _globalC;

      public static List<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new List<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         List<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         value = _globalB;
         if (value != null && Interlocked.CompareExchange(ref _globalB, null, value) == value) { return value; }
         value = _globalC;
         if (value != null && Interlocked.CompareExchange(ref _globalC, null, value) == value) { return value; }
         return new List<TItem>(capacity);
      }

      public static void Return(ref List<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
ReturnStart:
            if (_globalB == null) {
               if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
               Interlocked.CompareExchange(ref _globalB, value, null);
               return;
            }
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
            if (_globalC == null) {
               if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
               Interlocked.CompareExchange(ref _globalC, value, null);
               return;
            }
         }
      }
   }


   public static class StackCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 128;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static Stack<TItem> _local;
      private static Stack<TItem>  _globalA;
      private static Stack<TItem>  _globalB;
      private static Stack<TItem>  _globalC;

      public static Stack<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new Stack<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         Stack<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         value = _globalB;
         if (value != null && Interlocked.CompareExchange(ref _globalB, null, value) == value) { return value; }
         value = _globalC;
         if (value != null && Interlocked.CompareExchange(ref _globalC, null, value) == value) { return value; }
         return new Stack<TItem>(capacity);
      }

      public static void Return(ref Stack<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
ReturnStart:
         if (_globalB == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalB, value, null);
            return;
         }
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
         if (_globalC == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalC, value, null);
            return;
         }
      }
   }


   public static class QueueCache<TItem> {
      public const int MAX_ITEM_CAPACITY = 128;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static Queue<TItem> _local;
      private static Queue<TItem>  _globalA;
      private static Queue<TItem>  _globalB;
      private static Queue<TItem>  _globalC;

      public static Queue<TItem> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new Queue<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         Queue<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return value; }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return value; }
         value = _globalB;
         if (value != null && Interlocked.CompareExchange(ref _globalB, null, value) == value) { return value; }
         value = _globalC;
         if (value != null && Interlocked.CompareExchange(ref _globalC, null, value) == value) { return value; }
         return new Queue<TItem>(capacity);
      }

      public static void Return(ref Queue<TItem> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
ReturnStart:
         if (_globalB == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalB, value, null);
            return;
         }
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
         if (_globalC == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _globalC, value, null);
            return;
         }
      }
   }


}