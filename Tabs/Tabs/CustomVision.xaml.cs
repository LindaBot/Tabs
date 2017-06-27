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
        TodoItemManager manager;
        string responseString = "";

        public CustomVision()
        {
            InitializeComponent();
            manager = TodoItemManager.DefaultManager;
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

        public void DataBinding(List<String> message)
        {
            List<TodoItem> sentenceList = new List<TodoItem>();
            foreach (var sentence in message)
            {
                sentenceList.Add(new TodoItem { Name = sentence });
            }
            uploadList.ItemsSource = sentenceList;
        }

        /*
        public void makeButton(List<String> message)
        {
            var layout = new StackLayout();

            foreach (var sentense in message) { 
                Button button = new Button
                {
                    Text = "My button"
                };
                layout.Children.Add(button);
            }

            this.Content = layout;


            return;
        }
        */


        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        static string[] GetValueFromJSON(JObject json)
        {
            string[] attrib1Value = json.SelectToken(@"regions.lines.words.text").Value<string[]>();
            return attrib1Value;
        }

 

        private void GetInfo(object sender, EventArgs e)
        {
            TagLabel.Text = JToken.Parse(responseString).ToString(Newtonsoft.Json.Formatting.Indented);
            return;
        }

        public async Task MakePredictionRequest(MediaFile file)
        {
            Hint.Text = "Please Wait";
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
            
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            response = await client.PostAsync(uri, content);
            await Task.Delay(5000);
            if (response.IsSuccessStatusCode)
            {

                operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                response = await client.GetAsync(operationLocation);
                responseString = await response.Content.ReadAsStringAsync();
                


                JObject rss = JObject.Parse(responseString);

                List<String> message = new List<String>();
                try
                {
                    foreach (var section in rss["recognitionResult"]["lines"])
                    {
                        message.Add(section["text"].ToString());
                    }

                    Hint.Text = "Please select a message to upload";

                    DataBinding(message);

                    if (message.Count == 0)
                    {
                        Hint.Text = "Nothing found";
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

        // Event handlers
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as TodoItem;
            if (Device.OS != TargetPlatform.iOS && todo != null)
            {
                // Not iOS - the swipe-to-delete is discoverable there
                if (Device.OS == TargetPlatform.Android)
                {
                    await DisplayAlert(todo.Name, "Press-and-hold to upload \n\"" + todo.Name + "\"", "Got it!");
                }
                else
                {
                    // Windows, not all platforms support the Context Actions yet
                    if (await DisplayAlert("Upload?", "Do you wish to upload \n" + todo.Name + "?", "upload", "Cancel"))
                    {
                        await AddItem(todo);
                    }
                }
            }

            // prevents background getting highlighted
            uploadList.SelectedItem = null;
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var todo = mi.CommandParameter as TodoItem;
            await AddItem(todo);
        }

        async Task AddItem(TodoItem item)
        {
            await manager.SaveTaskAsync(item);
        }



    }
}