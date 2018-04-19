using Chip8Emu.Core.Cpu;
using Chip8Emu.Core.Loaders;
using Chip8Emu.Core.Cpu.Decoders;
using Chip8Emu.Core.Display;
using Chip8Emu.Core.Display.Backends.Console;
using Chip8Emu.Core.Input;
using Chip8Emu.Core.Input.Backends.WindowsKeyboard;
using Chip8Emu.Core.Sound;
using Chip8Emu.Core.Sound.Backends.Windows;

namespace Chip8Emu.Core
{
    public class Emulator
    {
        public Memory Memory { get; }

        public IDisplayDevice Display { get; }

        public ISoundDevice Sound { get; }

        public IInputDevice Input { get; }

        public CpuCore Cpu { get; }

        public Emulator( EmulatorConfig config )
        {
            Memory = new Memory();
            Display = new ConsoleDisplayDevice();
            Sound = new WindowsBeepSoundDevice();
            Input = new WindowsKeyboardInputDevice();
            Cpu = new CpuCore( Memory, Display, Sound, Input, new InstructionInterpreter() );
        }

        public void LoadProgram( string path )
        {
            var programLoader = new ProgramLoader( Cpu );
            programLoader.Load( path );
        }

        public void Run()
        {
            Cpu.Run();
        }
    }
}
