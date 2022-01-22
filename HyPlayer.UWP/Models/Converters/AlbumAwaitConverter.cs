using System;
using Windows.UI.Xaml.Data;
using HyPlayer.Uta.ProvidableItem;

namespace HyPlayer.UWP.Models.Converters;

public class AlbumAwaitConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SingleSongBase song)
            return song.Album.GetCoverImage().GetAwaiter().GetResult();
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}