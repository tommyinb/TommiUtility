using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Threading
{
    public class PredicateWait : WaitHandle
    {
        public PredicateWait()
        {
            Predicate = () => true;

            Interval = TimeSpan.FromMilliseconds(1);
        }
        public PredicateWait(Func<bool> predicate)
        {
            Predicate = predicate;

            Interval = TimeSpan.FromMilliseconds(1);
        }

        public Func<bool> Predicate { get; set; }
        public TimeSpan Interval { get; set; }

        public bool WaitOne(CancellationToken cancellationToken)
        {
            return WaitOne(TimeSpan.MaxValue, new CancellationToken?(cancellationToken));
        }
        public override bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            if (millisecondsTimeout >= 0)
            {
                var timeout = TimeSpan.FromMilliseconds(millisecondsTimeout);
                return WaitOne(timeout, cancellationToken: null);
            }
            else
            {
                return WaitOne(TimeSpan.MaxValue, cancellationToken: null);
            }
        }
        public override bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            return WaitOne(timeout, cancellationToken: null);
        }
        public bool WaitOne(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return WaitOne(timeout, new CancellationToken?(cancellationToken));
        }
        private bool WaitOne(TimeSpan timeout, CancellationToken? cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                if (Predicate())
                {
                    return true;
                }

                if (cancellationToken != null)
                {
                    if (cancellationToken.Value.IsCancellationRequested)
                    {
                        return false;
                    }
                }

                var timeLeft = timeout - stopwatch.Elapsed;

                if (timeLeft <= TimeSpan.Zero)
                {
                    return false;
                }

                Thread.Sleep(timeLeft > Interval ? Interval : timeLeft);
            }
        }
    }

    [TestClass]
    public class PredicateWaitTest
    {
        [TestMethod]
        public void Test()
        {
            var resultWaitee = false;
            var predicateWait = new PredicateWait(() => resultWaitee);
            var tokenSource = new CancellationTokenSource();

            var resultReached = false;
            Task.Run(() =>
            {
                tokenSource.CancelAfter(1);

                var finalResultWait = new PredicateWait(() => resultReached);
                finalResultWait.WaitOne();

                resultWaitee = true;
            });

            Assert.IsFalse(predicateWait.WaitOne(tokenSource.Token));
            Assert.IsFalse(resultWaitee);

            Assert.IsFalse(predicateWait.WaitOne(2));
            Assert.IsFalse(resultWaitee);

            resultReached = true;
            predicateWait.WaitOne();
            Assert.IsTrue(resultWaitee);
        }
    }
}
