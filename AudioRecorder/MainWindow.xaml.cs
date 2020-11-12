using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using AudioRecorder.Annotations;
using AudioRecorder.Audio;
using Path = System.IO.Path;

namespace AudioRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Services.AudioRecorder audioRecorder;
        public bool RecordState { get; set; } = false;
        private float lastPeak;
        private string fileName;
        private string uriApi = "http://192.168.16.46:5555/recognize";

        public MainWindow()
        {
            this.DataContext = this;
            this.audioRecorder = new Services.AudioRecorder();
            this.audioRecorder.SendAudio += (sender, args) => SendAudio();
            InitializeComponent();
            InitializeVoiceRecorder();
        }

        public void InitializeVoiceRecorder()
        {
            this.audioRecorder.SampleAggregator.MaximumCalculated += (sender, args) =>
                this.lastPeak = Math.Max(args.MaxSample, Math.Abs(args.MinSample));
            this.audioRecorder.BeginMonitoring();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Stop recording
            if (RecordState == true)
            {
                this.RecordState = false;
                this.audioRecorder.Stop();
                this.recordBtn.Content = "Record";
            }
            // Begin recording
            else
            {
                this.RecordState = true;
                this.fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + ".wav");
                this.audioRecorder.BeginRecording(fileName);
                OnPropertyChanged("RecordState");
                this.recordBtn.Content = "Stop";
            }
        }

        private async void SendAudio()
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent("--audio");
            var fn = Path.GetFileName(this.fileName);
            var audioByte = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(this.fileName));
            //content.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            content.Add(audioByte, "audio", fn);
            using var message = await client.PostAsync(this.uriApi, content);
            // var input = await message.Content.ReadAsStringAsync();

            if (message.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("Uploaded!");
            }
            else
            {
                MessageBox.Show("Something went wrong...");
            }
        }

        public float CurrentInputLevel => this.lastPeak * 100;

        public SampleAggregator SampleAggregator => this.audioRecorder.SampleAggregator;


        #region WPF Stuff

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}