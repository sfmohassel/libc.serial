using System;
using System.ComponentModel;
using System.Globalization;
using libc.serial.Resources;
namespace libc.serial.Internal {
    internal class EnumDescriptionTypeConverter : EnumConverter {
        public EnumDescriptionTypeConverter(Type type)
            : base(type) {
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType) {
            if (destinationType == typeof(string)) return Convert((Enum) value);
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public static string Convert(Enum value) {
            if (value != null) {
                var fi = value.GetType().GetField(value.ToString());
                if (fi != null) {
                    var attributes =
                        (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes.Length > 0 &&
                        attributes[0].Description != null) {
                        var att = attributes[0];
                        return att.Description;
                    }
                    return value.ToString();
                }
            }
            return Tran.Instance.Get("Unknown");
        }
        public static Enum ConvertBack(string value, Type targetType) {
            var fiList = targetType.GetFields();
            var o = Activator.CreateInstance(targetType);
            if (fiList.Length > 0)
                foreach (var fi in fiList) {
                    var attributes =
                        (DescriptionAttribute[]) fi.GetCustomAttributes(
                            typeof(DescriptionAttribute), false);
                    if (attributes.Length > 0 &&
                        !string.IsNullOrEmpty(attributes[0].Description)) {
                        var att = attributes[0];
                        if (att.Description == value) {
                            var res = fi.GetValue(o) as Enum;
                            return res;
                        }
                    }
                }
            throw new Exception("Enum field not found!!!");
        }
    }
}