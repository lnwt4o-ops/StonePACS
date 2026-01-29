using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace StonePACS.Views
{
    public class StatusColorConverter : IMultiValueConverter
    {
        public static StatusColorConverter Instance { get; } = new StatusColorConverter();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count > 0 && values[0] is string status)
            {
                return status switch
                {
                    "Scheduled" => new SolidColorBrush(Color.Parse("#F59E0B")), // Orange
                    "Completed" => new SolidColorBrush(Color.Parse("#10B981")), // Green
                    "In Progress" => new SolidColorBrush(Color.Parse("#3B82F6")), // Blue
                    "Cancelled" => new SolidColorBrush(Color.Parse("#EF4444")), // Red
                    _ => new SolidColorBrush(Color.Parse("#6B7280")) // Gray
                };
            }
            return new SolidColorBrush(Color.Parse("#6B7280"));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
