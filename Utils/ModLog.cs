using log4net;

namespace VariousModPatches
{
    public static class ModLog
    {
        private static ILog Logger => VariousModPatches.Instance.Logger;

        public static void Info(string patchName, string message)
        {
            Logger.Info($"[{patchName}] {message}");
        }

        public static void Warn(string patchName, string message)
        {
            Logger.Warn($"[{patchName}] {message}");
        }

        public static void Error(string patchName, string message)
        {
            Logger.Error($"[{patchName}] {message}");
        }
    }
}