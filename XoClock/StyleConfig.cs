using System.Configuration;

namespace XoClock
{
    class StyleConfig
    {
        public string FontColor;
        public string TextDropShadowColor;
        public string bgImagePath;
        public string BgColor;
        public double CornerRadius;
        public double BorderThickness;
        public double BackgroundOpacity;

        public static StyleConfig Load()
        {
            var ret = new StyleConfig();
            ret.FontColor = ConfigurationManager.AppSettings.Get("FontColor");
            ret.TextDropShadowColor = ConfigurationManager.AppSettings.Get("TextDropShadowColor");
            ret.bgImagePath = ConfigurationManager.AppSettings.Get("BgImage");
            ret.BgColor = ConfigurationManager.AppSettings.Get("BgColor");
            string radius = ConfigurationManager.AppSettings.Get("CornerRadius");
            string BackgroundOpacity = ConfigurationManager.AppSettings.Get("BackgroundOpacity");
            ret.CornerRadius = int.Parse(radius);
            ret.BorderThickness = 3;
            ret.BackgroundOpacity = double.Parse(BackgroundOpacity);
            return ret;
        }
    }
}
