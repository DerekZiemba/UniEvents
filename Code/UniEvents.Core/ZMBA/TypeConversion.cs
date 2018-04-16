using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ZMBA {
   public static class TypeConversion {
      private static IFormatProvider defaultProvider = System.Threading.Thread.CurrentThread.CurrentCulture;

      public static readonly StringConverter StringConverter = new StringConverter();
      public static readonly BooleanConverter BoolConverter = new MyBooleanConverter();
      public static readonly CharConverter CharConverter = new MyCharConverter();
      public static readonly SByteConverter SByteConverter = new MySByteConverter();
      public static readonly Int16Converter Int16Converter = new MyInt16Converter();
      public static readonly Int32Converter Int32Converter = new MyInt32Converter();
      public static readonly Int64Converter Int64Converter = new MyInt64Converter();
      public static readonly ByteConverter ByteConverter = new MyByteConverter();
      public static readonly UInt16Converter UInt16Converter = new MyUInt16Converter();
      public static readonly UInt32Converter UInt32Converter = new MyUInt32Converter();
      public static readonly UInt64Converter UInt64Converter = new MyUInt64Converter();
      public static readonly SingleConverter Float32Converter = new MyFloat32Converter();
      public static readonly DoubleConverter Float64Converter = new MyFloat64Converter();
      public static readonly DecimalConverter DecimalConverter = new MyDecimalConverter();
      public static readonly DateTimeConverter DateTimeConverter = new MyDateTimeConverter();
      public static readonly TimeSpanConverter TimeSpanConverter = new TimeSpanConverter();
      public static readonly GuidConverter GuidConverter = new GuidConverter();


      public static readonly Dictionary<Type, TypeConverter> TypeConverterLookup =((Func<Dictionary<Type, TypeConverter>>)(
      () => {
         var dict = new Dictionary<Type, TypeConverter>(40);
         dict[typeof(string)]=StringConverter;
         dict[typeof(bool)]=BoolConverter;
         dict[typeof(char)]=CharConverter;
         dict[typeof(sbyte)]=SByteConverter;
         dict[typeof(Int16)]=Int16Converter;
         dict[typeof(Int32)]=Int32Converter;
         dict[typeof(Int64)]=Int64Converter;
         dict[typeof(byte)]=ByteConverter;
         dict[typeof(UInt16)]=UInt16Converter;
         dict[typeof(UInt32)]=UInt32Converter;
         dict[typeof(UInt64)]=UInt64Converter;
         dict[typeof(Single)]=Float32Converter;
         dict[typeof(Double)]=Float64Converter;
         dict[typeof(Decimal)]=DecimalConverter;
         dict[typeof(DateTime)]=DateTimeConverter;
         dict[typeof(TimeSpan)]=TimeSpanConverter;
         dict[typeof(Guid)]=GuidConverter;
         dict[typeof(bool?)]=BoolConverter;
         dict[typeof(char?)]=CharConverter;
         dict[typeof(sbyte?)]=SByteConverter;
         dict[typeof(Int16?)]=Int16Converter;
         dict[typeof(Int32?)]=Int32Converter;
         dict[typeof(Int64?)]=Int64Converter;
         dict[typeof(byte?)]=ByteConverter;
         dict[typeof(UInt16?)]=UInt16Converter;
         dict[typeof(UInt32?)]=UInt32Converter;
         dict[typeof(UInt64?)]=UInt64Converter;
         dict[typeof(Single?)]=Float32Converter;
         dict[typeof(Double?)]=Float64Converter;
         dict[typeof(Decimal?)]=DecimalConverter;
         dict[typeof(DateTime?)]=DateTimeConverter;
         dict[typeof(TimeSpan?)]=TimeSpanConverter;
         dict[typeof(Guid?)]=GuidConverter;
         return dict;
      }))();


      public static TTo ConvertType<TTo>(object from) {
         return (TTo)TypeConverterLookup[typeof(TTo)].ConvertFrom(from);
      }


   
      private class MyBooleanConverter : BooleanConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            switch (fromTypeCode) {
               case TypeCode.Boolean:
                  return (Boolean)value;
               case TypeCode.Char:
                  switch ((char)value) {
                     case '1':
                     case 'T':
                     case 't':
                     case 'Y':
                     case 'y':
                        return true;
                     case '0':
                     case 'F':
                     case 'f':
                     case 'N':
                     case 'n':
                        return false;
                  }
                  return null;
               case TypeCode.SByte: return ((SByte)value) != 0;
               case TypeCode.Int16: return ((Int16)value) != 0;
               case TypeCode.Int32: return ((Int32)value) != 0;
               case TypeCode.Int64: return ((Int64)value) != 0;
               case TypeCode.Byte: return ((Byte)value) != 0;
               case TypeCode.UInt16: return ((UInt16)value) != 0;
               case TypeCode.UInt32: return ((UInt32)value) != 0;
               case TypeCode.UInt64: return ((UInt64)value) != 0;
               case TypeCode.Single: return ((Single)value) != 0;
               case TypeCode.Double: return ((Double)value) != 0;
               case TypeCode.Decimal: return ((Decimal)value) != 0;
               case TypeCode.DateTime: return ((DateTime)value) > DateTime.MinValue && ((DateTime)value) < DateTime.MaxValue;
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


      private class MyCharConverter : CharConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char: return (Char)value;
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToChar(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MySByteConverter : SByteConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToSByte(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyInt16Converter : Int16Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToInt16(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyInt32Converter : Int32Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToInt32(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


      private class MyInt64Converter : Int64Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToInt64(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


      private class MyByteConverter : ByteConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToByte(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


      private class MyUInt16Converter : UInt16Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToUInt16(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyUInt32Converter : UInt32Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToUInt32(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


      private class MyUInt64Converter : UInt64Converter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToUInt64(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyFloat32Converter : SingleConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToSingle(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyFloat64Converter : DoubleConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToDouble(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyDecimalConverter : DecimalConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? '1' : '0';
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToDecimal(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }

      private class MyDateTimeConverter : DateTimeConverter {
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => base.CanConvertFrom(context, sourceType) || typeof(IConvertible).IsAssignableFrom(sourceType);

         public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is null) { return base.ConvertFrom(context, culture, value); }

            Type fromType = value.GetType();
            if (fromType.IsNullable()) { fromType = fromType.GetGenericArguments()[0]; }
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);

            IConvertible convertible = value as IConvertible;
            switch (fromTypeCode) {
               case TypeCode.DBNull: return default;
               case TypeCode.Boolean: return (Boolean)value ? DateTime.MaxValue : DateTime.MinValue;
               case TypeCode.Char:
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
               case TypeCode.Decimal:
               case TypeCode.DateTime:
                  return convertible.ToDateTime(defaultProvider);
               default: return base.ConvertFrom(context, culture, value);
            }
         }
      }


   }
}
