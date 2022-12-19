using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware.LowLevel;

namespace MFConsoleApplication1
{
    public class Program
    {

        // remap com4 for Xbee
        static public void RemapCOM4to_TXAn2_RXAn3(System.IO.Ports.SerialPort ser)
        {
            // call this function **after** you open COM4 port
            if (ser.PortName != "COM4" || ser.IsOpen == false)
                throw new Exception("Only use COM4 and make sure it is open");
            // remap COM4 RX (in)  pin from P4.29/DIO17 to P0.26 (that is An3)
            // remap COM4 TX (out) pin from P4.28/DIO13 to P0.25 (that is An2)
            Register PINSEL9 = new Register(0xE002C024);
            PINSEL9.Write(0);// COM4 is now disconnected from P4.28 and P4.29
            Register PINSEL1 = new Register(0xE002C004);
            PINSEL1.SetBits(0xf << 18);// COM4 is now connected to An3 and An4
        }

        // read the distance as a float in cm.
        public static float ReadDistance()
        {
            // Create Distance Detector connected to An2
            FEZ_Components.DistanceDetector FrontDistanceDetector;
            FrontDistanceDetector = new FEZ_Components.DistanceDetector(FEZ_Pin.AnalogIn.An1, FEZ_Components.DistanceDetector.SharpSensorType.GP2Y0A21YK);    
            float frontdistance = FrontDistanceDetector.GetDistance_cm();
            // Print Distance to Debug Window
            Debug.Print(frontdistance.ToString());
            // dispose of object to get back memory
            FrontDistanceDetector.Dispose();
            return frontdistance;
        }
        
        // predefined ways of moving
        public static void Move(string direction)
        {
            FEZ_Shields.DCMotorDriver.Initialize();
            switch (direction)
            {
                case ("S") /*STP*/:
                    {
                        // stop
                        FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 50);
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
                case ("F") /*FWD*/:
                    {
                        // move forward for 5 seconds, then stop
                        FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 50);
                        Thread.Sleep(5000);
                        FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 50);
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
                case ("B") /*BCK*/:
                    {
                        // move backward for 5 seconds, then stop
                        FEZ_Shields.DCMotorDriver.MoveRamp(-100, -100, 50);
                        Thread.Sleep(5000);
                        FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 50);
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
                case ("L") /*LFT*/:
                    {
                        // rotate left
                        FEZ_Shields.DCMotorDriver.MoveRamp(100, -100, 50);
                        Thread.Sleep(5000);
                        FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 50);
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
                case ("R") /*RGT*/:
                    {
                        // rotate right
                        FEZ_Shields.DCMotorDriver.MoveRamp(-100, 100, 50);
                        Thread.Sleep(5000);
                        FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 50);
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
                default:
                    {
                        FEZ_Shields.DCMotorDriver.Dispose();
                        break;
                    }
            }
        }
        
        // let the rover automatically roam
        public static void Auto()
        {
            Move("F");
            Debug.Print(ReadDistance().ToString());
            Move("B");
            Debug.Print(ReadDistance().ToString());
            Move("R");
            Debug.Print(ReadDistance().ToString());
            Move("L");
            Debug.Print(ReadDistance().ToString());
            Move("S");
            Debug.Print(ReadDistance().ToString());
        }


