using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        string responseString = "";

        public CustomVision()
        {
            InitializeComponent();
        }

        private async void LoadCamera(object sender, EventArgs e)
        {
            /* Loads Image from camera */
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

            // This shows the image on the screen 
            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });



            await MakePredictionRequest(file);
        }

        private async void LoadImage(object sender, EventArgs e)
        /* Loads Image from gallery */
        {
            await CrossMedia.Current.Initialize();


            MediaFile file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;

            // This shows the image on the screen 
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

        static string[] GetValueFromJSON(JObject json) {
            string[] attrib1Value = json.SelectToken(@"regions.lines.words.text").Value<string[]>();
            return attrib1Value;
        }



        private void GetInfo(object sender, EventArgs e)
        {
            TagLabel.Text = responseString;
            return;
        }

        public async Task MakePredictionRequest(MediaFile file)
        {
            // An unhandled exception occured. occurred
            var client = new HttpClient();


            client.DefaultRequestHeaders.Add("ocp-apim-subscription-key", "cb6ad66ed309478290841e2c1884604f");

            string requestParameters = "handwriting=true";

            string url = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/recognizeText";

            string uri = url + "?" + requestParameters;

            

            HttpResponseMessage response = null;

            string operationLocation = null;

            byte[] byteData = GetImageAsByteArray(file);

            var content = new ByteArrayContent(byteData);
            
            TagLabel.Text = "";
            PredictionLabel.Text = "";
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
   
            response = await client.PostAsync(uri, content);
            
            if (response.IsSuccessStatusCode)
            {
                
                operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                response = await client.GetAsync(operationLocation);
                responseString = await response.Content.ReadAsStringAsync();

                JObject rss = JObject.Parse(responseString);
                try { 
                List<String> message = new List<String>();
                { 
                    foreach (var section in rss["recognitionResult"]["lines"])
                    {
                        message.Add(section["text"].ToString());
                        //etc
                    }


                    foreach (var sentense in message) {
                        TagLabel.Text += sentense + "\n";
                    }
                        



                    if (message.Count==0){
                            TagLabel.Text = "Nothing found";
                        }
                }
                }
                catch (Exception e)
                {
                    TagLabel.Text = e.ToString();
                }
            }

            //Get rid of file once we have finished using it
            file.Dispose();
        }


    }
}