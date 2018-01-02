using ccInfo;
using System;
using System.Drawing;

namespace ccVcontrol
{
    public class CommandInfo : ccPoint
    {
        public string command;        
        public decimal cmpRes;
        public string Text;
        public string decision;
        public string extraInfo;

        public ImgChecksAndTags imgInfo;
        public override string ToString()
        {
            return $"CommandInfo: {command} ({x},{y}):{cmpRes} {Text} d={decision} {extraInfo}";
        }
    }

    public class ImgChecksAndTags
    {
        public string ImageName { get; private set; }
        public ccPoint center;
        public decimal Threadshold;
        public int ImgSize { get; private set; }
        public ImgChecksAndTags(string imgName, decimal threadHoldScala = (decimal)0.09)
        {
            ImageName = imgName;

            var img = new Bitmap(ImageName);
            center = new ccPoint(img.Width / 2, img.Height / 2);
            ImgSize = img.Width * img.Height;
            Threadshold = ImgSize * threadHoldScala;

        }
        public override string ToString()
        {
            return $"{ImageName} {center.x}/{center.y} ";
        }
    }
}
