﻿using PicView.Avalonia.Services;
using PicView.Avalonia.ViewModels;
using PicView.Core.Config;
using PicView.Core.FileHandling;

namespace PicView.Avalonia.Navigation;

public static class SortingHelper
{
    public static async Task UpdateFileList(IPlatformSpecificService platformSpecificService, MainViewModel vm, FileListHelper.SortFilesBy sortFilesBy)
    {
        SettingsHelper.Settings.Sorting.SortPreference = (int)sortFilesBy;
        if (!NavigationHelper.CanNavigate(vm))
        {
            return;
        }
        var files = await Task.FromResult(platformSpecificService.GetFiles(vm.FileInfo)).ConfigureAwait(false);
        if (files is { Count: > 0 })
        {
            vm.ImageIterator.Pics = files;
            vm.ImageIterator.Index = files.IndexOf(vm.FileInfo.FullName);
            vm.SetTitle();
        }
    }

    public static async Task UpdateFileList(IPlatformSpecificService platformSpecificService, MainViewModel vm, bool ascending)
    {
        SettingsHelper.Settings.Sorting.Ascending = ascending;
        if (!NavigationHelper.CanNavigate(vm))
        {
            return;
        }
        var files = await Task.FromResult(platformSpecificService.GetFiles(vm.FileInfo)).ConfigureAwait(false);
        if (files is { Count: > 0 })
        {
            vm.ImageIterator.Pics = files;
            vm.ImageIterator.Index = files.IndexOf(vm.FileInfo.FullName);
            vm.SetTitle();
        }
    }
}