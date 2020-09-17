using System.Globalization;
using Newtonsoft.Json;
using NodaTime;
namespace libc.serial.Internal {
    internal sealed class Dat : NotifyModel, IUpdatable {
        public const string PrettyDatePattern = "dddd d MMMM yyyy";
        public const string PrettyDateTimePatter = "dddd d MMMM yyyy h:m:s tt";
        [JsonIgnore]
        private int _Day;
        [JsonIgnore]
        private int _Hour;
        [JsonIgnore]
        private int _Millisecond;
        [JsonIgnore]
        private int _Minute;
        [JsonIgnore]
        private int _Month;
        [JsonIgnore]
        private int _Second;
        [JsonIgnore]
        private int _Year;
        [JsonIgnore]
        private ZoneInfo _ZoneInfo;
        internal Dat() {
        }
        internal Dat(ZonedDateTime dateTime)
            : this(new ZoneInfo(dateTime.Zone, dateTime.Calendar), dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond) {
        }
        internal Dat(ZoneInfo zoneInfo, int year, int month, int day, int hour = 0, int minute = 0, int second = 0,
            int millisecond = 0)
            : this(zoneInfo) {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            Millisecond = millisecond;
        }
        public Dat(ZoneInfo zoneInfo, Instant instantUtc)
            : this(zoneInfo) {
            var k = instantUtc.InZone(ZoneInfo.Zone(), ZoneInfo.CalendarSystem());
            Year = k.Year;
            Month = k.Month;
            Day = k.Day;
            Hour = k.Hour;
            Minute = k.Minute;
            Second = k.Second;
            Millisecond = k.Millisecond;
        }
        public Dat(IUpdatable another) {
            UpdateFrom(another);
        }
        private Dat(ZoneInfo zoneInfo) {
            ZoneInfo = zoneInfo;
        }
        public ZoneInfo ZoneInfo {
            get => _ZoneInfo;
            set => Set(ref _ZoneInfo, value, () => ZoneInfo);
        }
        public int Year {
            get => _Year;
            set {
                Set(ref _Year, value, () => Year);
            }
        }
        public int Month {
            get => _Month;
            set {
                Set(ref _Month, value, () => Month);
            }
        }
        public int Day {
            get => _Day;
            set {
                Set(ref _Day, value, () => Day);
            }
        }
        public int Hour {
            get => _Hour;
            set {
                Set(ref _Hour, value, () => Hour);
            }
        }
        public int Minute {
            get => _Minute;
            set {
                Set(ref _Minute, value, () => Minute);
            }
        }
        public int Second {
            get => _Second;
            set {
                Set(ref _Second, value, () => Second);
            }
        }
        public int Millisecond {
            get => _Millisecond;
            set {
                Set(ref _Millisecond, value, () => Millisecond);
            }
        }
        public void UpdateFrom(IUpdatable o) {
            var item = (Dat) o;
            if (ZoneInfo == null) ZoneInfo = new ZoneInfo();
            ZoneInfo.Calendar = item.ZoneInfo.Calendar;
            ZoneInfo.ZoneId = item.ZoneInfo.ZoneId;
            Year = item.Year;
            Month = item.Month;
            Day = item.Day;
            Hour = item.Hour;
            Minute = item.Minute;
            Second = item.Second;
            Millisecond = item.Millisecond;
        }
        /// <summary>
        ///     returns JSON format
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
        public string ToString(string pattern, CultureInfo cultureInfo) {
            return DateTime().ToString(pattern, cultureInfo);
        }
        public string ToString(string pattern) {
            return ToString(pattern, ZoneInfo.GetCultureInfo());
        }
        public string ToStringPrettyDate() {
            return DateTime().ToString(PrettyDatePattern, ZoneInfo.GetCultureInfo());
        }
        public string ToStringPrettyDateTime() {
            return DateTime().ToString(PrettyDateTimePatter, ZoneInfo.GetCultureInfo());
        }
        public ZonedDateTime DateTime() {
            return new LocalDateTime(Year, Month, Day, Hour, Minute, Second, Millisecond, ZoneInfo.CalendarSystem())
                .InZoneLeniently(ZoneInfo.Zone());
        }
        public Instant Instant() {
            return DateTime().ToInstant();
        }
        public long UnixMilliseconds() {
            return Instant().ToUnixTimeMilliseconds();
        }
        public long UnixTicks() {
            return Instant().ToUnixTimeTicks();
        }
        public IsoDayOfWeek DayOfWeek() {
            return DateTime().DayOfWeek;
        }
        public Dat ToDate() {
            return new Dat(ZoneInfo, Year, Month, Day);
        }
        /// <summary>
        ///     Don't add days with this method!!!
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public Dat Add(Period period) {
            var k = DateTime().LocalDateTime + period;
            var dayEnd = ZoneInfo.Zone().AtLeniently(k);
            return new Dat(dayEnd);
        }
        /// <summary>
        ///     Dont't subtract days with this method!!!
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public Dat Subtract(Period period) {
            var k = DateTime().LocalDateTime - period;
            var dayEnd = ZoneInfo.Zone().AtLeniently(k);
            return new Dat(dayEnd);
        }
    }
}