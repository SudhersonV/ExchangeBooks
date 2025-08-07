using System.Collections.Generic;
using Xamarin.Forms;

namespace ExchangeBooks.Styles
{
    public partial class Styles
    {
        public static Style DefaultButtonStyle;
        public static Style ImageButtonStyle;
        public static Style EulaButtonStyle;

        static void InitButtons()
        {
            DefaultButtonStyle = CreateStyle<Button>(new Dictionary<BindableProperty, object>
            {
                { Button.FontFamilyProperty, RegularFont },
                { Button.FontSizeProperty, FontNormalSize },
                { View.MarginProperty, 10 },
                { Button.PaddingProperty, 0 },
                { View.HorizontalOptionsProperty, LayoutOptions.FillAndExpand },
                { View.VerticalOptionsProperty, LayoutOptions.Center },
                { Button.TextColorProperty, Colors.BlackTextColor },
                { VisualElement.BackgroundColorProperty, Colors.ButtonColor },
            });

            ImageButtonStyle = CreateStyle<ImageButton>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty, Colors.ButtonColor }
            });

            EulaButtonStyle = CreateStyle<Button>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty,  Colors.ButtonColor},
                { Button.TextColorProperty, Colors.IconColor }
            });
        }
    }
}
