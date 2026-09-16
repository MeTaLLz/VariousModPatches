namespace VariousModPatches
{
    public static class FileLogger
    {
        public static void Warn(string className, string message)
        {
            Instances.Mod.Logger.Warn($"({className}) {message}");
        }

        public static void Error(string className, string message)
        {
            Instances.Mod.Logger.Error($"({className}) {message}");
        }

        public static void Info(string className, string message)
        {
            Instances.Mod.Logger.Info($"({className}) {message}");
        }
    }
}