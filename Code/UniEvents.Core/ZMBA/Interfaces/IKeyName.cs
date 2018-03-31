using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA.Interfaces {
   public interface IKeyName {
      string Key { get; }
   }


   public class IKeyComparer<T> : IComparer<T>, IEqualityComparer<T> where T: IKeyName {
      public static readonly IKeyComparer<T> Ordinal = new IKeyComparer<T>(StringComparer.Ordinal);
      public static readonly IKeyComparer<T> OrdinalIgCase = new IKeyComparer<T>(StringComparer.OrdinalIgnoreCase);

      private readonly StringComparer _baseComparer;
      public IKeyComparer(StringComparer comparer) {
         _baseComparer = comparer;
      }

      public int Compare(T x, T y)=> _baseComparer.Compare(x.Key, y.Key);
      public bool Equals(T x, T y) => _baseComparer.Equals(x.Key, y.Key);
      public int GetHashCode(T obj) => _baseComparer.GetHashCode(obj.Key);

   }

}
