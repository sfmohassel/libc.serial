namespace libc.serial.Internal
{
    internal interface IUpdatable
    {
        void UpdateFrom(IUpdatable o);
    }
}