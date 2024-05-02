using Gma.System.MouseKeyHook;
using NAudio.Wave;
using Newtonsoft.Json;
using RecoilMacro.Model;
using RecoilMacro.Window;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using Application = System.Windows.Application;

namespace RecoilMacro
{
    public partial class MainWindow : System.Windows.Window
    {
        #region Variubles
        private IKeyboardMouseEvents m_GlobalHook;
        private bool isPulling = false;
        private bool macroEnabled = false;
        private List<Preset> presets;
        private double pullIntensity;
        private bool leftMouseButtonDown = false;
        private bool rightMouseButtonDown = false;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileOn;
        private AudioFileReader audioFileOff;
        #endregion

        #region Window Initialization 
        public MainWindow()
        {
            InitializeComponent();
            UpdateIntensityFromTextbox();
            Subscribe();
            Loaded += Window_Loaded;
            InitializeAudio();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            presets = LoadPresets();
            foreach (var preset in presets)
            {
                PresetComboBox.Items.Add(preset);
            }
            PresetComboBox.Items.Add(new Preset { Name = "Default", Intensity = 1.0, BothButtonsRequired = false });
            PresetComboBox.SelectedIndex = presets.Count; // Select "Default"
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
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.MouseUpExt -= GlobalHookMouseUpExt;
            m_GlobalHook.KeyDown -= GlobalHookKeyDown;
            m_GlobalHook.Dispose();
        }
        private void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.MouseUpExt += GlobalHookMouseUpExt;
            m_GlobalHook.KeyDown += GlobalHookKeyDown;
        }

        #endregion

        #region Sounds
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
        private void PlaySound(AudioFileReader audioFile)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
                outputDevice.Stop();
            outputDevice.Init(audioFile);
            audioFile.Position = 0;
            outputDevice.Play();
        }
        #endregion

        #region GlobalHooks

        private void GlobalHookKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                Dispatcher.Invoke(() =>
                {
                    macroEnabled = !macroEnabled;
                    PlaySound(macroEnabled ? audioFileOn : audioFileOff);
                });
            }
        }
        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                leftMouseButtonDown = true;
            else if (e.Button == MouseButtons.Right)
                rightMouseButtonDown = true;

            if (macroEnabled && IsTriggerConditionMet())
            {
                Dispatcher.Invoke(() => lblStatus.Content = "Status: Active");
                StartPullMouse();
            }
        }
        private void GlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
                leftMouseButtonDown = false;
            else if (e.Button == MouseButtons.Right)
                rightMouseButtonDown = false;

            if (macroEnabled && (leftMouseButtonDown == false || rightMouseButtonDown == false))
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
                return leftMouseButtonDown && rightMouseButtonDown;
            }
            return leftMouseButtonDown;
        }

        private void StartPullMouse()
        {

            #region WinAPI
            if (chkMethod.IsChecked == true)
            {
                if (!isPulling)
                {
                    isPulling = true;
                    Task.Run(async () =>
                    {
                        Random random = new Random();
                        while (isPulling)
                        {
                            int randomX = 0;
                            int randomY = random.Next(7, 15);
                            MoveMouse(randomX, (int)(pullIntensity * randomY));
                            await Task.Delay(10 + random.Next(-4, 3));
                        }
                    });
                }
            }
            #endregion

            #region InputSimulator
            else
            {
                if (!isPulling)
                {
                    isPulling = true;
                    var inputSimulator = new InputSimulator();
                    Task.Run(async () =>
                    {
                        Random random = new Random();
                        while (isPulling)
                        {
                            int randomY = random.Next(7, 15);
                            inputSimulator.Mouse.MoveMouseBy(0, (int)(pullIntensity * randomY));
                            await Task.Delay(10 + random.Next(-4, 3));
                        }
                    });
                }
            }
            #endregion

        }
        private void StopPullMouse()
        {
            isPulling = false;
        }

        #endregion

        #region textBox Events

        private void IntensiveTB_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateIntensityFromTextbox();
        }
        private void IntensiveTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !IsValidDecimalInput(fullText);
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
                pullIntensity = value;
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
                pullIntensity = selectedPreset.Intensity;
                IntensiveTB.Text = pullIntensity.ToString();
                chkBothButtons.IsChecked = selectedPreset.BothButtonsRequired;
                chkMethod.IsChecked = selectedPreset.WinApiMethod;
            }
        }

        public void SavePresets(List<Preset> presets)
        {
            string filePath = "presets.json";
            string json = JsonConvert.SerializeObject(presets, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<Preset> LoadPresets()
        {
            string filePath = "presets.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Preset>>(json) ?? new List<Preset>();
            }
            return new List<Preset>();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var currentPreset = PresetComboBox.SelectedItem as Preset;
            bool isUpdate = currentPreset != null && presets.Any(p => p.Name == currentPreset.Name);
            string defaultName = isUpdate ? currentPreset.Name : "New Preset " + (presets.Count + 1);
            var inputDialog = new PresetWindow("Enter preset name:", defaultName, isUpdate);

            if (inputDialog.ShowDialog() == true)
            {
                string presetName = string.IsNullOrEmpty(inputDialog.Answer) ? defaultName : inputDialog.Answer;

                if (isUpdate)
                {
                    currentPreset.Name = presetName;
                    currentPreset.Intensity = pullIntensity;
                    currentPreset.BothButtonsRequired = (bool)chkBothButtons.IsChecked;
                    currentPreset.WinApiMethod = (bool)chkMethod.IsChecked;
                }
                else
                {
                    var newPreset = new Preset { Name = presetName, Intensity = pullIntensity, BothButtonsRequired = (bool)chkBothButtons.IsChecked, WinApiMethod = (bool)chkMethod.IsChecked };
                    presets.Add(newPreset);
                    PresetComboBox.Items.Add(newPreset);
                }

                SavePresets(presets);
                PresetComboBox.SelectedItem = isUpdate ? currentPreset : presets.Last();
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