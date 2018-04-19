namespace Chip8Emu.Core.Cpu
{
    public interface IInstructionDecoder
    {
        bool Decode( CpuCore context, Instruction instruction );
    }
}
