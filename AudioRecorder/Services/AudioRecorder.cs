using System;
using AudioRecorder.Audio;
using NAudio.Wave;

namespace AudioRecorder.Services
{
    public class AudioRecorder
    {
        private readonly SampleAggregator sampleAggregator;

        private WaveIn waveIn;
        private WaveFileWriter writer;
        private WaveFormat recordingFormat;

        public event EventHandler SendAudio = delegate { };

        private string waveFileName;

        public AudioRecorder()
        {
            this.sampleAggregator = new SampleAggregator();
            this.recordingFormat = new WaveFormat(16000, 1);
            this.sampleAggregator.NotificationCount = this.recordingFormat.SampleRate / 10;
        }

        public void BeginMonitoring()
        {
            this.waveIn = new WaveIn();
            this.waveIn.DeviceNumber = 0;
            this.waveIn.DataAvailable += onDataAvailable;
            this.waveIn.RecordingStopped += onRecordingStopped;
            this.waveIn.WaveFormat = this.recordingFormat;
        }

        private void onRecordingStopped(object sender, StoppedEventArgs e)
        {
            this.writer.Dispose();

            // temporary put here
            SendAudio(this, EventArgs.Empty);
        }

        public void BeginRecording(string waveFileName)
        {
            this.waveIn.StartRecording();
            this.waveFileName = waveFileName;
            this.writer = new WaveFileWriter(waveFileName, recordingFormat);
        }

        public void Stop()
        {
            this.waveIn.StopRecording();
            // MessageBox.Show($"{waveFileName}");
        }

        private void onDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            WriteToFile(buffer, bytesRecorded);


            // this part for displaying wave
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short) ((buffer[i + 1] << 8) | buffer[i + 0]);
                float sample32 = sample / 32768f;
                this.sampleAggregator.Add(sample32);
            }
        }

        private void WriteToFile(byte[] buffer, int bytesRecorded)
        {
            long maxFileLength = this.recordingFormat.AverageBytesPerSecond * 60;

            var toWrite = (int) Math.Min(maxFileLength - this.writer.Length, bytesRecorded);

            if (toWrite > 0)
            {
                this.writer.Write(buffer, 0, bytesRecorded);
            }
            else
            {
                Stop();
            }
        }

        public SampleAggregator SampleAggregator => this.sampleAggregator;
    }
}