using NAudio.Wave;

namespace RecoilMacro.Helpers
{
    public class AudioService
    {
        #region Private Fields

        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileOn;
        private AudioFileReader _audioFileOff;

        #endregion

        #region Constructor

        public AudioService()
        {
            _outputDevice = new WaveOutEvent();
            _audioFileOn = new AudioFileReader("Source/on.wav");
            _audioFileOff = new AudioFileReader("Source/off.wav");
        }

        #endregion

        #region Methods

        public void PlaySound(bool enabled)
        {
            var file = enabled ? _audioFileOn : _audioFileOff;
            if (_outputDevice.PlaybackState == PlaybackState.Playing)
            {
                _outputDevice.Stop();
            }
            _outputDevice.Init(file);
            file.Position = 0;
            _outputDevice.Play();
        }

        #endregion
    }
}
