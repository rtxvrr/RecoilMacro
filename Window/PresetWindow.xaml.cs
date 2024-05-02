using System.Windows;

namespace RecoilMacro.Window
{
    public partial class PresetWindow : System.Windows.Window
    {
        public string Answer { get; private set; }
        public bool IsUpdate { get; set; }

        public PresetWindow(string question, string defaultAnswer = "", bool isUpdate = false)
        {
            InitializeComponent();
            Title = question;
            inputTextBox.Text = defaultAnswer;
            inputTextBox.SelectAll();
            inputTextBox.Focus();
            IsUpdate = isUpdate;
            SaveChangesButton.Visibility = isUpdate ? Visibility.Visible : Visibility.Collapsed;
            SaveButton.Visibility = !isUpdate ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = inputTextBox.Text;
            this.DialogResult = true;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = inputTextBox.Text;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
