using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExchangeBooks.ValueConverters
{
    public class Base64ToStreamConverter : IValueConverter, IMarkupExtension
    {
        public Base64ToStreamConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ImageSource.FromStream(() => new MemoryStream(System.Convert.FromBase64String(value.ToString())));
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
