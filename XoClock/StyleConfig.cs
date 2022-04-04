using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;

namespace XoClock
{
    class StyleConfig
    {
        private const string STYLE_JSON_FILE = "style.json";
        public string FontColor;
        public string TextDropShadowColor;
        public string BackgroundImage;
        public string BackgroundColor;
        public double BottomCornerRadius;
        public double TopCornerRadius;
        public double BorderThickness;
        public string BorderColor;
        public double BackgroundOpacity;
        /// <summary>
        /// if DevMode is set to true, configuration file is reloaded every 300ms
        /// if DevMode is set to false, configuration file is loaded once at startup
        /// </summary>
        public bool DevMode;

        public static StyleConfig Load()
        {
            var ret = LoadJson(STYLE_JSON_FILE);
            return ret;
        }

        private static StyleConfig LoadJson(string fileName)
        {
            string content = File.ReadAllText(fileName);
            var ret = JsonConvert.DeserializeObject<StyleConfig>(content);
            return ret;
        }

        public static StyleConfig LoadLegacy()
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

        private static string LoadString(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
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

        public static void Save(StyleConfig style)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            string json = JsonConvert.SerializeObject(style, settings);
            File.WriteAllText(@"style.json", json);
        }
    }
}
