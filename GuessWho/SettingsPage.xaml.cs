using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace GuessWho
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if(null != localSettings.Values["SubscriptionKey"])
                txtSubscriptionKey.Text = localSettings.Values["SubscriptionKey"].ToString();

            if(null != localSettings.Values["StorageAccountName"])
                txtStorageAccountName.Text = localSettings.Values["StorageAccountName"].ToString();

            if(null!= localSettings.Values["StorageAccountKey"])
                txtStorageAccountKey.Text = localSettings.Values["StorageAccountKey"].ToString();

            listViewPeopleGroup.ItemsSource = globals.gPersonGroupList;
            listViewPeopleGroup.DisplayMemberPath = "name";
            
            appbarPersonGroupNextButton.IsEnabled = HttpHandler.initDone;
        }

        private async void appBarTrainButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgressRingSettingsPage.IsActive = true;
            if (null != globals.gPersonGroupSelected && globals.gPersonGroupSelected.name.Equals("...") == false)
            {
                await PersonGroupCmds.TrainPersonGroups(globals.gPersonGroupSelected.personGroupId);
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select a PersonGroup to train", "Training Error");
                await dialog.ShowAsync();
            }
            ProgressRingSettingsPage.IsActive = false;
        }

        private async void appBarStatusButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgressRingSettingsPage.IsActive = true;
            if (null != globals.gPersonGroupSelected && globals.gPersonGroupSelected.name.Equals("...") == false)
            {
                string response = await PersonGroupCmds.StatusPersonGroups(globals.gPersonGroupSelected.personGroupId);
                globals.ShowJsonErrorPopup(response);
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select a PersonGroup to check training status for", "Status Error");
                await dialog.ShowAsync();
            }
            ProgressRingSettingsPage.IsActive = false;
        }
        
        private async void appbarSaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgressRingSettingsPage.IsActive = true;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Add("SubscriptionKey", txtSubscriptionKey.Text);
            localSettings.Values.Add("StorageAccountName", txtStorageAccountName.Text);
            localSettings.Values.Add("StorageAccountKey", txtStorageAccountKey.Text);
            ProgressRingSettingsPage.IsActive = false;
            MessageDialog dialog = new MessageDialog("Settings Saved! Continue to home page...");
            await dialog.ShowAsync();
            Frame.Navigate(typeof(MainPage));
        }
        
        private void appbarCancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void listViewPeopleGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PersonGroups personGroup = (PersonGroups)listViewPeopleGroup.SelectedItem;
            globals.gPersonGroupSelected = personGroup;
            globals.gPersonSelected = null;
        }

        private void appbarPersonGroupNextButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PersonGroupPage));
        }

        private void appbarFaceHomeButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
