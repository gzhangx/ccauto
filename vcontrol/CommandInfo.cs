﻿using ccInfo;
using System;
using System.Drawing;

namespace ccVcontrol
{
    public class CommandInfo
    {
        public string command;
        public int x;
        public int y;
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
        public ImgChecksAndTags(string imgName, decimal threadshold = 200000)
        {
            ImageName = imgName;

            var img = new Bitmap(ImageName);
            center = new ccPoint(img.Width / 2, img.Height / 2);
            ImgSize = img.Width * img.Height;
            Threadshold = ImgSize * (decimal)0.09;

        }
        public override string ToString()
        {
            return $"{ImageName} {center.x}/{center.y} ";
        }
    }
}
