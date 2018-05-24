using AForge.Video;
using AForge.Video.DirectShow;
using MetroFramework;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCIO
{
    public partial class Form1 : MetroForm
    {
        dcDBEntities db = new dcDBEntities();
        Timer webcamTimer = new Timer();

        private int tryCount = 0;

        public Form1()
        {
            InitializeComponent();
            FormLoadWebcamSettings();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblDate.Text = DateTime.Now.ToShortDateString();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MetroMessageBox.Show(this, "Kullanıcı adı ve ya şifre boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, 180);
                    tryCount++;
                    if (WebCamCheck())
                    {
                        CamCapture();
                    }
                }
                else
                {
                    Users user = db.Users.FirstOrDefault(x => x.Username == txtUsername.Text);
                    if (user != null)
                    {
                        if (txtUsername.Text == user.Username && Helpers.PasswordToMD5(txtPassword.Text) == user.Password)
                        {
                            Main main = new Main();
                            if (WebCamCheck())
                            {
                                cam.Stop();
                            }

                            webcamTimer.Stop();
                            this.Hide();
                            main.Show();

                        }
                        if (Helpers.PasswordToMD5(txtPassword.Text) != user.Password)
                        {
                            MetroMessageBox.Show(this, "Şifre Hatalı !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, 180);
                            tryCount++;
                            if (WebCamCheck())
                            {
                                CamCapture();
                            }
                        }
                    }
                    else
                    {
                        MetroMessageBox.Show(this, "Kullanıcı bulunamadı !", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, 180);
                        tryCount++;
                        if (WebCamCheck())
                        {
                            CamCapture();
                        }
                    }
                }
                if (tryCount >= 3)
                {
                    metroPanel1.Hide();
                    lblWarning.Visible = true;
                    if (WebCamCheck())
                    {
                        cam.Stop();
                    }
                    Helpers.SendEmail();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        #region WEBCAM

        #region WebCam Capture
        public static Bitmap _latestFrame;
        private FilterInfoCollection webcam;
        private VideoCaptureDevice cam;
        PictureBox pb;
        Bitmap bitmap;
        #endregion

        private void CamCapture()
        {
            Bitmap current = (Bitmap)bitmap.Clone();
            string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DCIO" + "\\";
            bool exists = System.IO.Directory.Exists(filepath);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(filepath);
            }

            string imgname = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Ticks.ToString();
            string newfoto = imgname + ".bmp";
            current.Save(filepath + newfoto);

            try
            {
                CamLogs cl = new CamLogs();
                cl.Name = newfoto;
                cl.CreatedDate = DateTime.Now;
                cl.isSeen = false;
                cl.Status = true;
                db.CamLogs.Add(cl);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private bool WebCamCheck()
        {
            webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (webcam.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void FormLoadWebcamSettings()
        {
            webcamTimer.Interval = 5000;
            webcamTimer.Tick += WebcamTimer_Tick;
            webcamTimer.Start();
            webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (WebCamCheck())
            {
                cam = new VideoCaptureDevice(webcam[0].MonikerString);
                cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
                cam.Start();
            }
            
        }

        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            Process proc = Process.GetProcessesByName("DCIO")[0];
            int memsize = 0; // memsize in Megabyte
            PerformanceCounter PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = proc.ProcessName;
            memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
            PC.Close();
            PC.Dispose();

            if (memsize > 300000)
            {
                cam.Stop();
                cam.Start();
            }
        }

        private void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            bitmap = (Bitmap)eventArgs.Frame.Clone();
            pb = new PictureBox();
            pb.Image = bitmap;

        }

        #endregion

    }
}
