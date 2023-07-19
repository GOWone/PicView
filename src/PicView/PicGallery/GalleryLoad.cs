﻿using PicView.ChangeImage;
using PicView.FileHandling;
using PicView.ImageHandling;
using PicView.UILogic;
using PicView.Views.UserControls.Gallery;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PicView.PicGallery;

internal static class GalleryLoad
{
    internal static bool IsLoading { get; set; }

    internal static void PicGallery_Loaded(object sender, RoutedEventArgs e)
    {
        // Add events and set fields, when it's loaded.
        UC.GetPicGallery.grid.MouseLeftButtonDown += (_, _) => ConfigureWindows.GetMainWindow.Focus();
        UC.GetPicGallery.x2.MouseLeftButtonDown += (_, _) => GalleryToggle.CloseHorizontalGallery();
    }

    internal class GalleryThumbHolder
    {
        internal string FileLocation { get; set; }
        internal string FileName { get; set; }
        internal string FileSize { get; set; }
        internal string FileDate { get; set; }
        internal BitmapSource BitmapSource { get; set; }

        internal GalleryThumbHolder(string fileLocation, string fileName, string fileSize, string fileDate,
            BitmapSource bitmapSource)
        {
            FileLocation = fileLocation;
            FileName = fileName;
            FileSize = fileSize;
            FileDate = fileDate;
            BitmapSource = bitmapSource;
        }

        internal static GalleryThumbHolder GetThumbData(int index)
        {
            var bitmapSource = Thumbnails.GetBitmapSourceThumb(Navigation.Pics[index], (int)GalleryNavigation.PicGalleryItemSize);
            var fileInfo = new FileInfo(Navigation.Pics[index]);
            var fileLocation = fileInfo.FullName;
            var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var fileSize = $"{(string)Application.Current.Resources["FileSize"]}: {fileInfo.Length.GetReadableFileSize()}";
            var fileDate = $"{(string)Application.Current.Resources["Modified"]}: {fileInfo.LastWriteTimeUtc.ToString(CultureInfo.CurrentCulture)}";
            return new GalleryThumbHolder(fileLocation, fileName, fileSize, fileDate, bitmapSource);
        }
    }

