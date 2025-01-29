namespace RecoilMacro.Window
{
    public partial class PresetWindow : System.Windows.Window
    {
        public PresetViewModel VM { get; }

        public PresetWindow(PresetViewModel viewModel)
        {
            InitializeComponent();
            VM = viewModel;
            DataContext = VM;
            VM.RequestClose += OnRequestClose;
        }

        void OnRequestClose(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
