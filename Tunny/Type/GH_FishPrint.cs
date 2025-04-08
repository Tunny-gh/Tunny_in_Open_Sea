using System;
using System.Drawing;
using System.IO;

using GH_IO.Serialization;

using Grasshopper.Kernel.Types;

using Tunny.Core.Util;

namespace Tunny.Type
{
    [LoggingAspect]
    public class GH_FishPrint : GH_Goo<Bitmap>
    {
        public GH_FishPrint()
        {
        }

        public GH_FishPrint(Bitmap bitmap) : base(bitmap)
        {
        }

        public GH_FishPrint(GH_Goo<Bitmap> other) : base(other)
        {
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "FishPrint";
        public override string TypeDescription => "Goo for Tunny fish print";
        public override IGH_Goo Duplicate() => new GH_FishPrint { Value = Value };

        public override bool Read(GH_IReader reader)
        {
            string base64 = string.Empty;
            if (reader.TryGetString("FishEgg_bin", ref base64))
            {
                Value = FromBase64(base64);
            }
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("FishEgg_bin", ToBase64(Value));
            return base.Write(writer);
        }

        public override bool CastFrom(object source)
        {
            if (source is Bitmap bitmap)
            {
                Value = bitmap;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            target = default;
            if (typeof(T).IsAssignableFrom(typeof(Bitmap)))
            {
                target = (T)(object)Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Bitmap FromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(bytes))
            {
                return new Bitmap(ms);
            }
        }

        private static string ToBase64(Bitmap value)
        {
            using (var ms = new MemoryStream())
            {
                value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public override string ToString()
        {
            return "GH_FishPrint";
        }
    }
}
