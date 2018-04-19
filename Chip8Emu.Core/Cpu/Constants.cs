namespace Chip8Emu.Core.Cpu
{
    public static class Constants
    {
        public const int INSTRUCTION_COUNT = 35;
        public const int FONT_GLYPH_BYTE_SIZE = 5;
        public const int CPU_FREQUENCY_HZ = 730;
        public const float CPU_FREQUENCY_SEC = ( 1f / CPU_FREQUENCY_HZ );
        public const int TIMER_FREQUENCY_HZ = 60;
        public const float TIMER_FREQUENCY_SEC = ( 1f / TIMER_FREQUENCY_HZ );
    }
}
