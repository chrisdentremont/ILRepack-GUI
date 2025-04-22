using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace ILRepack_GUI
{
    public static class Helpers
    {
        #region Assembly ListView Binding

        public class Assembly_Binding : INotifyPropertyChanged
        {
            private string originalPath;

            public string OriginalPath
            {
                get
                {
                    return originalPath;
                }
                set
                {
                    originalPath = value;

                    OnPropertyChanged("OriginalPath");
                }
            }


            private string path;

            public string Path
            {
                get
                {
                    return path;
                }
                set
                {
                    path = value;

                    OnPropertyChanged("Path");
                }
            }


            private string fileName;

            public string FileName
            {
                get
                {
                    return fileName;
                }
                set
                {
                    fileName = value;

                    OnPropertyChanged("FileName");
                }
            }


            private string fileSize;

            public string FileSize
            {
                get
                {
                    return fileSize;
                }
                set
                {
                    fileSize = value;

                    OnPropertyChanged("FileSize");
                }
            }


            private List<string>? dependencies;

            public List<string>? Dependencies
            {
                get
                {
                    return dependencies;
                }
                set
                {
                    dependencies = value;

                    OnPropertyChanged("Dependencies");

                    OnPropertyChanged("DependencyText");

                    OnPropertyChanged("ToolTip");
                }
            }


            public string DisplayName
            {
                get
                {
                    string[] split = FileName.Split('.');

                    return string.Join(".", split.Take(split.Length - 1));
                }
            }


            public string DependencyText
            {
                get
                {
                    if(Dependencies != null)
                    {
                        if(Dependencies.Count > 0)
                        {
                            return $"\t ⚠ Missing dependencies";
                        }
                        else
                        {
                            return " ";
                        }
                    }
                    else
                    {
                        return " ";
                    }
                }
            }


            public string? ToolTip
            {
                get
                {
                    if(Dependencies != null)
                    {
                        if(Dependencies.Count > 0)
                        {
                            return $"Missing {Dependencies.Count} dependencies";
                        }
                        else
                        {
                            return "No missing dependencies";
                        }
                    }
                    else
                    {
                        return "No missing dependencies";
                    }
                }
            }


            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Assembly ListView Binding


        #region Path Binding

        public class Path_Binding : INotifyPropertyChanged
        {
            private string path;

            public string Path
            {
                get
                {
                    return path;
                }
                set
                {
                    path = value;

                    OnPropertyChanged("Path");
                }
            }


            public SolidColorBrush CanFindPath
            {
                get
                {
                    if (Directory.Exists(path))
                    {
                        return new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Red);
                    }
                }
            }


            public string ToolTip
            {
                get
                {
                    if (Directory.Exists(path))
                    {
                        return "";
                    }
                    else
                    {
                        return "Could not find this path.";
                    }
                }
            }


            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Path Binding


        /// <summary>
        /// Gets a list of the missing dependency assemblies when attempting to load the assembly from the given <paramref name="assemblyPath"/>.
        /// </summary>
        /// <param name="assemblyPath">The path of the assembly to load.</param>
        /// <param name="tempFolderPath">The path of the temp folder to check for missing assemblies.</param>
        /// <returns>A <see cref="List{string}"/> of the names of dependency assemblies that were missing when trying to load the assembly, or 
        /// <see langword="null"/> if something went wrong while loading the assembly.</returns>
        /// <remarks>If no missing dependency assemblies are found, the returned list will be empty.</remarks>
        public static List<string>? GetMissingAssemblies(string assemblyPath, string tempFolderPath)
        {
            try
            {
                Assembly ass = Assembly.LoadFrom(assemblyPath);

                AssemblyName[] names = ass.GetReferencedAssemblies();

                List<string> missingAssemblies = new List<string>();

                for (int i = 0; i < names.Length; i++)
                {
                    try
                    {
                        Assembly.Load(names[i]);
                    }
                    catch (FileNotFoundException e)
                    {
                        if (e.FileName != null)
                        {
                            string[] fileName = e.FileName.Split(',');

                            //Check if it's in the temp folder

                            bool hasTempFile = false;

                            List<string> tempFiles = Directory.EnumerateFiles(tempFolderPath).ToList();

                            foreach(string file in tempFiles)
                            {
                                string tempFileName = Path.GetFileNameWithoutExtension(file);

                                if (tempFileName != null && tempFileName == fileName[0])
                                {
                                    hasTempFile = true;
                                }
                            }

                            if (!hasTempFile)
                            {
                                missingAssemblies.Add(fileName[0]);
                            }
                        }
                    }
                }

                return missingAssemblies;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
