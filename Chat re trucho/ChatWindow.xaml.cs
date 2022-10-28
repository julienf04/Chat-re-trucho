using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PlayerIOClient;

namespace Chat_re_trucho
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private const int _MAX_MESSAGES = 100;
        private const string _ERROR_CONNECTION = "It seems an error occurred with your connection";


        private readonly Thickness _TEXT_BLOCK_MESSAGE_PADDING = new Thickness(5, 5, 5, 5);
        private readonly Thickness _TEXT_BLOCK_MESSAGE_MARGIN = new Thickness(0, 0, 0, 3);
        private readonly FontFamily _TEXT_BLOCK_MESSAGE_FONT_FAMILY = new FontFamily("Comic Sans MS");
        private readonly int _TEXT_BLOCK_MESSAGE_FONT_SIZE = 16;
        private readonly TextWrapping _TEXT_BLOCK_MESSAGE_TEXT_WRAPPING = TextWrapping.Wrap;
        private readonly Brush _TEXT_BLOCK_MESSAGE_BACKGROUND = new SolidColorBrush(Color.FromRgb(69, 69, 69));
        private readonly HorizontalAlignment _TEXT_BLOCK_MESSAGE_HORIZONTAL_ALIGNMENT = HorizontalAlignment.Left;


        private bool _safe_disconnect = false;

        public ChatWindow()
        {
            InitializeComponent();

            string msgJoined = "'" + AppData.Username + "' has joined the chat";
            AddTextBlockMessage(msgJoined);

            this.Closed += ChatWindow_Closed;
            AppData.ClientConnection.OnDisconnect += ClientConnection_OnDisconnect;
            AppData.ClientConnection.OnMessage += ClientConnection_OnMessage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxMessage.Text == "")
                return;

            string msgToLocal = AppData.Username + ": " + textBoxMessage.Text;
            AddTextBlockMessage(msgToLocal);

            string msgToSend = textBoxMessage.Text;
            AppData.ClientConnection.Send("", msgToSend);

            scrollChatPanel.ScrollToBottom();
            textBoxMessage.Text = "";
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Button_Click(null, null);
        }


        private void AddTextBlockMessage(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = text;
                textBlock.Padding = _TEXT_BLOCK_MESSAGE_PADDING;
                textBlock.Margin = _TEXT_BLOCK_MESSAGE_MARGIN;
                textBlock.FontFamily = _TEXT_BLOCK_MESSAGE_FONT_FAMILY;
                textBlock.FontSize = _TEXT_BLOCK_MESSAGE_FONT_SIZE;
                textBlock.TextWrapping = _TEXT_BLOCK_MESSAGE_TEXT_WRAPPING;
                textBlock.Background = _TEXT_BLOCK_MESSAGE_BACKGROUND;
                textBlock.HorizontalAlignment = _TEXT_BLOCK_MESSAGE_HORIZONTAL_ALIGNMENT;
                textBlock.Foreground = AppData.MessagesColorPallete[Random.Shared.Next(0, 5)];

                if (chatPanel.Children.Count >= _MAX_MESSAGES)
                    chatPanel.Children.RemoveAt(0);

                chatPanel.Children.Add(textBlock);
            });
        }

        private void ClientConnection_OnMessage(object sender, Message e)
        {
            string msg = e.GetString(0);
            AddTextBlockMessage(msg);
        }

        private void ClientConnection_OnDisconnect(object sender, string message)
        {
            if (_safe_disconnect)
                return;

            MessageBox.Show(_ERROR_CONNECTION, "ERROR - Chat re trucho", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }

        private void ChatWindow_Closed(object? sender, EventArgs e)
        {
            _safe_disconnect = true;
            AppData.ClientConnection.Disconnect();
            AppData.Client.Logout();
        }
    }
}
