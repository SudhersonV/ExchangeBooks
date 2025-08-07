using System.Collections.Generic;
using Xamarin.Forms;

namespace ExchangeBooks.Styles
{
    public partial class Styles
    {
        public static Style DefaultStackLayoutStyle;
        public static Style SearchStackLayoutStyle;
        public static Style DefaultEntryStyle;
        public static Style DefaultPickerStyle;
        public static Style DefaultFrameStyle;
        public static Style ButtonFrameStyle;
        public static Style SearchFrameStyle;
        public static Style DefaultSwipeViewStyle;
        public static Style DefaultSwipeFrameStyle;
        public static Style ButtonSwipeFrameStyle;

        static void InitContainers()
        {

            DefaultStackLayoutStyle = CreateStyle<StackLayout>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty, Color.White }
            });

            DefaultEntryStyle = CreateStyle<Entry>(new Dictionary<BindableProperty, object>
            {
                { View.HorizontalOptionsProperty, LayoutOptions.FillAndExpand },
                { View.MarginProperty, new Thickness(5) },
                { VisualElement.BackgroundColorProperty, Color.LightGray }
            });

            SearchStackLayoutStyle = CreateStyle<StackLayout>(new Dictionary<BindableProperty, object>
            {
                { Layout.PaddingProperty, new Thickness(5,5,5,0) },
                { View.HorizontalOptionsProperty, LayoutOptions.FillAndExpand },
                { StackLayout.OrientationProperty, StackOrientation.Horizontal }
            }, DefaultStackLayoutStyle);

            DefaultPickerStyle = CreateStyle<Picker>(new Dictionary<BindableProperty, object>
            {
                { View.MarginProperty, new Thickness(5) },
                { View.HorizontalOptionsProperty, LayoutOptions.FillAndExpand },
                { VisualElement.BackgroundColorProperty, Color.LightGray }
            });

            DefaultFrameStyle = CreateStyle<Frame>(new Dictionary<BindableProperty, object>
            {
                { Frame.HasShadowProperty, true },
                { Frame.BorderColorProperty, Colors.FrameBorderColor },
                { VisualElement.BackgroundColorProperty, Colors.LabelColor },
                { Frame.CornerRadiusProperty, 5 },
                { View.MarginProperty, 5 },
                { VisualElement.HeightRequestProperty, 35 },
                { VisualElement.WidthRequestProperty, 150 }
            });

            ButtonFrameStyle = CreateStyle<Frame>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty, Colors.ButtonColor }
            }, DefaultFrameStyle);

            SearchFrameStyle = CreateStyle<Frame>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.HeightRequestProperty, 100 },
                { VisualElement.WidthRequestProperty, 100 }
            }, ButtonFrameStyle);

            DefaultSwipeViewStyle = CreateStyle<SwipeView>(new Dictionary<BindableProperty, object>
            {   
                { View.MarginProperty, 5}
            });

            DefaultSwipeFrameStyle = CreateStyle<Frame>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty, Colors.LabelColor },
                { Frame.CornerRadiusProperty, 5 }
            });

            ButtonSwipeFrameStyle = CreateStyle<Frame>(new Dictionary<BindableProperty, object>
            {
                { VisualElement.BackgroundColorProperty, Colors.ButtonColor }
            }, DefaultSwipeFrameStyle);
        }
    }
}
