using System.Windows;
using System.Windows.Input;
using RecoilMacro.Helpers;
using RecoilMacro.Model;
using RecoilMacro.ViewModel;

namespace RecoilMacro.Window
{
    public class PresetViewModel : ViewModelBase
    {
        #region Private Fields

        private string _windowTitle;
        private string _inputText;
        private bool _isUpdate;
        private readonly PresetManager _presetManager;
        private readonly Preset _originalPreset;

        #endregion

        #region Events

        public event Action<bool?> RequestClose;

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Constructor

        public PresetViewModel(
            string question,
            string defaultAnswer,
            bool isUpdateFlag,
            PresetManager manager,
            Preset editingPreset
        )
        {
            _windowTitle = question;
            _inputText = defaultAnswer;
            _isUpdate = isUpdateFlag;
            _presetManager = manager;
            _originalPreset = editingPreset;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        #endregion

        #region Properties

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public bool IsUpdate
        {
            get => _isUpdate;
            set
            {
                _isUpdate = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }

            var allPresets = _presetManager.GetPresets();
            bool nameAlreadyUsed = allPresets.Any(p =>
                p.Name.Equals(InputText, StringComparison.OrdinalIgnoreCase)
                && p != _originalPreset);

            if (nameAlreadyUsed)
            {
                MessageBox.Show("This preset name already exists.");
                return;
            }

            if (IsUpdate && _originalPreset != null)
            {
                if (_originalPreset.Name == "Default")
                {
                    MessageBox.Show("Default preset cannot be modified.");
                    return;
                }

                _originalPreset.Name = InputText;
                _presetManager.SavePresets();
            }
            else
            {
                var newPreset = new Preset
                {
                    Name = InputText,
                    Intensity = 1.0,
                    IncrementalStep = 0.1,
                    BothButtonsRequired = false,
                    VirtualDriverMethod = false
                };
                allPresets.Add(newPreset);
                _presetManager.SavePresets();
            }

            RequestClose?.Invoke(true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        #endregion
    }
}
