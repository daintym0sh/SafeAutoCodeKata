using DriverUtility; 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SafeAutoCodeKata
{
    class MainWindowContext : Context
    {
        private string fileName;
        public string FileName
        {
            get
            {
                if (fileName != null && fileName.Contains('\\'))
                {
                    List<string> path = fileName.Split('\\').ToList();
                    return path.Last().Substring(0, path.Last().Length);
                }
                return fileName;
            }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private DelegateCommand selectFileCmd;
        public ICommand SelectFileCmd
        {
            get
            {
                if (selectFileCmd == null)
                {
                    selectFileCmd = new DelegateCommand(new Action(SelectFile));
                }
                return selectFileCmd;
            }
        }

        public void SelectFile()
        {
            FileName = SelectFile("Text File (*.txt) | *.txt");
        }

        public string SelectFile(string filterValue)
        {
            string text = null;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = filterValue;
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                text = openFileDialog.FileName;
            }
            return text;
        }

        private DelegateCommand createReportCmd;
        public ICommand CreateReportCmd
        {
            get
            {
                if (createReportCmd == null)
                {
                    createReportCmd = new DelegateCommand(new Action(CreateReport));
                }
                return createReportCmd;
            }
        }

        public void CreateReport()
        {
            if (!(fileName == null))
            {
                string outputPath = Environment.CurrentDirectory;

                Boolean success = Reporting.createReport(fileName, outputPath);

                if (success)
                {
                    Process.Start(outputPath);
                }
                else
                {
                    MessageBox.Show("Unable to create report.");
                }
            }
        }
    }
}
