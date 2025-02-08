using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Algorithms.Sections
{
    public class BlendingModes
    {
        public Image<Bgr,byte> Multiply(Image<Bgr,byte> image1,Image<Bgr,byte> image2)
        {
            int maxWidth=(int)(Math.Max(image1.Width, image2.Width));
            int maxHeight=(int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0,0,0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width&&j<image2.Width) {
                        result.Data[i, j, 0] = (byte)Math.Min((image1.Data[i, j, 0] * image2.Data[i, j, 0]) / 255, 255);
                        result.Data[i, j, 1] = (byte)Math.Min((image1.Data[i, j, 1] * image2.Data[i, j, 1]) / 255, 255);
                        result.Data[i, j, 2] = (byte)Math.Min((image1.Data[i, j, 2] * image2.Data[i, j, 2]) / 255, 255);
                    }
                    
                }
            }
            return result;
        }
        public Image<Bgr, byte> Screen(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min(255-((255-image1.Data[i, j, 0]) * (255-image2.Data[i, j, 0])) / 255, 255);
                        result.Data[i, j, 1] = (byte)Math.Min(255-((255-image1.Data[i, j, 1]) * (255-image2.Data[i, j, 1])) / 255, 255);
                        result.Data[i, j, 2] = (byte)Math.Min(255-((255-image1.Data[i, j, 2]) * (255-image2.Data[i, j, 2])) / 255, 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Overlay(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                       double red,green, blue;
                        blue = image2.Data[i, j,0];
                        green= image2.Data[i, j,1];
                        red=image2.Data[i, j,2];
                      
                        if (blue<128)
                        {
                            result.Data[i, j, 0] = (byte)Math.Min(2*(image1.Data[i, j, 0] * image2.Data[i, j, 0]) / 255, 255);
                           
                        }
                        else
                        {
                            result.Data[i, j, 0] = (byte)Math.Min(2*(255 - ((255 - image1.Data[i, j, 0]) * (255 - image2.Data[i, j, 0])) / 255), 255);
                           
                        }
                        if (green < 128)
                        {
                            result.Data[i, j, 1] = (byte)Math.Min(2 * (image1.Data[i, j, 1] * image2.Data[i, j, 1]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 1] = (byte)Math.Min(2 * (255 - ((255 - image1.Data[i, j, 1]) * (255 - image2.Data[i, j, 1])) / 255), 255);

                        }
                        if (red < 128)
                        {
                            result.Data[i, j, 2] = (byte)Math.Min(2 * (image1.Data[i, j, 2] * image2.Data[i, j, 2]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 2] = (byte)Math.Min(2 * (255 - ((255 - image1.Data[i, j, 2]) * (255 - image2.Data[i, j, 2])) / 255), 255);

                        }
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Add(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min(image1.Data[i, j, 0] + image2.Data[i, j, 0], 255);
                        result.Data[i, j, 1] = (byte)Math.Min(image1.Data[i, j, 1] + image2.Data[i, j, 1], 255);
                        result.Data[i, j, 2] = (byte)Math.Min(image1.Data[i, j, 2] + image2.Data[i, j, 2], 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Substract(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Max(image1.Data[i, j, 0] - image2.Data[i, j, 0], 255);
                        result.Data[i, j, 1] = (byte)Math.Max(image1.Data[i, j, 1] - image2.Data[i, j, 1], 255);
                        result.Data[i, j, 2] = (byte)Math.Max(image1.Data[i, j, 2] - image2.Data[i, j, 2], 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Difference(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min(Math.Abs(image1.Data[i, j, 0] - image2.Data[i, j, 0]), 255);
                        result.Data[i, j, 1] = (byte)Math.Min(Math.Abs(image1.Data[i, j, 1] - image2.Data[i, j, 1]), 255);
                        result.Data[i, j, 2] = (byte)Math.Min(Math.Abs(image1.Data[i, j, 2] - image2.Data[i, j, 2]), 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Lighten(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Max(image1.Data[i,j,0],image2.Data[i,j,0]);  
                        result.Data[i, j, 1] = (byte)Math.Max(image1.Data[i, j, 1], image2.Data[i, j, 1]);
                        result.Data[i, j, 2] = (byte)Math.Max(image1.Data[i, j, 2], image2.Data[i, j, 2]);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Darken(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Max(image1.Data[i, j, 0], image2.Data[i, j, 0]);
                        result.Data[i, j, 1] = (byte)Math.Max(image1.Data[i, j, 1], image2.Data[i, j, 1]);
                        result.Data[i, j, 2] = (byte)Math.Max(image1.Data[i, j, 2], image2.Data[i, j, 2]);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> SoftLight(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        double red, green, blue;
                        blue = image2.Data[i, j, 0];
                        green = image2.Data[i, j, 1];
                        red = image2.Data[i, j, 2];

                        if (blue < 128)
                        {
                            result.Data[i, j, 0] = (byte)Math.Min( (image1.Data[i, j, 0] * image2.Data[i, j, 0]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 0] = (byte)Math.Min( (255 - ((255 - image1.Data[i, j, 0]) * (255 - image2.Data[i, j, 0])) / 255), 255);

                        }
                        if (green < 128)
                        {
                            result.Data[i, j, 1] = (byte)Math.Min( (image1.Data[i, j, 1] * image2.Data[i, j, 1]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 1] = (byte)Math.Min( (255 - ((255 - image1.Data[i, j, 1]) * (255 - image2.Data[i, j, 1])) / 255), 255);

                        }
                        if (red < 128)
                        {
                            result.Data[i, j, 2] = (byte)Math.Min( (image1.Data[i, j, 2] * image2.Data[i, j, 2]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 2] = (byte)Math.Min( (255 - ((255 - image1.Data[i, j, 2]) * (255 - image2.Data[i, j, 2])) / 255), 255);

                        }
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> HardLight(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        double red, green, blue;
                        blue = image2.Data[i, j, 0];
                        green = image2.Data[i, j, 1];
                        red = image2.Data[i, j, 2];

                        if (blue < 128)
                        {
                            result.Data[i, j, 0] = (byte)Math.Min(2 * (image1.Data[i, j, 0] * image2.Data[i, j, 0]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 0] = (byte)Math.Min(255- (2*((255 - image1.Data[i, j, 0]) * (255 - image2.Data[i, j, 0])) / 255), 255);

                        }
                        if (green < 128)
                        {
                            result.Data[i, j, 1] = (byte)Math.Min(2 * (image1.Data[i, j, 1] * image2.Data[i, j, 1]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 1] = (byte)Math.Min(255 - (2* ((255 - image1.Data[i, j, 1]) * (255 - image2.Data[i, j, 1])) / 255), 255);

                        }
                        if (red < 128)
                        {
                            result.Data[i, j, 2] = (byte)Math.Min(2 * (image1.Data[i, j, 2] * image2.Data[i, j, 2]) / 255, 255);

                        }
                        else
                        {
                            result.Data[i, j, 2] = (byte)Math.Min(255 - (2* ((255 - image1.Data[i, j, 2]) * (255 - image2.Data[i, j, 2])) / 255), 255);

                        }
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Divide(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min((image1.Data[i, j, 0]*255)/( image2.Data[i, j, 0]+1),255);
                        result.Data[i, j, 1] = (byte)Math.Min((image1.Data[i, j, 1] * 255) / (image2.Data[i, j, 1] + 1), 255);
                        result.Data[i, j, 2] = (byte)Math.Min((image1.Data[i, j, 2] * 255) / (image2.Data[i, j, 2] + 1), 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> ColorBurn(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min(255-(image1.Data[i, j, 0] * 255) / (image2.Data[i, j, 0] + 1), 255);
                        result.Data[i, j, 1] = (byte)Math.Min(255-(image1.Data[i, j, 1] * 255) / (image2.Data[i, j, 1] + 1), 255);
                        result.Data[i, j, 2] = (byte)Math.Min(255-(image1.Data[i, j, 2] * 255) / (image2.Data[i, j, 2] + 1), 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> MergeLayers(Image<Bgr, byte> layerA, Image<Bgr, byte> layerB)
        {
            int width = Math.Min(layerA.Width, layerB.Width);
            int height = Math.Min(layerA.Height, layerB.Height);

            Image<Bgr, byte> result = layerA.Clone(); // Start with the base layer

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Bgr pixelB = layerB[y, x]; // Get pixel from Layer B
                    if (pixelB.Blue != 0 || pixelB.Green != 0 || pixelB.Red != 0) // Assuming black = transparent
                    {
                        result[y, x] = pixelB; // Overwrite pixel
                    }
                }
            }
            return result;
        }

        public Image<Bgr, byte> ColorDoge(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min((image1.Data[i, j, 0] * 255) / (255-image2.Data[i, j, 0]+0.001), 255);
                        result.Data[i, j, 1] = (byte)Math.Min((image1.Data[i, j, 1] * 255) / (255-image2.Data[i, j, 1]+0.001), 255);
                        result.Data[i, j, 2] = (byte)Math.Min((image1.Data[i, j, 2] * 255) / (255 - image2.Data[i, j, 2] + 0.001), 255);
                    }

                }
            }
            return result;
        }
        public Image<Bgr, byte> Exclusion(Image<Bgr, byte> image1, Image<Bgr, byte> image2)
        {
            int maxWidth = (int)(Math.Max(image1.Width, image2.Width));
            int maxHeight = (int)(Math.Max(image1.Height, image2.Height));
            Image<Bgr, byte> result = new Image<Bgr, byte>(maxWidth, maxHeight);
            result.SetValue(new Bgr(0, 0, 0));
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    if (i < image1.Height && i < image2.Height && j < image1.Width && j < image2.Width)
                    {
                        result.Data[i, j, 0] = (byte)Math.Min(image1.Data[i, j, 0]+ image2.Data[i, j, 0]-2 * (image1.Data[i, j, 0] * image2.Data[i, j, 0]) / 255, 255);
                        result.Data[i, j, 1] = (byte)Math.Min(image1.Data[i, j, 1] + image2.Data[i, j, 1] - 2 * (image1.Data[i, j, 1] * image2.Data[i, j, 1]) / 255, 255);
                        result.Data[i, j, 2] = (byte)Math.Min(image1.Data[i, j, 2] + image2.Data[i, j, 2] - 2 * (image1.Data[i, j, 2] * image2.Data[i, j, 2]) / 255, 255);
                    }

                }
            }
            return result;
        }

    }
}
