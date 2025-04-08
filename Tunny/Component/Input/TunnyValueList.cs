using System;
using System.Drawing;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Special;

using Tunny.Core.Util;

namespace Tunny.Component.Input
{
    [LoggingAspect]
    public class TunnyValueList : GH_ValueList
    {
        public TunnyValueList()
        {
            Category = "Tunny";
            SubCategory = "Input";
            Name = "Tunny Value List";
            NickName = "<')))<<";
            Description = "Value list with Tunny styling";
            ListMode = GH_ValueListMode.CheckList;
            ListItems.Clear();
            ListItems.Add(new GH_ValueListItem("Tuna", "1"));
            ListItems.Add(new GH_ValueListItem("Salmon", "2"));
            ListItems.Add(new GH_ValueListItem("Cod", "3"));
            ListItems.Add(new GH_ValueListItem("Trout", "4"));
        }

        public override void CreateAttributes()
        {
            m_attributes = new TunnyValueListAttributes(this);
        }

        public override Guid ComponentGuid => new Guid("e29b732d-72bc-4734-a520-642bc9b518fb");

        public void SelectItemUnsafe(int index)
        {
            if (index < 0 || index >= ListItems.Count)
            {
                return;
            }

            bool flag = false;
            int num = ListItems.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                if (i == index)
                {
                    if (!ListItems[i].Selected)
                    {
                        flag = true;
                        break;
                    }
                }
                else if (ListItems[i].Selected)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                RecordUndoEvent("Select: " + ListItems[index].Name);
                int num2 = ListItems.Count - 1;
                for (int j = 0; j <= num2; j++)
                {
                    ListItems[j].Selected = j == index;
                }

                ExpireSolution(recompute: false);
            }
        }
    }

    internal sealed class TunnyValueListAttributes : GH_ValueListAttributes
    {
        public TunnyValueListAttributes(GH_ValueList owner) : base(owner)
        {
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                DrawObjects(canvas, graphics, channel);
            }
            else
            {
                base.Render(canvas, graphics, channel);
            }
        }

        private void DrawObjects(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            var tunnyStyle = new GH_PaletteStyle(Color.CornflowerBlue, Color.DarkBlue, Color.Black);

            GH_PaletteStyle normalStyle = GH_Skin.palette_normal_standard;
            GH_PaletteStyle warningStyle = GH_Skin.palette_warning_standard;
            GH_PaletteStyle hiddenStyle = GH_Skin.palette_hidden_standard;
            GH_Skin.palette_normal_standard = tunnyStyle;
            GH_Skin.palette_warning_standard = tunnyStyle;
            GH_Skin.palette_hidden_standard = tunnyStyle;
            base.Render(canvas, graphics, channel);
            GH_Skin.palette_normal_standard = normalStyle;
            GH_Skin.palette_warning_standard = warningStyle;
            GH_Skin.palette_hidden_standard = hiddenStyle;
        }
    }
}
