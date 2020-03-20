using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace ZMBA {

    internal static class StringBuilderCache {
      private const int CACHE_SIZE = 3;
      /* Attempt to detect maximum number of times SB allocates.  If more than 3 then dispose */
      private const int MAX_SB_RESIZE_COUNT = 3;
      private const int NOMINAL_CHAR_CAPACITY = 256;
      private const int LARGE_CHAR_CAPACITY = NOMINAL_CHAR_CAPACITY * (2^(MAX_SB_RESIZE_COUNT - 1));  // 1024
      private const int MIN_CHAR_CAPACITY = NOMINAL_CHAR_CAPACITY / (2^MAX_SB_RESIZE_COUNT);        // 32
      private const int MIN_MID_POINT = ((NOMINAL_CHAR_CAPACITY - MIN_CHAR_CAPACITY) / 2);          // 112, less than this size put in _small
      private const int MID_MAX_POINT = ((LARGE_CHAR_CAPACITY - NOMINAL_CHAR_CAPACITY) / 2) + NOMINAL_CHAR_CAPACITY; // 640, larger than this, put in _large
      /* If SB starting at nominal capacity re-allocates MAX_SB_RESIZE_COUNT (3 times) 
       * starting from NOMINAL_CHAR_CAPACITY (example: 256, then 512, then end at final size 1024)
       * Then it's time to throw it away, but give it some leeway so we can use it as the _large string builder instance
       * In the example, that would make it to big to keep around at 1280 chars 
       */
      private const int MAX_CHAR_CAPACITY = LARGE_CHAR_CAPACITY + NOMINAL_CHAR_CAPACITY;


      private static StringBuilder _small;
      private static StringBuilder _medium;
      private static StringBuilder _large;
      [ThreadStatic] private static StringBuilder _local;
      private static StringBuilder[] _cached = new StringBuilder[CACHE_SIZE];

      public static StringBuilder Take(int capacity = 32) {
        if (capacity <= MAX_CHAR_CAPACITY) {
          StringBuilder value = _local;

          if (value != null) { 
            _local = null; 
            return value; 
          }

          if (capacity < MIN_MID_POINT && (value = _small) != null && Interlocked.CompareExchange(ref _small, null, value) == value) {
            return value;
          }

          if (capacity >= MIN_MID_POINT && capacity <= MID_MAX_POINT && (value = _medium) != null && Interlocked.CompareExchange(ref _medium, null, value) == value) {
            return value;
          }

          if (capacity >= MID_MAX_POINT && capacity <= MAX_CHAR_CAPACITY && (value = _large) != null && Interlocked.CompareExchange(ref _large, null, value) == value) {
            return value;
          }

          for (int i = 0; i < CACHE_SIZE; i++) {
            value = _cached[i];
            if (value != null && Interlocked.CompareExchange(ref _cached[i], null, value) == value) { return value; }
          }
        }

        // For everything to be optimal it's best we are using multiples of our constants at the top of the file
        return new StringBuilder(capacity < MIN_CHAR_CAPACITY ? MIN_CHAR_CAPACITY : capacity);
      }

      public static string Release(ref StringBuilder value) {
        string str = value.ToString();
        Return(ref value);
        return str;
      }

      public static void Return(ref StringBuilder input) {
        var value = input; //Get copy to reference
        input = null; //Set reference to null to ensure it's not used after we return. 

        int capacity = value.Capacity;
        // It's too big. I hate doing this because if we had it for a while, 
        //  it's likely that it's in Gen2 and won't actually be collected for a while. But hey we can't hold onto memory forever
        if (capacity > MAX_CHAR_CAPACITY) { return; }
        // It definitely wasn't created by us. Meaning it was probably just allocated and still in Gen0.
        // Lets not hold onto it because when we return it, it's likely to be in Gen1+ and the consumer may cause it to grow.
        if (capacity < MIN_CHAR_CAPACITY) { return; }  

        if (capacity < MIN_MID_POINT && _small == null) {
          value.Clear();
          if (_small != null) { ReturnSlow(value); } else { _small = value; } // In case we're beat by another thread while clearing
          return;
        }

        if (capacity >= MIN_MID_POINT && capacity <= MID_MAX_POINT && _medium == null) {
          value.Clear();   
          if (_medium != null) { ReturnSlow(value); } else { _medium = value; } // In case we're beat by another thread while clearing
          return;
        }

        if (capacity >= MID_MAX_POINT && capacity <= MAX_CHAR_CAPACITY && _large == null) {
          value.Clear();
          if (_large != null) { ReturnSlow(value); } else { _large = value; } // In case we're beat by another thread while clearing
          return;
        }

        ReturnSlow(value);
      }

      private static void ReturnSlow(StringBuilder value) {
        for (int i = 0; i < CACHE_SIZE; i++) {
          if (_cached[i] == null) {
            // Intentionally not using interlocked here. 
            // In a worst case scenario two objects may be stored into same slot.
            // It is very unlikely to happen and will only mean that one of the objects will get collected.
            _cached[i] = value;
            return;
          }
        }
        // If it can't go anywhere else, put it in the _local slot
        // We use _local slot last to increase sharing of pooled objects, and the _local slot will only be used by a single thread. 
        if (_local == null) { _local = value; }
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
      for (int i = 0; i < CACHE_SIZE; i++) {
        value = _cached[i];
        if (value != null && Interlocked.CompareExchange(ref _cached[i], null, value) == value) { return value; }
      }

      return new List<string>(capacity);
    }

    public static void Return(ref List<string> item) {
      var value = item; //Get copy to reference
      item = null; //Set reference to null to ensure it's not used after it's returned. 
      if (value.Capacity <= MAX_ITEM_CAPACITY) {
ReturnStart:
        for (int i = 0; i < CACHE_SIZE; i++) {
          if (_cached[i] == null) {
            if (value.Count > 0) { value.Clear(); goto ReturnStart; }//In case _global was set during clear operation
            Interlocked.CompareExchange(ref _cached[i], value, null);
            return;
          }
        }
      }
    }
  }


}