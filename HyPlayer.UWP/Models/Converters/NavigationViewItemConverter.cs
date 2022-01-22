using System;
using Windows.UI.Xaml.Data;

namespace HyPlayer.UWP.Models.Converters;

public class NavigationViewItemHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (b)
                return 130.0;
            else
                return 40.0;
        }
    
        return 48.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class NavigationVieItemEnableConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool)
        {
            return !(bool)value;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}