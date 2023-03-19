namespace libc.serial.RequestReponse
{
  public enum ComRRConnectionStates
  {
    /// <summary>
    ///     Initial State
    /// </summary>
    None,

    /// <summary>
    ///     Message is sent
    /// </summary>
    Sent,

    /// <summary>
    ///     Response is received completely
    /// </summary>
    Done,

    /// <summary>
    ///     Response is received but not completely
    /// </summary>
    DoneButNotComplete,

    /// <summary>
    ///     Timeout while waiting for response
    /// </summary>
    Timedout,

    /// <summary>
    ///     Unknown error
    /// </summary>
    Error
  }
}