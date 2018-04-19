namespace Chip8Emu.Core.Input
{
    public interface IInputDevice
    {
        bool IsAnyKeyDown();

        bool IsKeyDown( Key key );

        Key WaitUntilKeyDown();
    }
}