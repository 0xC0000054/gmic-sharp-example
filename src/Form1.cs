////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp-example, a Windows Forms-based
// example application for gmic-sharp.
//
// Copyright (c) 2020 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using GmicSharp;
using GmicSharpExample.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GmicSharpExample
{
    public partial class Form1 : Form
    {
        private Gmic<GdiPlusGmicBitmap> gmicInstance;
        private string imageName;
        private bool formClosePending;

        public Form1()
        {
            InitializeComponent();
            gmicInstance = new Gmic<GdiPlusGmicBitmap>(new GdiPlusOutputImageFactory());
            gmicInstance.RunGmicCompleted += GmicInstance_RunGmicCompleted;
            gmicInstance.RunGmicProgressChanged += GmicInstance_RunGmicProgressChanged;
            formClosePending = false;
            statusTextLabel.Text = string.Empty;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (gmicInstance.IsBusy)
            {
                statusTextLabel.Text = Resources.StatusCanceling;
                formClosePending = true;
                gmicInstance.RunGmicAsyncCancel();
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        private void GmicInstance_RunGmicCompleted(object sender, RunGmicCompletedEventArgs<GdiPlusGmicBitmap> e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<RunGmicCompletedEventArgs<GdiPlusGmicBitmap>>(OnRunGmicCompleted), e);
            }
            else
            {
                OnRunGmicCompleted(e);
            }
        }

        private void OnRunGmicCompleted(RunGmicCompletedEventArgs<GdiPlusGmicBitmap> e)
        {
            if (formClosePending)
            {
                Close();
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 0;
                statusTextLabel.Text = string.Empty;

                if (e.Error != null)
                {
                    ShowErrorMessage(e.Error.Message);
                }
                else if (!e.Cancelled)
                {
                    OutputImageCollection<GdiPlusGmicBitmap> outputImages = e.OutputImages;

                    try
                    {
                        if (outputImages.Count > 0)
                        {
                            GdiPlusGmicBitmap gmicBitmap = outputImages[0];

                            pictureBox1.Image = (Image)gmicBitmap.Image.Clone();
                        }
                    }
                    finally
                    {
                        outputImages.Dispose();
                    }
                }
            }
        }

        private void GmicInstance_RunGmicProgressChanged(object sender, RunGmicProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(OnGmicUpdateProgress), e.ProgressPercentage);
            }
            else
            {
                OnGmicUpdateProgress(e.ProgressPercentage);
            }
        }

        private void OnGmicUpdateProgress(int progress)
        {
            if (progressBar1.Style == ProgressBarStyle.Marquee)
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
            }

            progressBar1.Value = progress;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;

                try
                {
                    pictureBox1.Image = new Bitmap(fileName);
                    imageName = Path.GetFileNameWithoutExtension(imageName);
                }
                catch (ArgumentException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (ExternalException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (IOException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (OutOfMemoryException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
                }
                catch (ArgumentException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (ExternalException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (IOException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
                catch (OutOfMemoryException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void runGmicButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                return;
            }

            if (pictureBox1.Image != null)
            {
                using (GdiPlusGmicBitmap image = new GdiPlusGmicBitmap(pictureBox1.Image))
                {
                    gmicInstance.AddInputImage(image, imageName);
                }
            }

            progressBar1.Style = ProgressBarStyle.Marquee;
            statusTextLabel.Text = Resources.StatusGmicRunning;

            try
            {
                gmicInstance.RunGmicAsync(textBox1.Text);
            }
            catch (ArgumentException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (GmicException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
