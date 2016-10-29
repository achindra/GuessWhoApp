using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;

namespace GuessWho
{
    public sealed partial class PersonsPage : Page
    {
        public PersonsPage()
        {
            InitializeComponent();
            AppBarButtonPersonRefresh_Click(null, null);
        }

        private async void AppBarButtonPersonRefresh_Click(object sender, RoutedEventArgs e)
        {
            textBlockPerson.Text = "People in " + globals.gPersonGroupSelected.name + " Group";
            if (null != globals.gPersonGroupSelected && (globals.gPersonGroupSelected.name.Equals("...") == false))
            {
                this.IsEnabled = false;
                personProgressRing.IsActive = true;
                
                appbarEditPersonButton.IsEnabled = false;
                appbarPersonNextButton.IsEnabled = false;
                appbarDeletePersonButton.IsEnabled = false;

                List<Persons> persons = await PersonsCmds.ListPersonInGroup(globals.gPersonGroupSelected.personGroupId);
                personListView.ItemsSource = persons;
                personListView.DisplayMemberPath = "name";
                
                personProgressRing.IsActive = false;
                this.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("No person group selected", "Refresh Error");
                await dialog.ShowAsync();
            }
        }

        private async void AppBarButtonAddPerson_Click(object sender, RoutedEventArgs e)
        {
            if (null == personListView.SelectedItem || ((Persons)personListView.SelectedItem).name.Equals("..."))
            {
                if (txtPerson.Text.Trim() != "" && txtPerson.Text != "...")
                {
                    this.IsEnabled = false;
                    personProgressRing.IsActive = true;

                    string response = await PersonsCmds.CreatePerson(globals.gPersonGroupSelected.personGroupId,// txtPerson.Text.ToLower().Replace(' ', '_'),
                                                                    txtPerson.Text, 
                                                                    txtPersonContext.Text);
                    
                    personProgressRing.IsActive = false;
                    this.IsEnabled = true;
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Add a name for person", "Add Error");
                    await dialog.ShowAsync();
                }
            }

            AppBarButtonPersonRefresh_Click(null, null);
        }

        private async void AppBarButtonEditPerson_Click(object sender, RoutedEventArgs e)
        {
            if (null != personListView.SelectedItem && !((PersonGroups)personListView.SelectedItem).name.Equals("..."))
            {
                this.IsEnabled = false;
                personProgressRing.IsActive = true;

                string response = await PersonsCmds.UpdatePerson(globals.gPersonGroupSelected.personGroupId,
                                                                globals.gPersonSelected.personId,
                                                                txtPerson.Text,
                                                                txtPersonContext.Text);
                
                personProgressRing.IsActive = false;
                this.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select an existing person and change name", "Update Error");
                await dialog.ShowAsync();
            }

            AppBarButtonPersonRefresh_Click(null, null);
        }

        private async void AppBarButtonDeletePerson_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Delete all Faces - Blobs - Thumbs
            if (null != personListView.SelectedItem)
            {
                this.IsEnabled = false;
                personProgressRing.IsActive = true;

                string response = await PersonsCmds.DeletePerson(globals.gPersonGroupSelected.personGroupId,
                                                                globals.gPersonSelected.personId);

                personProgressRing.IsActive = false;
                this.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Select a person group to delete!", "Delete Error");
                await dialog.ShowAsync();
            }

            AppBarButtonPersonRefresh_Click(null, null);
        }

        private void personListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsEnabled = false;
            personProgressRing.IsActive = true;
            appbarEditPersonButton.IsEnabled = true;
            appbarDeletePersonButton.IsEnabled = true;
            appbarPersonNextButton.IsEnabled = true;

            Persons person = (Persons)personListView.SelectedItem;
            txtPerson.Text = (null == person) ? "" : person.name;
            txtPersonContext.Text = (null == person) ? "" : person.userData;

            personProgressRing.IsActive = false;
            this.IsEnabled = true;

            globals.gPersonSelected = person;
            globals.gFaceSelected = null;
        }

        private void appbarPersonHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void appbarPersonNextButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PersonFace));
        }
    }
}
