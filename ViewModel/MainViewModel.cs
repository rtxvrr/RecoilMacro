using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Gma.System.MouseKeyHook;
using RecoilMacro.Converters;
using RecoilMacro.Helpers;
using RecoilMacro.Model;
using RecoilMacro.ViewModel;
using RecoilMacro.Window;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace RecoilMacro
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        #region Private Fields

        private bool _macroEnabled;
        private bool _isPulling;
        private bool _leftMouseButtonDown;
        private bool _rightMouseButtonDown;
        private bool _loadedDll;
        private bool _isWindowHidden;
        private bool _disableSound;
        private bool _bothButtonsRequired;
        private bool _useVirtualDriver;

        private double _pullIntensity;
        private double _incrementalStep;
        private double _currentIntensity;

        private string _statusMessage;
        private string _dllStatus = "DLL not loaded";

        private Brush _dllStatusColor = Brushes.Red;

        private IKeyboardMouseEvents _globalHook;
        private readonly AudioService _audioService;
        private readonly PresetManager _presetManager;
        private readonly Random _random = new();
        private CDD _dd;
        private ObservableCollection<Preset> _presets;
        private Preset _selectedPreset;
        private Stopwatch _stopwatch;

        #endregion

        #region Properties

        public bool MacroEnabled
        {
            get => _macroEnabled;
            set
            {
                _macroEnabled = value;
                OnPropertyChanged();

                if (!_disableSound)
                {
                    _audioService.PlaySound(_macroEnabled);
                }

                StatusMessage = _macroEnabled ? "Status: Active" : "Status: Inactive";
            }
        }

        public double PullIntensity
        {
            get => _pullIntensity;
            set
            {
                _pullIntensity = value;
                OnPropertyChanged();
            }
        }

        public double IncrementalStep
        {
            get => _incrementalStep;
            set
            {
                _incrementalStep = value;
                OnPropertyChanged();
            }
        }

        public bool BothButtonsRequired
        {
            get => _bothButtonsRequired;
            set
            {
                _bothButtonsRequired = value;
                OnPropertyChanged();
            }
        }

        public bool UseVirtualDriver
        {
            get => _useVirtualDriver;
            set
            {
                _useVirtualDriver = value;
                OnPropertyChanged();
            }
        }

        public bool DisableSound
        {
            get => _disableSound;
            set
            {
                _disableSound = value;
                OnPropertyChanged();
            }
        }

        public bool IsWindowHidden
        {
            get => _isWindowHidden;
            set
            {
                _isWindowHidden = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string DllStatus
        {
            get => _dllStatus;
            set
            {
                _dllStatus = value;
                OnPropertyChanged();
            }
        }

        public Brush DllStatusColor
        {
            get => _dllStatusColor;
            set
            {
                _dllStatusColor = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Preset> Presets
        {
            get => _presets;
            set
            {
                _presets = value;
                OnPropertyChanged();
            }
        }

        public Preset SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                _selectedPreset = value;
                OnPropertyChanged();

                if (_selectedPreset != null)
                {
                    PullIntensity = _selectedPreset.Intensity;
                    IncrementalStep = _selectedPreset.IncrementalStep;
                    BothButtonsRequired = _selectedPreset.BothButtonsRequired;
                    UseVirtualDriver = _selectedPreset.VirtualDriverMethod;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SavePresetCommand { get; }
        public ICommand LoadDllCommand { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _statusMessage = "Status: Waiting...";

            SavePresetCommand = new RelayCommand(SavePreset);
            LoadDllCommand = new RelayCommand(LoadDllFile);

            _presetManager = new PresetManager();
            _audioService = new AudioService();

            PullIntensity = 1.0;
            IncrementalStep = 0.1;
            BothButtonsRequired = false;
            UseVirtualDriver = false;
            DisableSound = false;

            var loadedPresets = _presetManager.GetPresets();
            _presets = new ObservableCollection<Preset>(loadedPresets);

            if (!_presets.Any(x => x.Name == "Default"))
            {
                _presets.Insert(0, new Preset
                {
                    Name = "Default",
                    Intensity = 1.0,
                    IncrementalStep = 0.1,
                    VirtualDriverMethod = false,
                    BothButtonsRequired = false
                });
            }

            SelectedPreset = _presets.FirstOrDefault();
            SubscribeHooks();
        }

        #endregion

        #region Preset Methods

        private void SavePreset(object obj)
        {
            if (_selectedPreset == null)
            {
                MessageBox.Show("No preset selected.");
                return;
            }

            bool isDefault = (_selectedPreset.Name == "Default");
            bool isUpdate = !isDefault;
            string defaultName = isUpdate ? _selectedPreset.Name : GetNextAutoName();

            var windowVm = new PresetViewModel(
                "Enter preset name:",
                defaultName,
                isUpdate,
                _presetManager,
                isDefault ? null : _selectedPreset
            );

            var presetWindow = new PresetWindow(windowVm);
            bool? result = presetWindow.ShowDialog();
            if (result == true)
            {
                var all = _presetManager.GetPresets();
                Presets = new ObservableCollection<Preset>(all);

                if (isDefault)
                {
                    var newPreset = _presets.FirstOrDefault(p =>
                        p.Name.Equals(windowVm.InputText, StringComparison.OrdinalIgnoreCase));
                    if (newPreset != null)
                    {
                        newPreset.Intensity = PullIntensity;
                        newPreset.IncrementalStep = IncrementalStep;
                        newPreset.BothButtonsRequired = BothButtonsRequired;
                        newPreset.VirtualDriverMethod = UseVirtualDriver;
                        _presetManager.SavePresets();
                    }
                }

                var found = _presets.FirstOrDefault(x =>
                    x.Name.Equals(windowVm.InputText, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    SelectedPreset = found;
                }
            }
        }

        private string GetNextAutoName()
        {
            var existingNums = _presets
                .Where(p => p.Name.StartsWith("Custom Preset ", StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    string suffix = p.Name.Substring("Custom Preset ".Length).Trim();
                    return int.TryParse(suffix, out int num) ? num : -1;
                })
                .Where(num => num > 0)
                .ToList();

            int nextNumber = existingNums.Any() ? existingNums.Max() + 1 : 1;
            return $"Custom Preset {nextNumber}";
        }

        #endregion

        #region DLL Methods

        private void LoadDllFile(object obj)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DD|*.DLL"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            _dd = new CDD();
            int ret = _dd.Load(ofd.FileName);
            if (ret != 1)
            {
                MessageBox.Show("Load Error");
                return;
            }
            ret = _dd.btn(0);
            if (ret != 1)
            {
                MessageBox.Show("Initialize Error");
                return;
            }
            DllStatus = "DLL Loaded!";
            DllStatusColor = Brushes.Green;
            _loadedDll = true;
        }

        #endregion

        #region Hooks

        private void SubscribeHooks()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseDownExt += GlobalHookMouseDownExt;
            _globalHook.MouseUpExt += GlobalHookMouseUpExt;
            _globalHook.KeyDown += GlobalHookKeyDown;
        }

        private void UnsubscribeHooks()
        {
            if (_globalHook == null) return;

            _globalHook.MouseDownExt -= GlobalHookMouseDownExt;
            _globalHook.MouseUpExt -= GlobalHookMouseUpExt;
            _globalHook.KeyDown -= GlobalHookKeyDown;
            _globalHook.Dispose();
        }

        private void GlobalHookKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                Application.Current.Dispatcher.Invoke(() => MacroEnabled = !MacroEnabled);
            }
            if (e.KeyCode == Keys.F6)
            {
                Application.Current.Dispatcher.Invoke(() => IsWindowHidden = !IsWindowHidden);
            }
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _leftMouseButtonDown = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _rightMouseButtonDown = true;
            }
            if (ShouldStartPullMouse())
            {
                StartPullMouse();
            }
        }

        private void GlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _leftMouseButtonDown = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _rightMouseButtonDown = false;
            }
            if (MacroEnabled && (!_leftMouseButtonDown || !_rightMouseButtonDown))
            {
                StopPullMouse();
            }
        }

        #endregion

        #region Macro Logic

        private bool ShouldStartPullMouse()
        {
            if (DoubleValidationRule.HasAnyErrors)
            {
                return false;
            }
            if (!MacroEnabled)
            {
                return false;
            }
            if (_bothButtonsRequired)
            {
                return _leftMouseButtonDown && _rightMouseButtonDown;
            }
            return _leftMouseButtonDown;
        }

        private void StartPullMouse()
        {
            if (_isPulling) return;

            _isPulling = true;
            _currentIntensity = _pullIntensity;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            Task.Run(async () =>
            {
                while (_isPulling)
                {
                    long ms = _stopwatch.ElapsedMilliseconds;
                    _currentIntensity = _pullIntensity + (_incrementalStep * ms / 1000.0);
                    int randomY = _random.Next(7, 15);

                    if (!_useVirtualDriver)
                    {
                        MoveMouse(0, (int)(_currentIntensity * randomY));
                    }
                    else
                    {
                        if (!_loadedDll)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                MessageBox.Show("DLL not loaded", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                            _useVirtualDriver = false;
                            StopPullMouse();
                            break;
                        }
                        _dd.movR(0, (int)(_currentIntensity * randomY));
                    }
                    await Task.Delay(10 + _random.Next(-4, 3));
                }
                _stopwatch.Stop();
            });
        }

        private void StopPullMouse()
        {
            _isPulling = false;
            Application.Current.Dispatcher.Invoke(() => StatusMessage = "Status: Inactive");
        }

        #endregion

        #region Cleanup/Dispose

        public void Cleanup()
        {
            UnsubscribeHooks();
        }

        public void Dispose()
        {
            Cleanup();
        }

        #endregion

        #region WinAPI

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        private void MoveMouse(int dx, int dy)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = 0;
            inputs[0].U.mi = new MOUSEINPUT
            {
                dx = dx,
                dy = dy,
                mouseData = 0,
                dwFlags = 0x0001,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };
            SendInput(1, inputs, INPUT.Size);
        }

        public struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }
}
