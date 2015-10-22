using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Randomize
{
    public class RandomDraw
    {
        public RandomDraw()
        {
            drawingValue = random.Value.NextDouble();
        }

        private static Lazy<Random> random = new Lazy<Random>();
        private double drawingValue;

        private double drawnProbability = 0;

        public bool Next(double probability)
        {
            if (drawingValue < drawnProbability)
                return false;

            drawnProbability += probability;

            return drawingValue < drawnProbability;
        }
    }

    [TestClass]
    public class RandomDrawTest
    {
        [TestMethod]
        public void Test()
        {
            for (int i = 0; i < 10; i++)
            {
                var draw = new RandomDraw();

                int drawnCount = 0;
                var drawing = new Action<double>(t =>
                    drawnCount += (draw.Next(t) ? 1 : 0));

                drawing(.5);
                drawing(.3);
                drawing(.2);
                Assert.AreEqual(1, drawnCount);
                
                drawing(.1);
                drawing(.7);
                Assert.AreEqual(1, drawnCount);
            }
        }
    }
}