    internal static async Task LoadAsync()
    {
        if (UC.GetPicGallery is null || IsLoading) { return; }

        IsLoading = true;
        var source = new CancellationTokenSource();
        var iterations = Navigation.Pics.Count;

        await Task.Run(async () =>
        {
            if (iterations < 3000)
            {
                var galleryThumbHolderList = new ConcurrentDictionary<int, GalleryThumbHolder>();
                async Task UpdateGalleryThumbHolderAsync(int index)
                {
                    try
                    {
                        if (!IsLoading || Navigation.Pics?.Count < Navigation.FolderIndex || Navigation.Pics?.Count < 1)
                        {
                            throw new TaskCanceledException();
                        }

                        var galleryThumbHolder = galleryThumbHolderList[index];
                        await UC.GetPicGallery.Container.Dispatcher.InvokeAsync(() =>
                        {
                            if (UC.GetPicGallery.Container.Children.Count <= 0 && UC.GetPicGallery.Container.Children.Count > index)
                            {
                                return;
                            }
                            var item = (PicGalleryItem)UC.GetPicGallery.Container.Children[index];
                            item.ThumbImage.Source = galleryThumbHolder.BitmapSource;
                            item.MouseEnter += delegate { item.Popup.IsOpen = true; };
                            item.MouseLeave += delegate { item.Popup.IsOpen = false; };
                            item.ThumbFileLocation.Text = galleryThumbHolder.FileLocation;
                            item.ThumbFileName.Text = galleryThumbHolder.FileName;
                            item.ThumbFileSize.Text = galleryThumbHolder.FileSize;
                            item.ThumbFileDate.Text = galleryThumbHolder.FileDate;
                            item.MouseLeftButtonUp += async delegate { await GalleryClick.ClickAsync(index).ConfigureAwait(false); };
                        }, DispatcherPriority.Background);
                        galleryThumbHolderList.TryRemove(index, out galleryThumbHolder);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Trace.WriteLine(e.Message);
#endif
                        GalleryFunctions.Clear();
                        IsLoading = false;
                    }
                }

                _ = Task.Run(async () =>
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        try
                        {
                            if (!IsLoading || Navigation.Pics?.Count < Navigation.FolderIndex || Navigation.Pics?.Count < 1)
                            {
                                GalleryFunctions.Clear();
                                IsLoading = false;
                                source.Token.ThrowIfCancellationRequested();
                                throw new TaskCanceledException();
                            }

                            Add(i);

                            if (!galleryThumbHolderList.ContainsKey(i))
                            {
                                var i2 = i;
                                _ = Task.Run(async () =>
                                {
                                    while (!galleryThumbHolderList.ContainsKey(i2))
                                    {
                                        await Task.Delay(200, source.Token).ConfigureAwait(true);
                                    }

                                    await UpdateGalleryThumbHolderAsync(i2).ConfigureAwait(true);
                                }, source.Token).ConfigureAwait(false);
                                continue;
                            }

                            if (!IsLoading || Navigation.Pics.Count < Navigation.FolderIndex || Navigation.Pics.Count < 1)
                            {
                                throw new TaskCanceledException();
                            }
                            var i1 = i;
                            await UpdateGalleryThumbHolderAsync(i1).ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
#if DEBUG
                            Trace.WriteLine(e.Message);
#endif
                            GalleryFunctions.Clear();
                            IsLoading = false;
                            return;
                        }
                    }
                }, source.Token).ConfigureAwait(false);

                Parallel.For(0, iterations, (i, loopState) =>
                {
                    try
                    {
                        if (!IsLoading || Navigation.Pics?.Count < Navigation.FolderIndex || Navigation.Pics?.Count < 1)
                        {
                            throw new TaskCanceledException();
                        }

                        // Check if the item already exists in the collection
                        if (galleryThumbHolderList.ContainsKey(i))
                        {
                            return; // Skip the current iteration
                        }

                        var galleryThumbHolderItem = GalleryThumbHolder.GetThumbData(i);
                        galleryThumbHolderList.TryAdd(i, galleryThumbHolderItem);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Trace.WriteLine(e.Message);
#endif
                        IsLoading = false;
                        loopState.Stop();
                    }
                });
            }
            else
            {
                for (int i = 0; i < iterations; i++)
                {
                    try
                    {
                        if (!IsLoading || Navigation.Pics?.Count < Navigation.FolderIndex || Navigation.Pics?.Count < 1)
                        {
                            throw new TaskCanceledException();
                        }
                        Add(i);

                        var galleryThumbHolderItem = await Task.FromResult(GalleryThumbHolder.GetThumbData(i)).ConfigureAwait(false);
                        await UC.GetPicGallery.Dispatcher.InvokeAsync(() =>
                        {
                            UpdatePic(i, galleryThumbHolderItem.BitmapSource, galleryThumbHolderItem.FileLocation,
                                galleryThumbHolderItem.FileName, galleryThumbHolderItem.FileSize,
                                galleryThumbHolderItem.FileDate);
                        }, DispatcherPriority.Background, source.Token);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        Trace.WriteLine(e.Message);
#endif
                        IsLoading = false;
                        return;
                    }
                }
            }
        }, source.Token).ConfigureAwait(false);
    }

    internal static async Task ReloadGalleryAsync()
    {
        while (IsLoading)
        {
            await Task.Delay(200).ConfigureAwait(false);
            ConfigureWindows.GetMainWindow.Dispatcher.Invoke(() =>
            {
                if (UC.GetPicGallery.Container.Children.Count is 0)
                {
                    IsLoading = false;
                }
            });
        }
        await LoadAsync().ConfigureAwait(false);
    }

    internal static void Add(int i)
    {
        var selected = i == Navigation.FolderIndex;
        UC.GetPicGallery.Dispatcher.Invoke(() =>
        {
            var item = new PicGalleryItem(null, i, selected);
            item.MouseLeftButtonUp += async delegate
            {
                await GalleryClick.ClickAsync(i).ConfigureAwait(false);
            };

            UC.GetPicGallery.Container.Children.Add(item);
        }, DispatcherPriority.Render);
    }

    internal static void UpdatePic(int i, BitmapSource? pic, string fileLocation, string fileName, string fileSize, string fileDate)
    {
        try
        {
            if (Navigation.Pics?.Count < Navigation.FolderIndex || Navigation.Pics?.Count < 1 || i >= UC.GetPicGallery.Container.Children.Count)
            {
                GalleryFunctions.Clear();
                return;
            }

            var item = (PicGalleryItem)UC.GetPicGallery.Container.Children[i];
            item.ThumbImage.Source = pic;
            item.MouseEnter += delegate
            {
                item.Popup.IsOpen = true;
            };
            item.MouseLeave += delegate
            {
                item.Popup.IsOpen = false;
            };
            item.ThumbFileLocation.Text = fileLocation;
            item.ThumbFileName.Text = fileName;
            item.ThumbFileSize.Text = fileSize;
            item.ThumbFileDate.Text = fileDate;

            if (i == Navigation.FolderIndex || Navigation.FolderIndex <= UC.GetPicGallery.Container.Children.Count)
            {
                return;
            }

            if (!Navigation.Reverse && Navigation.FolderIndex <= GalleryNavigation.HorizontalItems)
            {
                UC.GetPicGallery.Scroller.ScrollToRightEnd();
            }
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine(e.Message);
#endif
            // Suppress task cancellation
        }
    }
}