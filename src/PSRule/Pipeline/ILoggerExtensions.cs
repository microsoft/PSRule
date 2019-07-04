using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal static class ILoggerExtensions
    {
        public static void DebugMessage(this ILogger logger, string message)
        {
            if (!logger.ShouldWriteDebug())
            {
                return;
            }

            logger.WriteDebug(new DebugRecord(message));
        }
    }
}
