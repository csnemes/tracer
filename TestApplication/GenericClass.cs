using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    public class GenericClass<T>
    {
        public T GetDefault(T input)
        {
            return default(T);
        }

        IEnumerable<string> GetString()
        {
            yield return "12";
        }
    }
}
