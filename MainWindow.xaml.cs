using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SecretMessageEncoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }//end main
        
        //instance of filepath to use in events
        public string filePath = "";

        //instance of portable pixmap to use in events
        PortablePixMap PPM = new PortablePixMap();

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            //create instance of openfile dialog 
            OpenFileDialog ofdTemp = new OpenFileDialog();

            //filters only the desired file types 
            ofdTemp.Filter = "PPM Files|*.ppm";

            //displays dialog box and checks if is opened 
            bool fileSelected = ofdTemp.ShowDialog() == true;

            //keep record of file path 
            filePath = ofdTemp.FileName;

            //if user selected a file then open the image and send to imagebox
            if (fileSelected)
            {//then
                PPM = new PortablePixMap(filePath);
                //ppmFile = new PPM(filePath);

                ImageBox.Source = PPM.MakeBitmap();

            }//end if
        }//end event

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            //Displays a SaveFileDialog so the user can save the Image
            SaveFileDialog saveFile = new SaveFileDialog();

            //filter
            saveFile.Filter = "PPM Files|*.ppm";
            
            //checks if dialog is opened 
            bool fileSelected = saveFile.ShowDialog() == true;

            if (fileSelected)//then
            {
                //new instance streamreader, reads file
                StreamReader fileReader = new StreamReader(filePath);

                //read 1st line, assign value
                string format = fileReader.ReadLine();

                fileReader.Close();

                //Checks the format of the ppm file
                if (format == "P3")
                {
                    PPM.SaveP3(saveFile.FileName, PPM);
                }
                else if (format == "P6")
                {
                    PPM.SaveP6(saveFile.FileName, PPM);
                } else
                {
                    throw new Exception("Incorrect format");
                }//end if
            }//end if
        }//end event

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {

            //check for txtbox = null or imagebox = null
            if (txtBox.Text == String.Empty || ImageBox.Source == null)
            {
                MessageBox.Show("Upload a PPM image and enter a message to encrypt.");
            }
            else if (txtBox.Text.Length > 256)//check for message length
            {
                MessageBox.Show("Message is too long. Must be 255 characters or less");
            }
            else
            {                
                //call on embedText function to encode string
                PPM.embedText(txtBox.Text);

                MessageBox.Show("Message encrypted");
            }//end if


        }//end event

        private void txtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //handles null exception
            if (charCountBox != null)
            {
               charCountBox.Text = txtBox.Text.Length.ToString();
            }//end if
        }//end event

        private void MenuItem_Help_Click(object sender, RoutedEventArgs e)
        {
            //content for "Help" pop up
            string content =
                "• Open any image with a PPM extension.\n\n" +
                "• Once image has loaded, enter the message you want to encode into the message window, and click Encrypt.\n\n" +
                "• Save the newly encoded image as a PPM under 'Save'\n\n" +
                "• Exit the program and open the decoder program to decrypt the message\n\n" +
                "• Choose colorful, noisy images if possible, favoring BLUE, then RED then GREEN.";
            string header = "Instructions";
            MessageBox.Show(content, header);
        }//end event
    }//end class
}//end namespace
