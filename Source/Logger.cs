using System.Diagnostics;
using Verse;

namespace RimWorldGravshipRangeOnMapMod
{
    static class Logger
    {
        private const string ModIdentifier = "RimWorldGravshipRangeOnMapMod";

        public static void LogMessage(string message)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame.GetMethod();
            var className = method.DeclaringType.Name;

            Log.Message($"{ModIdentifier}.{className} {message}");
        }
    }
}
