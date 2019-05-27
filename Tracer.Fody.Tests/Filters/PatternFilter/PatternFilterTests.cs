using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using Whatever.Toolkit.Implementation.Sequences.HiLo;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public class PatternFilterTests
    {
        [Test]
        public void LiveIssue_OrderingProblem()
        {
            var input = @"
            <Filters>
                <Off pattern=""*"" />
                <On pattern = ""..[public]*.[public|method]*"" />
                <Off pattern = ""..*.ToString"" />
                <Off pattern = ""..*.Equals"" />
                <Off pattern = ""..*.GetHashCode"" />
                <Off pattern = ""..*.op_*"" />
                <Off pattern = ""..Date.*"" />
                <Off pattern = ""..ReflectionHelper.*"" />
                <Off pattern = ""Whatever..Helpers.*"" />
                <Off pattern = ""Whatever..*.CreateNullable"" />
                <Off pattern = ""Whatever..Assertions.*"" />
                <Off pattern = ""Whatever..ConfigurationHelper.*"" />
            </Filters>";

            var elem = XElement.Parse(input);
            var filter = new Fody.Filters.PatternFilter.PatternFilter(elem.Descendants());
            var result = filter.ShouldAddTrace(TestHelpers.GetMethodDefinition(typeof(HiLoSequenceGenerator), "GetNextAsync"));
            result.ShouldTrace.Should().BeTrue();
        }
    }
}

namespace Whatever.Toolkit.Implementation.Sequences.HiLo
{
    public class HiLoSequenceGenerator
    {
        public Task<int> GetNextAsync() { throw new NotImplementedException(); }
    }
}

