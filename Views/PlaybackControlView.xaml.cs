using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using YouTube_MP3_Downloader.Models;
using YouTube_MP3_Downloader.ViewModels;

namespace YouTube_MP3_Downloader.Views;

public partial class PlaybackControlView : UserControl
{
    public PlaybackControlView()
    {
        InitializeComponent();
        SeekSlider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
        SeekSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
    }

    private void OnThumbDragStarted(object sender, DragStartedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.BeginSeek();
        }
    }

    private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.EndSeek(SeekSlider.Value);
        }
    }

    private void SeekSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || !viewModel.HasDuration)
        {
            return;
        }

        // Press on the thumb: let the native drag proceed untouched.
        if (FindVisualParent<Thumb>(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        // Press on the track: suppress the track's default page jump and
        // apply click-to-seek using the exact ratio.
        var position = e.GetPosition(SeekSlider);
        var ratio = Math.Clamp(position.X / Math.Max(SeekSlider.ActualWidth, 1), 0, 1);
        var target = Math.Clamp(ratio * viewModel.DurationSeconds, 0, viewModel.DurationSeconds);
        e.Handled = true;
        viewModel.ClickSeek(target);
    }

    private static T? FindVisualParent<T>(DependencyObject? source)
        where T : DependencyObject
    {
        while (source is not null and not T)
        {
            source = VisualTreeHelper.GetParent(source);
        }

        return source as T;
    }

    private void LocalTracksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel viewModel && sender is ListBox list && list.SelectedItem is LocalTrack track)
        {
            viewModel.PlayLocalTrackCommand.Execute(track);
        }
    }
}