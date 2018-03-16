using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UniEvents.Models;
using static ZMBA.Common;

namespace UniEvents.Models.ApiModels {


   public class MethodMetadata {   
      public string Path { get; set; }
      public string Route { get; set; }
      public EHttpVerbs HttpMethod { get; set; }
      public string MethodName { get; set; }
      public string FullMethodName { get; set; }

      public InputOutput Output { get; set; }

      public List<MethodParam> Input { get; set; } = new List<MethodParam>();

      public class InputOutput {
         public JavascriptTypes JSType { get; set; }
         public string TypeName { get; set; }
         public bool IsCollection { get; set; }

         public void SetType(Type type) {
            if (type.IsNullable()) {
               type = Nullable.GetUnderlyingType(type);
            }
            TypeName = type.Name;

            if (type.IsArray) {
               JSType = JavascriptTypes.array;
               IsCollection = true;
               return;
            }
            switch (Type.GetTypeCode(type)) {
               case TypeCode.Boolean: JSType = JavascriptTypes.boolean; IsCollection = false; break;
               case TypeCode.Char:
               case TypeCode.String: JSType = JavascriptTypes.@string; IsCollection = false; break;
               case TypeCode.SByte:
               case TypeCode.Int16:
               case TypeCode.Int32:
               case TypeCode.Int64:
               case TypeCode.Byte:
               case TypeCode.UInt16:
               case TypeCode.UInt32:
               case TypeCode.UInt64:
               case TypeCode.Single:
               case TypeCode.Double:
               case TypeCode.Decimal: JSType = JavascriptTypes.number; IsCollection = false; break;
               case TypeCode.DateTime: JSType = JavascriptTypes.date; IsCollection = false; break;
               default:
                  Type[] interfaces = type.GetInterfaces();
                  foreach (var face in interfaces) {
                     if (face == typeof(IList)) {
                        JSType = JavascriptTypes.array;
                        IsCollection = true;
                        return;
                     }
                     if (face == typeof(ICollection) || face == typeof(IEnumerable)) {
                        JSType = JavascriptTypes.@object;
                        IsCollection = true;
                        return;
                     }
                     if (face.IsGenericType) {
                        var def = face.GetGenericTypeDefinition();
                        if (def == typeof(IList<>)) {
                           JSType = JavascriptTypes.array;
                           IsCollection = true;
                           return;
                        }
                        if (def == typeof(ICollection<>) || def == typeof(IEnumerable<>)) {
                           JSType = JavascriptTypes.@object;
                           IsCollection = true;
                           return;
                        }
                     }
                  }
                  JSType = JavascriptTypes.@object;
                  break;
            }
         }
      }

      public class MethodParam : InputOutput {
         public string Name { get; set; }    
         public bool IsOptional { get; set; }
         public ParamSource Source { get; set; }
      }

      public enum JavascriptTypes {
         @null,
         @boolean,
         @number,
         @string,
         @date,
         @array,
         @object
      }

      public enum ParamSource {
         Unknown,
         Url,
         QueryString,
         Body
      }
   }
}
