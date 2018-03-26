using Newtonsoft.Json;
using Plugin.Media;
using RedBit.Mobile.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RedBit.XamServerless
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            this.Title = "Serverless Sample";
        }

        private string _Status = string.Empty;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Status
        {
            get => _Status;
            set => SetProperty(ref _Status, value);
        }

        private bool _PictureButtonEnabled = true;

        /// <summary>
        /// Sets and gets the PictureButtonEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool PictureButtonEnabled
        {
            get => _PictureButtonEnabled;
            set => SetProperty(ref _PictureButtonEnabled, value);
        }


        private string _PhotoPath = string.Empty;

        /// <summary>
        /// Sets and gets the PhotoPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PhotoPath
        {
            get => _PhotoPath;
            set => SetProperty(ref _PhotoPath, value);
        }

        private Command _SnapPictureCommand;
        /// <summary>
        /// Gets the SnapPicture.
        /// </summary>
        public Command SnapPictureCommand
        {
            get
            {
                return _SnapPictureCommand
                    ?? (_SnapPictureCommand = new Command(async () =>
                    {
                        this.PictureButtonEnabled = false;

                        await CrossMedia.Current.Initialize();

                        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                        {
                            DisplayAlert("No Camera", ":( No camera available.");
                            return;
                        }

                        var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                        {
                            Directory = "Sample",
                            Name = "test.jpg",
                            PhotoSize = Plugin.Media.Abstractions.PhotoSize.Large,
                            CompressionQuality = 75,
                            AllowCropping = true
                        });

                        if (file != null)
                        {
                            Status = "Composing Picture ...";
                            var t = file.GetStream();
                            byte[] buffer = new byte[t.Length];
                            t.Read(buffer, 0, buffer.Length);
                            var b64 = Convert.ToBase64String(buffer);

                            this.PhotoPath = file.Path;
                            file.Dispose();

                            // upload to blob
                            Status = "Uploading to server ...";
                            var id = await UploadImage(b64);
                            Status = "Uploading complete, waiting for images to process ...";
                            var result = await WaitTillProcessingComplete(id);
                            DisplayAlert("Processing Done!", $"Download using {JsonConvert.SerializeObject(result.Images)}");

                        }

                        Status = "See your pic!";
                        this.PictureButtonEnabled = true;
                    }));
            }
        }


        private const string BASE_URL = "https://410ae54b.ngrok.io/api";
        private async Task<string> UploadImage(string base64Image)
        {
            using (HttpClient client = new HttpClient())
            {
                // get the URL
                var url = $"{BASE_URL}/ImageUploadFunc";

                // create the object to upload
                var imageObject = new Core.UploadPayload { Imageb64 = base64Image };

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("Accept", "application/json");

                    // set the body for the POST
                    msg.Content = new StringContent(JsonConvert.SerializeObject(imageObject), Encoding.UTF8, "application/json");
                    msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // send the response
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // parse out the response
                        var uploadResponse = JsonConvert.DeserializeObject<Core.UploadResponse>(responseContent);
                        return uploadResponse.Id;
                    }
                }
            }
        }


        private async Task<Core.UploadResponse> WaitTillProcessingComplete(string id)
        {
            using (HttpClient client = new HttpClient())
            {
                // get the URL
                var url = $"{BASE_URL}/ImageProcessingStatusFunction?id={id}";

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // send the response
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // parse out the response
                        var uploadResponse = JsonConvert.DeserializeObject<Core.UploadResponse>(responseContent);
                        if (uploadResponse.Images == null)
                            return await WaitTillProcessingComplete(id);
                        else
                            return uploadResponse;
                    }
                }
            }
        }
    }
}
