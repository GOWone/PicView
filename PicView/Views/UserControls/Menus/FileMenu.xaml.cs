﻿using System.Windows.Controls;
using System.Windows.Media;
using PicView.ChangeImage;
using PicView.ChangeTitlebar;
using PicView.FileHandling;
using PicView.UILogic;
using static PicView.Animations.MouseOverAnimations;

namespace PicView.Views.UserControls.Menus
{
    public partial class FileMenu : UserControl
    {
        public FileMenu()
        {
            InitializeComponent();

            // OpenBorder
            SetButtonIconMouseOverAnimations(OpenBorder, OpenBorderBrush, (SolidColorBrush)Resources["OpenBorderFill"]);
            OpenBorder.MouseLeftButtonDown += async (_, _) => await Open_Save.OpenAsync().ConfigureAwait(false);
            OpenButton.Click += async (_, _) => await Open_Save.OpenAsync().ConfigureAwait(false);

            // SaveButton
            SetButtonIconMouseOverAnimations(SaveButton, SaveButtonBrush, SaveButtonIconBrush);
            SaveButton.Click += async (_, _) => await Open_Save.SaveFilesAsync().ConfigureAwait(false);

            // CopyButton
            SetButtonIconMouseOverAnimations(CopyButton, CopyButtonBrush, CopyButtonIconBrush);
            CopyButton.Click += (_, _) => CopyPaste.Copy();

            // PasteButton
            SetButtonIconMouseOverAnimations(PasteButton, PasteButtonBrush, PasteButtonIconBrush);
            PasteButton.Click += async (_, _) => await CopyPaste.PasteAsync().ConfigureAwait(false);
            PasteButton.PreviewMouseLeftButtonDown += delegate { UC.Close_UserControls(); };

            // FileLocationBorder
            SetButtonIconMouseOverAnimations(FileLocationBorder, FileLocationBrush, (SolidColorBrush)Resources["LocationBorderFill"]);
            FileLocationBorder.MouseLeftButtonDown += (_, _) => Open_Save.Open_In_Explorer();
            FileLocationButton.Click += (_, _) => Open_Save.Open_In_Explorer();

            // PrintBorder
            SetButtonIconMouseOverAnimations(PrintBorder, PrintButtonBrush, (SolidColorBrush)Resources["PrintBorderFill"]);
            PrintBorder.MouseLeftButtonDown += (_, _) => Open_Save.Print(Navigation.Pics?[Navigation.FolderIndex]);
            PrintButton.Click += (_, _) => Open_Save.Print(Navigation.Pics?[Navigation.FolderIndex]);

            // ReloadButton
            SetButtonIconMouseOverAnimations(ReloadButton, ReloadButtonBrush, (SolidColorBrush)Resources["ReloadButtonIconBrush"]);
            ReloadButton.Click += async (_, _) => await ErrorHandling.ReloadAsync().ConfigureAwait(false);

            // RecycleButton
            SetButtonIconMouseOverAnimations(RecycleButton, RecycleButtonBrush, (SolidColorBrush)Resources["RecycleButtonIconBrush"]);
            RecycleButton.Click += async (_, _) => await DeleteFiles.DeleteFileAsync(true).ConfigureAwait(false);

            // OpenWithBorder
            SetButtonIconMouseOverAnimations(OpenWithBorder, OpenWithBorderBrush, (SolidColorBrush)Resources["OpenWithBorderFill"]);
            OpenWithBorder.MouseLeftButtonDown += (_, _) => Open_Save.OpenWith();
            OpenWith.Click += (_, _) => Open_Save.OpenWith();

            // RenameBorder
            SetButtonIconMouseOverAnimations(RenameBorder, RenameButtonBrush, (SolidColorBrush)Resources["RenameBorderFill"]);
            RenameBorder.MouseLeftButtonDown += (_, _) => { UC.Close_UserControls(); EditTitleBar.EditTitleBar_Text(); };
            RenameButton.Click += (_, _) => { UC.Close_UserControls(); EditTitleBar.EditTitleBar_Text(); };
        }
    }
}