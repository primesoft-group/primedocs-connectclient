﻿using System.Windows.Controls;

namespace PrimeDocs.ConnectClient.WinApp.Views
{
    /// <summary>
    /// Interaction logic for SuccessView.xaml
    /// </summary>
    public partial class SuccessView : UserControl
    {
        public SuccessView(ViewModel.ShellViewModel data)
        {
            InitializeComponent();
            this.DataContext = data;
        }
    }
}
