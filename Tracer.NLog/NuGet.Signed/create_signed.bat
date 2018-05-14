del .\lib\netstandard2.0\*.dll /F
del .\Tracer.NLog.il /F
ildasm ..\NuGet\lib\netstandard2.0\Tracer.NLog.dll /out:.\Tracer.NLog.il
ilasm .\Tracer.NLog.il /DLL /key=.\Tracer.NLog.snk /output=.\lib\netstandard2.0\Tracer.NLog.dll