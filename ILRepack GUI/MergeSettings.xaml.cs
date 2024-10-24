using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ILRepack_GUI
{
    /// <summary>
    /// Interaction logic for MergeSettings.xaml
    /// </summary>
    public partial class MergeSettings : Window
    {
        public bool wasCancelled;

        public string targetKind;

        public MergeSettings()
        {
            InitializeComponent();

            wasCancelled = false;

            targetKind = "";

            Target_Kind_Combobox.SelectedIndex = 0;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            targetKind = Target_Kind_Combobox.Text;

            Close();
        }

        private void Cancel_Button_Click(Object sender, RoutedEventArgs e)
        {
            wasCancelled = true;

            Close();
        }
    }
}
