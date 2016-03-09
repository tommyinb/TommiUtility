using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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
        public void Dispose()
        {
            try
            {
                thread.Abort();
            }
            catch (Exception) { }
        }

        public bool InvokeRequired
        {
            get { return true; }
        }
        public object Invoke(Delegate method, object[] args)
        {
            if (method == null) throw new ArgumentNullException();

            var invoke = new MessageLoopInvoke(method, args ?? new object[0]);
            invokes.Enqueue(invoke);

            invoke.WaitHandle.WaitOne();

            if (invoke.Exception != null) throw new MessageLoopException(invoke.Exception);

            return invoke.Result;
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            if (method == null) throw new ArgumentNullException();

            var invoke = new MessageLoopInvoke(method, args ?? new object[0]);

            invokes.Enqueue(invoke);

            return invoke;
        }
        public object EndInvoke(IAsyncResult result)
        {
            if (result is MessageLoopInvoke == false) throw new ArgumentException();

            var invoke = (MessageLoopInvoke)result;

            Contract.Assume(invoke.WaitHandle != null);
            invoke.WaitHandle.WaitOne();

            return invoke.Result;
        }

        private readonly Thread thread;
        private readonly ConcurrentQueue<MessageLoopInvoke> invokes = new ConcurrentQueue<MessageLoopInvoke>();
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(thread != null);
            Contract.Invariant(invokes != null);
        }
        
        private void RunLoop()
        {
            while (true)
            {
                MessageLoopInvoke invoke;
                if (invokes.TryDequeue(out invoke))
                {
                    if (invoke != null)
                    {
                        try
                        {
                            Contract.Assume(invoke.Method != null);
                            invoke.Result = invoke.Method.DynamicInvoke(invoke.Args);
                        }
                        catch (Exception ex)
                        {
                            invoke.Exception = ex;
                        }

                        invoke.IsCompleted = true;

                        Contract.Assume(invoke.WaitHandle != null);
                        invoke.WaitHandle.Set();
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }

    public sealed class MessageLoopInvoke : IAsyncResult, IDisposable
    {
        public MessageLoopInvoke(Delegate method, object[] args)
        {
            Contract.Requires<ArgumentNullException>(method != null);
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Ensures(Method != null);
            Contract.Ensures(Args != null);
            Contract.Ensures(WaitHandle != null);

            Method = method;
            Args = args;

            WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }
        public void Dispose()
        {
            WaitHandle.Dispose();
        }

        public readonly Delegate Method;
        public readonly object[] Args;

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
            get
            {
                Contract.Ensures(Contract.Result<bool>() == false);
                return false;
            }
        }

        public readonly EventWaitHandle WaitHandle;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(Method != null);
            Contract.Invariant(Args != null);
            Contract.Invariant(WaitHandle != null);
        }
    }

    [Serializable]
    public class MessageLoopException : Exception
    {
        public MessageLoopException(Exception innerException)
            : this(innerException.Message, innerException)
        {
            Contract.Requires<ArgumentNullException>(innerException != null);
        }
        public MessageLoopException(string message, Exception innerException)
            : base(message, innerException)
        {
            Contract.Requires<ArgumentNullException>(message != null);
            Contract.Requires<ArgumentNullException>(innerException != null);
        }
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
                Contract.Assume(asyncResult != null);

                asyncResult.AsyncWaitHandle.WaitOne();
                var asyncValue = messageLoop.EndInvoke(asyncResult);
                Assert.AreEqual(5, asyncValue);
            }
        }
    }
}
