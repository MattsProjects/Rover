using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SOUP
{
    public partial class SOUP : Form
    {
        private System.IO.Ports.SerialPort xbee = null;
        private Byte[] mybyte = new byte[1];
        private byte[] outgoing = new byte[3];
        private string ACKNAK = "";
        private string incoming = "";
        private string feedback = "";
        private string selection = "";
        

        public SOUP()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = "Idle";
            // Create and setup serial port
            xbee = new System.IO.Ports.SerialPort("COM7", 57600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            xbee.ReadTimeout = 5000;
        }

        private void imagePictureBox_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            
            try
            {
                sendButton.Enabled = false;
                xbee.Close();
                xbee.Open();

                // Send the Rover a message from the drop-down box
                selection = commandCombo.SelectedItem.ToString();
                outgoing = System.Text.Encoding.UTF8.GetBytes(selection);
                xbee.Write(outgoing, 0, outgoing.Length);

                // run the background worker to wait for rover repsonses.
                backgroundWorker2.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message.ToString();
                xbee.Close();
                xbee.Dispose();
                sendButton.Enabled = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Saved_File = "";
            saveFD.InitialDirectory = "C:";
            saveFD.Title = "Save Image";
            saveFD.FileName = "";
            saveFD.Filter = "JPG Files|*.jpg|All Files|*.*";
            if (saveFD.ShowDialog() != DialogResult.Cancel)
            {
                Saved_File = saveFD.FileName;
                Bitmap img = new Bitmap(imagePictureBox.Image);
                img.Save(Saved_File, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Saved_File = "";
            saveFD.InitialDirectory = "C:";
            saveFD.Title = "Save Image";
            saveFD.FileName = "";
            saveFD.Filter = "JPG Files|*.jpg|All Files|*.*";
            if (saveFD.ShowDialog() != DialogResult.Cancel)
            {
                Saved_File = saveFD.FileName;
                Bitmap img = new Bitmap(imagePictureBox.Image);
                img.Save(Saved_File,System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Opened_File = "";
            openFD.InitialDirectory = "C:";
            openFD.Title = "Open Image";
            openFD.FileName = "";
            openFD.Filter = "JPG Files|*.jpg|All Files|*.*";

            if (openFD.ShowDialog() != DialogResult.Cancel)
            {
                Opened_File = openFD.FileName;
                Bitmap img = new Bitmap(Opened_File);
                imagePictureBox.Image = img;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(imagePictureBox.Image);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           try
           {
                 
            }
        catch (Exception ex)
            {
                xbee.Close();
                toolStripStatusLabel1.Text = ex.Message.ToString();
               
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
    
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // this background worker waits for responses from the rover.
                // Communication Template:
                // 1. Rover should send ACK of command
                // 2. Rover should send some feedback/data from the task
                // 3. Rover should send a Task Complete notification
                // ie: send LFT, recevie ACK, Turning Left, Maneuver Complete.

                // Check for ACK
                xbee.ReadTimeout = 5000;
                incoming = xbee.ReadLine().ToString();
                ACKNAK = incoming.Trim();
                switch (ACKNAK)
                {
                    case ("ACK"):
                        {

                            switch (selection)
                            {
                                // handle normal ACK, feedback, done commands with default
                                // or handle as special case ie: pic, where more serial comm is necessary.
                                case ("PIC"):
                                    {
                                        
                                        backgroundWorker2.ReportProgress(10);
                                        // Check for feedback
                                        incoming = xbee.ReadLine().ToString();
                                        toolStripStatusLabel1.Text = incoming.Trim();

                                        // setup byte to hold imagesize data
                                        byte[] imagesize = new byte[5];

                                        // create the incoming buffer with a size received from the rover.
                                        string strimagesize = "";
                                        xbee.ReadTimeout = 30000;
                                        strimagesize = xbee.ReadLine().ToString();
                                        int size = Convert.ToInt32(strimagesize);
                                        byte[] mybyte = new byte[size];
                                        backgroundWorker2.ReportProgress(20);

                                        // Grab image from rover
                                        for (int i = 0; i < mybyte.Length; i++)
                                        {
                                            mybyte[i] = (byte)xbee.ReadByte();
                                            backgroundWorker2.ReportProgress((int)((float)i / (float)mybyte.Length * 100) + 20);
                                        }

                                        // save image to PC
                                        string filename = System.DateTime.Now.Ticks.ToString() + ".jpg";
                                        System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                                        fs.Write(mybyte, 0, mybyte.Length);
                                        fs.Close();

                                        // display image
                                        // System.Windows.Forms.MessageBox.Show(filename);
                                        Bitmap picture = new Bitmap(filename);
                                        imagePictureBox.Image = picture;

                                        // Check for Task Complete
                                        incoming = xbee.ReadLine().ToString();
                                        toolStripStatusLabel1.Text = incoming.Trim();

                                        //cleanup
                                        outgoing = null;
                                        imagesize = null;
                                        mybyte = null;
                                        xbee.DiscardInBuffer();
                                        xbee.DiscardOutBuffer();
                                        xbee.Close();
                                        

                                        break;
                                    }
                                default:
                                    {
                                    
                                        backgroundWorker2.ReportProgress(10);

                                        // Check for Feedback
                                        incoming = xbee.ReadLine().ToString();
                                        toolStripStatusLabel1.Text = incoming.Trim();

                                        // update progress
                                        backgroundWorker2.ReportProgress(50);

                                        // display the feedback for some time
                                        System.Threading.Thread.Sleep(2000);

                                        // do something with the feedback
                                        feedback = incoming.Trim();

                                        // Check for Done. Set timeout higher to wait for long tasks to complete.
                                        xbee.ReadTimeout = 60000;
                                        incoming = xbee.ReadLine().ToString();
                                        toolStripStatusLabel1.Text = incoming.Trim();

                                        backgroundWorker2.ReportProgress(100);

                                        break;
                                    }
                            }
                            break;
                        }
                    case ("NAK"):
                        {
                            toolStripStatusLabel1.Text = "Did not receive command. Try again.";
                            xbee.Close();
                            xbee.Dispose();
                            sendButton.Enabled = true;
                            break;
                        }
                    default:
                        {
                            toolStripStatusLabel1.Text = "Did not receive command. Try again.";
                            xbee.Close();
                            xbee.Dispose();
                            sendButton.Enabled = true;
                            break;
                        }
                }
            }


            catch (Exception ex)
            {
                xbee.Close();
                toolStripStatusLabel1.Text = ex.Message.ToString();
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            sendButton.Enabled = true;
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
