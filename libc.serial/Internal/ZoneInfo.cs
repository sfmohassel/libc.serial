using System.Collections.Generic;
using System.Globalization;
using NodaTime;

namespace libc.serial.Internal
{
    internal class ZoneInfo : NotifyModel
    {
        private static readonly IDictionary<Calendars, CultureInfo> cultures = new Dictionary<Calendars, CultureInfo>
        {
            {
                Calendars.Gregorian, new CultureInfo("en")
            },
            {
                Calendars.Persian, new CultureInfo("fa")
            }
        };

        private Calendars _Calendar;
        private string _ZoneId;

        public ZoneInfo()
        {
        }

        public ZoneInfo(DateTimeZone zone, CalendarSystem calendarSystem)
        {
            ZoneId = zone.Id;
            Calendar = calendarSystem.GetCalendar();
        }

        public ZoneInfo(string zoneId, Calendars calendar)
        {
            ZoneId = zoneId;
            Calendar = calendar;
        }

        public string ZoneId
        {
            get => _ZoneId;
            set => Set(ref _ZoneId, value, () => ZoneId);
        }

        public Calendars Calendar
        {
            get => _Calendar;
            set => Set(ref _Calendar, value, () => Calendar);
        }

        public DateTimeZone Zone()
        {
            return ZoneId.GetZone();
        }

        public CalendarSystem CalendarSystem()
        {
            return Calendar.GetCalendarSystem();
        }

        public Dat Now()
        {
            return new Dat(SystemClock.Instance.GetCurrentInstant().InZone(Zone(), CalendarSystem()));
        }

        public CultureInfo GetCultureInfo()
        {
            return cultures[Calendar];
        }
    }
}