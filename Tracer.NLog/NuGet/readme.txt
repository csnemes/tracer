-----------------------------------------------------------------------------
Tracer.NLog.Fody
-----------------------------------------------------------------------------
This Fody(https://github.com/Fody/Fody/) add-in injects code to log method entries and exits.

See https://github.com/csnemes/tracer for details.

Add the Tracer element to FodyWeavers.xml under Weavers element in order to configure the Tracer.
<?xml version="1.0" encoding="utf-8" ?>
<Weavers>
  <Tracer filter="pattern">
  ...
  </Tracer>
</Weavers>

Within the <Tracer> element specify <On> And <Off> elements with patterns to select which methods to trace.
(e.g. <On pattern="TestApplication.*..My*.[public]*Method" /> or <Off pattern="TestApplication.*..[internal]My*.*Method" />)

The pattern format is the following:
namespace_part.class_part.member_part

namespace_part is the dot separated definition of the namespace sought. You can use * and ? in namespace parts as you would when looking for files.
Use double dot (..) to specify any number of interim namespaces. For example My*.Core..Package? will match MyNamespace.Core.Package1 and 
MyNamespace.Core.Other.Package2 and MyNamespace.Core.Other.Some.Package3.

The class_part format is [public|internal]class_name_filter where the [] part is optional. If not specified all visibilities are logged.
Class_name_filter can contain * and ?. Eg. [public]*Repository logs all public classes ending with Repository 

The member_part format is [public|private|protected|internal|get|set|method|instance|static]member_name_filter where the [] part is optional. 
If visibility, instance or member type is not specified then all are logged. Member_name_filter can contain * and ?.

When considering filters more specified filters (containing less * and ? approx.) trumps less specified filters.
So <On pattern="RootNamespace.*Repository.[public]GetBy*" /> and <Off pattern="RootNamespace.*Repository.GetByUserId" />
will log all public GetBy methods in any Repository except GetByUserId.
If the weaver cannot decide the order based on specificity the definition order counts.

 
