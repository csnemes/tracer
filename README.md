Tracer 3.0.0
======

Tracing and logging rewriter using Fody. It adds trace enter and trace leave log entries for the methods specified. Such calls include incoming and outgoing arguments as well as time spent in the method. It also rewrites static log entries to properly configured log calls. Tracer is the rewriter core on which one of the specific adapters like Tracer.Log4Net is built uppon. Creating custom adapters for your specific needs is very easy.
Version 2.x or higher supports netstandard 2.0.
See [Wiki](https://github.com/csnemes/tracer/wiki) for details.

Existing adapters are (supporting netstandard 2.0):

|Adapter     |NuGet package              |Version |
|------------|---------------------------|-------:|
|Log4Net     |Tracer.Log4Net.Fody        | 3.0.0  |
|Serilog     |Tracer.Serilog.Fody        | 3.0.0  |
|NLog        |Tracer.4NLog.Fody          | 3.0.0  |


Should you have any question/problem send an email to csaba.nemes@outlook.com or add an issue/request.

Compatibility:
---
  - netstandard 2.0 (.NET Framework 4.0+ before 1.5.0)

To install:
---
  - using NuGet: Install-Package Tracer.Log4Net.Fody 
  - build and use the binaries

To build:
---
Use Visual Studio 2017

Version History for Tracer:
---
* 1.0.0 
    Initial release
* 1.1.0
    - Trace leave now logs when a method is exited with an exception
    - Bug fix on static log rewrites
    - Tracer now creates verifiable code
* 1.1.1
    - Fixed static log rewrite for constructors and closures/lambdas
* 1.2.0
    - In the configuration TraceOn target value extended with 'none' which means no tracing by default
    - Changed TraceLeave signature to receive start and end ticks instead of elapsed ticks
* 1.2.2
    - Updated to Fody 1.29.4
* 1.2.3
    - Added support for strong named custom adapters 
* 1.2.4
	- Added option to trace log constructors with traceConstructors flag. Just add traceConstructors="true" to Tracer element in weaver config file. 
* 1.3.0
    - Static log rewrite now supports rewriting static property getters (e.g one can use Log.IsDebug to avoid costly calls)
    - Fix: Static constructors are excluded from tracing
    - Assembly level xml trace configuration is extended. Multiple TraceOn and NoTrace elements can be specified. Both supports
    namespace attribute which defines the scope of the configuration set. See documentation for more details.
    - property getter/setter rewriting can be turned off using traceProperties flag in xml configuration
    - NoTrace and TraceOn attributes now can be also applied on properties
* 1.3.1
    - bug fix: on some machines resolving method reference of static log methods did not work properly
* 1.4.0
    - Static log rewrite now supports rewriting static generic methods (only instantiated calls)
* 1.5.0
    - Added support for trace logging async methods
* 1.6.0
    - Updated dependency to Fody 2.x
* 2.0.0
    - Moved to netstandard 2.0    
* 2.2.0
    - Updated to Fody 3.x
* 3.0.0
    - Fixed issue with missing local function extra parameter name
    - Switched to new Fody package format, upgraded to Fody 3.3.x
    - Added pattern based filter for specifying which methods to trace	
    - Fixed generic async rewrite issues causing PEVerify errors
    - Added the possibility to pass in extra configuration parameters to trace enter and leave methods (breaks trace enter and leave signature)

Version History for Tracer.Log4Net:
---
* 1.0.0 
    Initial release
* 1.1.0
    - Log4Net adapter uses the log4net rendering mechanism when logging arguments
* 1.1.1
    - Modified message now contains method name. 
    - Added custom properties to support different logging format requirements.
* 1.2.0
    - Internal changes to support the changed TraceLeave signature
* 1.2.1 
    - Updated to log4net package 2.0.5 
* 1.2.2
    - Updated to Fody 1.29.4
* 1.3.0
    - Adapter and Log class extended with properties from ILog interface (IsError, IsDebug, etc.)
    - Fix: fixed an issue with logging IEnumerators. Logger now properly resets the enumerator after logging.
* 1.3.1
    - adding LogUseSafeParameterRendering key to appSettings with a true value will esacpe log4net's DefaultRenderer during trace parameter rendering.
* 1.3.2
    - documentation added to Log methods    
* 1.3.3
    - Updated to log4net package 2.0.8    
* 2.0.0
    - Moved to netstandard 2.0    
* 2.1.0
    Can use TracerFodySpecialKeyPrefix environment variable to define prefix (instead of $) in $return and $exception.
* 2.2.0
    - Updated to Fody 3.x
    - TraceOn and NoTrace can be put on structs and constructors 
* 3.0.0
    - See Tracer.Fody Version History

Version History for Tracer.Serilog:
---
* 1.0.0 
    Initial release
* 1.0.1
    Framework version set to 4.5
* 1.1.0
    Added the ability to destructure types in trace enter and leave. Use DestructureAttribute to mark a type as target for destructuring.
    Use the assembly level DestructureTypeAttribute to mark types outside of your codebase.
* 2.0.0
    Moved to netstandard 2.0
* 2.1.0
    Can use TracerFodySpecialKeyPrefix environment variable to define prefix (instead of $) in $return and $exception.
* 2.2.0
    Updated to Fody 3.x
    TraceOn and NoTrace can be put on structs and constructors       
* 3.0.0
    - See Tracer.Fody Version History

Version History for Tracer.4NLog:
---
* 1.0.0 
    Initial release
* 1.0.3
    Fixed issue with wrong callsite
* 1.0.4
    Fixed issue with logger name not containing namespace 
* 2.2.0
    Updated to Fody 3.x
    TraceOn and NoTrace can be put on structs and constructors      
* 3.0.0
    - See Tracer.Fody Version History
    
Notes:
---
