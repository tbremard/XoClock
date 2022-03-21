using System.Configuration;

namespace XoClock
{
    class StyleConfig
    {
        public string FontColor;
        public string TextDropShadowColor;
        public string BackgroundImage;
        public string BackgroundColor;
        public double BottomCornerRadius;
        public double TopCornerRadius;
        public double BorderThickness;
        public double BackgroundOpacity;
        public string BorderColor;

        public static StyleConfig Load()
        {
            var ret = new StyleConfig();
            ret.FontColor = LoadString("FontColor");
            ret.TextDropShadowColor = LoadString("TextDropShadowColor");
            ret.BackgroundImage = LoadString("BackgroundImage");
            ret.BackgroundColor = LoadString("BackgroundColor");
            ret.BottomCornerRadius = LoadDouble("BottomCornerRadius", 10);
            ret.TopCornerRadius = LoadDouble("TopCornerRadius", 10);
            ret.BorderThickness = LoadDouble("BorderThickness", 0);
            ret.BackgroundOpacity = LoadDouble("BackgroundOpacity", 1);
            ret.BorderColor = LoadString("BorderColor");
            return ret;
        }

        private static string LoadString(string color)
        {
            return ConfigurationManager.AppSettings.Get(color);
        }

        private static double LoadDouble(string key, double defaultValue)
        {
            double ret;
            string value = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrEmpty(value))
            {
                ret = defaultValue;
            }
            else
            {
                if(!double.TryParse(value, out ret))
                {
                    ret = defaultValue;
                }
            }
            return ret;

        }
    }
}
