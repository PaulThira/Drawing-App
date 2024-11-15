using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.Sections
{
    public class MorphologicalOperations
    {
        public float PixelAbsoluteValue(Bgr pixel)
        {
            return (float)Math.Sqrt(pixel.Blue * pixel.Blue + pixel.Green * pixel.Green + pixel.Red * pixel.Red);
        }
        public  Image<Bgr, byte> Erodation(Image<Bgr, byte> image, int mask)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(image.Size);
            for (int y = (int)(mask / 2); y < result.Height - (int)(mask / 2); ++y)
            {
                for (int x = (int)(mask / 2); x < result.Width - (int)(mask / 2); ++x)
                {
                    List<float> listOfAbsoluteValues = new List<float>();
                    List<Bgr> pixels = new List<Bgr>();
                    for (int i = -(int)(mask / 2); i <= (int)(mask / 2); i++)
                    {
                        for (int j = -(int)(mask / 2); j <= (int)(mask / 2); j++)
                        {
                            listOfAbsoluteValues.Add(PixelAbsoluteValue(image[y + i, x + j]));
                            pixels.Add(image[y + i, x + j]);
                        }
                    }
                    float minim = listOfAbsoluteValues.Min();
                    int index = -1;
                    for (int i = 0; i < listOfAbsoluteValues.Count; i++)
                    {
                        if (listOfAbsoluteValues[i] == minim)
                        {
                            index = i;
                            break;
                        }
                    }
                    result[y, x] = pixels[index];


                }
            }
            return result;
        }
        public  Image<Bgr, byte> Dilatation(Image<Bgr, byte> image, int mask)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(image.Size);
            for (int y = (int)(mask / 2); y < result.Height - (int)(mask / 2); ++y)
            {
                for (int x = (int)(mask / 2); x < result.Width - (int)(mask / 2); ++x)
                {
                    List<float> listOfAbsoluteValues = new List<float>();
                    List<Bgr> pixels = new List<Bgr>();
                    for (int i = -(int)(mask / 2); i <= (int)(mask / 2); i++)
                    {
                        for (int j = -(int)(mask / 2); j <= (int)(mask / 2); j++)
                        {
                            listOfAbsoluteValues.Add(PixelAbsoluteValue(image[y + i, x + j]));
                            pixels.Add(image[y + i, x + j]);
                        }
                    }
                    float minim = listOfAbsoluteValues.Max();
                    int index = -1;
                    for (int i = 0; i < listOfAbsoluteValues.Count; i++)
                    {
                        if (listOfAbsoluteValues[i] == minim)
                        {
                            index = i;
                            break;
                        }
                    }
                    result[y, x] = pixels[index];


                }
            }
            return result;
        }
        public  Image<Bgr, byte> Opening(Image<Bgr, byte> image, int mask_dilatation, int mask_erodation)
        {
            Image<Bgr, byte> eroded = new Image<Bgr, byte>(image.Size);
            eroded = Erodation(image, mask_erodation);
            return Dilatation(eroded, mask_dilatation);
        }
        public Image<Bgr, byte> Closing(Image<Bgr, byte> image, int mask_dilatation, int mask_erodation)
        {
            Image<Bgr, byte> eroded = new Image<Bgr, byte>(image.Size);
            eroded = Dilatation(image, mask_erodation);
            return Erodation(eroded, mask_dilatation);
        }
    }
}
