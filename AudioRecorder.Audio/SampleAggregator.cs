using System;
using System.Diagnostics;

namespace AudioRecorder.Audio
{
    public class SampleAggregator
    {
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        public event EventHandler Restart = delegate { };
        private float maxValue;
        private float minValue;
        public int NotificationCount { get; set; }
        private int count;

        public void RaiseRestart()
        {
            Restart(this, EventArgs.Empty);
        }

        private void Reset()
        {
            this.count = 0;
            this.maxValue = this.minValue = 0;
        }

        public void Add(float value)
        {
            this.maxValue = Math.Max(this.maxValue, value);
            this.minValue = Math.Min(this.minValue, value);
            this.count++;

            if (this.count >= this.NotificationCount && this.NotificationCount > 0)
            {
                MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(this.minValue, this.maxValue));
                this.Reset();
            }
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            this.MaxSample = maxValue;
            this.MinSample = minValue;
        }

        public float MaxSample { get; set; }
        public float MinSample { get; set; }
    }
}