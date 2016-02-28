﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace BGRotator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon TrayIcon;

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // TRAY ICON
            TrayIcon = (TaskbarIcon)FindResource("TrayIcon");
        }

        public MainWindow GetMainWindow
        {
            get
            {
                return Windows.OfType<MainWindow>().FirstOrDefault();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void Favorite_Click(object sender, EventArgs e)
        {
            GetMainWindow.FavoriteWallpaper();
        }

        private void Trash_Click(object sender, EventArgs e)
        {
            GetMainWindow.TrashWallpaper();
        }

        private void Next_Click(object sender, EventArgs e)
        {
            GetMainWindow.NextWallpaper();
        }
    }
}
