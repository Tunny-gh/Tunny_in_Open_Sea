using System;
using System.Drawing;
using System.Linq;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

using LiveChartsCore;

namespace Tunny.Component.Operation
{
    internal sealed class ConstructFishAttributeAttributes : Tunny_ComponentAttributes
    {
        public ConstructFishAttributeAttributes(IGH_Component component) : base(component)
        {
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.First:
                    if (Selected)
                    {
                        RenderInputComponentBoxes(graphics);
                    }
                    break;
                case GH_CanvasChannel.Wires:
                    DrawWires(canvas, graphics);
                    break;
                case GH_CanvasChannel.Objects:
                    DrawObjects(canvas, graphics, channel);
                    break;
                default:
                    base.Render(canvas, graphics, channel);
                    break;
            }
        }

        private void RenderInputComponentBoxes(Graphics graphics)
        {
            Brush fill = new SolidBrush(Color.FromArgb(Convert.ToInt32("9988008B", 16)));
            Pen edge = Pens.DarkMagenta;
            foreach (IGH_Param input in Owner.Params.Input)
            {
                foreach (Guid guid in input.Sources.Select(s => s.InstanceGuid))
                {
                    RenderBox(graphics, fill, edge, guid);
                }
            }
        }

        private void DrawWires(GH_Canvas canvas, Graphics graphics)
        {
            Wire[] wires = Owner.Attributes.Selected
                ? (new[]
                {
                        new Wire(3, Color.DarkMagenta),
                })
                : (new[]
                {
                        new Wire(2, Color.FromArgb(Convert.ToInt32("3388008B", 16))),
                });

            for (int i = 0; i < 3; i++)
            {
                DrawPath(canvas, graphics, Owner.Params.Input[i], wires[0]);
            }
        }

        private void DrawObjects(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            var style = new GH_PaletteStyle(Color.MediumOrchid, Color.DarkMagenta, Color.Black);
            GH_PaletteStyle normalStyle = GH_Skin.palette_normal_standard;
            GH_PaletteStyle warningStyle = GH_Skin.palette_warning_standard;
            GH_PaletteStyle hiddenStyle = GH_Skin.palette_hidden_standard;
            GH_Skin.palette_normal_standard = style;
            GH_Skin.palette_warning_standard = style;
            GH_Skin.palette_hidden_standard = style;
            base.Render(canvas, graphics, channel);
            GH_Skin.palette_normal_standard = normalStyle;
            GH_Skin.palette_warning_standard = warningStyle;
            GH_Skin.palette_hidden_standard = hiddenStyle;
        }
    }
}
