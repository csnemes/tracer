del .\lib\net40\*.dll /F
del .\Tracer.NLog.il /F
ildasm ..\NuGet\lib\net40\Tracer.NLog.dll /out:.\Tracer.NLog.il
ilasm .\Tracer.NLog.il /DLL /key=.\Tracer.NLog.snk /output=.\lib\net40\Tracer.NLog.dll