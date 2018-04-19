namespace Chip8Emu.Core.Display
{
    public interface IDisplayDevice
    {
        void OnStartFrame();

        void OnFinishFrame();

        void Clear();

        bool DrawSprite(byte baseX, byte baseY, byte[] data);
    }
}
