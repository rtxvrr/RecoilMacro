using Application = System.Windows.Application;

namespace RecoilMacro
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsWindowHidden))
                {
                    if (vm.IsWindowHidden) Hide(); else Show();
                }
            };
        }
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is MainViewModel vm) vm.Cleanup();
            Application.Current.Shutdown();
        }
    }
}
