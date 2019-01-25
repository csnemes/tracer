using System;
using System.Globalization;
using TestApplication.Log4Net.Netstd;
using Tracer.Log4Net;

namespace TestApplication.Log4Net.Core
{
    public class MyApplication
    {
        public void Run()
        {
            var myNetStandard = new MyNetstandardClass();
            myNetStandard.AddTwoNumbers(20, 22);

            StaticLogRewrites();

            ExceptionTests();

            AsyncTests();

            //log4net is faster because it does NOT writes class and method names. It seems that stack trace is missing 
            var perfComp = new PerfComp();
            perfComp.SpeedTest();
        }

        public void AsyncTests()
        {
            var x = new MyAsyncClass();
            x.Run();
        }

        public void StaticLogRewrites()
        {
            Log.Debug(new MyClass());
            Log.Debug("Hello");
            Log.Debug("Hello", new ApplicationException("Failure."));
            Log.DebugFormat("A{0}", 1);
            Log.DebugFormat("A{0}-B{1}", 1, 2);
            Log.DebugFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.DebugFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.DebugFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            Log.Info(new MyClass());
            Log.Info("Hello");
            Log.Info("Hello", new ApplicationException("Failure."));
            Log.InfoFormat("A{0}", 1);
            Log.InfoFormat("A{0}-B{1}", 1, 2);
            Log.InfoFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.InfoFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.InfoFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            Log.Warn(new MyClass());
            Log.Warn("Hello");
            Log.Warn("Hello", new ApplicationException("Failure."));
            Log.WarnFormat("A{0}", 1);
            Log.WarnFormat("A{0}-B{1}", 1, 2);
            Log.WarnFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.WarnFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.WarnFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            Log.Error(new MyClass());
            Log.Error("Hello");
            Log.Error("Hello", new ApplicationException("Failure."));
            Log.ErrorFormat("A{0}", 1);
            Log.ErrorFormat("A{0}-B{1}", 1, 2);
            Log.ErrorFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.ErrorFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.ErrorFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            Log.Fatal(new MyClass());
            Log.Fatal("Hello");
            Log.Fatal("Hello", new ApplicationException("Failure."));
            Log.FatalFormat("A{0}", 1);
            Log.FatalFormat("A{0}-B{1}", 1, 2);
            Log.FatalFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.FatalFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.FatalFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            //closure
            int idx = 1;
            Action act = () =>
            {
                idx++;
                Log.Debug("Closure log");
            };

            act();

            //lambda w/out closure
            Action act2 = () =>
            {
                Log.Debug("No closure log");
            };

            act2();
        }

        private void ExceptionTests()
        {
            ThrowException(1);
            try
            {
                ThrowException(0);
            }
            catch { }
            ThrowExceptionOuter(1);
            try
            {
                ThrowExceptionOuter(0);
            }
            catch { }
        }

        public int ThrowExceptionOuter(int inp)
        {
            return ThrowException(inp);
        }

        public int ThrowException(int inp)
        {
            return 1 / inp;
        }

        private class MyClass
        {
            public override string ToString()
            {
                return "ToStringOfMyClass";
            }
        }
    }
}
