using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace ZMBA.Exceptions {


   public class UnsupportedArgumentTypeException : ArgumentException {
      private const string Description = "Unsupported Argument Type.";

      public UnsupportedArgumentTypeException(string message = null, Type type = null, string param = null, Exception inner = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
         : base(BuildMessage(Description, message, type, param, caller, line, file), inner) {
      }

      protected UnsupportedArgumentTypeException(string message, Exception inner) : base(message, inner) { }
      protected UnsupportedArgumentTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

      protected static string BuildMessage(string desc, string message, Type type, string param, string caller, int line, string file) {
         var sb = StringBuilderCache.Take(64 + message != null ? message.Length : 0);
         sb.Append(desc);
         if (message.IsNotWhitespace()) { sb.Append(message); }
         if (type != null) { sb.AppendLine().Append(" | Type: ").Append(type.Name); }
         if (param.IsNotWhitespace()) { sb.Append(" | Param: ").Append(param); }
         if (caller.IsNotWhitespace()) { sb.Append(" | Method: ").Append(caller); }
         if (line > 0) { sb.Append(" | Line: ").Append(line); }
         if (file.IsNotWhitespace()) { sb.AppendLine().Append(" | File: ").Append(file); }
         return StringBuilderCache.Release(ref sb);
      }
   }

   public class UnsupportedArgumentTypeException<T> : UnsupportedArgumentTypeException {
      private const string Description = "Unsupported Generic Argument Type.";

      public UnsupportedArgumentTypeException(string message = null, string param = null, Exception inner = null, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
         : base(BuildMessage(Description, message, typeof(T), param, caller, line, file), inner) {
      }
      protected UnsupportedArgumentTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
   }


}
