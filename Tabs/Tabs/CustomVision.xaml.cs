﻿using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });


            await MakePredictionRequest(file);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            // An unhandled exception occured. occurred
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "26e87165db3d41c58764deb3859c045f");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/d163e053-5416-425f-86b6-c104f780d27f/image?iterationId=5048e35b-553d-4510-a7f8-e6116b9c0384";

            HttpResponseMessage response;
            
            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                TagLabel.Text = "";
                PredictionLabel.Text = "";
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    response = await client.PostAsync(url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();

                            JObject rss = JObject.Parse(responseString);

                            //Querying with LINQ
                            //Get all Prediction Values
                            var Probability = from p in rss["Predictions"] select (string)p["Probability"];
                            var Tag = from p in rss["Predictions"] select (string)p["Tag"];

                            //Truncate values to labels in XAML
                            foreach (var item in Tag)
                            {
                                TagLabel.Text += item + ": \n";
                            }

                            foreach (var item in Probability)
                            {
                                PredictionLabel.Text += item + "\n";
                            }

                        }
                }
                catch (Exception)
                {
                    TagLabel.Text = "Cannot connect to the internet, please check your internet or try later";
                }


                



                //Get rid of file once we have finished using it
                file.Dispose();
            }
        }
    }
}