using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using IronOcr;
using IronSoftware.Drawing;

namespace ccUi
{
    public class OcrUtil
    {
        public static void DoOcr(string fileName)
        {
            IronTesseract ocr = new IronTesseract();
            ocr.Configuration.ReadBarCodes = false;
            ocr.Configuration.ReadDataTables = false;
            ocr.Language = OcrLanguage.English;

            var bmp = ToGrayScale(Bitmap.FromFile(fileName) as Bitmap);
            bmp.Save("temp.bmp");
            using (OcrInput input = new OcrInput("temp.bmp"))
            {
                //input.DeNoise();
                OcrResult result = ocr.Read(input);
                foreach (var blk in result.Blocks)
                {
                    Console.WriteLine(blk);
                }
            }
        }


        public static Bitmap ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            System.Drawing.Color c;

            for (int y = 0; y < Bmp.Height; y++)
            {
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    rgb /= 100;
                    rgb *= 100;
                    Bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(rgb, rgb, rgb));
                }
            }
            return Bmp;
        }
    }
}
   