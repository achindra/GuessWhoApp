using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace GuessWho
{
    public class FaceData
    {
        public string persistedFaceId { get; set; }
        public string userData { get; set; }
    }
    class PersonFaceCmds
    {

        public static async Task<string> AddPersonFace(string personGroupId, string personId, string userData)
        {
            string responseBody = null;
            string uri = HttpHandler.BaseUri + "/" + personGroupId + "/persons/" + personId + "/persistedFaces?userData=" + userData + "&userData=" + userData;
            string jsonString = "{\"url\":\"" + HttpHandler.storagePath + "originals/" + userData + "\"}";
            HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await HttpHandler.client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            else
            {
                responseBody = await response.Content.ReadAsStringAsync();
                globals.ShowJsonErrorPopup(responseBody);
            }
            return responseBody;
        }

        public static async Task<string> DeletePersonFace(string personGroupId, string personId, string persistedFaceId)
        {
            string responseBody = null;
            string uri = HttpHandler.BaseUri + "/" + personGroupId + "/persons/" + personId + "/persistedFaces/" + persistedFaceId;
            HttpResponseMessage response = await HttpHandler.client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();
            }
            else
            {
                responseBody = await response.Content.ReadAsStringAsync();
                globals.ShowJsonErrorPopup(responseBody);
            }
            return responseBody;
        }

        public static async Task<FaceData> GetPersonFace(string personGroupId, string personId, string persistedFaceId)
        {
            FaceData face = null;

            string uri = HttpHandler.BaseUri + "/" + personGroupId + "/persons/" + personId + "/persistedFaces/" + persistedFaceId;
            HttpResponseMessage response = await HttpHandler.client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                face = JsonConvert.DeserializeObject<FaceData>(responseBody);
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                globals.ShowJsonErrorPopup(responseBody);
            }
            return face;
        }

        public static async Task updateToBlob(StorageFile file)
        {
            CloudBlockBlob blob = null;
            string personGroupId, personId, fileName, blobFileName = null;
            BitmapImage bitmapImage = new BitmapImage();
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);

            if (null != fileStream)
            {
                personGroupId = globals.gPersonGroupSelected.personGroupId;
                personId = globals.gPersonSelected.personId;
                fileName = System.Guid.NewGuid() + "." + file.Name.Split('.').Last<string>();
                blobFileName = personGroupId + "/" + personId + "/" + fileName;
                
                await HttpHandler.blobContainer.CreateIfNotExistsAsync();
                blob = HttpHandler.blobContainer.GetBlockBlobReference(blobFileName);
                await blob.DeleteIfExistsAsync();
                await blob.UploadFromFileAsync(file);
            }
            AddFaceToPerson(blobFileName);
        }

        private static async void AddFaceToPerson(string blobName)
        {
            if (null != blobName)
            {
                //Associate with a face
                string personGroupId = globals.gPersonGroupSelected.personGroupId;
                string personId = globals.gPersonSelected.personId;
                if(null == await AddPersonFace(personGroupId, personId, blobName))
                {
                    //failed, delete blob
                    var blob = HttpHandler.blobContainer.GetBlockBlobReference(blobName);
                    await blob.DeleteIfExistsAsync();
                    blob = HttpHandler.thumbContainer.GetBlockBlobReference(blobName);
                    await blob.DeleteIfExistsAsync();
                }
            }
        }
    }
}
