using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRepack_GUI
{
    public static class Helpers
    {
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


            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
