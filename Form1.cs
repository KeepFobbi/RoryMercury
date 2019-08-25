using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Net;
using System.Text.RegularExpressions;

namespace Rory_Mercury
{
    public partial class Form1 : Form
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc");
        private static string globalPath;
        private static string UserMessage;
        private static Telegram.Bot.Types.Message message;
        private static bool musicFlag = false;
        private static bool backgroundFlag = false;

        //https://api.telegram.org/bot956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc/getUpdates

        private static readonly int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private static readonly int APPCOMMAND_VOLUME_UP = 0xA0000;
        private static readonly int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private static readonly int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            InitializeComponent();
            globalPath = AppDomain.CurrentDomain.BaseDirectory;

            this.Text = Bot.GetMeAsync().Result.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;


            Bot.StartReceiving(Array.Empty<UpdateType>());
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            message = messageEventArgs.Message;
            MessageType type = message.Type;

            if (type == MessageType.Document && backgroundFlag == true)
            {
                uploadPhotoDesktop(messageEventArgs);
                backgroundFlag = false;
            }
            else if (message == null || type != MessageType.Text)
                return;

            UserMessage = message.Text;

            if (UserMessage == "/start")
                await Bot.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}. Меня зовут {Bot.GetMeAsync().Result.FirstName}");

            else if (UserMessage == "/off")
                stateMachine("-s");

            else if (UserMessage == "/restart")
                stateMachine("-r");

            else if (UserMessage == "/mute")
                changeVolume(APPCOMMAND_VOLUME_MUTE);

            else if (UserMessage == "/volup")
                changeVolume(APPCOMMAND_VOLUME_UP);

            else if (UserMessage == "/voldown")
                changeVolume(APPCOMMAND_VOLUME_DOWN);

            else if (musicFlag == true)
            {
                WebClient webClient = new WebClient();
                Stream data = webClient.OpenRead("https://www.youtube.com/results?search_query=" + UserMessage.Replace(" ", "+"));
                StreamReader reader = new StreamReader(data);
                Regex regex = new Regex("href=[\"']([^\"']*)[\"']");
                MatchCollection match = regex.Matches(reader.ReadToEnd());

                System.Diagnostics.Process.Start($"https://www.youtube.com{match[66].Groups[1]}");
                // включить видео
                musicFlag = false;
            }

            else if(UserMessage == "/open" && musicFlag == false)
            {
                musicFlag = true;
                await Bot.SendTextMessageAsync(message.Chat.Id, "Введи название песни:");
                return;       
            }

            else if (UserMessage == "/change")
            {
                backgroundFlag = true;
                await Bot.SendTextMessageAsync(message.Chat.Id, "Отправь фото как документ и оно установиться на твоем рабочем столе.");
            }

            else if (UserMessage == "/screen")
            {
                Directory.CreateDirectory("Screen");
                screenAsync(globalPath + "Screen");
            }
        }

        private static void stateMachine(string Act)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $"shutdown {Act} -t 00";
            process.StartInfo = startInfo;
            process.Start();
        }

        private static async void SendPhotoAsync(string path)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);
            path.Split(Path.DirectorySeparatorChar).Last();

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await Bot.SendPhotoAsync(
                    message.Chat.Id,
                    fileStream,
                    "Вот ваш скрин, Господин.");
            }
        }

        private static void screenAsync(string path)
        {
            path += @"\\printscreen.jpg";
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            printscreen.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

            SendPhotoAsync(path);
        }

        private void changeVolume(int APPCOMMAND_VOLUME)
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle, (IntPtr)APPCOMMAND_VOLUME);
        }

        public static async void uploadPhotoDesktop(MessageEventArgs messageEventArgs)
        {
            string tempPath = globalPath + "Desktop background\\background.jpg";
            var file = await Bot.GetFileAsync(messageEventArgs.Message.Document.FileId);
            Directory.CreateDirectory("Desktop background");
            FileStream fs = new FileStream(tempPath, FileMode.Create);
            await Bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();

            SystemParametersInfo(20, 0, tempPath, 1 | 2);
            await Bot.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, $"Есть!");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Bot.StopReceiving();
        }
    }
}
