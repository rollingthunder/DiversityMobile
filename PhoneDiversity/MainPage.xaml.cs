﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SterlingToLINQ.DiversityService;

namespace SterlingToLINQ
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Handle selection changed on ListBox
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //// If selected index is -1 (no selection) do nothing
            //if (MainListBox.SelectedIndex == -1)
            //    return;

            //// Navigate to the new page
            //NavigationService.Navigate(new Uri("/DetailsPage.xaml?selectedItem=" + MainListBox.SelectedIndex, UriKind.Relative));

            //// Reset selected index to -1 (no selection)
            //MainListBox.SelectedIndex = -1;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).LoadServiceData.Execute(null);
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            App._DeactivateEngine();
            App._ActivateEngine();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                var tb = (sender as TextBox);
                var binding = tb.GetBindingExpression(TextBox.TextProperty);
                if (binding != null)
                    binding.UpdateSource();
            }
        }

        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            var iu = new IdentificationUnit()
            {
                LastIdentificationCache = "TestIU From WP7",
                TaxonomicGroup = "fungus",
                RowGUID = Guid.NewGuid()
            };
            var iu2 = new IdentificationUnit()
            {
                LastIdentificationCache = "TestIU From WP7",
                TaxonomicGroup = "fungus",
                RowGUID = Guid.NewGuid(),
                
            };
            
            App.Repository.InsertIUAsync(234465, iu);
        }
    }
}