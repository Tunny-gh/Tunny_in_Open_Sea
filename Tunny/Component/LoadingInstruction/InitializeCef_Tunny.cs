using System;

using Grasshopper.Kernel;

using Tunny.Core.Util;

namespace Tunny.Component.LoadingInstruction
{
    [LoggingAspect]
    public class InitializeCef_Tunny : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            InitializeCefRuntimeResolver();

            return GH_LoadingInstruction.Proceed;
        }

        private static void InitializeCefRuntimeResolver()
        {
            try
            {
#if NET48
                CefSharp.CefRuntime.SubscribeAnyCpuAssemblyResolver(TEnvVariables.ComponentFolder);
#endif
            }
            catch (Exception e)
            {
                TLog.Error($"CefSharp Assembly Resolver error: {e.Message}: {e.StackTrace}");
            }
        }
    }
}
