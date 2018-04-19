using System.Runtime.InteropServices;
using System.Threading;

namespace Chip8Emu.Core.Sound.Backends.Windows
{
    public class WindowsBeepSoundDevice : ISoundDevice
    {
        private readonly Thread mThread;
        private readonly ManualResetEventSlim mEvent = new ManualResetEventSlim();

        public WindowsBeepSoundDevice()
        {
            mThread = new Thread( ThreadMain ) { Name = "WindowsBeepSoundDeviceThread" };
            mThread.Start();
        }

        public void StartBeep()
        {
            if (!mEvent.IsSet)
                mEvent.Set();
        }

        public void EndBeep()
        {
            // Reset the event state, so we block the beep thread
            mEvent.Reset();
        }

        private void ThreadMain()
        {
            while ( true )
            {
                // Wait until we have stuff to do
                mEvent.Wait();

                // Beep
                Beep( 800, 60 );
            }
        }

        [DllImport( "kernel32.dll" )]
        private static extern bool Beep( int freq, int duration );
    }
}
