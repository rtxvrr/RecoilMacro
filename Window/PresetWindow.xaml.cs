using System.Windows;
using System.Windows.Controls;

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
            DataContext = this;
        }
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Focus();
            textBox.SelectAll();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }
            Answer = inputTextBox.Text;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
