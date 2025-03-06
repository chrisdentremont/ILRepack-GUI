using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using static ILRepack_GUI.Helpers;

using Microsoft.Win32;


namespace ILRepack_GUI 
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string mainAssemblyFile;

        public ObservableCollection<Assembly_Binding> assemblyBindings;

        string keyfilePath;

        public string targetKind;

        public string tempFolderLocation;

        public List<string> resolvePaths;

        //TODO: Fix dependency assembly issue

        public MainWindow()
        {
            InitializeComponent();
            
            mainAssemblyFile = "";

            keyfilePath = "";

            assemblyBindings = new ObservableCollection<Assembly_Binding>();

            resolvePaths = new List<string>();

            Other_Assembly_TreeView.ItemsSource = assemblyBindings;

            Target_Kind_Combobox.SelectedIndex = 0;

            Merge_Button.IsEnabled = false;

            tempFolderLocation = Directory.GetCurrentDirectory() + "\\ILRepack_GUI_temp";

            if (Directory.Exists(tempFolderLocation))
            {
                Directory.Delete(tempFolderLocation, true);
            }

            DirectoryInfo info = Directory.CreateDirectory(tempFolderLocation);

            info.Attributes = FileAttributes.Normal;


            //Test to make sure ILRepack is installed
            Process testProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C ilrepack",
                    WorkingDirectory = tempFolderLocation,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            testProcess.Start();

            string err = testProcess.StandardError.ReadToEnd();

            testProcess.WaitForExit();

            if (!string.IsNullOrEmpty(err))
            {
                if(err.Contains("is not recognized"))
                {
                    string text = "ILRepack is not installed on this machine! Run the following command in the terminal to install it:\n\ndotnet tool " +
                        "install -g dotnet-ilrepack\n\nOnce you have run this, open this program again.";
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);

                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    string text = "An unknown error has occured: " + err;
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);

                    Process.GetCurrentProcess().Kill();
                }
            }

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            if (Directory.Exists(tempFolderLocation))
            {
                Directory.Delete(tempFolderLocation, true);
            }
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
                string fileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);

                bool assemblyInOther = false;

                //Check if file is in other assemblies
                foreach (Assembly_Binding assembly in assemblyBindings)
                {
                    if (fileName == assembly.FileName)
                    {
                        assemblyInOther = true;
                    }
                }

                if (!assemblyInOther)
                {
                    mainAssemblyFile = fileName;

                    Main_Assembly_Text_Display.Text = fileName;

                    using (FileStream stream = File.Create(tempFolderLocation + $"\\{fileName}"))
                    {
                        byte[] contents = File.ReadAllBytes(openFileDialog.FileName);

                        stream.Write(contents, 0, contents.Length);
                    }

                    CheckMissingAssemblies();

                    Main_Assembly_Text_Display.Text = mainAssemblyFile;

                    //TODO: Change requirements for this
                    Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyFile != "";
                }
                else
                {
                    string text = "This assembly already exists in the list of other assemblies. Please delete it first if you want to " +
                        "make it the main assembly.";
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void Main_Assembly_File_Dropped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 1)
                {
                    string text = "Only one file can be provided.";
                    MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //Delete old temp file
                    if (mainAssemblyFile != "")
                    {
                        string oldFileName = mainAssemblyFile;

                        if (File.Exists(tempFolderLocation + $"\\{oldFileName}"))
                        {
                            File.Delete(tempFolderLocation + $"\\{oldFileName}");
                        }
                    }

                    string fileExt = files[0].Split('.').Last();

                    if (fileExt == "dll" || fileExt == "exe")
                    {
                        string fileName = files[0].Substring(files[0].LastIndexOf('\\') + 1);

                        bool assemblyInOther = false;

                        //Check if file is in other assemblies
                        foreach (Assembly_Binding assembly in assemblyBindings)
                        {
                            if (fileName == assembly.FileName)
                            {
                                assemblyInOther = true;
                            }
                        }

                        if (!assemblyInOther)
                        {
                            mainAssemblyFile = fileName;

                            Main_Assembly_Text_Display.Text = fileName;

                            using (FileStream stream = File.Create(tempFolderLocation + $"\\{fileName}"))
                            {
                                byte[] contents = File.ReadAllBytes(files[0]);

                                stream.Write(contents, 0, contents.Length);
                            }

                            CheckMissingAssemblies();
                        }
                        else
                        {
                            string text = "This assembly already exists in the list of other assemblies. Please delete it first if you want to " +
                                "make it the main assembly.";
                            MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        string text = "The file is not an assembly (.dll or .exe).";
                        MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyFile != "";
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
                    string fileName = file.Substring(file.LastIndexOf("\\") + 1);

                    if (fileName == mainAssemblyFile)
                    {
                        invalidFiles += fileName + " is already uploaded as the main assembly.\n";
                    }
                    else
                    {
                        bool fileAlreadyExists = false;

                        foreach (Assembly_Binding assembly in assemblyBindings)
                        {
                            if (assembly.FileName == fileName)
                            {
                                fileAlreadyExists = true;

                                invalidFiles += $"{assembly.FileName} is already in the list.\n";
                            }
                        }

                        if (!fileAlreadyExists)
                        {
                            string newFileLocation = tempFolderLocation + $"\\{fileName}";

                            assemblyBindings.Add(new Assembly_Binding()
                            {
                                OriginalPath = file,
                                Path = newFileLocation,
                                FileName = fileName,
                                FileSize = GetFileSize(File.ReadAllBytes(file)),
                            });

                            using (FileStream stream = File.Create(newFileLocation))
                            {
                                byte[] contents = File.ReadAllBytes(file);

                                stream.Write(contents, 0, contents.Length);
                            }

                            CheckMissingAssemblies();
                        }
                    }
                }

                if(invalidFiles != "")
                {
                    MessageBox.Show(invalidFiles, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyFile != "";
            }
        }

        private void Other_Assembly_TreeView_Dropped(object sender, DragEventArgs e)
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
                        string fileName = files[i].Substring(files[i].LastIndexOf('\\') + 1);

                        if (fileName == mainAssemblyFile)
                        {
                            invalidFiles += fileName + " is already uploaded as the main assembly.\n";
                        }
                        else
                        {
                            bool fileAlreadyExists = false;

                            foreach (Assembly_Binding assembly in assemblyBindings)
                            {
                                if (assembly.FileName == fileName)
                                {
                                    fileAlreadyExists = true;

                                    invalidFiles += $"{assembly.FileName} is already in the list.\n";
                                }
                            }

                            if (!fileAlreadyExists)
                            {
                                string newFileLocation = tempFolderLocation + $"\\{fileName}";

                                assemblyBindings.Add(new Assembly_Binding()
                                {
                                    OriginalPath = files[i],
                                    Path = newFileLocation,
                                    FileName = fileName,
                                    FileSize = GetFileSize(File.ReadAllBytes(files[i])),
                                });

                                using (FileStream stream = File.Create(newFileLocation))
                                {
                                    byte[] contents = File.ReadAllBytes(files[i]);

                                    stream.Write(contents, 0, contents.Length);
                                }

                                CheckMissingAssemblies();
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

                //TODO: Change 
                Merge_Button.IsEnabled = assemblyBindings.Count != 0 && mainAssemblyFile != "";
            }
        }
        

        private void Remove_Assemblies_Click(object sender, RoutedEventArgs e)
        {
            Assembly_Binding? selectedBinding = ((Button)e.OriginalSource).DataContext as Assembly_Binding;

            if(selectedBinding != null)
            {
                if (assemblyBindings.Contains(selectedBinding))
                {
                    assemblyBindings.Remove(selectedBinding);

                    if (File.Exists(tempFolderLocation + $"\\{selectedBinding.FileName}"))
                    {
                        File.Delete(tempFolderLocation + $"\\{selectedBinding.FileName}");
                    }
                }
            }

            CheckMissingAssemblies();
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
            string mainAssemblyFileName = mainAssemblyFile.Substring(mainAssemblyFile.LastIndexOf('\\') + 1);

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

            //Resolve paths
            foreach (string path in resolvePaths)
            {
                mergeArguments += $" /lib:\"{path}\"";
            }

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

            //Add main assembly first
            mergeArguments += $" \"{mainAssemblyFile}\"";

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
                    WorkingDirectory = tempFolderLocation,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            mergeProcess.Start();

            string stan = mergeProcess.StandardOutput.ReadToEnd();
            string err = mergeProcess.StandardError.ReadToEnd();

            mergeProcess.WaitForExit();

            bool hasError = false;

            if (!string.IsNullOrEmpty(err))
            {
                Directory.CreateDirectory("crash-reports");

                string date = DateTime.Now.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_');

                File.WriteAllText(@"crash-reports\crash_report_" + date + ".txt", err);

                string text = "Something went wrong while merging the assemblies! Check the crash report for details.";
                MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Error);

                hasError = true;
            }
            else
            {
                string text = "The assemblies were merged!";
                MessageBox.Show(text, "ILRepack GUI", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            #region Reset UI

            if (!hasError)
            {
                //Empty temp folder
                DirectoryInfo info = new DirectoryInfo(tempFolderLocation);

                foreach(FileInfo file in info.GetFiles())
                {
                    file.Delete();
                }

                foreach(DirectoryInfo dir in info.GetDirectories())
                {
                    dir.Delete(true);
                }


                assemblyBindings = new ObservableCollection<Assembly_Binding>();

                Other_Assembly_TreeView.ItemsSource = assemblyBindings;

                Main_Assembly_Text_Display.Text = "Drag and drop a file here...";

                Settings_Check_Debug_File.IsChecked = false;
                Settings_Check_Merge_Types.IsChecked = false;
                Settings_Check_XML_Doc.IsChecked = false;
                Settings_Check_Rename_Int.IsChecked = false;
                Settings_Check_Int_Ser.IsChecked = false;
                Settings_Check_Parallel.IsChecked = false;
                Settings_Check_Sign_Key.IsChecked = false;
            }

            #endregion Reset UI
        }


        private void Info_Window_Button_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow window = new InfoWindow();
            window.Show();
        }


        private void Add_Resolve_Paths_Click(object sender, RoutedEventArgs e)
        {
            ResolvePathWindow window = new ResolvePathWindow(resolvePaths);
            window.ShowDialog();

            resolvePaths = new List<string>();

            //Update resolve paths
            foreach(Path_Binding binding in window.pathBindings)
            {
                if (Directory.Exists(binding.Path))
                {
                    resolvePaths.Add(binding.Path);
                }
            }

            CheckMissingAssemblies();
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

        public string GetFileSize(byte[] contents)
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


        public void CheckMissingAssemblies()
        {
            foreach(Assembly_Binding binding in assemblyBindings)
            {
                List<string>? missingDeps = GetMissingAssemblies(binding.OriginalPath, tempFolderLocation);

                List<string>? stillMissingDeps = missingDeps;

                if(missingDeps != null)
                {
                    //Check if the dependency is in one of the resolve paths
                    foreach(string dependency in missingDeps)
                    {
                        foreach (string path in resolvePaths)
                        {
                            string[] filesInResolvePath = Directory.GetFiles(path, "*.dll");

                            for (int i = 0; i < filesInResolvePath.Length; i++)
                            {
                                if (filesInResolvePath.Contains(dependency))
                                {
                                    stillMissingDeps?.Remove(dependency);
                                }
                            }
                        }
                    }
                }

                binding.Dependencies = stillMissingDeps;
            }

            Other_Assembly_TreeView.ItemsSource = assemblyBindings;
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