using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravel
{
    public static class AssemblyLoader
    {
        private static Dictionary<string, Assembly> AssembliesLoaded = new Dictionary<string, Assembly>();

        public static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (Object sender, ResolveEventArgs args) =>
            {
                String thisExe = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                System.Reflection.AssemblyName embeddedAssembly = new System.Reflection.AssemblyName(args.Name);
                String resourceName = thisExe + "." + embeddedAssembly.Name + ".dll";

                if (AssembliesLoaded.ContainsKey(resourceName))
                {
                    return AssembliesLoaded[resourceName];
                }

                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    var assembly = System.Reflection.Assembly.Load(assemblyData);

                    AssembliesLoaded.Add(resourceName, assembly);
                    return assembly;
                }
            };
        }
    }
}