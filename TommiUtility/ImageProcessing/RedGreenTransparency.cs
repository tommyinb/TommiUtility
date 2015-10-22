using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.ImageProcessing
{
    public sealed class RedGreenTransparency : IDisposable
    {
        public RedGreenTransparency(Bitmap redBitmap, Bitmap greenBitmap)
        {
            if (redBitmap.Size.Equals(greenBitmap.Size) == false)
            {
                throw new ArgumentException();
            }

            this.redBitmap = CopyBitmap(redBitmap);
            redData = this.redBitmap.LockBits(
                new Rectangle(Point.Empty, this.redBitmap.Size),
                ImageLockMode.ReadOnly, this.redBitmap.PixelFormat);

            this.greenBitmap = CopyBitmap(greenBitmap);
            greenData = this.greenBitmap.LockBits(
                new Rectangle(Point.Empty, this.greenBitmap.Size),
                ImageLockMode.ReadOnly, this.greenBitmap.PixelFormat);
        }
        private static Bitmap CopyBitmap(Bitmap bitmap)
        {
            var copyBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(copyBitmap))
            {
                graphics.DrawImage(bitmap, Point.Empty);
            }

            return bitmap;
        }
        public void Dispose()
        {
            redBitmap.UnlockBits(redData);
            redBitmap.Dispose();

            greenBitmap.UnlockBits(greenData);
            greenBitmap.Dispose();
        }

        private Bitmap redBitmap;
        private BitmapData redData;

        private Bitmap greenBitmap;
        private BitmapData greenData;

        public unsafe Bitmap Run()
        {
            var outputBitmap = new Bitmap(redBitmap.Width, redBitmap.Height, PixelFormat.Format32bppArgb);
            
            var outputData = outputBitmap.LockBits(
                new Rectangle(Point.Empty, outputBitmap.Size),
                ImageLockMode.WriteOnly, outputBitmap.PixelFormat);

            var redPointer = (uint*)redData.Scan0;
            var greenPointer = (uint*)greenData.Scan0;
            var outputPointer = (uint*)outputData.Scan0;

            for (int j = 0; j < outputBitmap.Height; j++)
            {
                for (int i = 0; i < outputBitmap.Width; i++,
                    redPointer++, greenPointer++, outputPointer++)
                {
                    var redPixel = *redPointer;
                    var greenPixel = *greenPointer;

                    var outputPixel = CalculatePixel(redPixel, greenPixel);
                    *outputPointer = outputPixel;
                }
            }

            outputBitmap.UnlockBits(outputData);

            return outputBitmap;
        }
        private uint CalculatePixel(uint redPixel, uint greenPixel)
        {
            var redR = (double)((redPixel >> 16) & 0xFF) / 0xFF;
            var greenR = (double)((greenPixel >> 16) & 0xFF) / 0xFF;
            var outputA = 1 - (redR - greenR);

            var outputR = greenR / outputA;

            var redG = (double)((redPixel >> 8) & 0xFF) / 0xFF;
            var outputG = redG / outputA;

            var redB = (double)(redPixel & 0xFF) / 0xFF;
            var outputB = redB / outputA;

            return ((uint)Math.Round(outputA * 0xFF, MidpointRounding.AwayFromZero) << 24)
                | ((uint)Math.Round(outputR * 0xFF, MidpointRounding.AwayFromZero) << 16)
                | ((uint)Math.Round(outputG * 0xFF, MidpointRounding.AwayFromZero) << 8)
                | (uint)Math.Round(outputB * 0xFF, MidpointRounding.AwayFromZero);
        }
    }

    [TestClass]
    public class RedGreenTransparencyTest
    {
        [TestMethod]
        public unsafe void Test()
        {
            var inputBitmap = new Bitmap(30, 30);
            using (var inputGraphics = Graphics.FromImage(inputBitmap))
            {
                inputGraphics.DrawEllipse(Pens.Red, new Rectangle(-5, -5, 25, 25));
                inputGraphics.DrawEllipse(Pens.Green, new Rectangle(10, 5, 25, 25));
                inputGraphics.DrawEllipse(Pens.Blue, new Rectangle(5, 15, 25, 25));

                var goldPen = new Pen(Color.FromArgb(150, Color.Gold), 5);
                inputGraphics.DrawEllipse(goldPen, new Rectangle(15, 15, 25, 25));
                inputGraphics.DrawEllipse(goldPen, new Rectangle(20, 5, 25, 25));
            }

            var redBitmap = new Bitmap(30, 30);
            using (var redGraphics = Graphics.FromImage(redBitmap))
            {
                var redPen = new Pen(Color.FromArgb(0xFF, 0, 0));
                redGraphics.FillRectangle(redPen.Brush, new Rectangle(-1, -1, 32, 32));

                redGraphics.DrawImage(inputBitmap, Point.Empty);
            }

            var greenBitmap = new Bitmap(30, 30);
            using (var greenGraphics = Graphics.FromImage(greenBitmap))
            {
                var greenPen = new Pen(Color.FromArgb(0, 0xFF, 0));
                greenGraphics.FillRectangle(greenPen.Brush, new Rectangle(-1, -1, 32, 32));

                greenGraphics.DrawImage(inputBitmap, Point.Empty);
            }

            var transparency = new RedGreenTransparency(redBitmap, greenBitmap);
            var outputBitmap = transparency.Run();

            var inputMemory = new MemoryStream();
            inputBitmap.Save(inputMemory, ImageFormat.Bmp);
            var inputBytes = inputMemory.ToArray();

            var outputMemory = new MemoryStream();
            outputBitmap.Save(outputMemory, ImageFormat.Bmp);
            var outputBytes = outputMemory.ToArray();

            for (int i = 0; i < inputBytes.Length; i++)
            {
                var inputByte = inputBytes[i];
                var outputByte = outputBytes[i];

                var difference = Math.Abs(inputByte - outputByte);
                Assert.IsTrue(difference <= 1);
            }
        }
    }
}
