using System.Collections.Generic;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;

using Rhino;

using Tunny.Core.Util;

namespace Tunny.Component.LoadingInstruction
{
    [LoggingAspect]
    public class CheckIncompatibility_Tunny : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            var incompatibilityPlugins = new List<string>
            {
                "Wallacei",
            };

            var names = Instances.ComponentServer.Libraries.Select(x => x.Name).ToList();
            foreach (string plugin in incompatibilityPlugins)
            {
                if (names.Contains(plugin))
                {
                    string message = $"🐟Tunny Information🐟: \"{plugin} \" is potentially incompatible with Tunny. If you experience any issues, please try uninstall {plugin} and restart Rhino.";
                    RhinoApp.WriteLine(message);
                }
            }

            return GH_LoadingInstruction.Proceed;
        }
    }
}
