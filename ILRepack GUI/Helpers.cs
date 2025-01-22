using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ILRepack_GUI
{
    public static class Helpers
    {
        #region Assembly ListView Binding

        public class Assembly_Binding : INotifyPropertyChanged
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
                }
            }


            public string DisplayName
            {
                get
                {
                    string[] split = FileName.Split('.');

                    if(Dependencies != null)
                    {
                        if(Dependencies.Count > 0)
                        {
                            return string.Join(".", split.Take(split.Length - 1)) + " ⚠";
                        }
                        else
                        {
                            return string.Join(".", split.Take(split.Length - 1));
                        }
                    }
                    else
                    {
                        return string.Join(".", split.Take(split.Length - 1));
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
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }


            public SolidColorBrush ForegroundColor
            {
                get
                {
                    if(Dependencies != null)
                    {
                        if(Dependencies.Count > 0)
                        {
                            return new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            return new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Black);
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


        /// <summary>
        /// Gets a list of the missing dependency assemblies when attempting to load the assembly from the given <paramref name="assemblyPath"/>.
        /// </summary>
        /// <param name="assemblyPath">The path of the assembly to load.</param>
        /// <returns>A <see cref="List{string}"/> of the names of dependency assemblies that were missing when trying to load the assembly, or 
        /// <see langword="null"/> if something went wrong while loading the assembly.</returns>
        /// <remarks>If no missing dependency assemblies are found, the returned list will be empty.</remarks>
        public static List<string>? GetMissingAssemblies(string assemblyPath)
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

                            missingAssemblies.Add(fileName[0]);
                        }
                    }
                }

                return missingAssemblies;
            }
            catch
            {
                return null;
            }
        }


        public static void DeleteDirectory(string target_dir)
        {
            if (!Directory.Exists(target_dir))
            {
                return;
            }

            File.SetAttributes(target_dir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}
