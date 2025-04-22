using System.Windows;

namespace ILRepack_GUI.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(string header)
        {
            InitializeComponent();

            Progress_Bar.IsIndeterminate = true;

            Header_Text.Text = header;
        }
    }
}
