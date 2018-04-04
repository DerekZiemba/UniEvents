using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMBA.Interfaces {

   public interface IKey<T> {
      T Key { get; }
   }
   public interface IValue<T> {
      T Value { get; }
   }

   public interface IKVP<TKey, TValue> : IKey<TKey>, IValue<TValue> { }

   public interface IName { string Name { get; } }

   public interface IDescription { string Description { get; } }


}
