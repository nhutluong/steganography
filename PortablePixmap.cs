using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace SecretMessageEncoder
{
    internal class PortablePixMap
    {
        //special "class" that represents a group of constants(unchangeable/read-only variables)
        private enum State
        {   
            //constants to use in encryption
            Hiding,
            Filling_With_Zeros
        };

        private string _readFirstLine;//string to hold first line value

        //list to hold value of the encoded list
        public List<string> encodedList = new List<string>();

        //list to hold ppm p3 file for saving
        public List<string> p3List = new List<string>();

        //list to hold ppm p6 file for saving
        public byte[] p6List;

        //BITMAP'S SIZE.
        private int _width;
        private int _height;

        //PIXEL ARRAY.
        private byte[] _pixelData;

        //NUMBER OF BYTES PER ROW.
        private int _stride;

        //properties
        public int Width
        {
            get { return _width; }
        }//end property
        public int Height
        {
            get { return _height; }
        }//end property

        //constructors
        public PortablePixMap()
        {
            //no paramaters
        }//end constructor

        public PortablePixMap(string filePath)
        {            
            FileType(filePath);
        }//end constructor

        //methods
        private void FileType(string filePath)
        {
            //read the first line
            _readFirstLine = File.ReadLines(filePath).First();

            if (_readFirstLine.Contains("P3"))//then
            {
                //load p3 file
                LoadP3(filePath);
            }
            else if (_readFirstLine.Contains("P6"))//then
            {
                //load p6 file
                LoadP6(filePath);
            }
            else
            {
                throw new Exception("Incorrect image format");
            }//end if
        }//end method

        public void LoadP3(string filePath)
        {
            //new instance of stream reader
            StreamReader fileReader = new StreamReader(filePath);

            //read first 4 lines of ppm file and assign to string
            string ppmType = fileReader.ReadLine();
            string ppmComment = fileReader.ReadLine();
            string ppmSize = fileReader.ReadLine();
            string ppmRGBMax = fileReader.ReadLine();

            //add first 4 lines to list
            p3List.Add(ppmType.ToString());
            p3List.Add(ppmComment.ToString());
            p3List.Add(ppmSize.ToString());
            p3List.Add(ppmRGBMax.ToString());

            //split values of 3rd line and store the values
            string[] fileValues = ppmSize.Split();
            _width = Convert.ToInt32(fileValues[0]);
            _height = Convert.ToInt32(fileValues[1]);
            int counter = 0;

            //get pixel data
            _pixelData = new byte[_width * _height * 4];

            //calculate stride
            _stride = _width * 4;
                        
            if (ppmRGBMax.Contains("255"))
            {
                //retrieve remaining bytes
                while (!fileReader.EndOfStream)
                {
                    //new instance of color
                    Color imgColor = new Color();

                    //convert to int by reading each line
                    int blue = Convert.ToInt32(fileReader.ReadLine());
                    int green = Convert.ToInt32(fileReader.ReadLine());
                    int red = Convert.ToInt32(fileReader.ReadLine());

                    //reads colors from file, convert to byte, adds to list
                    imgColor.R = Convert.ToByte(red);
                    p3List.Add(red.ToString());

                    imgColor.G = Convert.ToByte(green);
                    p3List.Add(green.ToString());

                    imgColor.B = Convert.ToByte(blue);
                    p3List.Add(blue.ToString());

                    //set color in position
                    _pixelData[counter++] = imgColor.R;
                    _pixelData[counter++] = imgColor.G;
                    _pixelData[counter++] = imgColor.B;
                    _pixelData[counter++] = 255;

                }//end while
            }//end if   

            //close stream reader
            fileReader.Close();
        }//end method

        public void LoadP6(string filePath) {

            //read all bytes to byte array
            byte[] fileBytes = File.ReadAllBytes(filePath);
            //add bytes to list
            p6List = fileBytes;
            encodedList.Add(p6List.ToString());

            //declare instance of streamreader, read file
            StreamReader fileReader = new StreamReader(filePath);
            string counter = string.Empty;

            //line 1, read ppm type
            string ppmType = fileReader.ReadLine();

            if (!ppmType.Contains("P6"))//then
            {
                throw new Exception("Exception: could not read. Not ppm P6 format");
            }//end if

            counter += ppmType + "\n";            

            //line 2, read line and skip
            string ppmComment = fileReader.ReadLine();
            counter += ppmComment + "\n";


            //line 3, get image width and height
            string ppmSize = fileReader.ReadLine();
            string[] ppmDimensionValues = ppmSize.Split(" ");

            _width = Convert.ToInt32(ppmDimensionValues[0]);
            _height = Convert.ToInt32(ppmDimensionValues[1]);
            
            counter += ppmSize + "\n";

            //line 4, read max rgb
            string ppmRGBMax = fileReader.ReadLine();
            counter += ppmRGBMax + "\n";

            //close streamreader
            fileReader.Close();            

            //get pixel data
            _pixelData = new byte[_width * _height * 4];

            //calculate stride
            _stride = _width * 4;

            //line 5 ++
            int count = 0;

            //new instance of binaryreader, read file
            BinaryReader binaryReader = new BinaryReader(new FileStream(filePath, FileMode.Open));

            //loop to index position
            while (count < counter.Length)
            {
                char read = binaryReader.ReadChar();
                count++;
            }//end while

            //reset count
            count = 0;
                        
            //determine if end of file reached, current position compared to pixel lenght of image
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                //new instance of color
                Color imgColor = new Color();

                //reads colors from file, convert to byte
                imgColor.B = binaryReader.ReadByte();
                
                imgColor.G = binaryReader.ReadByte();
                
                imgColor.R = binaryReader.ReadByte();
                
                //set color in position
                _pixelData[count++] = imgColor.R;
                _pixelData[count++] = imgColor.G;
                _pixelData[count++] = imgColor.B;
                _pixelData[count++] = 255;
            }//end while

            //close binaryreader
            binaryReader.Close();

        }//end method

        public WriteableBitmap MakeBitmap()
        {
            // Create the WriteableBitmap.
            int dpi = 96;

            WriteableBitmap wbitmap = new WriteableBitmap(_width, _height, dpi, dpi, PixelFormats.Bgra32, null);

            // Load the pixel data.
            Int32Rect rect = new Int32Rect(0, 0, _width, _height);
            wbitmap.WritePixels(rect, _pixelData, _stride, 0);

            // Return the bitmap.
            return wbitmap;
        }//end method

        public Color GetPixelColor(int x, int y)
        {
            //GET PIXEL DATA
            byte[] pixelComponentData = GetPixelData(x, y);

            //CREAT COLOR INSTANCE
            Color returnColor = new Color();

            //POPULATE COLOR INSTANCE DATA THEN RETURN
            returnColor.R = pixelComponentData[0];
            returnColor.G = pixelComponentData[1];
            returnColor.B = pixelComponentData[2];
            returnColor.A = pixelComponentData[3];

            return returnColor;
        }//end method

        public byte[] GetPixelData(int x, int y)
        {
            //STARTING PIXEL INDEX
            int index = y * _stride + x * 4;

            //GET PIXEL COMPONENT VALUES 
            byte blu = _pixelData[index++];// ++ to march forward to get next component 
            byte grn = _pixelData[index++];
            byte red = _pixelData[index++];
            byte alp = _pixelData[index];

            //RETURN DATA
            return new byte[] { red, grn, blu, alp };
        }//end method
        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        /// <param name="alpha">0-255 level of alpha</param>
        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int index = y * _stride + x * 4;
            _pixelData[index++] = blue;
            _pixelData[index++] = green;
            _pixelData[index++] = red;
            _pixelData[index++] = alpha;
        }//end method

        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        public void SetPixel(int x, int y, byte red, byte green, byte blue)
        {
            SetPixel(x, y, red, green, blue, 255);
        }//end method

        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="color">Color instance</param>
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);//stackoverflow
        }//end method

        /// <summary>
        /// Set all pixels to a specific color.
        /// </summary>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        /// <param name="alpha">0-255 level of alpha</param>
        public void SetPixels(byte red, byte green, byte blue, byte alpha)
        {
            int byteCount = _width * _height * 4;
            int index = 0;

            while (index < byteCount)
            {
                _pixelData[index++] = blue;
                _pixelData[index++] = green;
                _pixelData[index++] = red;
                _pixelData[index++] = alpha;
            }//end while
        }//end method

        /// <summary>
        /// Set all pixels to a specific color.
        /// </summary>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        public void SetPixels(byte red, byte green, byte blue)
        {
            SetPixels(red, green, blue, 255);
        }//end method
 
        public void SaveP3(string filePath, PortablePixMap ppm)
        {
            //Use streamwriter to write file
            var writer = new StreamWriter(filePath);
            //write first 4 lines 
            writer.Write("P3" + "\n");
            writer.Write("# Created by GIMP version 2.10.30 PNM plug-in..." + "\n");
            writer.Write($"{ppm.Width} {ppm.Height}" + "\n");
            writer.Write("255" + "\n");
            
            //for loop through height and width to write color data        
            for (int x = 0; x < ppm.Height; x++)
            {
                for (int y = 0; y < ppm.Width; y++)
                {
                    Color color = ppm.GetPixelColor(y, x);
                    writer.Write(color.R.ToString() + "\n");
                    writer.Write(color.G.ToString() + "\n");
                    writer.Write(color.B.ToString() + "\n");
                }//end for
            }//end for

            //close the stream writer
            writer.Close();
        }//end method
        public void SaveP6(string filePath, PortablePixMap ppm)
        {
            //Use streamwriter to write file
            var writer = new StreamWriter(filePath);
            //write first 4 lines
            writer.Write("P6" + "\n");
            writer.Write("# Created by GIMP version 2.10.30 PNM plug-in..." + "\n");
            writer.Write($"{ppm.Width} {ppm.Height}" + "\n");
            writer.Write("255" + "\n");

            //close stream writer
            writer.Close();

            //Switch to binary writer to write the data
            var binaryWriter = new BinaryWriter(new FileStream(filePath, FileMode.Append));

            //for loop through height and width to write color data
            for (int x = 0; x < ppm.Height; x++)
            {
                for (int y = 0; y < ppm.Width; y++)
                {
                    Color color = ppm.GetPixelColor(y, x);
                    binaryWriter.Write(color.R);
                    binaryWriter.Write(color.G);
                    binaryWriter.Write(color.B);
                }//end for
            }//end for

            //close binary writer
            binaryWriter.Close();
        }//end method

        public void Encryption(string message)
        {
            var ascii = new List<int>();//to store individual value of pixels
            foreach (char character in message)
            {
                int asciiValue = Convert.ToInt16(character);//convert to ascii
                var firstDigit = asciiValue / 1000;//extract first digit 
                var secondDigit = (asciiValue - (firstDigit * 1000)) / 100; //Extract the second digit 
                var thirdDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100))) / 10;//Extract the third digit
                var fourthDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100) + (thirdDigit * 10)));

                ascii.Add(firstDigit); //Add the first digit 
                ascii.Add(secondDigit); // Add the second digit 
                ascii.Add(thirdDigit); // Add the third digit 
                ascii.Add(fourthDigit); // Add the fourth digit 
            }//end foreach

            var count = 0;//have a count

            for (int row = 0; row < _width; row++)
            {
                for (int column = 0; column < _height; column++)
                {
                    Color color = GetPixelColor(row, column);

                    // Set ascii value in A of the pixel
                    SetPixel(row, column, Color.FromArgb((byte)(color.A - ((count < ascii.Count) ? ascii[count] : 0)), color.R, color.G, color.B)); 

                    count++;
                }//end for
               
            }//end for
            
        }//end method

        public void embedText(string text)
        {
            //initially set state to constant hiding
            State state = State.Hiding;

            //holds the index of the character that is being hidden
            int charIndex = 0;

            //holds the value of the character converted to integer
            int charValue = 0;

            //holds the index of the color element (R or G or B) that is currently being processed
            long pixelElementIndex = 0;

            //holds the number of trailing zeros that have been added when finishing the process
            int zeros = 0;

            //hold pixel elements
            int R = 0, G = 0, B = 0;

            //pass through height
            for (int i = 0; i < _height; i++)
            {
                //pass through width
                for (int j = 0; j < _width; j++)
                {
                    //holds the pixel that is currently being processed
                    Color pixel = GetPixelColor(j, i);

                    //clear the least significant bit (LSB) from each pixel element
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    //for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        //check if new 8 bits has been processed
                        if (pixelElementIndex % 8 == 0)
                        {
                            //check if the whole process has finished
                            //finished when 8 zeros are added
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                //apply the last pixel on the image                                
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    //set the pixels
                                    SetPixel(j, i, Color.FromRgb((byte)R, (byte)G, (byte)B));
                                }//end if

                                //return the bitmap with the text hidden in
                               //return bmp;
                            }//end if

                            // check if all characters has been hidden
                            if (charIndex >= text.Length)
                            {
                                //start adding zeros to mark the end of the text
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                //move to the next character and process again
                                charValue = text[charIndex++];
                            }//end if
                        }//end if

                        // check which pixel element has the turn to hide a bit in its LSB
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        //the rightmost bit in the character                                       
                                        R += charValue % 2;

                                        //removes the added rightmost bit of the character                                        
                                        charValue /= 2;
                                    }//end if
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }//end if
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }//end if

                                    SetPixel(j, i, Color.FromRgb((byte)R, (byte)G, (byte)B));
                                }
                                break;
                        }//end switch case

                        //increment color element index
                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            //increment the value of zeros until it is 8
                            zeros++;
                        }//end if
                    }//end for
                }//end for
            }//end for

            //return bmp;
        }//end method

        public static string extractText(PortablePixMap ppm)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            // holds the text that will be extracted from the image
            string extractedText = String.Empty;

            // pass through the rows
            for (int i = 0; i < ppm.Height; i++)
            {
                // pass through each row
                for (int j = 0; j < ppm.Width; j++)
                {
                    Color pixel = ppm.GetPixelColor(j, i);

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // get the LSB from the pixel element (will be pixel.R % 2)
                                    // then add one bit to the right of the current character
                                    // this can be done by (charValue = charValue * 2)
                                    // replace the added bit (which value is by default 0) with
                                    // the LSB of the pixel element, simply by addition
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }//end switch

                        colorUnitIndex++;

                        // if 8 bits has been added, then add the current character to the result text
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse? of course, since each time the process happens on the right (for simplicity)
                            charValue = reverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            // convert the character value from int to char
                            char c = (char)charValue;

                            // add the current character to the result text
                            extractedText += c.ToString();
                        }//end if
                    }//end for
                }//end for
            }//end for

            return extractedText;
        }//end method

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }//end for

            return result;
        }//end method

    }//end class
}//end namespace
