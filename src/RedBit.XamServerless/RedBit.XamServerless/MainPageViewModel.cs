using Plugin.Media;
using RedBit.Mobile.Core;
using System;
using System.Collections.Generic;
using System.Text;
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
                            var url = await RedBit.XamServerless.Core.BlobManager.Default.AddOriginalImage(buffer);
                            DisplayAlert("Upload Done!", $"Download using {url}");

                        }

                        Status = "See your pic!";
                        this.PictureButtonEnabled = true;
                    }));
            }
        }

        

    }
}
