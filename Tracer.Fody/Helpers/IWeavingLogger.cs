namespace Tracer.Fody.Helpers
{
    public interface IWeavingLogger
    {
        void LogDebug(string message);

        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}
