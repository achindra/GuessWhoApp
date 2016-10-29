using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Media.Capture;

namespace GuessWho
{
    public class ImageCollection
    {
        public string persistedFaceId { get; set; }
        public BitmapImage image { get; set; }
        public FaceData faceInfo { get; set; }

        public ImageCollection(string PersistedFaceId, BitmapImage Image, FaceData FaceInfo)
        {
            this.persistedFaceId = PersistedFaceId;
            this.image = Image;
            this.faceInfo = FaceInfo;
        }

    }

    public class ImageChannel
    {
        public string ImagePath { get; set; }
        public string PersistedFaceId { get; set; }
        public FaceData FaceInfo { get; set; }

        public BitmapImage Image
        {
            get
            {
                //return new BitmapImage(new Uri("ms-appx://" + this.ImagePath));
                return new BitmapImage(new Uri(HttpHandler.storagePath + "thumbnails/" + this.ImagePath));
            }
        }
    }

    public sealed partial class PersonFace : Page 
    {
        public PersonFace()
        {
            InitializeComponent();
            AppBarButtonPersonFaceRefresh_Click(null, null);
        }

        private ObservableCollection<ImageCollection> _myFaceImageCollection = new ObservableCollection<ImageCollection>();
        public ObservableCollection<ImageCollection> FaceImageCollection
        {
            get { return _myFaceImageCollection; }
            set { _myFaceImageCollection = value; }
        }

        private async void AppBarButtonPersonFaceRefresh_Click(object sender, RoutedEventArgs e)
        {
            List<ImageChannel> imageChannel = new List<ImageChannel>();
            textBlockFace.Text = "Face - " + globals.gPersonSelected.name;
            if (null != globals.gPersonSelected &&
                globals.gPersonSelected.name.Equals("...") == false)                
            {
                appbarFaceRefreshButton.IsEnabled = false;
                personFaceProgressRing.IsActive = true;

                appbarFaceAddFromCameraButton.IsEnabled = false;
                appbarFaceAddFromFileButton.IsEnabled = false;
                appbarDeleteFaceButton.IsEnabled = false;

                //FaceImageCollection.Clear();

                foreach (string persistedFaceId in globals.gPersonSelected.persistedFaceIds)
                {
                    FaceData faceInfo = await PersonFaceCmds.GetPersonFace(globals.gPersonGroupSelected.personGroupId,
                                                                         globals.gPersonSelected.personId,
                                                                         persistedFaceId);
                    if (null != faceInfo)
                    {
                        //BitmapImage bmpImage = new BitmapImage(new Uri(HttpHandler.storagePath + "thumbnails/" + faceInfo.userData));
                        //FaceImageCollection.Add(new ImageCollection(persistedFaceId, bmpImage, faceInfo));
                        
                        imageChannel.Add(new ImageChannel()
                        {
                            ImagePath = faceInfo.userData,
                            PersistedFaceId = persistedFaceId,
                            FaceInfo = faceInfo
                        });
                    }
                }
                personFaceListView.ItemsSource = imageChannel;

                personFaceProgressRing.IsActive = false;
                appbarFaceAddFromCameraButton.IsEnabled = true;
                appbarFaceAddFromFileButton.IsEnabled = true;
                appbarFaceRefreshButton.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Person to be selected to find associated Faces", "Refresh Error");
                await dialog.ShowAsync();
            }
        }

        private async void AppBarButtonDeleteFace_Click(object sender, RoutedEventArgs e)
        {
            if(null != globals.gFaceSelected)
            {                
                personFaceProgressRing.IsActive = true;
                appbarDeleteFaceButton.IsEnabled = false;
                appbarFaceAddFromCameraButton.IsEnabled = false;
                appbarFaceAddFromFileButton.IsEnabled = false;
                appbarFaceRefreshButton.IsEnabled = false;

                string response = await PersonFaceCmds.DeletePersonFace(globals.gPersonGroupSelected.personGroupId,
                                                                        globals.gPersonSelected.personId,
                                                                        globals.gFaceSelected.persistedFaceId);

                personFaceProgressRing.IsActive = false;
                appbarFaceRefreshButton.IsEnabled = true;
                appbarFaceAddFromCameraButton.IsEnabled = true;
                appbarFaceAddFromFileButton.IsEnabled = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Selected to face to delete", "Delete Error");
                await dialog.ShowAsync();
            }
            AppBarButtonPersonFaceRefresh_Click(null, null);
        }

        private void personFaceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != personFaceListView.SelectedItem)
            {
                globals.gFaceSelected = ((ImageChannel)personFaceListView.SelectedItem).FaceInfo;
                appbarDeleteFaceButton.IsEnabled = true;
            }
            else
            {
                appbarDeleteFaceButton.IsEnabled = false;
            }
        }

        private async void appbarFaceAddFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            appbarDeleteFaceButton.IsEnabled = false;
            appbarFaceAddFromCameraButton.IsEnabled = false;
            appbarFaceAddFromFileButton.IsEnabled = false;
            appbarFaceRefreshButton.IsEnabled = false;

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".jpeg"); filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".png"); filePicker.FileTypeFilter.Add(".gif");

            StorageFile file = await filePicker.PickSingleFileAsync();
            if (null != file)
            {                
                await PersonFaceCmds.updateToBlob(file);                
            }

            personFaceProgressRing.IsActive = false;
            appbarFaceRefreshButton.IsEnabled = true;
            appbarFaceAddFromCameraButton.IsEnabled = true;
            appbarFaceAddFromFileButton.IsEnabled = true;
        }

        private async void appbarFaceAddFromCameraButton_Click(object sender, RoutedEventArgs e)
        {
            appbarDeleteFaceButton.IsEnabled = false;
            appbarFaceAddFromCameraButton.IsEnabled = false;
            appbarFaceAddFromFileButton.IsEnabled = false;
            appbarFaceRefreshButton.IsEnabled = false;

            CameraCaptureUI dialog = new CameraCaptureUI();
            StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (null != file)
            {
                await PersonFaceCmds.updateToBlob(file);
            }

            personFaceProgressRing.IsActive = false;
            appbarFaceRefreshButton.IsEnabled = true;
            appbarFaceAddFromCameraButton.IsEnabled = true;
            appbarFaceAddFromFileButton.IsEnabled = true;
        }

        private void appbarFaceHomeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

    }
}
