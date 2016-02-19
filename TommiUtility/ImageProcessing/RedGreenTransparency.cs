using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(redBitmap != null);
            Contract.Requires<ArgumentNullException>(greenBitmap != null);

            if (redBitmap.Size.Equals(greenBitmap.Size) == false)
            {
                throw new ArgumentException();
            }

            this.redBitmap = CopyBitmap(redBitmap);
            var redRectangle = new Rectangle(Point.Empty, redBitmap.Size);
            redData = redBitmap.LockBits(redRectangle, ImageLockMode.ReadOnly, redBitmap.PixelFormat);

            this.greenBitmap = CopyBitmap(greenBitmap);
            var greenRectangle = new Rectangle(Point.Empty, greenBitmap.Size);
            greenData = greenBitmap.LockBits(greenRectangle, ImageLockMode.ReadOnly, greenBitmap.PixelFormat);
        }
        private static Bitmap CopyBitmap(Bitmap bitmap)
        {
            Contract.Requires<ArgumentNullException>(bitmap != null);
            Contract.Ensures(Contract.Result<Bitmap>() != null);

            var copyBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);

            Contract.Assume((copyBitmap.PixelFormat & PixelFormat.Indexed) == 0);
            using (var graphics = Graphics.FromImage(copyBitmap))
            {
                graphics.DrawImage(bitmap, Point.Empty);
            }

            return copyBitmap;
        }
        public void Dispose()
        {
            redBitmap.UnlockBits(redData);
            redBitmap.Dispose();

            greenBitmap.UnlockBits(greenData);
            greenBitmap.Dispose();
        }

        private readonly Bitmap redBitmap;
        private readonly BitmapData redData;
        private readonly Bitmap greenBitmap;
        private readonly BitmapData greenData;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(redBitmap != null);
            Contract.Invariant(redData != null);
            Contract.Invariant(greenBitmap != null);
            Contract.Invariant(greenData != null);
        }

        public unsafe Bitmap Run()
        {
            Contract.Ensures(Contract.Result<Bitmap>() != null);

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
            var inputBitmap = new Bitmap(30, 30, PixelFormat.Format32bppArgb);
            Contract.Assume((inputBitmap.PixelFormat & PixelFormat.Indexed) == 0);
            using (var inputGraphics = Graphics.FromImage(inputBitmap))
            {
                inputGraphics.DrawEllipse(Pens.Red, new Rectangle(-5, -5, 25, 25));
                inputGraphics.DrawEllipse(Pens.Green, new Rectangle(10, 5, 25, 25));
                inputGraphics.DrawEllipse(Pens.Blue, new Rectangle(5, 15, 25, 25));

                var goldPen = new Pen(Color.FromArgb(150, Color.Gold), 5);
                inputGraphics.DrawEllipse(goldPen, new Rectangle(15, 15, 25, 25));
                inputGraphics.DrawEllipse(goldPen, new Rectangle(20, 5, 25, 25));
            }

            var redBitmap = new Bitmap(30, 30, PixelFormat.Format32bppArgb);
            Contract.Assume((redBitmap.PixelFormat & PixelFormat.Indexed) == 0);
            using (var redGraphics = Graphics.FromImage(redBitmap))
            {
                var redPen = new Pen(Color.FromArgb(0xFF, 0, 0));
                redGraphics.FillRectangle(redPen.Brush, new Rectangle(-1, -1, 32, 32));

                redGraphics.DrawImage(inputBitmap, Point.Empty);
            }

            var greenBitmap = new Bitmap(30, 30, PixelFormat.Format32bppArgb);
            Contract.Assume((greenBitmap.PixelFormat & PixelFormat.Indexed) == 0);
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

            Assert.AreEqual(inputBytes.Length, outputBytes.Length);
            Contract.Assume(outputBytes.Length == inputBytes.Length);
            for (int i = 0; i < inputBytes.Length; i++)
            {
                var inputByte = inputBytes[i];
                var outputByte = outputBytes[i];

                var difference = inputByte - outputByte;
                var differenceWithinOne = difference <= 1 && difference >= -1;
                Assert.IsTrue(differenceWithinOne);
            }
        }
    }
}
