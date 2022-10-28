using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PlayerIOClient;
using System.Net.NetworkInformation;

namespace Chat_re_trucho
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string _ROOM_ID = "RoomId";
        private const string _ROOM_TYPE = "Chat re trucho";
        private const int _MAX_USERS_PER_ROOM = 40;
        private const string _ERROR_AUTHENTICATE = "Authentication error. Check your internet connection";
        private const string _ERROR_CREATE_JOIN_ROOM = "Error creating or joining a room";
        private const string _USERNAME_NOT_ALLOWED_CREATE_JOIN_ROOM = "Your username is already in use. Please enter a different username";


        private bool? _usernameAllowed;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxUsername.Text == "")
                return;

            string username = TextBoxUsername.Text;
            Client client;
            Connection connection;
            if (!ConnectWithServer(username, out client, out connection))
                return;


            AppData.Username = username;
            AppData.Client = client;
            AppData.ClientConnection = connection;


            ChatWindow mainWindow = new ChatWindow();
            this.Close();
            mainWindow.Show();
        }

        private void TextBoxUsername_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Button_Click(null, null);
        }


        private bool ConnectWithServer(string username, out Client client, out Connection connection)
        {
            client = null;
            connection = null;

            if (!TryAuthenticate(username, out client))
            {
                MessageBox.Show(_ERROR_AUTHENTICATE, "ERROR - Chat re trucho", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            switch (TryCreateJoinRoom(client, out connection))
            {
                case CreateJoinRoomResult.Error:
                    client.Logout();
                    connection.Disconnect();
                    MessageBox.Show(_ERROR_CREATE_JOIN_ROOM, "ERROR - Chat re trucho", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                case CreateJoinRoomResult.UsernameNotAllowed:
                    client.Logout();
                    connection.Disconnect();
                    MessageBox.Show(_USERNAME_NOT_ALLOWED_CREATE_JOIN_ROOM, "Warning - Chat re trucho", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
            }


            return true;
        }

        private bool TryAuthenticate(string username, out Client client)
        {
            client = null;
            try
            {
                client = PlayerIO.Authenticate(
                    "chat-re-trucho-vgyqsglz0qzrsrs8ecmhg",
                    "public",
                    new Dictionary<string, string>
                    {
                        { "userId", username },
                    },
                    null
                    );
            }
            catch (Exception) { }


            return client != null;
        }

        private CreateJoinRoomResult TryCreateJoinRoom(Client client, out Connection connection)
        {
            connection = null;
            bool? usernameAllowed;

            try
            {
                RoomInfo[] roomInfo = client.Multiplayer.ListRooms(_ROOM_TYPE, null, 0, 0);

                if (roomInfo != null && roomInfo.Length > 0 && roomInfo[roomInfo.Length - 1].OnlineUsers < _MAX_USERS_PER_ROOM)
                    connection = client.Multiplayer.JoinRoom(
                        _ROOM_ID + roomInfo.Length.ToString(),
                        null
                        );
                else
                    connection = client.Multiplayer.CreateJoinRoom(
                        _ROOM_ID + (roomInfo.Length + 1).ToString(),
                        _ROOM_TYPE,
                        true,
                        null,
                        null
                        );

                usernameAllowed = CheckUsernameIsAllowed(connection);
            }
            catch (Exception)
            {
                return CreateJoinRoomResult.Error;
            }

            return usernameAllowed == true ? CreateJoinRoomResult.Success
                : usernameAllowed == false ? CreateJoinRoomResult.UsernameNotAllowed
                : CreateJoinRoomResult.Error;
        }

        private bool? CheckUsernameIsAllowed(Connection connection)
        {
            _usernameAllowed = null;
            connection.OnMessage += Connection_OnMessage;
            connection.Send(Message.Create("allow"));

            SpinWait.SpinUntil(() => _usernameAllowed != null || !connection.Connected);

            connection.OnMessage -= Connection_OnMessage;
            return _usernameAllowed;
        }

        private void Connection_OnMessage(object sender, Message e)
        {
            if (e.Type == "allow")
                _usernameAllowed = e.GetBoolean(0);
        }
    }


    public enum CreateJoinRoomResult
    {
        Success,
        Error,
        UsernameNotAllowed
    }
}
