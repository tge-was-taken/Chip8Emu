using System.Diagnostics;
using System.Threading;
using Chip8Emu.Core.Display;
using Chip8Emu.Core.Input;
using Chip8Emu.Core.Sound;
// ReSharper disable InconsistentNaming

namespace Chip8Emu.Core.Cpu
{
    /// <summary>
    /// Represents a Chip8 cpu context.
    /// </summary>
    public class CpuCore
    {
        private readonly IInstructionDecoder mDecoder;

        // Referenced by instruction decoder
        internal ISoundDevice Sound { get; }
        internal IDisplayDevice Display { get; }
        internal IInputDevice Input { get; }

        /// <summary>
        /// General purpose registers.
        /// </summary>
        public byte[] GPR { get; } = new byte[16];
        
        /// <summary>
        /// V0 register.
        /// </summary>
        public byte V0
        {
            get => GPR[0];
            set => GPR[0] = value;
        }

        /// <summary>
        /// V1 register.
        /// </summary>
        public byte V1
        {
            get => GPR[1];
            set => GPR[1] = value;
        }

        /// <summary>
        /// V2 register.
        /// </summary>
        public byte V2
        {
            get => GPR[2];
            set => GPR[2] = value;
        }

        /// <summary>
        /// V3 register.
        /// </summary>
        public byte V3
        {
            get => GPR[3];
            set => GPR[3] = value;
        }

        /// <summary>
        /// V4 register.
        /// </summary>
        public byte V4
        {
            get => GPR[4];
            set => GPR[4] = value;
        }

        /// <summary>
        /// V5 register.
        /// </summary>
        public byte V5
        {
            get => GPR[5];
            set => GPR[5] = value;
        }

        /// <summary>
        /// V6 register.
        /// </summary>
        public byte V6
        {
            get => GPR[6];
            set => GPR[6] = value;
        }

        /// <summary>
        /// V7 register.
        /// </summary>
        public byte V7
        {
            get => GPR[7];
            set => GPR[7] = value;
        }

        /// <summary>
        /// V8 register.
        /// </summary>
        public byte V8
        {
            get => GPR[8];
            set => GPR[8] = value;
        }

        /// <summary>
        /// V9 register.
        /// </summary>
        public byte V9
        {
            get => GPR[9];
            set => GPR[9] = value;
        }

        /// <summary>
        /// VA register.
        /// </summary>
        public byte VA
        {
            get => GPR[0xA];
            set => GPR[0xA] = value;
        }

        /// <summary>
        /// VB register.
        /// </summary>
        public byte VB
        {
            get => GPR[0xB];
            set => GPR[0xB] = value;
        }

        /// <summary>
        /// VC register.
        /// </summary>
        public byte VC
        {
            get => GPR[0xC];
            set => GPR[0xC] = value;
        }

        /// <summary>
        /// VD register.
        /// </summary>
        public byte VD
        {
            get => GPR[0xD];
            set => GPR[0xD] = value;
        }

        /// <summary>
        /// VE register.
        /// </summary>
        public byte VE
        {
            get => GPR[0xE];
            set => GPR[0xE] = value;
        }

        /// <summary>
        /// VF register.
        /// </summary>
        public byte VF
        {
            get => GPR[0xF];
            set => GPR[0xF] = value;
        }

        /// <summary>
        /// Memory address register.
        /// </summary>
        public ushort I { get; set; }

        /// <summary>
        /// Program counter register.
        /// </summary>
        public ushort PC { get; set; }

        /// <summary>
        /// Delay timer register.
        /// </summary>
        public TimerRegister DT { get; set; } = new TimerRegister( ( Constants.TIMER_FREQUENCY_SEC * Stopwatch.Frequency ) );

        /// <summary>
        /// Sound timer register.
        /// </summary>
        public TimerRegister ST { get; set; } = new TimerRegister( ( Constants.TIMER_FREQUENCY_SEC * Stopwatch.Frequency ) );

        /// <summary>
        /// Stack pointer register.
        /// </summary>
        public byte SP { get; set; }

        /// <summary>
        /// Program stack.
        /// </summary>
        public ushort[] Stack { get; } = new ushort[16];

        /// <summary>
        /// Gets if the cpu is paused.
        /// </summary>
        public bool Paused { get; private set; }

        public Memory Memory { get; }

        public CpuCore( Memory memory, IDisplayDevice display, ISoundDevice sound, IInputDevice input, IInstructionDecoder decoder )
        {
            Memory = memory;
            mDecoder = decoder;
            Display = display;
            Sound = sound;
            Input = input;

            ST.Elapsed += ( s, e ) => { Sound.StartBeep(); };
            ST.Stopped += ( s, e ) => { Sound.EndBeep(); };
        }

        private void ExecuteInstruction()
        {
            var data = Memory.ReadUShort( PC );
            var instruction = new Instruction( data );
            if ( mDecoder.Decode( this, instruction ) )
                PC += Instruction.SIZE;
        }

        public void Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var targetDeltaTime = Constants.CPU_FREQUENCY_SEC * Stopwatch.Frequency;
            var startTime = stopwatch.ElapsedTicks;
            while ( true )
            {
                if ( Paused )
                {
                    Thread.Sleep( 100 );
                    continue;
                }

                // Do the things
                Display.OnStartFrame();
                ExecuteInstruction();
                Display.OnFinishFrame();

                // Get cycle end & delta time
                var targetEndTime = startTime + targetDeltaTime;
                var endTime = stopwatch.ElapsedTicks;

                if ( endTime < targetEndTime )
                {
                    // We're ahead of our target time
                    // Slow down until we've met it
                    while ( endTime < targetEndTime )
                    {
                        Thread.SpinWait( 1 );
                        endTime = stopwatch.ElapsedTicks;
                    }
                }

                // Update timers!
                var deltaTime = stopwatch.ElapsedTicks - startTime;
                DT.Update( deltaTime );
                ST.Update( deltaTime );

                startTime = endTime;
            }
        }

        public void Pause()
        {
            Paused = true;
        }
    }
}
