using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace GuessWho
{
    public sealed partial class PersonGroupPage : Page
    {
        public PersonGroupPage()
        {
            InitializeComponent();
            AppBarButtonPersonGroupRefresh_Click(null, null);
        }

        private async void AppBarButtonPersonGroupRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            personGroupProgressRing.IsActive = true;
            
            appbarEditPersonGroupButton.IsEnabled = false;
            appbarPersonGroupNextButton.IsEnabled = false;
            appbarDeletePersonGroupButton.IsEnabled = false;

            List<PersonGroups> personGroups = await PersonGroupCmds.ListPersonGroups();
            personGroupListView.ItemsSource = personGroups;
            personGroupListView.DisplayMemberPath = "name";
            globals.gPersonGroupList = personGroups;

            personGroupProgressRing.IsActive = false;
            this.IsEnabled = true;
        }

        private async void AppBarButtonAddPersonGroup_Click(object sender, RoutedEventArgs e)
        {
            if (null == personGroupListView.SelectedItem || ((PersonGroups)personGroupListView.SelectedItem).name.Equals("..."))
            {
                if (txtPersonGroup.Text.Trim() != "" && txtPersonGroup.Text != "...")
                {
                    this.IsEnabled = false;
                    personGroupProgressRing.IsActive = true;

                    string response = await PersonGroupCmds.AddPersonGroups(txtPersonGroup.Text.ToLower().Replace(' ', '_'),
                                                                            txtPersonGroup.Text,
                                                                            txtPersonGroupContext.Text);
                    
                    personGroupProgressRing.IsActive = false;
                    this.IsEnabled = true;
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Add a name to person group", "Add Error");
                    await dialog.ShowAsync();
                }
            }

            AppBarButtonPersonGroupRefresh_Click(null, null);
        }

        private async void AppBarButtonEditPersonGroup_Click(object sender, RoutedEventArgs e)
        {
            if (null != personGroupListView.SelectedItem && !((PersonGroups)personGroupListView.SelectedItem).name.Equals("..."))
            {
                this.IsEnabled = false;
                personGroupProgressRing.IsActive = true;

                string response = await PersonGroupCmds.UpdatePersonGroups(globals.gPersonGroupSelected.personGroupId,
                                                                           txtPersonGroup.Text,
                                                                           txtPersonGroupContext.Text);
                
                personGroupProgressRing.IsActive = false;
                this.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select an existing person group and change name", "Update Error");
                await dialog.ShowAsync();
            }

            AppBarButtonPersonGroupRefresh_Click(null, null);
        }
        
        private async void AppBarButtonDeletePerson_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Delete all Person - Faces - Blobs - Thumbs
            if (null != personGroupListView.SelectedItem)
            {
                this.IsEnabled = false;
                personGroupProgressRing.IsActive = true;

                string response = await PersonGroupCmds.DeletePersonGroups(((PersonGroups)personGroupListView.SelectedItem).personGroupId);
                
                personGroupProgressRing.IsActive = false;
                this.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select a person group to delete!", "Delete Error");
                await dialog.ShowAsync();
            }

            AppBarButtonPersonGroupRefresh_Click(null, null);
        }

        private void peopleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsEnabled = false;
            personGroupProgressRing.IsActive = true;
            appbarEditPersonGroupButton.IsEnabled = true;
            appbarDeletePersonGroupButton.IsEnabled = true;
            appbarPersonGroupNextButton.IsEnabled = true;

            PersonGroups personGroup = (PersonGroups)personGroupListView.SelectedItem;
            txtPersonGroup.Text = (null == personGroup) ? "" : personGroup.name;
            txtPersonGroupContext.Text = (null == personGroup) ? "" : personGroup.userData;

            personGroupProgressRing.IsActive = false;
            this.IsEnabled = true;

            globals.gPersonGroupSelected = personGroup;
            globals.gPersonSelected = null;            
        }

        private void appbarPersonGroupHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void appbarPersonGroupNextButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PersonsPage));
        }
        
    }
}
