using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using RecoilMacro.Helpers;

namespace RecoilMacro.Services
{
    public class MacroService
    {
        private bool _isPulling;
        private double _currentIntensity;
        private Stopwatch _stopwatch;
        private readonly Random _random = new();

        private bool _useVirtualDriver;
        private bool _loadedDll;

        private double _baseIntensity;
        private double _incrementalStep;

        private readonly CDD _dd;

        public event Action<double> OnIntensityChanged;

        public bool IsPulling => _isPulling;
        public double CurrentIntensity => _currentIntensity;
        public CDD Cdd => _dd;

        public MacroService(bool loadedDll, CDD dd)
        {
            _loadedDll = loadedDll;
            _dd = dd;
        }

        public void UpdateConfig(double baseIntensity, double step, bool useVirtualDriver, bool loadedDll)
        {
            _baseIntensity = baseIntensity;
            _incrementalStep = step;
            _useVirtualDriver = useVirtualDriver;
            _loadedDll = loadedDll;
        }

        public void StartPull()
        {
            if (_isPulling) return;

            _isPulling = true;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _currentIntensity = _baseIntensity;

            Task.Run(async () =>
            {
                while (_isPulling)
                {
                    long ms = _stopwatch.ElapsedMilliseconds;
                    _currentIntensity = _baseIntensity + (_incrementalStep * ms / 1000.0);

                    OnIntensityChanged?.Invoke(_currentIntensity);

                    int randomY = _random.Next(7, 15);

                    if (!_useVirtualDriver)
                    {
                        WinApiHelper.MoveMouse(0, (int)(_currentIntensity * randomY));
                    }
                    else
                    {
                        if (!_loadedDll)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.MessageBox.Show("DLL not loaded", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                            _useVirtualDriver = false;
                            StopPull();
                            break;
                        }
                        _dd.movR(0, (int)(_currentIntensity * randomY));
                    }
                    await Task.Delay(10 + _random.Next(-4, 3));
                }
                _stopwatch.Stop();
            });
        }

        public void StopPull()
        {
            _isPulling = false;
            _stopwatch?.Stop();
        }
    }
}
