using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tracer.Fody.Filters
{
    /// <summary>
    /// Class representing the namespace scope of assembly level TraceOn and NoTrace definitions
    /// </summary>
    internal class NamespaceScope
    {
        public static NamespaceScope All = new NamespaceScope(null, MatchMode.SelfAndChildren);

        private enum MatchMode
        {
            ExactMatch,
            OnlyChildren,
            SelfAndChildren
        }

        private readonly string _namespace;
        private readonly MatchMode _matchMode;

        private NamespaceScope(string ns, MatchMode matchMode)
        {
            _namespace = ns;
            _matchMode = matchMode;
        }

        public static NamespaceScope Parse(string inp)
        {
            var namespc = String.Empty;
            var matchMode = MatchMode.ExactMatch;
            if (inp.EndsWith(".*"))
            {
                matchMode = MatchMode.OnlyChildren;
                namespc = inp.Substring(0, inp.Length - 2);
            }
            else if (inp.EndsWith("+*"))
            {
                matchMode = MatchMode.SelfAndChildren;
                namespc = inp.Substring(0, inp.Length - 2);
            }
            else
            {
                if (inp.EndsWith(".") || inp.EndsWith("*"))
                {
                    throw new ApplicationException("Namespace must end with a literal or with .* or +* symbols.");
                }
                if (inp.Contains('*'))
                {
                    throw new ApplicationException("Namespace can only contain an asterisk at the end.");
                }
                namespc = inp;
            }

            if (String.IsNullOrWhiteSpace(namespc))
            {
                throw new ApplicationException("Namespace cannot be empty if specified.");    
            }
            
            return new NamespaceScope(namespc, matchMode);
        }
    }
}
