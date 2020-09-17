using System;
namespace libc.serial.Internal {
    internal static class DateAndTimeExtensions {
        // public static (Dat from, Dat to) DayInterval(this Dat k) {
        //     var s = k.ToDate();
        //     var e = s.Add(Period.FromDays(1)).Subtract(Period.FromMilliseconds(1));
        //     return (s, e);
        // }
        // public static int Age(this DateTime birthDate) {
        //     var now = DateTime.Today;
        //     var age = now.Year - birthDate.Year;
        //     if (now < birthDate.AddYears(age)) age--;
        //     return age;
        // }
        // public static string ToPrettyString(this TimeSpan span) {
        //     if (span == TimeSpan.Zero) return "0 minutes";
        //     var sb = new StringBuilder();
        //     if (span.Days > 0)
        //         sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : string.Empty);
        //     if (span.Hours > 0)
        //         sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : string.Empty);
        //     if (span.Minutes > 0)
        //         sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : string.Empty);
        //     return sb.ToString();
        // }
        internal static bool IsValid(this Dat item) {
            try {
                if (item == null) return false;
                var k = item.DateTime();
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}