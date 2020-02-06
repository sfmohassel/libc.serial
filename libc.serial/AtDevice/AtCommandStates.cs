namespace libc.serial.AtDevice {
    public enum AtCommandStates {
        /// <summary>
        ///     Just a created object
        /// </summary>
        None,
        /// <summary>
        ///     Command started interacting with device
        /// </summary>
        Started,
        /// <summary>
        ///     Finished interacting with device
        /// </summary>
        Completed,
        /// <summary>
        ///     Finished because of a timeout
        /// </summary>
        Timedout,
        Exception
    }
}