using Gma.System.MouseKeyHook;
using RecoilMacro.Helpers;
using RecoilMacro.Window;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Preset = RecoilMacro.Model.Preset;

namespace RecoilMacro
{
    public partial class MainWindow : System.Windows.Window
    {
        #region Variubles
        private IKeyboardMouseEvents? _mGlobalHook;
        private bool _isPulling = false;
        private bool _macroEnabled = false;
        private List<Preset> _presets;
        private double _pullIntensity;
        private bool _leftMouseButtonDown = false;
        private bool _rightMouseButtonDown = false;
        private readonly AudioService _audioService;
        private readonly Random _random = new();
        private readonly PresetManager _presetManager;
        private bool ShouldStartPullMouse => MacroEnabled && IsTriggerConditionMet();
        private const int MinMouseMoveDelay = 10;
        private const int MouseMoveRandomFactor = 0;
        private const int MinMouseMoveIntensity = 7;
        private const int MaxMouseMoveIntensity = 15;

        public bool MacroEnabled
        {
            get => _macroEnabled;
            set
            {
                _macroEnabled = value;
                _audioService.PlaySound(_macroEnabled);

            }
        }

        public double PullIntensity
        {
            get => _pullIntensity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Intensity must be > 0");
                _pullIntensity = value;
            }
        }
        #endregion

        #region Window Initialization 
        public MainWindow()
        {
            InitializeComponent();
            UpdateIntensityFromTextbox();
            Subscribe();
            try
            {
                _audioService = new AudioService();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initialize audio system: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _presetManager = new PresetManager();
            Loaded += Window_Loaded;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_audioService == null)
            {
                this.Close();
            }
            _presets = _presetManager.GetPresets();
            PresetComboBox.Items.Add(new Preset { Name = "Default", Intensity = 1.0, BothButtonsRequired = false });
            foreach (var preset in _presets)
            {
                PresetComboBox.Items.Add(preset);
            }
            PresetComboBox.SelectedIndex = 0; // Select "Default"
        }

        #endregion

