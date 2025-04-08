using System;

using Grasshopper.Kernel;

using Tunny.Core.Util;

namespace Tunny.Component.LoadingInstruction
{
    [LoggingAspect]
    public class InitializeTLog_Tunny : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            try
            {
                TLog.InitializeLogger();
            }
            catch (Exception)
            {
            }

            return GH_LoadingInstruction.Proceed;
        }
    }
}
