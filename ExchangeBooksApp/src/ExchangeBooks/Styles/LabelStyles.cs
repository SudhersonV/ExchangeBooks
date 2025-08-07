using Xamarin.Forms;
using System.Collections.Generic;
using ExchangeBooks.Constants;

namespace ExchangeBooks.Styles
{
    public partial class Styles
    {
        public static Style DefaultLabelStyle;
        public static Style DefaultHeaderLabelStyle;
        public static Style DefaultIconLabelStyle;
        public static Style IndicatorStyle;
        public static Style DefaultSwitchStyle;

        static void InitLabels()
        {
            DefaultLabelStyle = CreateStyle<Label>(new Dictionary<BindableProperty, object>
            {
                { Label.FontFamilyProperty, RegularFont },
                { Label.FontSizeProperty, FontLargeSize },
                { View.MarginProperty, 10 },
                { View.HorizontalOptionsProperty, LayoutOptions.Start },
                { View.VerticalOptionsProperty, LayoutOptions.Center },
                { Label.TextColorProperty, Colors.BlackTextColor }
            });

            DefaultHeaderLabelStyle = CreateStyle<Label>(new Dictionary<BindableProperty, object>
            {
                { Label.FontAttributesProperty, FontAttributes.Bold },
                { Label.TextColorProperty, Colors.BlackTextColor }
            }, DefaultLabelStyle);

            DefaultIconLabelStyle = CreateStyle<Label>(new Dictionary<BindableProperty, object>
            {
                { Label.FontFamilyProperty, MaterialIcons.FontFamily },
                { View.MarginProperty, 0 },
                { View.HorizontalOptionsProperty, LayoutOptions.Center },
                { Label.HorizontalTextAlignmentProperty, TextAlignment.Center},
                { Label.VerticalTextAlignmentProperty, TextAlignment.Center},
                { Label.TextColorProperty, Colors.IconColor },
            }, DefaultLabelStyle);

            DefaultSwitchStyle = CreateStyle<Switch>(new Dictionary<BindableProperty, object>
            {
                { Switch.OnColorProperty, Colors.ButtonColor },
                { Switch.ThumbColorProperty, Colors.IconColor },
                { View.MarginProperty, 10 },
                { View.HorizontalOptionsProperty, LayoutOptions.Start },
                { View.VerticalOptionsProperty, LayoutOptions.Center }
            });

            IndicatorStyle = CreateStyle<ActivityIndicator>(new Dictionary<BindableProperty, object>
            {
                { ActivityIndicator.ColorProperty, Colors.IconColor },
            });
        }
    }
}
