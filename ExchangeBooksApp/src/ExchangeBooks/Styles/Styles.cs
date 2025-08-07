using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExchangeBooks.Styles
{
    public partial class Styles
    {
        static Styles()
        {
            InitContainers();
            InitLabels();
            InitButtons();
        }

        public static string RegularFont = "Arial";
        public static string BoldFont = "TimesNewRoman";

        public static double FontLargeSize = GetFontLargeSize();
        public static double FontNormalSize = GetFontNormalSize();
        public static double FontSmallSize = GetFontSmallSize();
        public static double FontSmallTiny = GetFontTinySize();

        public static Style CreateStyle<T>(Dictionary<BindableProperty, object> properties, Style basedOn = null)
        {
            var style = new Style(typeof(T));

            if (basedOn != null)
                style.BasedOn = basedOn;

            properties.ToList().ForEach(property =>
            {
                style.Setters.Add(property.Key, property.Value);
            });

            return style;
        }

        private static double GetFontLargeSize()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    return 20.0;
                default:
                    return 14.0;
            }
        }
        private static double GetFontNormalSize()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    return 16.0;
                case Device.Android:
                    return 12.0;
                default:
                    return 14.0;
            }
        }
        private static double GetFontSmallSize()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    return 14.0;
                default:
                    return 12.0;
            }
        }
        private static double GetFontTinySize()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    return 12.0;
                default:
                    return 10.0;
            }
        }
    }
}
