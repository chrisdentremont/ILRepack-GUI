using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using static ILRepack_GUI.Helpers;

namespace ILRepack_GUI.Windows
{
    /// <summary>
    /// Interaction logic for ResolvePathWindow.xaml
    /// </summary>
    public partial class ResolvePathWindow : Window
    {
        public List<Path_Binding> pathBindings;


        public ResolvePathWindow(List<string> resolvePaths)
        {
            InitializeComponent();

            pathBindings = new List<Path_Binding>();

            foreach(string path in resolvePaths)
            {
                pathBindings.Add(new Path_Binding() { Path = path });
            }

            Path_ListView.ItemsSource = pathBindings;
        }


        private void Add_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            string enteredPath = Path_TextBox.Text;

            //Check if path is already entered
            bool pathExists = false;

            foreach(Path_Binding binding in pathBindings)
            {
                if(binding.Path == Path_TextBox.Text)
                {
                    pathExists = true;
                }
            }

            if (!pathExists)
            {
                if (Directory.Exists(enteredPath))
                {
                    pathBindings.Add(new Path_Binding() { Path = enteredPath });

                    Path_ListView.ItemsSource = null;

                    Path_ListView.ItemsSource = pathBindings;

                    Path_TextBox.Text = "Add a path...";
                }
                else
                {
                    string text = "Could not find the entered path.";
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string text = "That path is already in the list.";
                MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Remove_Path_Click(object sender, RoutedEventArgs e)
        {
            Path_Binding bindingToDelete = (e.Source as Button).DataContext as Path_Binding;

            pathBindings.Remove(bindingToDelete);

            Path_ListView.ItemsSource = null;

            Path_ListView.ItemsSource = pathBindings;
        }


        private void Path_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(Path_TextBox.Text))
                {
                    string enteredPath = Path_TextBox.Text;

                    //Check if path is already entered
                    bool pathExists = false;

                    foreach (Path_Binding binding in pathBindings)
                    {
                        if (binding.Path == Path_TextBox.Text)
                        {
                            pathExists = true;
                        }
                    }

                    if (!pathExists)
                    {
                        if (Directory.Exists(enteredPath))
                        {
                            Path.GetFullPath(enteredPath);
                            pathBindings.Add(new Path_Binding() { Path = enteredPath });

                            Path_ListView.ItemsSource = null;

                            Path_ListView.ItemsSource = pathBindings;

                            Path_TextBox.Text = "";
                        }
                        else
                        {
                            string text = "Could not find the entered path.";
                            MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        string text = "That path is already in the list.";
                        MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void Path_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Path_TextBox.Text == "Add a path...")
            {
                Path_TextBox.Text = "";
            }
        }


        private void Path_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Path_TextBox.Text))
            {
                Path_TextBox.Text = "Add a path...";
            }
        }


        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
