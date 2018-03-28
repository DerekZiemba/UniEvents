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
         return new StringBuilder(capacity);
      }

      public static void Return(StringBuilder value) {
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            if (_local == null || value.Capacity > _local.Capacity) {
               _local = value;
               return;
            }
            if (_globalA == null) {
               Interlocked.CompareExchange(ref _globalA, value, null);
               return;
            }
            if (_globalB == null) {
               Interlocked.CompareExchange(ref _globalB, value, null);
               return;
            }
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
      private static List<TItem>  _globalA;
      private static List<TItem>  _globalB;

      public static Int64 CountNew;
      public static Int64 CountRet;
      public static Int64 CountDisposed;

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
         Interlocked.Increment(ref CountNew);
         return new List<TItem>(capacity);
      }

      public static void Return(List<TItem> value) {
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            if (_globalB == null) {
               value.Clear();
               if(_globalB != null) { goto TryLocal; } //If it was set by another thread during the clear;
               if (Interlocked.CompareExchange(ref _globalB, value, null) == null) {
                  Interlocked.Increment(ref CountRet);
               } else {
                  Interlocked.Increment(ref CountDisposed);
               }
               return;
            }
            if (_globalA == null) {
               value.Clear();
               if (_globalA != null) { goto TryLocal; } //If it was set by another thread during the clear;
               if (Interlocked.CompareExchange(ref _globalA, value, null) == null) {
                  Interlocked.Increment(ref CountRet);
               } else {
                  Interlocked.Increment(ref CountDisposed);
               }
               return;
            }
TryLocal:
            if (_local == null || value.Capacity > _local.Capacity) {
               value.Clear();
               _local = value;
               Interlocked.Increment(ref CountRet);
               return;
            }
         }
         CountDisposed++;
      }
   }



}