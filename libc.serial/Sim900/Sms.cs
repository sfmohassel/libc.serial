namespace libc.serial.Sim900
{
    public class Sms
    {
        public string Mobile { get; set; }
        public string Text { get; set; }
        public int InboxIndex { get; set; }
        public SmsFlags Flag { get; set; }
        public long SendDateTicks { get; set; }
    }
}