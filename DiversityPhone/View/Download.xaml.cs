﻿using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels.Utility;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;

namespace DiversityPhone.View
{
    public partial class Download : PhoneApplicationPage
    {
        private UploadVM VM { get { return DataContext as UploadVM; } }
        
        CommandButtonAdapter uploadall;

        public Download()
        {
            InitializeComponent();
            this.BackKeyPress += Sync_BackKeyPress;
        }

        void Sync_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = VM;
            if (vm != null && vm.IsUploading)
            {
                if (vm.CancelUpload.CanExecute(null))
                {
                    vm.CancelUpload.Execute(null);
                    e.Cancel = true;
                }
            }
        }        

        private void syncPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (uploadall == null && VM != null)
            {                
                uploadall = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.StartUpload);
            }
        }

        private void syncPage_Unloaded(object sender, RoutedEventArgs e)
        {            
            if (uploadall != null)
            {
                uploadall.Dispose();
                uploadall = null;
            }
        }
    }
}