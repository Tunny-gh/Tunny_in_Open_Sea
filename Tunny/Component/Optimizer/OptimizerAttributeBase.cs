using System;
using System.Drawing;
using System.Linq;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

using Tunny.Core.Util;

namespace Tunny.Component.Optimizer
{
    [LoggingAspect]
    internal class OptimizerAttributeBase : Tunny_ComponentAttributes
    {
        private readonly Color _capsuleFillColor;
        private readonly Color _capsuleEdgeColor;
        private readonly Color _capsuleTextColor;

        public OptimizerAttributeBase(IGH_Component component, Color fill, Color edge, Color text) : base(component)
        {
            _capsuleFillColor = fill;
            _capsuleEdgeColor = edge;
            _capsuleTextColor = text;
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
                case GH_CanvasChannel.Objects:
                    DrawObjects(canvas, graphics, channel);
                    break;
                case GH_CanvasChannel.Wires:
                    DrawWires(canvas, graphics);
                    break;
                default:
                    base.Render(canvas, graphics, channel);
                    break;
            }
        }

        private void DrawObjects(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            var style = new GH_PaletteStyle(_capsuleFillColor, _capsuleEdgeColor, _capsuleTextColor);
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

        private void DrawWires(GH_Canvas canvas, Graphics graphics)
        {
            Wire[] wires = Owner.Attributes.Selected
                ? (new[]
                {
                        new Wire(3, Color.DarkBlue),
                        new Wire(3, Color.Green),
                        new Wire(3, Color.DarkMagenta),
                        new Wire(3, Color.Crimson),
                        new Wire(3, Color.DimGray),
                        new Wire(3, Color.DimGray),
                })
                : (new[]
                {
                        new Wire(2, Color.FromArgb(Convert.ToInt32("3300008B", 16))),
                        new Wire(2, Color.FromArgb(Convert.ToInt32("33008000", 16))),
                        new Wire(2, Color.FromArgb(Convert.ToInt32("338B008B", 16))),
                        new Wire(2, Color.FromArgb(Convert.ToInt32("33DC143C", 16))),
                        new Wire(2, Color.FromArgb(Convert.ToInt32("33696969", 16))),
                        new Wire(2, Color.FromArgb(Convert.ToInt32("33696969", 16))),
                });
            for (int i = 0; i < Owner.Params.Input.Count; i++)
            {
                DrawPath(canvas, graphics, Owner.Params.Input[i], wires[i]);
            }
        }

        private void RenderInputComponentBoxes(Graphics graphics)
        {
            Brush[] fill = {
                    new SolidBrush(Color.FromArgb(Convert.ToInt32("9900008B", 16))),
                    new SolidBrush(Color.FromArgb(Convert.ToInt32("99008000", 16))),
                    new SolidBrush(Color.FromArgb(Convert.ToInt32("998B008B", 16))),
                    new SolidBrush(Color.FromArgb(Convert.ToInt32("99DC143C", 16))),
                };
            Pen[] edge = new[] { Pens.DarkBlue, Pens.Green, Pens.DarkMagenta, Pens.Crimson };
            for (int i = 0; i < 4; i++)
            {
                foreach (Guid guid in Owner.Params.Input[i].Sources.Select(s => s.InstanceGuid))
                {
                    RenderBox(graphics, fill[i], edge[i], guid);
                }
            }
        }
    }
}
