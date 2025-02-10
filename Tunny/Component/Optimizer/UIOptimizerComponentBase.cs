using System.Drawing;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

using Tunny.CommonUI;
using Tunny.CommonUI.Message;
using Tunny.Core.Util;
using Tunny.WPF;

namespace Tunny.Component.Optimizer
{
    public class UIOptimizeComponentBase : OptimizeComponentBase
    {
        private static CommonSharedItems CoSharedItems => CommonSharedItems.Instance;
        internal MainWindow MainWindow;

        public UIOptimizeComponentBase(string name, string nickname, string description)
          : base(name, nickname, description)
        {
        }

        private void ShowOptimizationWindow()
        {
            CoSharedItems.GH_DocumentEditor = Instances.DocumentEditor;
            TEnvVariables.GrasshopperWindowHandle = CoSharedItems.GH_DocumentEditor.Handle;

            GhInOutInstantiate();
            if (!GhInOut.IsLoadCorrectly)
            {
                TunnyMessageBox.Error_ComponentLoadFail();
            }
            else
            {
                MainWindow = new MainWindow(this);
                MainWindow.Show();
            }
        }

        public override void CreateAttributes()
        {
            m_attributes = new UIOptimizerComponentAttributes(this);
        }

        private sealed class UIOptimizerComponentAttributes : OptimizerAttributeBase
        {
            public UIOptimizerComponentAttributes(IGH_Component component)
              : base(component, Color.CornflowerBlue, Color.DarkBlue, Color.Black)
            {
            }

            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas _, GH_CanvasMouseEvent e)
            {
                ((UIOptimizeComponentBase)Owner).MakeFishPrintByCaptureToTopOrder();
                ((UIOptimizeComponentBase)Owner).ShowOptimizationWindow();
                return GH_ObjectResponse.Handled;
            }
        }
    }
}
