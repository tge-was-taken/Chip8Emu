using System;

namespace Chip8Emu.Core.Cpu
{
    /// <summary>
    /// Represents a special purpose register used as a counter.
    /// </summary>
    public class TimerRegister
    {
        public byte Value { get; set; }

        public float Interval { get; set; }

        public float Remaining { get; set; }

        public bool IsActive => Value != 0;

        public event EventHandler Elapsed;

        public event EventHandler Stopped;

        /// <summary>
        /// Creates a new instance of a timer register.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="interval">Interval in ticks</param>
        public TimerRegister( float interval )
        {
            Value = 0;
            Interval = interval;
            Remaining = 0;
        }

        public void Update( long deltaTime )
        {
            // Nothing to do
            if ( !IsActive )
                return;

            // Decrement remaining time
            if ( Remaining > 0 )
                Remaining -= deltaTime;

            if ( Remaining <= 0 )
            {
                // No more remaining time for this slice
                --Value;
                Elapsed?.Invoke( this, EventArgs.Empty );

                if ( Value != 0 )
                {
                    // Subtract current remaining time from specified interval because it can be negative
                    Remaining = Interval - Remaining;
                }
                else
                {
                    // We're done!
                    Stopped?.Invoke( this, EventArgs.Empty );
                    Remaining = 0;
                }
            }
        }

        // For convienience
        public static implicit operator byte(TimerRegister register)
        {
            return register.Value;
        }
    }
}