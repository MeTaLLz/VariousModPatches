using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace VariousModPatches
{
    public static class Reflection
    {
        public static Type GetType(Mod mod, string typeName, string patchName)
        {
            var type = mod.Code.GetType(typeName);
            if (type == null)
                FileLogger.Warn(patchName, $"{typeName} type not found");
            return type;
        }

        public static MethodInfo GetMethod(Type type, string methodName, BindingFlags flags, string patchName)
        {
            var method = type.GetMethod(methodName, flags);
            if (method == null)
                FileLogger.Warn(patchName, $"{methodName} method not found");
            return method;
        }

        public static PropertyInfo GetProperty(Type type, string propName, BindingFlags flags, string patchName)
        {
            var prop = type.GetProperty(propName, flags);
            if (prop == null)
                FileLogger.Warn(patchName, $"{propName} property not found");
            return prop;
        }

        public static FieldInfo GetField(Type type, string fieldName, BindingFlags flags, string patchName)
        {
            var field = type.GetField(fieldName, flags);
            if (field == null)
                FileLogger.Warn(patchName, $"{fieldName} field not found");
            return field;
        }

        public static bool TryCreateILHook(MethodInfo method, ILContext.Manipulator manipulator,
                                              out ILHook hook, string patchName)
        {
            hook = null;
            try
            {
                hook = new ILHook(method, manipulator);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error(patchName, $"ILHook failed: {ex.Message}");
                return false;
            }
        }
    }
}