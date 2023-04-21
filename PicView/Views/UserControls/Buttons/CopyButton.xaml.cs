﻿using PicView.Animations;
using PicView.Properties;
using System.Windows.Controls;
using static PicView.Animations.MouseOverAnimations;

namespace PicView.Views.UserControls.Buttons
{
    public partial class CopyButton : UserControl
    {
        public CopyButton()
        {
            InitializeComponent();

            Loaded += delegate
            {
                if (Properties.Settings.Default.DarkTheme)
                {
                    SetButtonIconMouseOverAnimations(TheButton, ButtonBrush, IconBrush);
                }
                else
                {
                    TheButton.MouseEnter += (s, x) => ButtonMouseOverAnim(ButtonBrush, true);
                    TheButton.MouseLeave += (s, x) => ButtonMouseLeaveAnimBgColor(ButtonBrush);

                    if (!Settings.Default.DarkTheme)
                    {
                        AnimationHelper.LightThemeMouseEvent(this, IconBrush);
                    }
                }
                
            };
        }
    }
}