        public static void Main()
        {
                     
            /********************************************************************************/
            // Serial command test

            byte[] incoming = new byte[3];
            byte[] outgoing = new byte[20];
            string strincoming = "";

            System.IO.Ports.SerialPort xbee =
                new System.IO.Ports.SerialPort("COM1", 57600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            try
            {
                while (true)
                {
                    xbee.Open();
                    xbee.Read(incoming, 0, incoming.Length);
                    char[] charincoming = Encoding.UTF8.GetChars(incoming);
                    for (int i = 0; i < charincoming.Length; i++)
                        strincoming += charincoming[i];

                    Debug.Print(strincoming);

                    switch (strincoming)
                    {
                        case "PNG":
                            {
                                // this background worker waits for responses from the rover.
                                // Communication Template:
                                // 1. Rover should send ACK of command
                                // 2. Rover should send some feedback/data from the task
                                // 3. Rover should send a Task Complete notification

                                strincoming = "";
                                // Send ACK
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                
                                // do a task...
                                
                                // Send Feedback
                                outgoing = Encoding.UTF8.GetBytes("Ping Received\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                // Send Task Complete
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();
                                break;

                            }
                        case "ATO":
                            {
                                // start automatic mode

                                strincoming = "";
                                // Send ACK
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                               
                                // Send Feedback
                                outgoing = Encoding.UTF8.GetBytes("Running in Auto...\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                
                                // start automatic mode
                                Auto();
                                
                                // Send done
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();
                                break;

                            }
                        case "LFT":
                            {
                                // Rotate left
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                outgoing = Encoding.UTF8.GetBytes("Turning Left\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // Rotate Left
                                Move("L");
                            
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();

                                break;
                            }
                        case "RGT":
                            {
                                // Rotate right
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Turning Right\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // rotate right
                                Move("R");
                                

                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();

                                break;
                            }
                        case "FWD":
                            {
                                // drive forward
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Moving Forward\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // move forward
                                Move("F");

                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();

                                break;
                            }
                        case "REV":
                            {
                                // send ack and feedback
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Moving Reverse\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // move backward
                                Move("B");

                                // send done
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();

                                break;
                            }
                        case "STP":
                            {
                                // stop
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Stopping\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // stop
                                Move("S");

                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();

                                break;
                            }
                        case "RBT":
                            {
                                // Reboot (special case, send feedback before task)
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Rebooting\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();
                                
                                Microsoft.SPOT.Hardware.PowerState.RebootDevice(true);
                                break;
                            }
                        case "PIC":
                            {
                                // shoot and download a photo
                                strincoming = "";
                                // send ACK
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                // send Feedback
                                outgoing = Encoding.UTF8.GetBytes("Taking Picture\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // Create camera
                                C328R camera;
                                // Create COM4 port for camera
                                System.IO.Ports.SerialPort MyCOM4 = new System.IO.Ports.SerialPort("COM4", 38400, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                                camera = new C328R(MyCOM4);
                                // Remap COM4 to An2 and An3
                                RemapCOM4to_TXAn2_RXAn3(MyCOM4);
                                // Synchronize with camera
                                Debug.Print("Synchronizing FEZ with camera...");
                                Debug.Print(camera.Sync().ToString());
                                // Set baud rate - 38400 works pretty reliably.
                                camera.SetBaudRate(C328R.BaudRate.Baud38400);
                                // Set light frequency - 60Hz for USA 
                                camera.LigtFrequency(C328R.FrequencyType.F60Hz);
                                // sleep for a bit to settle the AGC.
                                Thread.Sleep(3000);
                                // Initialize the camera
                                Debug.Print("Initializing the camera...");

                                Debug.Print(camera.Initial(C328R.ColorType.Jpeg, C328R.PreviewResolution.R160x120, C328R.JpegResolution.R640x480).ToString());

                                // Create Picture data buffer
                                byte[] pictureData;

                                // Get instant Jpeg picture or instant raw picture. Turn on LED during capture.
                                FEZ_Components.LED onboard = new FEZ_Components.LED(FEZ_Pin.Digital.LED);
                                onboard.ShutOff();
                                onboard.TurnOn();
                                // Debug.Print(camera.GetRawPicture(C328R.PictureType.Preview, out pictureData, 10000).ToString());
                                Debug.Print("Taking a picture...");
                                Debug.Print(camera.GetJpegPicture(C328R.PictureType.Jpeg, out pictureData, 800).ToString());
                                onboard.ShutOff();
                                onboard.Dispose();

                                // send the image
                                // send the size of the image first
                                byte[] imagesize = new byte[5];
                                imagesize = Encoding.UTF8.GetBytes(pictureData.Length.ToString().Trim() + "\r\n");
                                xbee.Write(imagesize, 0, imagesize.Length);
                                Thread.Sleep(2000);
                                // because the Xbee buffer is small,
                                // we must make sure not to overrun it when writing the byte to it.
                                Debug.Print("Transmitting Image...");
                                for (int i = 0; i < pictureData.Length; i++)
                                {
                                    // send one element of the picture
                                    xbee.Write(pictureData, i, 1);
                                    // pause after 100 to prevent the xbee buffer overrun.
                                    if (i % 100 == 0)
                                        Thread.Sleep(50);
                                }


                                // save the image to SD card
                                PersistentStorage SDCard = new PersistentStorage("SD");
                                // Mount the file system as a volume
                                SDCard.MountFileSystem();
                                // Assume that SDCard was mounted as Volume 0, and access it through NETMF
                                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                                Directory.SetCurrentDirectory(rootDirectory);
                                string pictureName = "image.jpg";
                                FileStream fs = new FileStream(pictureName, FileMode.Create);
                                Debug.Print("Saving the image to on-board SD card...");
                                fs.Write(pictureData, 0, pictureData.Length);
                                fs.Close();
                                fs.Dispose();
                                // unmount the SD card so you can remove it.
                                SDCard.UnmountFileSystem();
                                SDCard.Dispose();

                                // get back the memory
                                pictureData = null;
                                // cleanup
                                camera.Reset(true);
                                MyCOM4.Close();

                                // send Task Complete
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                xbee.Close();
                                Debug.Print("Finished.");
                                break;
                            }
                        case "DST":
                            {
                                // get distance measurement
                                strincoming = "";
                                // Send ACK
                                outgoing = Encoding.UTF8.GetBytes("ACK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                
                                // Read Distance
                                string distance = ReadDistance().ToString();
                                
                                // Send Feedback
                                outgoing = Encoding.UTF8.GetBytes("Distance: " + distance + "\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);

                                // Send Task Complete
                                outgoing = Encoding.UTF8.GetBytes("Done\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();
                                break;

                            }
                        default:
                            {
                                strincoming = "";
                                outgoing = Encoding.UTF8.GetBytes("NAK\r\n");
                                xbee.Write(outgoing, 0, outgoing.Length);
                                xbee.Close();
                                break;
                            }

                    }

                }
                /********************************************************************************/

                /********************************************************************************/
                //// camera test
                //FEZ_Components.LED onboard = new FEZ_Components.LED(FEZ_Pin.Digital.LED);
                //onboard.ShutOff();
                ////// Create camera
                //C328R camera;
                //// Create COM4 port for camera
                //System.IO.Ports.SerialPort MyCOM4 = new System.IO.Ports.SerialPort("COM4", 38400, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                //camera = new C328R(MyCOM4);
                //// Remap COM4 to An2 and An3
                //RemapCOM4to_TXAn2_RXAn3(MyCOM4);
                //// Synchronize with camera
                //Debug.Print(camera.Sync().ToString());
                //// Set baud rate - 38400 works pretty reliably.
                //camera.SetBaudRate(C328R.BaudRate.Baud38400);
                //// Set light frequency - 60Hz for USA 
                //camera.LigtFrequency(C328R.FrequencyType.F60Hz);
                //// sleep for a bit to settle the AGC.
                //Thread.Sleep(3000);
                //// Initialize the camera
                //Debug.Print(camera.Initial(C328R.ColorType.Jpeg, C328R.PreviewResolution.R160x120, C328R.JpegResolution.R640x480).ToString());

                //// Create Picture data buffer
                //byte[] pictureData;

                //// Get instant Jpeg picture or instant raw picture
                //onboard.TurnOn();
                //// Debug.Print(camera.GetRawPicture(C328R.PictureType.Preview, out pictureData, 10000).ToString());
                //Debug.Print(camera.GetJpegPicture(C328R.PictureType.Jpeg, out pictureData, 800).ToString());
                //onboard.ShutOff();

                //// Save image to SDCard
                //Debug.Print("Saving jpg Image to SDCard...");
                //if (pictureData.Length > 0)
                //{
                //    // Create a new storage device
                //    PersistentStorage SDCard = new PersistentStorage("SD");
                //    // Mount the file system as a volume
                //    SDCard.MountFileSystem();
                //    // Assume that SDCard was mounted as Volume 0, and access it through NETMF
                //    string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                //    Directory.SetCurrentDirectory(rootDirectory);
                //    // Some unique picture name based on time
                //    string pictureName = "image.jpg";

                //    // Save the data into file
                //    FileStream fs = new FileStream(pictureName, FileMode.Create);
                //    fs.Write(pictureData, 0, pictureData.Length);
                //    fs.Close();

                //    // unmount the SD card so you can remove it.
                //    SDCard.UnmountFileSystem();
                //    onboard.ShutOff();
                //}
                //else
                //{
                //    camera.Reset(true);
                //    Debug.Print("Something Happened. Image wasn't grabbed...");
                //}
                /********************************************************************************/

                /********************************************************************************/
                // Distance Detector Test
                // Create Distance Detector connected to An2
                //FEZ_Components.DistanceDetector FrontDistanceDetector;
                //FrontDistanceDetector = new FEZ_Components.DistanceDetector(FEZ_Pin.AnalogIn.An1, FEZ_Components.DistanceDetector.SharpSensorType.GP2Y0A21YK);

                //// Read Distance
                //float frontdistance = FrontDistanceDetector.GetDistance_cm();
                //// Print Distance to Debug Window
                //Debug.Print(frontdistance.ToString());

                //FrontDistanceDetector.Dispose();
                /********************************************************************************/

                ///********************************************************************************/
                //// camera test
                //FEZ_Components.LED onboard = new FEZ_Components.LED(FEZ_Pin.Digital.LED);
                //onboard.ShutOff();
                ////// Create camera
                //C328R camera;
                //// Create COM4 port for camera
                //System.IO.Ports.SerialPort MyCOM4 = new System.IO.Ports.SerialPort("COM4", 38400, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                //camera = new C328R(MyCOM4);
                //// Remap COM4 to An2 and An3
                //RemapCOM4to_TXAn2_RXAn3(MyCOM4);
                //// Synchronize with camera
                //Debug.Print(camera.Sync().ToString());
                //// Set baud rate - 38400 works pretty reliably.
                //camera.SetBaudRate(C328R.BaudRate.Baud38400);
                //// Set light frequency - 60Hz for USA 
                //camera.LigtFrequency(C328R.FrequencyType.F60Hz);
                //// sleep for a bit to settle the AGC.
                //Thread.Sleep(3000);
                //// Initialize the camera
                //Debug.Print(camera.Initial(C328R.ColorType.Jpeg, C328R.PreviewResolution.R160x120, C328R.JpegResolution.R640x480).ToString());

                //// Create Picture data buffer
                //byte[] pictureData;

                //// Get instant Jpeg picture or instant raw picture
                //onboard.TurnOn();
                //// Debug.Print(camera.GetRawPicture(C328R.PictureType.Preview, out pictureData, 10000).ToString());
                //Debug.Print(camera.GetJpegPicture(C328R.PictureType.Jpeg, out pictureData, 800).ToString());
                //onboard.ShutOff();
                //// Save image to SDCard
                //Debug.Print("Saving jpg Image to SDCard...");
                //if (pictureData.Length > 0)
                //{
                //    // Create a new storage device
                //    PersistentStorage SDCard = new PersistentStorage("SD");
                //    // Mount the file system as a volume
                //    SDCard.MountFileSystem();
                //    // Assume that SDCard was mounted as Volume 0, and access it through NETMF
                //    string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                //    Directory.SetCurrentDirectory(rootDirectory);
                //    // Some unique picture name based on time
                //    string pictureName = "image.jpg";

                //    // Save the data into file
                //    FileStream fs = new FileStream(pictureName, FileMode.Create);
                //    fs.Write(pictureData, 0, pictureData.Length);
                //    fs.Close();

                //    // unmount the SD card so you can remove it.
                //    SDCard.UnmountFileSystem();
                //    onboard.ShutOff();
                //}
                //else
                //{
                //   camera.Reset(true);
                //   Debug.Print("Something Happened. Image wasn't grabbed...");
                //}
                ///********************************************************************************/

                ///********************************************************************************/
                // Wireless image transfer test
                // create the xbee serial port
                //System.IO.Ports.SerialPort xbee =
                //    new System.IO.Ports.SerialPort("COM1", 57600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                // open the xbee
                //xbee.Open();
                //// send the image
                //// send the size of the image first
                //byte[] imagesize = new byte[5];
                //imagesize = Encoding.UTF8.GetBytes(pictureData.Length.ToString());
                //xbee.Write(imagesize, 0, imagesize.Length);
                //Thread.Sleep(3000);
                //// because the Xbee buffer is small,
                //// we must make sure not to overrun it when writing the byte to it.
                //for (int i = 0; i < pictureData.Length; i++)
                //{
                //    // send one element of the picture
                //    xbee.Write(pictureData, i, 1);
                //    // pause after 100 to prevent the xbee buffer overrun.
                //    if (i % 100 == 0)
                //        Thread.Sleep(50);
                //}
                //// get back the memory
                //pictureData = null;
                //// close the xbee.
                //xbee.Close();
                ///********************************************************************************/

                /********************************************************************************/
                // XBee wireless communications test
                // create serial port for XBEE. Connect XBEE_RX to Di1 and XBEE_TX to Di0.
                //System.IO.Ports.SerialPort xbee =
                //    new System.IO.Ports.SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);

                //xbee.Open();
                //// create "Hello!" string as bytes
                //byte[] sentmsg = Encoding.UTF8.GetBytes("Hello! Please tell me your name:\r");
                //byte[] receivedmsg = new byte[4];

                //// send string twice with a 1 sec gap between
                //while (true)
                //{
                //    xbee.Write(sentmsg, 0, sentmsg.Length);
                //    xbee.Read(receivedmsg, 0, receivedmsg.Length);
                //    char[] name = System.Text.UTF8Encoding.UTF8.GetChars(receivedmsg);
                //    string myname = "";
                //    for (int i = 0; i < name.Length; i++)
                //        myname += name[i];               

                //    string reply = "\rHello " + myname + "!\r";
                //    byte[] replymsg = Encoding.UTF8.GetBytes(reply);
                //    xbee.Write(replymsg, 0, replymsg.Length);
                //}
                //xbee.Close();
                /********************************************************************************/

                /********************************************************************************/
                //// clock test
                //// set the time to 9/9/2009 at 9:09:09
                //DateTime time = new DateTime(2010, 3, 9, 15, 32, 30);
                //Microsoft.SPOT.Hardware.Utility.SetLocalTime(time);
                //while (true)
                //{
                //    Debug.Print(DateTime.Now.ToString());
                //    Thread.Sleep(500);
                //}
                /********************************************************************************/

                /********************************************************************************/
                //// SD Card test
                //// ... check if SD is inserted
                //// SD Card is inserted
                //// Create a new storage device
                //PersistentStorage SDCard = new PersistentStorage("SD");
                //// Mount the file system as a volume
                //SDCard.MountFileSystem();
                //// Assume that SDCard was mounted as Volume 0, and access it through NETMF
                //string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
                //FileStream FileHandle = new FileStream(rootDirectory + "\\hello.txt", FileMode.Create);
                //byte[] data = Encoding.UTF8.GetBytes("This string will go in the file!");
                //FileStream FileHandle2 = new FileStream(rootDirectory + "\\hello2.txt", FileMode.Create);
                //byte[] data2 = Encoding.UTF8.GetBytes("This string will go in the other file!");
                //// write the data and close the file
                //FileHandle.Write(data, 0, data.Length);
                //FileHandle.Close();
                //FileHandle2.Write(data2, 0, data2.Length);
                //FileHandle2.Close();
                //// if we need to unmount
                //SDCard.UnmountFileSystem();
                /********************************************************************************/

                /********************************************************************************/
                // LED Test (On-board)
                // Create LED object from "onboard led" pin
                //FEZ_Components.LED OnBoardLED = new FEZ_Components.LED(FEZ_Pin.Digital.LED);
                //OnBoardLED.TurnOn();
                //Thread.Sleep(3000);
                //OnBoardLED.ShutOff();
                //OnBoardLED.StartBlinking(500, 500);
                //Thread.Sleep(100000);
                //OnBoardLED.Dispose();
                //OnBoardLED.StopBlinking();
                //OnBoardLED.Dispose();

                // LED Test (External)
                //FEZ_Components.LED myLED = new FEZ_Components.LED(FEZ_Pin.Digital.Di8);
                //myLED.ShutOff();
                //myLED.StartBlinking(500, 500);
                //Thread.Sleep(10000);
                //myLED.Dispose();
                /********************************************************************************/

                /********************************************************************************/
                // Servo Test
                // Create ServoMotor object assigned to the Servo Motor connected to Di8
                //FEZ_Components.ServoMotor MyServo = new FEZ_Components.ServoMotor(FEZ_Pin.Digital.Di8);
                //MyServo.SetPosition(0);
                //Thread.Sleep(2000);
                //MyServo.SetPosition(90);
                //Thread.Sleep(2000);
                //MyServo.SetPosition(180);
                //Thread.Sleep(2000);
                //MyServo.SetPosition(0);
                //Thread.Sleep(2000);
                //MyServo.Dispose();
                /********************************************************************************/

                /********************************************************************************/
                // DC Motor Driver Test
                // Initialize Motor Driver
                // 3-12-10: first powered roving test. result: good, but lost tread in turn.

                //// 3-14-10: first "intelligence" test. stop and turn when distance is too close.
                //FEZ_Components.DistanceDetector FrontDistanceDetector;
                //FrontDistanceDetector = new FEZ_Components.DistanceDetector(FEZ_Pin.AnalogIn.An2, FEZ_Components.DistanceDetector.SharpSensorType.GP2Y0A21YK);
                //FEZ_Shields.DCMotorDriver.Initialize();
                //Thread.Sleep(20000);
                //float distance;

                //FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 50);
                //while(true)
                //{
                //    distance = FrontDistanceDetector.GetDistance_cm();
                //    if (distance<15)
                //    {
                //       FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 10);
                //       FEZ_Shields.DCMotorDriver.MoveRamp(-100, 100, 10);
                //       Thread.Sleep(5000);
                //       FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 50);
                //    }
                //}


                //FrontDistanceDetector.Dispose();

                //FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 50);
                //Thread.Sleep(7000);
                //FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 10);
                //Thread.Sleep(2000);
                //FEZ_Shields.DCMotorDriver.MoveRamp(-50, 50, 50);
                //Thread.Sleep(10000);
                //FEZ_Shields.DCMotorDriver.MoveRamp(-50, -50, 10);
                //Thread.Sleep(3000);
                //FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 50);
                //Thread.Sleep(5000);
                //FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 10);
                // rotate the robot in half speed. motors are rotating in opposite direction of each others.
                //FEZ_Shields.DCMotorDriver.MoveRamp(-100, 100, 20);
                //Thread.Sleep(3000);
                // full stop
                //FEZ_Shields.DCMotorDriver.Move(0, 0);
                //// Move forward, acelerate gradually to (%50) half speed. the change is speed is 1% every 10 mSecond
                //FEZ_Shields.DCMotorDriver.MoveRamp(100, 100, 10);
                //Thread.Sleep(2000);
                //// stop gradually and very slowly
                //FEZ_Shields.DCMotorDriver.MoveRamp(0, 0, 10);
                //Thread.Sleep(2000);
                //// Move backward with full speed gradually.
                //FEZ_Shields.DCMotorDriver.MoveRamp(-10, -10, 20);
                //Thread.Sleep(2000);

                /********************************************************************************/

            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                //Microsoft.SPOT.Hardware.PowerState.RebootDevice(true);

            }
        }
    }
}