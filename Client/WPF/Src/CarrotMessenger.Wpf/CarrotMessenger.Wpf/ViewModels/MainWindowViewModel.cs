using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace CarrotMessenger.Wpf.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<MessageViewModel> Messages { get; }
        public ObservableCollection<ContactViewModel> ChatList { get; }
        public string? MessageText { get; set; }
        public string ContactFilter { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand FilterContactsCommand { get; set; }
        private readonly ChatService _chatService;
        private readonly GetChatListService _getChatListService;

        public MainWindowViewModel()
        {
            SendMessageCommand = new SimpleCommand(SendMessage);
            FilterContactsCommand = new SimpleCommand((s) => { });
            Messages = new ObservableCollection<MessageViewModel>();
            ChatList = new ObservableCollection<ContactViewModel>();


            _chatService = new ChatService();
            _getChatListService = new GetChatListService();
            
            _chatService.RecievedMessageDelegate += (message) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Messages.Add(new MessageViewModel() { Text = message, Time = DateTime.Now.ToString() });
                });
            };
            Task.Run(_chatService.StartClient);
        }

        private void SendMessage(object obj)
        {
            Messages.Add(new MessageViewModel() { Text = MessageText, Time = DateTime.Now.ToString() });
            _chatService.SendMessage(MessageText);
            MessageText = string.Empty;
            OnPropertyChanged(nameof(MessageText));
        }
    }

    public class ChatService
    {
        public event Action<string> RecievedMessageDelegate;

        private readonly ClientWebSocket _clientWebSocket;

        public ChatService()
        {
            _clientWebSocket = new ClientWebSocket();
        }

        public async Task StartClient()
        {
            var serverUri = new Uri("ws://172.16.24.121:5000/");
            try
            {
                await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine("Connected to server");
                await ReceiveMessages(_clientWebSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendMessages(ClientWebSocket client, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
        }

        async Task ReceiveMessages(ClientWebSocket client)
        {
            byte[] buffer = new byte[1024];
            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("Disconnected from server");
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                    RecievedMessageDelegate?.Invoke(message);
                }
            }
        }

        public async void SendMessage(string message)
        {
            await SendMessages(_clientWebSocket, message);
        }
    }

    public class GetChatListService
    {
        public async Task<IEnumerable<ContactDto>> SearchContacts(string filter)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"http://localhost:5074/contacts/search?query={filter}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var contacts = JsonSerializer.Deserialize<IEnumerable<ContactDto>>(content);
            return contacts;
        }
    }

    public record ContactDto(string Name, int Status);
}