using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TestApplication.Log4Net.Netstd;

namespace TestApplication.Log4Net.Core
{
    public class MyApplication
    {
        public void Run()
        {
            var myNetStandard = new MyNetstandardClass();
            myNetStandard.AddTwoNumbers(20, 22);
            Thread.Sleep(500);
        }
    }
}
