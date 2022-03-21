using System.Configuration;

namespace XoClock
{
    class StyleConfig
    {
        public string FontColor;
        public string TextDropShadowColor;
        public string BackgroundImage;
        public string BackgroundColor;
        public double CornerRadius;
        public double BorderThickness;
        public double BackgroundOpacity;

        public static StyleConfig Load()
        {
            var ret = new StyleConfig();
            ret.FontColor = LoadString("FontColor");
            ret.TextDropShadowColor = LoadString("TextDropShadowColor");
            ret.BackgroundImage = LoadString("BackgroundImage");
            ret.BackgroundColor = LoadString("BackgroundColor");
            ret.CornerRadius = LoadDouble("CornerRadius", 10);
            ret.BorderThickness = LoadDouble("BorderThickness", 0);
            ret.BackgroundOpacity = LoadDouble("BackgroundOpacity", 1);
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
