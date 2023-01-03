using System;
using System.Windows.Data;

namespace the_game
{
    class SubstractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = value;
            if (value != null && parameter != null)
            {
                try
                {
                    double x = (double)value;
                    double y = double.Parse(parameter.ToString());
                    result = x / y;
                }
                catch
                {

                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
