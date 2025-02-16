﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using PicView.Avalonia.Helpers;
using PicView.Avalonia.Keybindings;
using PicView.Avalonia.MacOS.Views;
using PicView.Avalonia.Services;
using PicView.Avalonia.ViewModels;
using PicView.Core.Config;
using PicView.Core.FileHandling;
using PicView.Core.Localization;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;

namespace PicView.Avalonia.MacOS;

public class App : Application, IPlatformSpecificService
{
    private ExifWindow? _exifWindow;
    private SettingsWindow? _settingsWindow;
    private KeybindingsWindow? _keybindingsWindow;
    private AboutWindow? _aboutWindow;

    public override void Initialize()
    {
        ProfileOptimization.SetProfileRoot(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/"));
        ProfileOptimization.StartProfile("ProfileOptimization");
        base.OnFrameworkInitializationCompleted();
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        bool settingsExists;
        try
        {
            settingsExists = await SettingsHelper.LoadSettingsAsync();
            await Task.Run(() => TranslationHelper.LoadLanguage(SettingsHelper.Settings.UIProperties.UserLanguage));
        }
        catch (TaskCanceledException)
        {
            return;
        }
        var w = desktop.MainWindow = new MacMainWindow();
        var vm = new MainViewModel(this);
        w.DataContext = vm;
        if (!settingsExists)
        {
            WindowHelper.CenterWindowOnScreen();
            vm.CanResize = true;
            vm.IsAutoFit = false;
        }
        else
        {
            if (SettingsHelper.Settings.WindowProperties.AutoFit)
            {
                desktop.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                vm.SizeToContent = SizeToContent.WidthAndHeight;
                vm.CanResize = false;
                vm.IsAutoFit = true;
            }
            else
            {
                vm.CanResize = true;
                vm.IsAutoFit = false;
                WindowHelper.InitializeWindowSizeAndPosition(w);
            }
        }
        w.Show();

        await vm.StartUpTask();
        await KeybindingsHelper.LoadKeybindings(vm).ConfigureAwait(false);
        w.KeyDown += async (_, e) => await MainKeyboardShortcuts.MainWindow_KeysDownAsync(e).ConfigureAwait(false);
        w.KeyUp += (_, e) => MainKeyboardShortcuts.MainWindow_KeysUp(e);
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            w.KeyBindings.Add(new KeyBinding { Command = vm.ToggleUICommand, Gesture = new KeyGesture(Key.Z, KeyModifiers.Alt) });
        });

        vm.ShowExifWindowCommand = ReactiveCommand.Create(() =>
        {
            if (_exifWindow is null)
            {
                _exifWindow = new ExifWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _exifWindow.Show(w);
                _exifWindow.Closing += (s, e) => _exifWindow = null;
            }
            else
            {
                _exifWindow.Activate();
            }
            vm.CloseMenuCommand.Execute(null);
        });

        vm.ShowSettingsWindowCommand = ReactiveCommand.Create(() =>
        {
            if (_settingsWindow is null)
            {
                _settingsWindow = new SettingsWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _settingsWindow.Show(w);
                _settingsWindow.Closing += (s, e) => _settingsWindow = null;
            }
            else
            {
                _settingsWindow.Activate();
            }
            vm.CloseMenuCommand.Execute(null);
        });

        vm.ShowKeybindingsWindowCommand = ReactiveCommand.Create(() =>
        {
            if (_keybindingsWindow is null)
            {
                _keybindingsWindow = new Views.KeybindingsWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _keybindingsWindow.Show(w);
                _keybindingsWindow.Closing += (s, e) => _keybindingsWindow = null;
            }
            else
            {
                _keybindingsWindow.Activate();
            }
            vm.CloseMenuCommand.Execute(null);
        });
        
        vm.ShowAboutWindowCommand = ReactiveCommand.Create(() =>
        {
            if (_aboutWindow is null)
            {
                _aboutWindow = new AboutWindow
                {
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                _aboutWindow.Show(w);
                _aboutWindow.Closing += (s, e) => _aboutWindow = null;
            }
            else
            {
                _aboutWindow.Activate();
            }
            vm.CloseMenuCommand.Execute(null);
        });
    }

    public void SetCursorPos(int x, int y)
    {
        // TODO: Implement SetCursorPos
    }

    public List<string> GetFiles(FileInfo fileInfo)
    {
        var files = FileListHelper.RetrieveFiles(fileInfo);
        return SortHelper.SortIEnumerable(files, this);
    }

    public int CompareStrings(string str1, string str2)
    {
        return string.CompareOrdinal(str1, str2);
    }
}