namespace AudioRecorder.Core
{
    public interface IWaveFormRenderer
    {
        void AddValue(float maxValue, float minValue);
    }
}