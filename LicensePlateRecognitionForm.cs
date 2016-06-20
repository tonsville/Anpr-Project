//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using System.Diagnostics;
using Emgu.CV.Util;
using System.IO;
using System.Drawing.Imaging;

namespace LicensePlateRecognition
{
   public partial class LicensePlateRecognitionForm : Form
   {
      private LicensePlateDetector _licensePlateDetector;
      private Capture capture;
      
    
        public LicensePlateRecognitionForm()
      {
         InitializeComponent();
         _licensePlateDetector = new LicensePlateDetector("");
         Mat m = new Mat("license-plate.jpg", LoadImageType.AnyColor);
         UMat um = m.ToUMat(AccessType.ReadWrite);
         imageBox1.Image = um;
         ProcessImage(m);
        }

      private void ProcessImage(IInputOutputArray image)
      {
         Stopwatch watch = Stopwatch.StartNew(); // time the detection process

         List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
         List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
         List<RotatedRect> licenseBoxList = new List<RotatedRect>();
         List<string> words = _licensePlateDetector.DetectLicensePlate(
            image,
            licensePlateImagesList,
            filteredLicensePlateImagesList,
            licenseBoxList);

         watch.Stop(); //stop the timer
         processTimeLabel.Text = String.Format("License Plate Recognition time: {0} milli-seconds", watch.Elapsed.TotalMilliseconds);

         panel1.Controls.Clear();
         Point startPoint = new Point(10, 10);
         //for (int i = 0; i < words.Count; i++)
         if (words.Count > 0)
         {
            Mat dest = new Mat();
            CvInvoke.VConcat(licensePlateImagesList[0], filteredLicensePlateImagesList[0], dest);
            AddLabelAndImage(
               ref startPoint,
               String.Format("License: {0}", words[0]),
               dest);
            PointF[] verticesF = licenseBoxList[0].GetVertices();
            Point[] vertices = Array.ConvertAll(verticesF, Point.Round);
            using(VectorOfPoint pts = new VectorOfPoint(vertices))
               CvInvoke.Polylines(image, pts, true, new Bgr(Color.Green).MCvScalar,2);
            
         }

      }

      private void AddLabelAndImage(ref Point startPoint, String labelText, IImage image)
      {
         Label label = new Label();
         panel1.Controls.Add(label);
         label.Text = labelText;
         label.Width = 100;
         label.Height = 30;
         label.Location = startPoint;
         startPoint.Y += label.Height;

         ImageBox box = new ImageBox();
         panel1.Controls.Add(box);
         box.ClientSize = image.Size;
         box.Image = image;
         box.Location = startPoint;
         startPoint.Y += box.Height + 10;
      }

      private void button1_Click(object sender, EventArgs e)
      {
         DialogResult result = openFileDialog1.ShowDialog();
         if (result == DialogResult.OK)
         {
            Mat img;
            try
            {
               img = CvInvoke.Imread(openFileDialog1.FileName, LoadImageType.Color);

            }
            catch
            {
               MessageBox.Show(String.Format("Invalide File: {0}", openFileDialog1.FileName));
               return;
            }
            UMat uImg = img.ToUMat(AccessType.ReadWrite);
            imageBox1.Image = uImg;
            ProcessImage(uImg);
         }
      }


        private void btnip_camera_Click(object sender, EventArgs e)
        {
            capture = null; 

            //Camera
            //working on vlc:
            //string sourceURL = "rtsp://admin:admin@192.168.0.105/xunmei0.live";

            string sourceURL = "rtsp://" + edituser_name.Text + ":" + edituser_pass.Text + "@" + editcamera_ip.Text + "/xunmei" + editcamera_index.Text + ".live";

            //MessageBox.Show(sourceURL, "ANPR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            capture = new Capture(sourceURL);
            Application.Idle += ProcessFrame;
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            //UMat image = new UMat();
            
            Mat image = new Mat();
            image = capture.QueryFrame();
    
            if (image != null)
            {
                imageBox1.BackgroundImage = image.Bitmap;
                imageBox1.Image = image;
                //imageBox1.Image = (image)image.Bitmap;
                //image = Image.FromStream(stream);
                //imageBox1.Image = image;




                //imageBox1.Image = (Image)image.Bitmap;
                // imageBox1.Image = Image.FromStrea(stream);

            }
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            imageBox1.Image.Save(@"C:\Users\ADELPHI08\Desktop\ANPRTest", ImageFormat.Jpeg);
        }
    }

}