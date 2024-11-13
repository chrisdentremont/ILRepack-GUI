using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using static ILRepack_GUI.Helpers;

using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Reflection;


namespace ILRepack_GUI 
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string mainAssemblyPath;

        public ObservableCollection<Assembly_Binding> assemblyBindings;

        string keyfilePath;

        public string targetKind;

        //TODO: Fix dependency assembly issue
        //TODO: Move help icon to bottom of window

        public MainWindow()
        {
            InitializeComponent();

            mainAssemblyPath = "";

            keyfilePath = "";

            assemblyBindings = new ObservableCollection<Assembly_Binding>();

            Other_Assembly_ListView.ItemsSource = assemblyBindings;

            Target_Kind_Combobox.SelectedIndex = 0;

            Merge_Button.IsEnabled = false;
        }


        private void Main_Assembly_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Assemblies (.dll, .exe)|*.dll;*.exe",
                Multiselect = false,
                Title = "Select main assembly"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                mainAssemblyPath = openFileDialog.FileName;

                Main_Assembly_Text_Display.Text = mainAssemblyPath.Substring(mainAssemblyPath.LastIndexOf('\\') + 1);

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyPath != "";
            }
        }


        private void Main_Assembly_File_Dropped(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if(files.Length > 1)
                {
                    string text = "Only one file can be provided.";
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    string fileExt = files[0].Split('.').Last();

                    if(fileExt == "dll" || fileExt == "exe")
                    {
                        mainAssemblyPath = files[0];

                        Main_Assembly_Text_Display.Text = mainAssemblyPath.Substring(mainAssemblyPath.LastIndexOf('\\') + 1);
                    }
                    else
                    {
                        string text = "The file is not an assembly (.dll or .exe).";
                        MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyPath != "";
            }
        }


        private void Other_Files_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Assemblies (.dll, .exe)|*.dll;*.exe",
                Title = "Select assemblies to merge",
                Multiselect = true
            };

            bool? result = openFileDialog.ShowDialog();

            if(result == true)
            {
                string invalidFiles = "";
                foreach(string file in openFileDialog.FileNames)
                {
                    if (file == mainAssemblyPath)
                    {
                        invalidFiles += file.Substring(file.LastIndexOf('\\') + 1) + " is already uploaded as the main assembly.\n";
                    }
                    else
                    {
                        bool fileExists = false;

                        foreach (Assembly_Binding assembly in assemblyBindings)
                        {
                            if (assembly.FileName == file.Substring(file.LastIndexOf("\\") + 1))
                            {
                                fileExists = true;

                                invalidFiles += $"{assembly.FileName} is already in the list.\n";
                            }
                        }

                        if (!fileExists)
                        {
                            assemblyBindings.Add(new Assembly_Binding()
                            {
                                Path = file,
                                FileName = file.Substring(file.LastIndexOf("\\") + 1),
                                FileSize = GetFileSize(File.ReadAllBytes(file))
                            });
                        }
                    }
                }

                if(invalidFiles != "")
                {
                    MessageBox.Show(invalidFiles, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyPath != "";
            }
        }


        private void Other_Assembly_ListView_Dropped(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                string invalidFiles = "";
                for(int i = 0; i < files.Length; i++)
                {
                    string fileExt = files[i].Split('.').Last();

                    if(fileExt == "dll" || fileExt == "exe")
                    {
                        if (files[i] == mainAssemblyPath)
                        {
                            invalidFiles += files[i].Substring(files[i].LastIndexOf('\\') + 1) + " is already uploaded as the main assembly.\n";
                        }
                        else
                        {
                            bool fileExists = false;

                            foreach (Assembly_Binding assembly in assemblyBindings)
                            {
                                if (assembly.FileName == files[i].Substring(files[i].LastIndexOf("\\") + 1))
                                {
                                    fileExists = true;

                                    invalidFiles += $"{assembly.FileName} is already in the list.\n";
                                }
                            }

                            if (!fileExists)
                            {
                                assemblyBindings.Add(new Assembly_Binding()
                                {
                                    Path = files[i],
                                    FileName = files[i].Substring(files[i].LastIndexOf("\\") + 1),
                                    FileSize = GetFileSize(File.ReadAllBytes(files[i]))
                                });
                            }
                        }
                    }
                    else
                    {
                        invalidFiles += files[i].Substring(files[i].LastIndexOf('\\') + 1) + " is not a valid assembly.\n";
                    }
                }

                if(invalidFiles != "")
                {
                    MessageBox.Show(invalidFiles, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyPath != "";
            }
        }


        private void Merge_Button_Click(object sender, RoutedEventArgs e)
        {
            if (assemblyBindings.Count == 0)
            {
                MessageBox.Show("No assemblies are selected to merge!", "EMD Assembly Merger", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            targetKind = Target_Kind_Combobox.Text;

            string fileSaveLocation = "";

            string saveDialogFilter = targetKind == "library" ? "DLL (*.dll)|*.dll" : "EXE (*.exe)|*.exe";
            string saveDialogExt = targetKind == "library" ? ".dll" : ".exe";
            string mainAssemblyFileName = mainAssemblyPath.Substring(mainAssemblyPath.LastIndexOf('\\') + 1);

            //Get save location
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                Title = "Select location to save merged assembly",
                Filter = saveDialogFilter,
                FileName = mainAssemblyFileName.Split('.')[0] + saveDialogExt
            };

            bool? result = saveDialog.ShowDialog();

            if (result == true)
            {
                fileSaveLocation = saveDialog.FileName;
            }
            else
            {
                return;
            }

            #region Build Arguments

            string mergeArguments = $"/out:\"{fileSaveLocation}\"";

            //Target kind
            mergeArguments += $" /target:\"{targetKind}\"";

            //TODO: Figure out why this isn't working
            //Include debug file
            if(Settings_Check_Debug_File.IsChecked != true)
            {
                mergeArguments += " /ndebug";
            }

            //Merge identical types
            if(Settings_Check_Merge_Types.IsChecked == true)
            {
                mergeArguments += " /union";
            }

            //Merge XML docs
            if(Settings_Check_XML_Doc.IsChecked == true)
            {
                mergeArguments += " /xmldocs";
            }

            //Rename internalized types
            if(Settings_Check_Rename_Int.IsChecked == true)
            {
                mergeArguments += " /renameInternalized";
            }

            //Don't internalize Serializable
            if(Settings_Check_Int_Ser.IsChecked == true)
            {
                mergeArguments += " /excludeinternalizeserializable";
            }

            //Sign output with keyfile
            if(Settings_Check_Sign_Key.IsChecked == true)
            {
                mergeArguments += $" /keyfile:\"{keyfilePath}\"";
            }

            foreach(Assembly_Binding assembly in assemblyBindings)
            {
                mergeArguments += $" \"{assembly.Path}\""; 
            }

            #endregion Build Arguments


            Process mergeProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C ilrepack {mergeArguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true 
                }
            };

            mergeProcess.Start();

            string stan = mergeProcess.StandardOutput.ReadToEnd();
            string err = mergeProcess.StandardError.ReadToEnd();

            mergeProcess.WaitForExit();

            if (!string.IsNullOrEmpty(err))
            {
                Directory.CreateDirectory("crash-reports");

                string date = DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_');

                File.WriteAllText(@"crash-reports\crash_report_" + date + ".txt", err);

                string text = "Something went wrong while merging the assemblies! Check the crash report for details.";
                MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string text = "The assemblies were merged!";
                MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            #region Reset UI

            assemblyBindings = new ObservableCollection<Assembly_Binding>();

            Other_Assembly_ListView.ItemsSource = assemblyBindings;

            Main_Assembly_Text_Display.Text = "";

            Settings_Check_Debug_File.IsChecked = false;
            Settings_Check_Merge_Types.IsChecked = false;
            Settings_Check_XML_Doc.IsChecked = false;
            Settings_Check_Rename_Int.IsChecked = false;
            Settings_Check_Int_Ser.IsChecked = false;
            Settings_Check_Parallel.IsChecked = false;
            Settings_Check_Sign_Key.IsChecked = false;

            #endregion Reset UI
        }

        private void Remove_Assemblies_MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            if(Other_Assembly_ListView.SelectedItems.Count == 0)
            {
                Remove_Assemblies_MenuItem.IsEnabled = false;
            }
            else
            {
                Remove_Assemblies_MenuItem.IsEnabled = true;
            }
        }

        private void Remove_Assemblies_Click(object sender, RoutedEventArgs e)
        {
            List<Assembly_Binding> selectedBindings = new List<Assembly_Binding>(); 

            foreach(Assembly_Binding assembly in Other_Assembly_ListView.SelectedItems)
            {
                selectedBindings.Add(assembly);
            }


            foreach(Assembly_Binding assembly in selectedBindings)
            {
                if(assemblyBindings.Contains(assembly))
                {
                    assemblyBindings.Remove(assembly);
                }
            }

            Other_Assembly_ListView.ItemsSource = assemblyBindings;
        }

        private void Info_Window_Button_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow window = new InfoWindow();
            window.Show();
        }


        private void Sign_Key_Checked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select keyfile to sign output assembly"
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                keyfilePath = dialog.FileName;
            }
            else
            {
                Settings_Check_Sign_Key.IsChecked = false;
            }
        }


        private void Sign_Key_Unchecked(object sender, RoutedEventArgs e)
        {
            keyfilePath = "";
        }

        public static string GetFileSize(byte[] contents)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = contents.Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }

    #region ListView Width Converters

    public class WidthConverterName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double listViewWidth && parameter is string columnCountStr &&
                        int.TryParse(columnCountStr, out int columnCount))
            {
                return (listViewWidth * .70) - 10;  // Adjust for padding/margin
            }
            return 100;  // Default width if binding fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class WidthConverterSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double listViewWidth && parameter is string columnCountStr &&
                        int.TryParse(columnCountStr, out int columnCount))
            {
                return (listViewWidth * .30) - 10;  // Adjust for padding/margin
            }
            return 100;  // Default width if binding fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion ListView Width Converters
}