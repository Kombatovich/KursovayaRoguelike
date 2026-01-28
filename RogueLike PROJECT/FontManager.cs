using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace RogueLike_PROJECT
{
    public static class FontManager
    {
        private static PrivateFontCollection? _pfc;
        private static FontFamily? _family;

        public static void LoadOnce()
        {
            if (_family != null) return;

            string path = Path.Combine(AppContext.BaseDirectory, @"Assets\CGXYZPC-Regular.ttf");
            if (!File.Exists(path))
                path = Path.Combine(AppContext.BaseDirectory, @"Assets\CGXYZPC-Regular.otf");

            if (File.Exists(path))
            {
                _pfc = new PrivateFontCollection();
                _pfc.AddFontFile(path);
                _family = _pfc.Families[0];
            }
        }

        public static Font Px(float px, FontStyle style = FontStyle.Regular)
        {
            LoadOnce();
            if (_family == null) return new Font(FontFamily.GenericSansSerif, px, style, GraphicsUnit.Pixel);
            return new Font(_family, px, style, GraphicsUnit.Pixel);
        }
    }
}
