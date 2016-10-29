using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.UI.Xaml;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Windows.UI.Popups;

namespace GuessWho
{
    static class HttpHandler
    {

        private static string _subscriptionKey;
        //{
        //    get
        //    {
        //        return _subscriptionKey;
        //    }
        //    set
        //    {
        //        if (null == _subscriptionKey)
        //            _subscriptionKey = value;
        //    }
        //}

        private static string _storageAccountKey;
        //{
        //    get
        //    {
        //        return _storageAccountKey;
        //    }
        //    set
        //    {
        //        if (null == _subscriptionKey)
        //            _subscriptionKey = value;
        //    }
        //}

        private static string _storageAccountName;
        //{
        //    get
        //    {
        //        return _storageAccountName;
        //    }
        //    set
        //    {
        //        if (null == _storageAccountName)
        //            _storageAccountName = value;
        //    }
        //}

        /// <summary>
        /// Pubic properties
        /// </summary>

        private static string _baseUri;
        public static string BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                if (null == _baseUri)
                    _baseUri = value;
            }
        }

        private static HttpClient _client;
        public static HttpClient client
        {
            get
            {
                return _client;
            }
            set
            {
                if (null == _client)
                    _client = value;
            }
        }

        private static string _storagePath;
        public static string storagePath
        {
            get
            {
                return _storagePath;
            }
            set
            {
                if (null == _storagePath)
                    _storagePath = value;
            }
        }

        private static CloudBlobContainer _blobContainer;
        public static CloudBlobContainer blobContainer
        {
            get
            {
                return _blobContainer;
            }
            set
            {
                if (null == _blobContainer)
                    _blobContainer = value;
            }
        }

        private static CloudBlobContainer _thumbContainer;
        public static CloudBlobContainer thumbContainer
        {
            get
            {
                return _thumbContainer;
            }
            set
            {
                if (null == _thumbContainer)
                    _thumbContainer = value;
            }
        }

        private static CloudBlobContainer _tempContainer;
        public static CloudBlobContainer tempContainer
        {
            get
            {
                return _tempContainer;
            }
            set
            {
                if (null == _tempContainer)
                    _tempContainer = value;
            }
        }

        private static StorageCredentials _creds;
        public static StorageCredentials cred
        {
            get
            {
                return _creds;
            }
            set
            {
                if (null == _creds)
                    _creds = value;
            }
        }

        public static bool initDone = false;

        public static async void init()
        {
            if (0 == ApplicationData.Current.LocalSettings.Values.Count)
            {
                MessageDialog dialog = new MessageDialog("First time? Go to Settings and configure few things.");
                await dialog.ShowAsync();
                return;
            }
            _subscriptionKey = ApplicationData.Current.LocalSettings.Values["SubscriptionKey"].ToString();
            _storageAccountName = ApplicationData.Current.LocalSettings.Values["StorageAccountName"].ToString();
            _storageAccountKey = ApplicationData.Current.LocalSettings.Values["StorageAccountKey"].ToString();

            if (_subscriptionKey.Equals(""))
            {
                MessageDialog dialog = new MessageDialog("Cogniive service API subscription key not set. Go to Settings and fix it.");
                await dialog.ShowAsync();
                return;
            }
            else
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            }

            if (_storageAccountName.Equals("") || _storageAccountKey.Equals(""))
            {
                MessageDialog dialog = new MessageDialog("Storage account name and/or key not set. Go to Settings and fix it.");
                await dialog.ShowAsync();
                return;
            }
            else
            {
                cred = new StorageCredentials(_storageAccountName, _storageAccountKey);
            }

            BaseUri = Application.Current.Resources["BaseURI"].ToString();
            storagePath = Application.Current.Resources["StoragePath"].ToString();
            blobContainer = new CloudBlobContainer(new Uri(storagePath + "originals"), cred);
            thumbContainer = new CloudBlobContainer(new Uri(storagePath + "thumbnails"), cred);
            tempContainer = new CloudBlobContainer(new Uri(storagePath + "visitors"), cred);
            
            initDone = true;
        }

        //public static string subscriptionKey;
        //public static string StorageAccountKey;
        //public static readonly string subscriptionKey = ApplicationData.Current.LocalSettings.Values["SubscriptionKey"].ToString();
        //public static readonly string StorageAccountName = ApplicationData.Current.LocalSettings.Values["StorageAccountName"].ToString();
        //public static readonly string StorageAccountKey = ApplicationData.Current.LocalSettings.Values["StorageAccountKey"].ToString();
        //public static readonly string baseUri = Application.Current.Resources["BaseURI"].ToString();
        //private static readonly string subscriptionKey = Application.Current.Resources["SubscriptionKey"].ToString();        
        //private static readonly string StorageAccountName = Application.Current.Resources["StorageAccountName"].ToString();
        //private static readonly string StorageAccountKey = Application.Current.Resources["StorageAccountKey"].ToString();
        //private static readonly StorageCredentials cred = new StorageCredentials(StorageAccountName, StorageAccountKey);

        //public static readonly string storagePath = Application.Current.Resources["StoragePath"].ToString();
        //public static readonly CloudBlobContainer blobContainer = new CloudBlobContainer(new Uri(storagePath + "originals"), cred);
        //public static readonly CloudBlobContainer thumbContainer = new CloudBlobContainer(new Uri(storagePath + "thumbnails"), cred);
        //public static readonly CloudBlobContainer tempContainer = new CloudBlobContainer(new Uri(storagePath + "visitors"), cred);
        //private static readonly StorageCredentials cred = new StorageCredentials(StorageAccountName, StorageAccountKey);




        //public static string SubscriptionKey
        //{
        //    get
        //    {
        //        return subscriptionKey;
        //    }
        //}

        //public static string BaseUri
        //{
        //    get
        //    {
        //        return baseUri;
        //    }
        //}
    }
}
