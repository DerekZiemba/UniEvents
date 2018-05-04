using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ZMBA {

   internal static class StringBuilderCache {
      private const int CACHE_SIZE = 3;
      public const int MAX_ITEM_CAPACITY = 320;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      private static StringBuilder[] _cached = new StringBuilder[CACHE_SIZE];

      public static StringBuilder Take(int capacity = 32) {
         if (capacity > MAX_ITEM_CAPACITY) { return new StringBuilder(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         StringBuilder value = null;
         for(int i = 0; i < CACHE_SIZE; i++) {
            value = _cached[i];
            if (value != null && Interlocked.CompareExchange(ref _cached[i], null, value) == value) { return value.Clear(); }
         }
         return new StringBuilder(capacity);
      }

      public static void Return(ref StringBuilder item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            for(int i = 0; i < CACHE_SIZE; i++) {
               if(_cached[i] == null) {
                  Interlocked.CompareExchange(ref _cached[i], value, null);
                  return;
               }
            }
         }
      }

      public static string Release(ref StringBuilder value) {
         string str = value.ToString();
         Return(ref value);
         return str;
      }
   }


   internal static class StringListCache {
      private const int CACHE_SIZE = 5;
      public const int MAX_ITEM_CAPACITY = 512;
      public const int MIN_ITEM_CAPACITY = MAX_ITEM_CAPACITY / (2^3); //Allow 3 resizes before going over max capacity

      private static List<string>[] _cached = new List<string>[CACHE_SIZE];

      public static List<string> Take(int capacity = MIN_ITEM_CAPACITY) {
         if (capacity > MAX_ITEM_CAPACITY) { return new List<string>(capacity); }
         if (capacity < MIN_ITEM_CAPACITY) { capacity = MIN_ITEM_CAPACITY; }
         List<string> value = null;
         for(int i = 0; i < CACHE_SIZE; i++) {
            value = _cached[i];
            if(value != null && Interlocked.CompareExchange(ref _cached[i], null, value) == value) { return value; }
         }

         return new List<string>(capacity);
      }

      public static void Return(ref List<string> item) {
         var value = item; //Get copy to reference
         item = null; //Set reference to null to ensure it's not used after it's returned. 
         if (value.Capacity <= MAX_ITEM_CAPACITY) {
            ReturnStart:
            for(int i = 0; i < CACHE_SIZE; i++) {
               if(_cached[i] == null) {
                  if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
                  Interlocked.CompareExchange(ref _cached[i], value, null);
                  return;
               }
            }
         }
      }
   }


}