using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetailFileMp3
{
    public partial class Form1 : Form
    {
        List<string> lstFileNames = new List<string>();
        List<string> lstExtension = new List<string>() { "mp3", "flac", "m4a" };

        public Form1()
        {
            InitializeComponent();
            //string fileName= @"E:\Musics\You And You Only Chipmunk.mp3";
            //TagLib.File file = TagLib.File.Create(fileName);
            //file.Tag.Title = Path.GetFileNameWithoutExtension(fileName);
            //file.Save();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            lstFileNames = new List<string>();
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                if (File.GetAttributes(path) == FileAttributes.Directory)
                {
                    string[] fileNames = Directory.GetFiles(path);
                    string[] pathNames = Directory.GetDirectories(path);
                    foreach (string pathName in pathNames)
                    {
                        if (Directory.Exists(pathName))
                            GetFiles(pathName);
                    }
                    foreach (string fileName in fileNames)
                    {
                        string extension = Path.GetExtension(fileName).TrimStart('.');
                        if (File.Exists(fileName) && lstExtension.Any(x => x.Equals(extension.ToLower())))
                            lstFileNames.Add(fileName);
                    }
                }
            }
            lstFiles.Items.AddRange(lstFileNames.ToArray());
            lbTotal.Text = lstFileNames.Count.ToString();

            ChangeTitle();
        }
        private void GetFiles(string path)
        {
            string[] fileNames = Directory.GetFiles(path);
            string[] pathNames = Directory.GetDirectories(path);
            foreach (string pathName in pathNames)
            {
                if (Directory.Exists(pathName))
                    GetFiles(pathName);
            }
            foreach (string fileName in fileNames)
            {
                string extension = Path.GetExtension(fileName).TrimStart('.');
                if (File.Exists(fileName) && lstExtension.Any(x => x.Equals(extension.ToLower())))
                    lstFileNames.Add(fileName);
            }
        }

        private void ChangeTitle()
        {
            BackgroundWorker bWorker = new BackgroundWorker() { WorkerReportsProgress = true };
            Timer timer = new Timer() { Interval = 250 };
            bool IsComplete = false;
            int Total = lstFileNames.Count;
            int Current = 0;
            pgPercent.Value = 0;

            timer.Tick += (sender, e) =>
            {
                if (!IsComplete)
                {
                    bWorker.ReportProgress(Convert.ToInt32(((Current * 1.0f) / Total) * 100));
                }
            };
            bWorker.DoWork += (sender, e) =>
            {
                foreach (string fileName in lstFileNames)
                {
                    TagLib.File file = TagLib.File.Create(fileName);
                    file.Tag.Title = Path.GetFileNameWithoutExtension(fileName);
                    file.Save();
                    Current++;
                }
                IsComplete = true;
            };
            bWorker.ProgressChanged += (sender, e) =>
            {
                if (pgPercent.InvokeRequired)
                {
                    Action<int> action = (val) => { pgPercent.Value = val; };
                    pgPercent.Invoke(action, e.ProgressPercentage);
                }
                else
                {
                    pgPercent.Value = e.ProgressPercentage;
                }
            };
            bWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (pgPercent.InvokeRequired)
                {
                    Action action = () => { pgPercent.Visible = false; };
                    pgPercent.Invoke(action);
                }
                else
                {
                    pgPercent.Visible = false;
                }

                timer.Enabled = false;
                timer.Dispose();
                bWorker.Dispose();
            };

            if (Total > 0)
            {
                pgPercent.Visible = true;
                bWorker.RunWorkerAsync();
                timer.Enabled = true;
            }
        }
    }
}
