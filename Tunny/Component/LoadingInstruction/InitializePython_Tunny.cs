using System;
using System.IO;

using Grasshopper.Kernel;

using Python.Runtime;

using Tunny.Core.Util;

namespace Tunny.Component.LoadingInstruction
{
    [LoggingAspect]
    public class InitializePython_Tunny : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            InitializePythonDllPath();

            return GH_LoadingInstruction.Proceed;
        }

        private static void InitializePythonDllPath()
        {
            try
            {
                Runtime.PythonDLL = Path.Combine(TEnvVariables.PythonDllPath);
            }
            catch (Exception e)
            {
                TLog.Error($"Python RuntimeDLL path set error: {e.Message}: {e.StackTrace}");
            }
        }
    }
}
