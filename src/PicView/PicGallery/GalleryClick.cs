﻿using PicView.ChangeImage;
using PicView.ChangeTitlebar;
using PicView.ConfigureSettings;
using PicView.ImageHandling;
using PicView.Properties;
using PicView.UILogic;
using PicView.Views.UserControls.Gallery;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static PicView.ChangeImage.Navigation;
using static PicView.ImageHandling.Thumbnails;
using static PicView.UILogic.Sizing.ScaleImage;
using static PicView.UILogic.UC;

namespace PicView.PicGallery;

/// <summary>
/// Logic for what happens when user clicks on
/// thumbnail gallery item
/// </summary>
internal static class GalleryClick
{
    internal static async Task ClickAsync(int id)
    {
        if (Settings.Default.FullscreenGallery || Settings.Default.IsBottomGalleryShown && !GalleryFunctions.IsGalleryOpen)
        {
            await ItemClickAsync(id).ConfigureAwait(false);
            return;
        }

        await ConfigureWindows.GetMainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            ConfigureWindows.GetMainWindow.Focus();
            ConfigureWindows.GetMainWindow.MainImage.Visibility = Visibility.Hidden;
            var galleryItem = GetPicGallery.Container.Children[id] as PicGalleryItem;
            ConfigureWindows.GetMainWindow.MainImage.Source = galleryItem.ThumbImage.Source;
        });

        Border? border = null;
        Image? image = null;

        var imageSize = ImageSizeFunctions.GetImageSize(Pics[id]);
        if (imageSize.HasValue)
        {
            await ConfigureWindows.GetMainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Send, () =>
            {
                SetTitle.SetLoadingString();
                if (Settings.Default.IsBottomGalleryShown)
                {
                    GalleryToggle.CloseHorizontalGallery();
                }

                GetPicGallery.Height = Settings.Default.IsBottomGalleryShown
                    ? GalleryNavigation.PicGalleryItemSize + 22
                    : 0;
                FitImage(imageSize.Value.Width, imageSize.Value.Height);
            });
        }

        var fromSize = GalleryNavigation.PicGalleryItemSize;
        var toSize = new[] { XWidth <= 0 ? ConfigureWindows.GetMainWindow.ActualWidth : XWidth, XHeight <= 0 ? ConfigureWindows.GetMainWindow.ActualHeight : XHeight };
        var acceleration = 0.2;
        var deceleration = 0.4;
        var duration = TimeSpan.FromSeconds(.3);

        var widthAnimation = new DoubleAnimation
        {
            From = fromSize,
            To = toSize[0],
            Duration = duration,
            AccelerationRatio = acceleration,
            DecelerationRatio = deceleration,
            FillBehavior = FillBehavior.Stop
        };

        var heightAnimation = new DoubleAnimation
        {
            From = fromSize,
            To = toSize[1],
            Duration = duration,
            AccelerationRatio = acceleration,
            DecelerationRatio = deceleration,
            FillBehavior = FillBehavior.Stop
        };

        widthAnimation.Completed += async delegate
        {
            await ConfigureWindows.GetMainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                if (!Settings.Default.IsBottomGalleryShown)
                {
                    GetPicGallery.Visibility = Visibility.Collapsed; // prevent it from popping up again
                }
                border.Opacity = 0;
                ConfigureWindows.GetMainWindow.ParentContainer.Children.Remove(border);
                image = null;
                GalleryFunctions.IsGalleryOpen = false;
                ConfigureWindows.GetMainWindow.MainImage.Visibility = Visibility.Visible;
            });
            await ItemClickAsync(id).ConfigureAwait(false);
        };

        image = new Image
        {
            Source = GetThumb(id) ?? ImageFunctions.ShowLogo() ?? ImageFunctions.ImageErrorMessage(),
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Need to add border for background to pictures with transparent background
        border = new Border
        {
            Background = ConfigColors.BackgroundColorBrush,
            Child = image
        };

        await ConfigureWindows.GetMainWindow.Dispatcher.InvokeAsync(() =>
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = duration,
                AccelerationRatio = acceleration,
                DecelerationRatio = deceleration,
                FillBehavior = FillBehavior.Stop
            };
            GetPicGallery.Container.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);

            GetPicGallery.x2.Visibility = Visibility.Hidden;
            ConfigureWindows.GetMainWindow.ParentContainer.Children.Add(border);

            border.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
            border.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        }, DispatcherPriority.Send);
    }

    private static async Task ItemClickAsync(int id)
    {
        await ConfigureWindows.GetMainWindow.Dispatcher.InvokeAsync(() =>
        {
            GetPicGallery.Scroller.CanContentScroll = false; // Enable animations
            // Deselect current item
            GalleryNavigation.SetSelected(GalleryNavigation.SelectedGalleryItem, false);
            GalleryNavigation.SetSelected(FolderIndex, false);

            // Restore interface elements if needed
            if (!Settings.Default.ShowInterface || Settings.Default.Fullscreen && Settings.Default.ShowAltInterfaceButtons)
            {
                HideInterfaceLogic.ShowNavigation(true);
                HideInterfaceLogic.ShowShortcuts(true);
            }

            // Select next item
            GalleryNavigation.SetSelected(id, true);
            GalleryNavigation.SelectedGalleryItem = id;
            GalleryNavigation.ScrollToGalleryCenter();
        });
        // Change image
        await LoadPic.LoadPicAtIndexAsync(id).ConfigureAwait(false);
    }
}