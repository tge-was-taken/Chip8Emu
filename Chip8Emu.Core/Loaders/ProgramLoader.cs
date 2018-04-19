using System.IO;
using Chip8Emu.Core.Cpu;

namespace Chip8Emu.Core.Loaders
{
    internal class ProgramLoader
    {
        private readonly Memory mMemory;
        private readonly CpuCore mCpu;

        public ProgramLoader( CpuCore cpu )
        {
            mMemory = cpu.Memory;
            mCpu = cpu;
        }

        public void Load( string path )
        {
            var bytes = File.ReadAllBytes( path );
            mMemory.WriteBytes( 0x200, bytes );
            mCpu.PC = 0x200;
        }
    }
}
