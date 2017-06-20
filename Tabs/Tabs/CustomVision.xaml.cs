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

        private async void loadImage(object sender, EventArgs e)
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

        async Task MakePredictionRequest(MediaFile file)
        {
            // An unhandled exception occured. occurred
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("ocp-apim-subscription-key", "cb6ad66ed309478290841e2c1884604f");

            string requestParameters = "language=unk&detectOrientation=true";

            string url = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

            string uri = url + "?" + requestParameters;

            HttpResponseMessage response;
            
            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                TagLabel.Text = "";
                PredictionLabel.Text = "";
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    response = await client.PostAsync(uri, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                        

                            JObject rss = JObject.Parse(responseString);

                        string message = "";
                        foreach (var element in rss["regions"][0]["lines"])
                        {
                            
                            string line = "";
                            foreach (var word in element["words"])
                            {
                                line += word["text"].ToString() + " ";
                                
                                /*
                                foreach (var text in word["text"])
                                {
                                    line += text;
                                }
                                */
                            }

                             message += line + "end of sentense   " ;
                            //etc
                        }
                        TagLabel.Text = message;



                        if (responseString == ""){
                                TagLabel.Text = "Error";
                            }
                        }
                    
                }
                catch (Exception error)
                {
                    TagLabel.Text = error.ToString();
                }
            }
            //Get rid of file once we have finished using it
            file.Dispose();
        }
    }
}