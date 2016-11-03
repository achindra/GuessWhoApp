using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using System.Net.Http;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Auth;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GuessWho
{
    public static class globals
    {
        public static PersonGroups gPersonGroupSelected { get; set; }
        public static Persons gPersonSelected { get; set; }
        public static FaceData gFaceSelected { get; set; }
        public static List<PersonGroups> gPersonGroupList { get; set; }

        public static async void ShowJsonErrorPopup(string responseBody)
        {
            if (null != responseBody)
            {
                ResponseObject errorObject = JsonConvert.DeserializeObject<ResponseObject>(responseBody);
                MessageDialog dialog = new MessageDialog(errorObject.error.message,
                                                                 (null != errorObject.error.code) ?
                                                                        errorObject.error.code.ToString() :
                                                                        errorObject.error.statusCode.ToString());
                await dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Unknown error in operation");
                await dialog.ShowAsync();
            }
        }
    }

    public class Error
    {
        public string code { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
    }

    public class ResponseObject
    {
        public Error error { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        ImageBrush backgroundImage = new ImageBrush();

        public MainPage()
        {
            InitializeComponent();
            
            HttpHandler.init();
            btnFileQuery.IsEnabled = HttpHandler.initDone;
            backgroundImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/ic_launcher.png"));
        }

        private async void btnFileQuery_Click(object sender, RoutedEventArgs e)
        {
            if (null != btnFileQuery)
                btnFileQuery.Background = backgroundImage;

            txtResponse.Text = "";
            btnFileQuery.IsEnabled = false;
            StorageFile file = null;
            if (true == appbarToggleCamera.IsChecked)
            {
                CameraCaptureUI dialog = new CameraCaptureUI();
                file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);

                FaceQuery(file);
            }
            else
            {
                FileOpenPicker filePicker = new FileOpenPicker();
                filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                filePicker.ViewMode = PickerViewMode.Thumbnail;

                filePicker.FileTypeFilter.Clear();
                filePicker.FileTypeFilter.Add(".jpeg"); filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".png"); filePicker.FileTypeFilter.Add(".gif");

                file = await filePicker.PickSingleFileAsync();
            }
            FaceQuery(file);
            btnFileQuery.IsEnabled = true;
        }



        private async void FaceQuery(StorageFile file)
        {
            CloudBlockBlob blob = null;
            string blobFileName = null;
            if (null != file)
            {
                txtResponse.Text = "";
                progressRingMainPage.IsActive = true;
                BitmapImage bitmapImage = new BitmapImage();
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(fileStream);
                CapturedPhoto.Source = bitmapImage;
                CapturedPhoto.Tag = file.Path;

                blobFileName = System.Guid.NewGuid() + "." + file.Name.Split('.').Last<string>();

                await HttpHandler.tempContainer.CreateIfNotExistsAsync();
                BlobContainerPermissions permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await HttpHandler.tempContainer.SetPermissionsAsync(permissions);
                blob = HttpHandler.tempContainer.GetBlockBlobReference(blobFileName);
                await blob.DeleteIfExistsAsync();
                await blob.UploadFromFileAsync(file);

                string uri = "https://api.projectoxford.ai/face/v1.0/detect?returnFaceId=true";
                string jsonString = "{\"url\":\"" + HttpHandler.storagePath + "visitors/" + blobFileName + "\"}";
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpHandler.client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (null == globals.gPersonGroupList)
                        globals.gPersonGroupList = await PersonGroupCmds.ListPersonGroups();

                    List<string> names = await VisitorCmds.CheckVisitorFace(responseBody, globals.gPersonGroupList);
                    txtResponse.Text = string.Join(", ", names.ToArray());
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    globals.ShowJsonErrorPopup(responseBody);
                }

                await blob.DeleteAsync();
                progressRingMainPage.IsActive = false;
            }
        }

        private void appbarSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void appbarToggleCamera_Checked(object sender, RoutedEventArgs e)
        {
            if(null != appbarToggleFile)
                appbarToggleFile.IsChecked = false;

            backgroundImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/ic_launcher.png"));
            if (null != btnFileQuery)
                btnFileQuery.Background = backgroundImage;
        }

        private void btnFileQuery_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (null != btnFileQuery)
                btnFileQuery.Background = backgroundImage;
        }

        private void appbarToggleFile_Checked(object sender, RoutedEventArgs e)
        {
            if(null != appbarToggleCamera)
                appbarToggleCamera.IsChecked = false;

            backgroundImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/galleryicon.png"));
            if (null != btnFileQuery)
                btnFileQuery.Background = backgroundImage;
        }
    }
}
