using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Laner
{
    public partial class LaneFollower : Form
    {
        string i;

        SerialPort _serialPort = new SerialPort("COM3", 2400);

        const byte STOP = 0x7F;
        const byte FLOAT = 0x0F;
        const byte FORWARD = 0x6f;
        const byte BACKWARD = 0x5F;


        private VideoCapture _capture;
        private Thread _captureThread;
        public LaneFollower()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture();
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
        }
        private void DisplayWebcam()
        {
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.Open();

            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();

                CvInvoke.Resize(frame, frame, pictureBox1.Size);

                Image<Bgr, Byte> bgrImage = frame.ToImage<Bgr, Byte>();

                Image<Gray, Byte> grayImage = frame.ToImage<Gray, Byte>();

                grayImage = grayImage.ThresholdBinary(new Gray(200), new Gray(255));

                bgrImage = bgrImage & grayImage.Convert<Bgr, Byte>();

                int leftWhiteCount = 0;
                for (int x = 0; x < grayImage.Width / 3; x++)
                {
                    for (int y = 0; y < grayImage.Height; y++)
                    {
                        if (grayImage.Data[y, x, 0] == 255)
                            leftWhiteCount++;
                    }
                }

                int midWhiteCount = 0;
                for (int x = grayImage.Width / 3; x < ( 2 * grayImage.Width) / 3; x++)
                {
                    for (int y = 0; y < grayImage.Height; y++)
                    {
                        if (grayImage.Data[y, x, 0] == 255)
                            midWhiteCount++;
                    }
                }

                int rightWhiteCount = 0;
                for (int x = (2 * grayImage.Width) / 3; x < grayImage.Width; x++)
                {
                    for (int y = 0; y < grayImage.Height; y++)
                    {
                        if (grayImage.Data[y, x, 0] == 255)
                            rightWhiteCount++;
                    }
                }

                if (leftWhiteCount > rightWhiteCount)
                {
                    i = "Turn Left";

                    byte left = FLOAT;
                    byte right = FORWARD;

                    byte[] buffer = { 0x01, left, right };
                    _serialPort.Write(buffer, 0, 3);

                }


                else if (rightWhiteCount > leftWhiteCount)
                {
                    i = "Turn Right";

                    byte left = FORWARD;
                    byte right = FLOAT;

                    byte[] buffer = { 0x01, left, right };
                    _serialPort.Write(buffer, 0, 3);

                }

                int totalRedCount = 0;
                for (int x = 0; x < bgrImage.Width; x++)
                {
                    for (int y = 0; y < bgrImage.Height; y++)
                    {
                        if (bgrImage.Data[y, x, 2] > 150 && bgrImage.Data[y, x, 0] < 60 && bgrImage.Data[y, x, 2] < 60)
                            totalRedCount++;
                    }
                }

                const int red_turn_threshold = 5000;

                if (totalRedCount > red_turn_threshold)
                {
                    i = "Turn Left";

                    byte left = FLOAT;
                    byte right = FORWARD;

                    byte[] buffer = { 0x01, left, right };
                    _serialPort.Write(buffer, 0, 3);
                }
                else if (Math.Abs(leftWhiteCount - rightWhiteCount) < 1000)
                {
                    // Go straight
                    i = "Go Straight";

                    byte left = FORWARD;
                    byte right = FORWARD;

                    byte[] buffer = { 0x01, left, right };
                    _serialPort.Write(buffer, 0, 3);
                }

                else
                {
                    i = "Go Straight";

                    byte left = FORWARD;
                    byte right = FORWARD;

                    byte[] buffer = { 0x01, left, right };
                    _serialPort.Write(buffer, 0, 3);

                }


                label1.Invoke(new Action(() => label1.Text = i.ToString()));
                /*
                
                if (whiteCount <= (grayImage.Width * grayImage.Height)/2 )
                    this.BackColor = Color.Green;
                if (whiteCount >= (grayImage.Width * grayImage.Height) / 2)
                    this.BackColor = Color.Crimson;
                */
                // this.BackColor = Color.Crimson;


                pictureBox1.Image = grayImage.Bitmap;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();

        }

        private void button1_Click(object sender, EventArgs e)
        {
          //  i += 25;
        }

        private void Downbutton_Click(object sender, EventArgs e)
        {
          //  i -= 25;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

      
    }
}

