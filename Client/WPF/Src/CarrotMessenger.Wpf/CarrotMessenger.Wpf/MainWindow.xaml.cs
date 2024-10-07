using CarrotMessenger.Wpf.ViewModels;
using System.Windows;

namespace CarrotMessenger.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;
            ChatList.DataContext = ViewModel;
        }
    }
}