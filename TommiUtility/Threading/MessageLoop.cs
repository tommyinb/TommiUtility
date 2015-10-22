using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Threading
{
    public sealed class MessageLoop : ISynchronizeInvoke, IDisposable
    {
        public MessageLoop()
        {
            thread = new Thread(new ThreadStart(RunLoop));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        public bool InvokeRequired
        {
            get { return true; }
        }
        public object Invoke(Delegate method, object[] args)
        {
            var invoke = new MessageLoopInvoke
            {
                Method = method,
                Args = args,
                WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            };
            invokes.Enqueue(invoke);

            while (invoke.IsCompleted == false) ;

            if (invoke.Exception != null)
            {
                throw new MessageLoopException(invoke.Exception);
            }

            return invoke.Result;
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            var invoke = new MessageLoopInvoke
            {
                Method = method,
                Args = args,
                WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            };

            invokes.Enqueue(invoke);

            return invoke;
        }
        public object EndInvoke(IAsyncResult result)
        {
            if (result is MessageLoopInvoke == false)
            {
                throw new ArgumentException();
            }

            var invoke = (MessageLoopInvoke)result;

            invoke.WaitHandle.WaitOne();

            return invoke.Result;
        }

        private Thread thread;
        private ConcurrentQueue<MessageLoopInvoke> invokes = new ConcurrentQueue<MessageLoopInvoke>();
        private void RunLoop()
        {
            while (true)
            {
                MessageLoopInvoke invoke;
                if (invokes.TryDequeue(out invoke))
                {
                    try
                    {
                        invoke.Result = invoke.Method.DynamicInvoke(invoke.Args);
                    }
                    catch (Exception ex)
                    {
                        invoke.Exception = ex;
                    }

                    invoke.IsCompleted = true;
                    invoke.WaitHandle.Set();
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                thread.Abort();
            }
            catch (Exception) { }
        }
    }

    public class MessageLoopInvoke : IAsyncResult
    {
        public Delegate Method { get; set; }
        public object[] Args { get; set; }

        public bool IsCompleted { get; set; }
        public object Result { get; set; }
        public System.Exception Exception { get; set; }

        public object AsyncState
        {
            get { return this; }
        }
        public WaitHandle AsyncWaitHandle
        {
            get { return WaitHandle; }
        }
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public EventWaitHandle WaitHandle { get; set; }
    }

    [Serializable]
    public class MessageLoopException : Exception
    {
        public MessageLoopException(Exception innerException)
            : this(innerException.Message, innerException) { }

        public MessageLoopException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    [TestClass]
    public class MessageLoopTest
    {
        [TestMethod]
        public void Test()
        {
            using (var messageLoop = new MessageLoop())
            {
                var syncValue = 0;
                messageLoop.Invoke(new Action<int>(t => { syncValue = t; }), new object[] { 3 });
                Assert.AreEqual(3, syncValue);

                var asyncResult = messageLoop.BeginInvoke(new Func<int>(() => 5), new object[0]);
                asyncResult.AsyncWaitHandle.WaitOne();
                var asyncValue = messageLoop.EndInvoke(asyncResult);
                Assert.AreEqual(5, asyncValue);
            }
        }
    }
}
