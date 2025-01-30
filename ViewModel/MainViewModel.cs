using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using RecoilMacro.Converters;
using RecoilMacro.Helpers;
using RecoilMacro.Model;
using RecoilMacro.Services;
using RecoilMacro.ViewModel;
using RecoilMacro.Window;

namespace RecoilMacro
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        #region Private Fields

        private readonly HookService _hookService;
        private readonly MacroService _macroService;
        private readonly PresetManager _presetManager;
        private readonly AudioService _audioService;
        private bool _macroEnabled;
        private bool _disableSound;
        private bool _bothButtonsRequired;
        private bool _useVirtualDriver;
        private bool _loadedDll;
        private bool _isWindowHidden;
        private double _pullIntensity;
        private double _incrementalStep;
        private string _statusMessage;
        private string _dllStatus = "DLL not loaded";
        private Brush _dllStatusColor = Brushes.Red;
        private ObservableCollection<Preset> _presets;
        private Preset _selectedPreset;
        private bool _leftMouseDown;
        private bool _rightMouseDown;
        private bool _isSettingHotkey1;
        private bool _isSettingHotkey2;
        private string _hotkey1 = "None";
        private Preset _hotkey1Preset;
        private string _hotkey2 = "None";
        private Preset _hotkey2Preset;

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

        public bool DisableSound
        {
            get => _disableSound;
            set
            {
                _disableSound = value;
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
                UpdateMacroConfig();
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

        public double PullIntensity
        {
            get => _pullIntensity;
            set
            {
                _pullIntensity = value;
                OnPropertyChanged();
                UpdateMacroConfig();
            }
        }

        public double IncrementalStep
        {
            get => _incrementalStep;
            set
            {
                _incrementalStep = value;
                OnPropertyChanged();
                UpdateMacroConfig();
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

        public string Hotkey1
        {
            get => _hotkey1;
            set
            {
                _hotkey1 = value;
                OnPropertyChanged();
            }
        }

        public Preset Hotkey1Preset
        {
            get => _hotkey1Preset;
            set
            {
                _hotkey1Preset = value;
                OnPropertyChanged();
            }
        }

        public string Hotkey2
        {
            get => _hotkey2;
            set
            {
                _hotkey2 = value;
                OnPropertyChanged();
            }
        }

        public Preset Hotkey2Preset
        {
            get => _hotkey2Preset;
            set
            {
                _hotkey2Preset = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SavePresetCommand { get; }
        public ICommand LoadDllCommand { get; }
        public ICommand StartSetHotkey1Command { get; }
        public ICommand StartSetHotkey2Command { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            StatusMessage = "Status: Waiting...";
            _audioService = new AudioService();
            _presetManager = new PresetManager();
            var dd = new CDD();
            _macroService = new MacroService(_loadedDll, dd);
            _hookService = new HookService();
            _hookService.KeyDown += OnKeyDown;
            _hookService.MouseDownExt += OnMouseDownExt;
            _hookService.MouseUpExt += OnMouseUpExt;
            _hookService.Start();
            SavePresetCommand = new RelayCommand(SavePreset);
            LoadDllCommand = new RelayCommand(LoadDllFile);
            StartSetHotkey1Command = new RelayCommand(_ => StartSetHotkey1());
            StartSetHotkey2Command = new RelayCommand(_ => StartSetHotkey2());
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
        }

        #endregion

        #region Update Macro

        private void UpdateMacroConfig()
        {
            _macroService.UpdateConfig(
                baseIntensity: PullIntensity,
                step: IncrementalStep,
                useVirtualDriver: UseVirtualDriver,
                loadedDll: _loadedDll
            );
        }

        #endregion

        #region Preset Logic

        private void SavePreset(object obj)
        {
            if (SelectedPreset == null)
            {
                MessageBox.Show("No preset selected.");
                return;
            }
            bool isDefault = (SelectedPreset.Name == "Default");
            bool isUpdate = !isDefault;
            string suggestedName = isUpdate ? SelectedPreset.Name : GetNextAutoName();
            var windowVm = new PresetViewModel(
                "Enter preset name:",
                suggestedName,
                isUpdate,
                _presetManager,
                isDefault ? null : SelectedPreset
            );
            var presetWindow = new PresetWindow(windowVm);
            bool? result = presetWindow.ShowDialog();
            if (result != true) return;
            var freshList = _presetManager.GetPresets();
            Presets = new ObservableCollection<Preset>(freshList);
            var changedPreset = Presets
                .FirstOrDefault(p => p.Name.Equals(windowVm.InputText, StringComparison.OrdinalIgnoreCase));

            if (changedPreset == null) return;
            changedPreset.Intensity = PullIntensity;
            changedPreset.IncrementalStep = IncrementalStep;
            changedPreset.BothButtonsRequired = BothButtonsRequired;
            changedPreset.VirtualDriverMethod = UseVirtualDriver;
            _presetManager.SavePresets();
            SelectedPreset = changedPreset;
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
                .Where(x => x > 0)
                .ToList();

            int next = existingNums.Any() ? existingNums.Max() + 1 : 1;
            return $"Custom Preset {next}";
        }

        #endregion

        #region DLL

        private void LoadDllFile(object obj)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "DD|*.DLL"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            var dd = _macroService.Cdd;

            int ret = dd.Load(ofd.FileName);
            if (ret != 1)
            {
                MessageBox.Show("Load Error");
                return;
            }
            ret = dd.btn(0);
            if (ret != 1)
            {
                MessageBox.Show("Initialize Error");
                return;
            }
            DllStatus = "DLL Loaded!";
            DllStatusColor = Brushes.Green;
            _loadedDll = true;
            UpdateMacroConfig();
        }

        #endregion

        #region Hotkeys

        private void StartSetHotkey1()
        {
            Hotkey1 = "Press key...";
            _isSettingHotkey1 = true;
            _isSettingHotkey2 = false;
        }

        private void StartSetHotkey2()
        {
            Hotkey2 = "Press key...";
            _isSettingHotkey2 = true;
            _isSettingHotkey1 = false;
        }

        private bool IsHotkeyMatch(string configured, string pressed)
        {
            return configured.Equals(pressed, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Hook Events

        private void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (_isSettingHotkey1)
            {
                e.Handled = true;
                Hotkey1 = e.KeyCode.ToString();
                _isSettingHotkey1 = false;
                return;
            }
            if (_isSettingHotkey2)
            {
                e.Handled = true;
                Hotkey2 = e.KeyCode.ToString();
                _isSettingHotkey2 = false;
                return;
            }

            if (IsHotkeyMatch(Hotkey1, e.KeyCode.ToString()) && Hotkey1Preset != null)
            {
                SelectedPreset = Hotkey1Preset;
            }
            else if (IsHotkeyMatch(Hotkey2, e.KeyCode.ToString()) && Hotkey2Preset != null)
            {
                SelectedPreset = Hotkey2Preset;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.F3)
            {
                MacroEnabled = !MacroEnabled;
            }
            if (e.KeyCode == System.Windows.Forms.Keys.F6)
            {
                IsWindowHidden = !IsWindowHidden;
            }
        }

        private void OnMouseDownExt(MouseEventExtArgs e)
        {
            if (_isSettingHotkey1)
            {
                e.Handled = true;
                Hotkey1 = e.Button.ToString();
                _isSettingHotkey1 = false;
                return;
            }
            if (_isSettingHotkey2)
            {
                e.Handled = true;
                Hotkey2 = e.Button.ToString();
                _isSettingHotkey2 = false;
                return;
            }

            if (IsHotkeyMatch(Hotkey1, e.Button.ToString()) && Hotkey1Preset != null)
            {
                SelectedPreset = Hotkey1Preset;
            }
            else if (IsHotkeyMatch(Hotkey2, e.Button.ToString()) && Hotkey2Preset != null)
            {
                SelectedPreset = Hotkey2Preset;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _leftMouseDown = true;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _rightMouseDown = true;
            }
            if (ShouldStartPullMouse())
            {
                _macroService.StartPull();
            }
        }

        private void OnMouseUpExt(MouseEventExtArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _leftMouseDown = false;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _rightMouseDown = false;
            }
            if (_macroEnabled && (!_leftMouseDown || !_rightMouseDown))
            {
                _macroService.StopPull();
            }
        }

        private bool ShouldStartPullMouse()
        {
            if (DoubleValidationRule.HasAnyErrors) return false;
            if (!_macroEnabled) return false;
            if (_bothButtonsRequired)
            {
                return _leftMouseDown && _rightMouseDown;
            }
            return _leftMouseDown;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _hookService.Dispose();
            _macroService.StopPull();
        }

        #endregion
    }
}
