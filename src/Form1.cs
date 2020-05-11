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
using System.Threading;
using System.Windows.Forms;

namespace GmicSharpExample
{
    public partial class Form1 : Form
    {
        private Gmic<GdiPlusGmicBitmap> gmicInstance;
        private string imageName;
        private CancellationTokenSource cancellationToken;
        private bool formClosePending;

        public Form1()
        {
            InitializeComponent();
            gmicInstance = new Gmic<GdiPlusGmicBitmap>(new GdiPlusOutputImageFactory());
            gmicInstance.GmicDone += GmicInstance_GmicDone;
            gmicInstance.GmicProgress += GmicInstance_GmicProgress;
            cancellationToken = null;
            formClosePending = false;
            statusTextLabel.Text = string.Empty;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (gmicInstance.GmicRunning)
            {
                statusTextLabel.Text = Resources.StatusCanceling;
                formClosePending = true;
                cancellationToken.Cancel();
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        private void GmicInstance_GmicDone(object sender, GmicCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<GmicCompletedEventArgs>(OnGmicDone), e);
            }
            else
            {
                OnGmicDone(e);
            }
        }

        private void OnGmicDone(GmicCompletedEventArgs e)
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
                else if (!e.Canceled)
                {
                    GdiPlusGmicBitmap gmicBitmap = gmicInstance.OutputImages[0];

                    pictureBox1.Image = gmicBitmap.Image;
                }
            }
        }

        private void GmicInstance_GmicProgress(object sender, GmicProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(OnGmicUpdateProgress), e.Progress);
            }
            else
            {
                OnGmicUpdateProgress(e.Progress);
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

            cancellationToken?.Dispose();
            cancellationToken = new CancellationTokenSource();
            progressBar1.Style = ProgressBarStyle.Marquee;
            statusTextLabel.Text = Resources.StatusGmicRunning;

            try
            {
                gmicInstance.RunGmic(textBox1.Text, cancellationToken.Token);
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
            catch (OperationCanceledException)
            {
                // Ignore it
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