        #region Event Subscriptions and Unsubscriptions
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Unsubscribe();
            Application.Current.Shutdown();
        }

        private void Unsubscribe()
        {
            _mGlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            _mGlobalHook.MouseUpExt -= GlobalHookMouseUpExt;
            _mGlobalHook.KeyDown -= GlobalHookKeyDown;
            _mGlobalHook.Dispose();
        }
        private void Subscribe()
        {
            _mGlobalHook = Hook.GlobalEvents();
            _mGlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            _mGlobalHook.MouseUpExt += GlobalHookMouseUpExt;
            _mGlobalHook.KeyDown += GlobalHookKeyDown;
        }

        #endregion

        #region GlobalHooks

        private void GlobalHookKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                Dispatcher.Invoke(() =>
                {
                    MacroEnabled = !MacroEnabled;
                    _audioService.PlaySound(MacroEnabled);
                });
            }
        }
        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _leftMouseButtonDown = true;
            else if (e.Button == MouseButtons.Right)
                _rightMouseButtonDown = true;

            if (ShouldStartPullMouse)
            {
                Dispatcher.Invoke(() => lblStatus.Content = "Status: Active");
                StartPullMouse();
            }
        }
        private void GlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _leftMouseButtonDown = false;
            else if (e.Button == MouseButtons.Right)
                _rightMouseButtonDown = false;

            if (_macroEnabled && (_leftMouseButtonDown == false || _rightMouseButtonDown == false))
            {
                Dispatcher.Invoke(() => lblStatus.Content = "Status: Inactive");
                StopPullMouse();
            }
        }
        #endregion

        #region Mouse Control Logic
        private bool IsTriggerConditionMet()
        {
            if (chkBothButtons.IsChecked == true)
            {
                return _leftMouseButtonDown && _rightMouseButtonDown;
            }
            return _leftMouseButtonDown;
        }

        private void StartPullMouse()
        {
            try
            {
                #region WinAPI
                if (chkMethod.IsChecked == true)
                {
                    if (!_isPulling)
                    {
                        _isPulling = true;
                        Task.Run(async () =>
                        {
                            while (_isPulling)
                            {
                                int randomY = _random.Next(MinMouseMoveIntensity, MaxMouseMoveIntensity);
                                MoveMouse(MouseMoveRandomFactor, (int)(PullIntensity * randomY));
                                await Task.Delay(MinMouseMoveDelay + _random.Next(-4, 3));
                            }
                        });
                    }
                }
                #endregion
                #region InputSimulator
                else
                {
                    if (!_isPulling)
                    {
                        _isPulling = true;
                        var inputSimulator = new InputSimulator();
                        Task.Run(async () =>
                        {
                            while (_isPulling)
                            {
                                int randomY = _random.Next(MinMouseMoveIntensity, MaxMouseMoveIntensity);
                                inputSimulator.Mouse.MoveMouseBy(MouseMoveRandomFactor, (int)(PullIntensity * randomY));
                                await Task.Delay(MinMouseMoveDelay + _random.Next(-4, 3));
                            }
                        });
                    }
                }
                #endregion
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error starting pulling mouse: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void StopPullMouse()
        {
            _isPulling = false;
        }

        #endregion

        #region textBox Events

        private void IntensiveTB_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateIntensityFromTextbox();
        }
        private void IntensiveTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
                e.Handled = !IsValidDecimalInput(fullText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error input intensive number: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
        }
        private void IntensiveTB_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (decimal.TryParse(textBox.Text, out decimal number))
            {
                if (textBox.Text.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) == -1)
                {
                    textBox.Text = $"{number}{System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}0";
                }
            }
        }
        private bool IsValidDecimalInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return decimal.TryParse(text, out _);
        }
        private void UpdateIntensityFromTextbox()
        {
            if (double.TryParse(IntensiveTB.Text, out double value))
            {
                PullIntensity = value;
            }
        }
        private void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidDecimalInput(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        #endregion

        #region Presets
        private void PresetComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetComboBox.SelectedItem is Preset selectedPreset)
            {
                PullIntensity = selectedPreset.Intensity;
                IntensiveTB.Text = PullIntensity.ToString();
                chkBothButtons.IsChecked = selectedPreset.BothButtonsRequired;
                chkMethod.IsChecked = selectedPreset.WinApiMethod;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var currentPreset = PresetComboBox.SelectedItem as Preset;
            bool isUpdate = currentPreset != null && _presets.Any(p => p.Name == currentPreset.Name);
            string defaultName = isUpdate ? currentPreset.Name : "New Preset " + (_presets.Count + 1);
            var inputDialog = new PresetWindow("Enter preset name:", defaultName, isUpdate);

            if (inputDialog.ShowDialog() == true)
            {
                string presetName = string.IsNullOrEmpty(inputDialog.Answer) ? defaultName : inputDialog.Answer;

                if (isUpdate)
                {
                    currentPreset.Name = presetName;
                    currentPreset.Intensity = PullIntensity;
                    currentPreset.BothButtonsRequired = (bool)chkBothButtons.IsChecked;
                    currentPreset.WinApiMethod = (bool)chkMethod.IsChecked;
                }
                else
                {
                    var newPreset = new Preset { Name = presetName, Intensity = PullIntensity, BothButtonsRequired = (bool)chkBothButtons.IsChecked, WinApiMethod = (bool)chkMethod.IsChecked };
                    _presets.Add(newPreset);
                    PresetComboBox.Items.Add(newPreset);
                }

                _presetManager.SavePresets();
                PresetComboBox.SelectedItem = isUpdate ? currentPreset : _presets.Last();
            }

        }

        #endregion

        #region WinApi Method
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

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

        const uint INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_MOVE = 0x0001;

        private void MoveMouse(int dx, int dy)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi = new MOUSEINPUT()
            {
                dx = dx,
                dy = dy,
                mouseData = 0,
                dwFlags = MOUSEEVENTF_MOVE,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            SendInput(1, inputs, INPUT.Size);
        }
        #endregion
    }
}