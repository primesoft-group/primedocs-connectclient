﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PrimeDocs.ConnectClient.WinApp.ViewWindows;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using PrimeDocs.ConnectClient.WinApp.Views;

namespace PrimeDocs.ConnectClient.WinApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            AppVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void InvokeLicenseUrl(object sender, System.Windows.RoutedEventArgs e)
        {
            string url = ((Button)e.Source).ToolTip.ToString();
            System.Diagnostics.Process.Start(url);
        }

        private void ShowFullLicense(object sender, System.Windows.RoutedEventArgs e)
        {
            ResponseWindow dial = new ResponseWindow();
            License licenseWindow = new License();
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream("PrimeDocs.ConnectClient.WinApp.Licenses.Licenses.txt"))
            {
                var reader = new StreamReader(stream);
                licenseWindow.Content.Text = reader.ReadToEnd();
            }
            dial.Height = 400;
            dial.Width = 700;
            dial.Content = licenseWindow;
            this.ShowMetroDialogAsync(dial);
        }
    }
}
