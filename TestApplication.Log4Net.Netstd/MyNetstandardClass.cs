using System;
using System.Threading;

namespace TestApplication.Log4Net.Netstd
{
    public class MyNetstandardClass
    {
        public int AddTwoNumbers(int num1, int num2)
        {
            InternalMethod("Nothing");
            return num1 + num2;
        }

        private void InternalMethod(string param)
        {
            Thread.Sleep(100);
        }
    }
}
