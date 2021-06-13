namespace libc.serial.RequestReponse
{
    public enum ComRRConnectionErrors
    {
        None,
        Exception,
        InvalidState,
        InvalidResponseLenght,
        InvalidResponseHeader,
        InvalidResponseChecksum
    }
}