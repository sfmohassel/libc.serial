namespace libc.serial.RequestReponse
{
    public enum ComRRConnectionStates
    {
        /// <summary>
        ///     وضعیت اولیه
        /// </summary>
        None,

        /// <summary>
        ///     ارسال انجام شده است
        /// </summary>
        Sent,

        /// <summary>
        ///     پاسخ بطور کامل دریافت شده است
        /// </summary>
        Done,

        /// <summary>
        ///     پاسخ بطور ناقص دریافت شده است
        /// </summary>
        DoneButNotComplete,

        /// <summary>
        ///     عدم دریافت پاسخ در زمان مقتضی
        /// </summary>
        Timedout,

        /// <summary>
        ///     خطای غیر قابل پیش بینی رخ داده است
        /// </summary>
        Error
    }
}