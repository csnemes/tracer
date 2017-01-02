using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tracer.NLog;

namespace TestApplication.NLog
{
    public class MyApplication
    {
        public void Run()
        {
            InnerMethod("hello", 42);

            Log.OriginalLogger.Trace("original logger");

            if (Log.IsTraceEnabled)
            {
                Log.Trace("Trace enabled");
            }

            Log.Trace(42);
            Log.Trace(new CultureInfo("en-us"), 42);
            Log.Trace(new MyData("Hello"));

            Log.Trace("something");
            Log.Trace(new CultureInfo("en-us"), "something");

            Log.Trace(() => "42");

            Log.Trace(new ApplicationException("error"));
            Log.Trace(new ApplicationException("error"), "errormsg");
            Log.Trace(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Trace(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Trace("Msg:{0}", 42);
            Log.Trace("Msg:{0},{1}", 42, "fourtythree");
            Log.Trace("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Trace("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Trace(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Trace(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Trace(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Trace(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Debug enabled");
            }

            Log.Debug(42);
            Log.Debug(new CultureInfo("en-us"), 42);
            Log.Debug(new MyData("Hello"));

            Log.Debug("something");
            Log.Debug(new CultureInfo("en-us"), "something");

            Log.Debug(() => "42");

            Log.Debug(new ApplicationException("error"));
            Log.Debug(new ApplicationException("error"), "errormsg");
            Log.Debug(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Debug(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Debug("Msg:{0}", 42);
            Log.Debug("Msg:{0},{1}", 42, "fourtythree");
            Log.Debug("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Debug("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Debug(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Debug(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Debug(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Debug(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            if (Log.IsInfoEnabled)
            {
                Log.Info("Info enabled");
            }

            Log.Info(42);
            Log.Info(new CultureInfo("en-us"), 42);
            Log.Info(new MyData("Hello"));

            Log.Info("something");
            Log.Info(new CultureInfo("en-us"), "something");

            Log.Info(() => "42");

            Log.Info(new ApplicationException("error"));
            Log.Info(new ApplicationException("error"), "errormsg");
            Log.Info(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Info(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Info("Msg:{0}", 42);
            Log.Info("Msg:{0},{1}", 42, "fourtythree");
            Log.Info("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Info("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Info(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Info(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Info(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Info(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            if (Log.IsWarnEnabled)
            {
                Log.Warn("Warn enabled");
            }

            Log.Warn(42);
            Log.Warn(new CultureInfo("en-us"), 42);
            Log.Warn(new MyData("Hello"));

            Log.Warn("something");
            Log.Warn(new CultureInfo("en-us"), "something");

            Log.Warn(() => "42");

            Log.Warn(new ApplicationException("error"));
            Log.Warn(new ApplicationException("error"), "errormsg");
            Log.Warn(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Warn(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Warn("Msg:{0}", 42);
            Log.Warn("Msg:{0},{1}", 42, "fourtythree");
            Log.Warn("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Warn("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Warn(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Warn(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Warn(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Warn(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            if (Log.IsErrorEnabled)
            {
                Log.Error("Error enabled");
            }

            Log.Error(42);
            Log.Error(new CultureInfo("en-us"), 42);
            Log.Error(new MyData("Hello"));

            Log.Error("something");
            Log.Error(new CultureInfo("en-us"), "something");

            Log.Error(() => "42");

            Log.Error(new ApplicationException("error"));
            Log.Error(new ApplicationException("error"), "errormsg");
            Log.Error(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Error(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Error("Msg:{0}", 42);
            Log.Error("Msg:{0},{1}", 42, "fourtythree");
            Log.Error("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Error("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Error(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Error(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Error(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Error(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            if (Log.IsFatalEnabled)
            {
                Log.Fatal("Fatal enabled");
            }

            Log.Fatal(42);
            Log.Fatal(new CultureInfo("en-us"), 42);
            Log.Fatal(new MyData("Hello"));

            Log.Fatal("something");
            Log.Fatal(new CultureInfo("en-us"), "something");

            Log.Fatal(() => "42");

            Log.Fatal(new ApplicationException("error"));
            Log.Fatal(new ApplicationException("error"), "errormsg");
            Log.Fatal(new ApplicationException("error"), "Msg:{0},{1}", 42, 43);
            Log.Fatal(new ApplicationException("error"), new CultureInfo("en-us"), "Msg:{0},{1}", 42, 43);

            Log.Fatal("Msg:{0}", 42);
            Log.Fatal("Msg:{0},{1}", 42, "fourtythree");
            Log.Fatal("Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Fatal("Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

            Log.Fatal(new CultureInfo("en-us"), "Msg:{0}", 42);
            Log.Fatal(new CultureInfo("en-us"), "Msg:{0},{1}", 42, "fourtythree");
            Log.Fatal(new CultureInfo("en-us"), "Msg:{0},{1},{2}", 42, "fourtythree", 44);
            Log.Fatal(new CultureInfo("en-us"), "Msg:{0},{1},{2},{3}", 42, "fourtythree", 44, 45);

        }


        public string InnerMethod(string inp, int inp2)
        {
            return "answer";
        }
    }
}
