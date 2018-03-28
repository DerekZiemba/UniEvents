using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ZMBA {

   public static class StringBuilderCache {
      private const int MAX_ITEM_CAPACITY = 320;
      private const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static StringBuilder _local;
      private static StringBuilder _globalA;
      private static StringBuilder _globalB;

      private static StringBuilder Clean(StringBuilder value, int mincap) {
         value.Clear();
         if (value.Capacity < mincap) { value.Capacity = mincap; }
         return value;
      }

      public static StringBuilder Take(int capacity = 32) {
         if (capacity > MAX_ITEM_CAPACITY) { return new StringBuilder(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         StringBuilder value = null;
         value = _local;
         if (value != null) { _local = null; return Clean(value, capacity); }
         value = _globalA;
         if (value != null && Interlocked.CompareExchange(ref _globalA, null, value) == value) { return Clean(value, capacity); }
         value = _globalB;
         if (value != null && Interlocked.CompareExchange(ref _globalB, null, value) == value) { return Clean(value, capacity); }
         return new StringBuilder(capacity);
      }

      public static void Return(StringBuilder value) {
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            if (_local == null) {
               _local = value;
               value = null;
               return;
            } else if (value.Capacity > _local.Capacity) {
               StringBuilder temp = _local; _local = value; value = temp;
            }
            if (_globalA == null && Interlocked.CompareExchange(ref _globalA, value, null) == null) { }
         }
      }

      public static string Release(StringBuilder value) {
         string str = value.ToString();
         Return(value);
         return str;
      }
   }


   public static class ListCache<TItem> {
      private const int MAX_ITEM_CAPACITY = 128;
      private const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      [ThreadStatic] private static List<TItem> _local;
      private static List<TItem>  _global;

      private static List<TItem> Clean(List<TItem> value, int mincap) {
         value.Clear();
         if (value.Capacity < mincap) { value.Capacity = mincap; }
         return value;
      }

      public static List<TItem> Take(int capacity) {
         if (capacity > MAX_ITEM_CAPACITY) { return new List<TItem>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         List<TItem> value = null;
         value = _local;
         if (value != null) { _local = null; return Clean(value, capacity); }
         value = _global;
         if (value != null && Interlocked.CompareExchange(ref _global, null, value) == value) { return Clean(value, capacity); }
         return new List<TItem>(capacity);
      }

      public static void Return(List<TItem> value) {
         if (value.Capacity > MAX_ITEM_CAPACITY) { return; }
         if (_local == null) {
            _local = value;
            return;
         }
         if (_global == null && Interlocked.CompareExchange(ref _global, value, null) == null) { }
      }
   }



}