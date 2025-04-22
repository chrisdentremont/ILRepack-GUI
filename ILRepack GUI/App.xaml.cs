using ILRepack_GUI.Windows;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ILRepack_GUI 
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
