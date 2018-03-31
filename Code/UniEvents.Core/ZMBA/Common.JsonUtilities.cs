using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Reflection;
using Newtonsoft.Json;
using CmpOp = System.Globalization.CompareOptions;
using static System.Runtime.CompilerServices.MethodImplOptions;


namespace ZMBA {

   public static partial class Common {


      public static JsonSerializer PrettyJsonSerializer { get; private set; } = ((Func<JsonSerializer>)(
         () => {
            var ser = new Newtonsoft.Json.JsonSerializer();
            ser.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            ser.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            ser.Formatting = Formatting.Indented;
            ser.MissingMemberHandling = MissingMemberHandling.Ignore;
            return ser;
         }))();

      public static JsonSerializer CompactJsonSerializer { get; private set; } = ((Func<JsonSerializer>)(
         () => {
            var ser = new Newtonsoft.Json.JsonSerializer();
            ser.NullValueHandling = NullValueHandling.Ignore;
            ser.TypeNameHandling = TypeNameHandling.None;
            ser.MissingMemberHandling = MissingMemberHandling.Ignore;
            return ser;
         }))();


      public static string ToJson<T>(this T obj, bool pretty = true) {
         return (pretty ? PrettyJsonSerializer : CompactJsonSerializer).Serialize(obj);
      }

      public static string Serialize<T>(this Newtonsoft.Json.JsonSerializer ser, T value) {
         using (StringWriter sw = new StringWriter(new StringBuilder(256), (IFormatProvider)CultureInfo.InvariantCulture)) {
            using (JsonTextWriter writer = new JsonTextWriter(sw)) {
               ser.Serialize(writer, value, typeof(T));
            }
            return sw.ToString();
         }
      }

      public static byte[] SerializeToGZippedBytes<T>(this JsonSerializer ser, T value) {
         using (var mem = new MemoryStream())
         using (var zip = new GZipStream(mem, CompressionMode.Compress))
         using (var buffer = new BufferedStream(zip, 8192))
         using (var stream = new StreamWriter(buffer, Encoding.UTF8))
         using (var writer = new JsonTextWriter(stream)) {
            ser.Serialize(writer, value, typeof(T));
            writer.Flush();
            return mem.ToArray();
         }
      }

      public static T DeserializeGZippedBytes<T>(this JsonSerializer ser, byte[] bytes) {
         if(bytes is null) { return default; }
         using (var mem = new MemoryStream(bytes))
         using (var zip = new GZipStream(mem, CompressionMode.Decompress))
         using (var stream = new StreamReader(zip, Encoding.UTF8))
         using (var reader = new JsonTextReader(stream)) {
            return ser.Deserialize<T>(reader);
         }
      }


      public static T Deserialize<T>(this JsonSerializer ser, string value) {
         using (JsonTextReader reader = new JsonTextReader(new StringReader(value))) {
            return ser.Deserialize<T>(reader);
         }
      }

      public static T DeserializeOrDefault<T>(this JsonSerializer ser, string value, T @default = default(T)) {
         if (value.IsNotWhitespace()) {
            try {
               return ser.Deserialize<T>(value);
#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception ex) { }
#pragma warning restore CS0168 // Variable is declared but never used
         }
         return @default;
      }





   }

}
