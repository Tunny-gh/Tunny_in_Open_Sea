using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

using Tunny.Core.Util;

namespace Tunny.Component
{
    [LoggingAspect]
    internal class Tunny_ComponentAttributes : GH_ComponentAttributes
    {
        protected Tunny_ComponentAttributes(IGH_Component component) : base(component)
        {
        }

        internal void RenderBox(Graphics graphics, Brush fill, Pen edge, Guid guid)
        {
            GH_Document doc = Owner.OnPingDocument();
            IGH_DocumentObject obj = doc?.FindObject(guid, false);
            if (obj == null)
            {
                return;
            }
            if (!obj.Attributes.IsTopLevel)
            {
                Guid topLevelGuid = obj.Attributes.GetTopLevel.InstanceGuid;
                obj = doc.FindObject(topLevelGuid, true);
            }
            var rectangle = GH_Convert.ToRectangle(obj.Attributes.Bounds);
            rectangle.Inflate(5, 5);
            graphics.FillRectangle(fill, rectangle);
            graphics.DrawRectangle(edge, rectangle);
        }

        internal static void DrawPath(GH_Canvas canvas, Graphics graphics, IGH_Param param, Wire wire)
        {
            PointF p1 = param.Attributes.InputGrip;
            foreach (IGH_Param source in param.Sources)
            {
                PointF p0 = source.Attributes.OutputGrip;
                if (!canvas.Painter.ConnectionVisible(p0, p1))
                {
                    continue;
                }

                GraphicsPath wirePath = GH_Painter.ConnectionPath(p0, p1, GH_WireDirection.right, GH_WireDirection.left);
                if (wirePath == null)
                {
                    continue;
                }

                var brush = new LinearGradientBrush(p0, p1, wire.ColorEnd, wire.ColorStart);

                var wirePen = new Pen(brush, wire.Width);
                graphics.DrawPath(wirePen, wirePath);
                wirePen.Dispose();
                wirePath.Dispose();
            }
        }

        internal sealed class Wire
        {
            public readonly int Width;
            public Color ColorStart;
            public Color ColorEnd;

            public Wire(int wireWidth, Color wireColor)
            {
                Width = wireWidth;
                ColorStart = wireColor;
                ColorEnd = wireColor;
            }
            public Wire(int wireWidth, Color wireColorStart, Color wireColorEnd)
            {
                Width = wireWidth;
                ColorStart = wireColorStart;
                ColorEnd = wireColorEnd;
            }
        }
    }
}
