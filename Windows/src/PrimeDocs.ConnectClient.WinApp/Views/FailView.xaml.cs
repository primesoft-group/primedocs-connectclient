﻿using System.Windows.Controls;

namespace PrimeDocs.ConnectClient.WinApp.Views
{
    /// <summary>
    /// Interaction logic for FailView.xaml
    /// </summary>
    public partial class FailView : UserControl
    {
        public FailView(ViewModel.ShellViewModel data)
        {
            InitializeComponent();
            this.DataContext = data;
        }
        
    }
}
