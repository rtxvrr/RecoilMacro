using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoilMacro.Helpers
{
    public class AudioService
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileOn;
        private AudioFileReader audioFileOff;

        public AudioService()
        {
            InitializeAudio();
        }

        private void InitializeAudio()
        {
            try
            {
                outputDevice = new WaveOutEvent();
                audioFileOn = new AudioFileReader("Source/on.wav");
                audioFileOff = new AudioFileReader("Source/off.wav");
            }
            catch (Exception e)
            {
                Debug.Print(Convert.ToString(e));
                throw;
            }
        }

        public void PlaySound(bool enabled)
        {
            var audioFile = enabled ? audioFileOn : audioFileOff;
            if (outputDevice.PlaybackState == PlaybackState.Playing)
                outputDevice.Stop();
            outputDevice.Init(audioFile);
            audioFile.Position = 0;
            outputDevice.Play();
        }
    }
}
