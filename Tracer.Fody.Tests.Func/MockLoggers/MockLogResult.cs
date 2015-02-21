using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Tests.Func.MockLoggers
{
    [Serializable]
    public class MockLogResult
    {
        private readonly List<MockCallInfo> _calls;
        private readonly bool _areReturnValuesOk;

        public MockLogResult(List<MockCallInfo> calls, bool areReturnValuesOk)
        {
            _calls = calls;
            _areReturnValuesOk = areReturnValuesOk;
        }

        public int Count
        {
            get { return _calls.Count; }
        }

        public MockCallInfo ElementAt(int idx)
        {
            return _calls[idx];
        }
    }
}
