using System.ComponentModel;

namespace libc.serial.Internal
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum Calendars
    {
        [Description("Gregorian")]
        Gregorian,

        [Description("PersianArithmetic")]
        Persian
    }
}