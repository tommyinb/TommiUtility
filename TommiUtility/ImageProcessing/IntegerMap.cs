using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TommiUtility.Test;

namespace TommiUtility.ImageProcessing
{
    public class IntegerMap : IDisposable
    {
        public readonly Bitmap Bitmap;
        private readonly BitmapData bitmapData;

        public IntegerMap(Bitmap bitmap)
        {
            Contract.Requires<ArgumentNullException>(bitmap != null);

            Bitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(this.Bitmap))
            {
                graphics.DrawImage(bitmap, 0, 0);
            }

            var rectangle = new Rectangle(Point.Empty, Bitmap.Size);
            bitmapData = Bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        }
        public void Dispose()
        {
            Bitmap.UnlockBits(bitmapData);
            Bitmap.Dispose();
        }

        public unsafe uint this[int x, int y]
        {
            get
            {
                Contract.Requires<ArgumentException>(x >= 0);
                Contract.Requires<ArgumentException>(y >= 0);
                Contract.Requires<ArgumentException>(x < Bitmap.Size.Width);
                Contract.Requires<ArgumentException>(y < Bitmap.Size.Height);

                var pointer = (uint*)bitmapData.Scan0 + bitmapData.Width * y + x;
                return *pointer;
            }
        }
    }

    [TestClass]
    public class IntegerMapTest
    {
        [TestMethod]
        public void Test()
        {
            var bitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb);
            bitmap.SetPixel(0, 0, Color.White);
            bitmap.SetPixel(0, 1, Color.Red);
            bitmap.SetPixel(1, 0, Color.Lime);
            bitmap.SetPixel(1, 1, Color.Blue);

            List<uint> values = new List<uint>();

            using (var integerMap = new IntegerMap(bitmap))
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        Contract.Assume(i < integerMap.Bitmap.Size.Width);
                        Contract.Assume(j < integerMap.Bitmap.Size.Height);
                        var value = integerMap[i, j];

                        values.Add(value);
                    }
                }
            }

            AssertUtil.SequenceEqual(new uint[] { 0xFFFFFFFF, 0xFFFF0000, 0xFF00FF00, 0xFF0000FF }, values);
        }
    }
}
