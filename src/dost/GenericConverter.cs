using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Globalization;

namespace dost.util
{
    public class GenericConverter : IValueConverter
    {
        public Func<object, Type, object, CultureInfo, object> ConvertFunc { get; set; }
        public Func<object, Type, object, CultureInfo, object> ConvertBackFunc { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertFunc(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBackFunc(value, targetType, parameter, culture);
        }

        #endregion
    }
}
