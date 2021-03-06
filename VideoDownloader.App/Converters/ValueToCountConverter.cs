﻿using System;
using System.Windows.Data;

namespace VideoDownloader.App.Converters
{
    [ValueConversion(typeof(string), typeof(int))]
    public class ValueToCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = value as string;
            return int.Parse(result ?? throw new InvalidOperationException());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
