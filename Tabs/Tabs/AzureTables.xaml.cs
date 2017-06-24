using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AzureTables : ContentPage
    {
        TodoItem manager;
        MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;
        public AzureTables ()
        {
            InitializeComponent ();
            manager = TodoItemManager.DefaultManager;
        }

        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://msaagain.azurewebsites.net"
        );

        private async void Get(object sender, EventArgs e)
        {
            CurrentPlatform.Init();
            TodoItem item = new TodoItem { Text = "Awesome item" };
            await MobileService.GetTable<TodoItem>().InsertAsync(item);
            return;
        }

        async Task AddItem(TodoItem item)
        {
            await manager.SaveTaskAsync(item);
            todoList.ItemsSource = await manager.GetTodoItemsAsync();
        }

        public async void Get(object sender, EventArgs e)
        {
            var todo = new TodoItem { Name = "Awesome item" };
            await AddItem(todo);

            newItemName.Text = string.Empty;
            newItemName.Unfocus();
        }


    }


}
