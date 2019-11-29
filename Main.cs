﻿using System;
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
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;

namespace Rory_Mercury
{
    public partial class Main : Form
    {
        protected static readonly TelegramBotClient Bot = new TelegramBotClient("956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc");
        protected static Telegram.Bot.Types.Message message;
        private static string UserMessage;
        private static string globalPath;
        private static bool musicFlag = false;
        private static bool backgroundFlag = false;
        private static MatchCollection globalIp;
        private readonly string path = "states.txt";
        private static long ChatId;

        //https://api.telegram.org/bot956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc/getUpdates

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public Main()
        {
            InitializeComponent();

            WebClient webClient = new WebClient();
            Stream data = webClient.OpenRead("https://2ip.ru/");
            StreamReader reader = new StreamReader(data);
            Regex regex = new Regex(@"([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})");
            globalIp = regex.Matches(reader.ReadToEnd());

            globalPath = AppDomain.CurrentDomain.BaseDirectory;

            this.Text = Bot.GetMeAsync().Result.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            Bot.StartReceiving(Array.Empty<UpdateType>());
        }

        private NotifyIcon NI = new NotifyIcon();
        private void Main_Load(object sender, EventArgs e)
        {
            if (File.Exists("states.txt"))
            {
                using (FileStream fstream = File.OpenRead(path))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    ChatId = Convert.ToInt64(textFromFile);
                    startMenu("start");
                }
            }

            NI.BalloonTipText = $"Ваш IP:  {globalIp[0].Value}";
            NI.BalloonTipTitle = "Rory Mercury";
            NI.BalloonTipIcon = ToolTipIcon.Info;
            NI.Icon = this.Icon;
            NI.Visible = true;
            NI.ShowBalloonTip(1000);
        }

        private void NI_BalloonTipClosed(Object sender, EventArgs e)
        {
            NI.Visible = false;
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            message = messageEventArgs.Message;
            MessageType type = message.Type;
            ChatId = message.Chat.Id;

            if (type == MessageType.Document && backgroundFlag == true)
            {
                uploadPhotoDesktop(messageEventArgs);
                backgroundFlag = false;
                startMenu("main");
            }
            else if (message == null || type != MessageType.Text)
                return;

            UserMessage = message.Text;

            if (musicFlag == true)
            {
                WebClient webClient = new WebClient();
                Stream data = webClient.OpenRead("https://www.youtube.com/results?search_query=" + UserMessage.Replace(" ", "+"));
                StreamReader reader = new StreamReader(data);
                Regex regex = new Regex("href=[\"']([^\"']*)[\"']");
                MatchCollection match = regex.Matches(reader.ReadToEnd());

                System.Diagnostics.Process.Start($"https://www.youtube.com{match[66].Groups[1]}");
                musicFlag = false;
                startMenu("main");
            }
            if (message.Text == "/start")
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500); // simulate longer running task                

                using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] array = System.Text.Encoding.Default.GetBytes(message.Chat.Id.ToString());
                    fstream.Write(array, 0, array.Length);
                }

                startMenu("start");
            }

        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        private async void changeVolume(int APPCOMMAND_VOLUME)
        {
            Form main = new Form();
            SendMessageW(main.Handle, WM_APPCOMMAND, main.Handle, (IntPtr)APPCOMMAND_VOLUME);
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            message = callbackQueryEventArgs.CallbackQuery.Message;
            ChatId = message.Chat.Id;

            //await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"в процессе: '{callbackQuery.Data}'");

            //await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"ку 2 {callbackQuery.Data}");

            if (callbackQuery.Data == "Сделать скрин")
            {
                Directory.CreateDirectory("Screen");
                screenAsync(globalPath + "Screen");
                startMenu("main");
            }

            else if (callbackQuery.Data == "Mute")
                changeVolume(APPCOMMAND_VOLUME_MUTE);

            else if (callbackQuery.Data == "Добавить")
                changeVolume(APPCOMMAND_VOLUME_UP);              

            else if (callbackQuery.Data == "Убавить")
                changeVolume(APPCOMMAND_VOLUME_DOWN);             

            else if (callbackQuery.Data == "Выкл.")
                stateMachine("-s");

            else if (callbackQuery.Data == "Рестарт")
                stateMachine("-r");

            else if (callbackQuery.Data == "Звук")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("Добавить"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Убавить"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Mute")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("back")
                        }
                    });

                await Bot.SendTextMessageAsync(ChatId, "Что вы хотите сделать?", replyMarkup: inlineKeyboard);

            }

            else if (callbackQuery.Data == "Сост. машины")
            {
                var inlineKeyboard2 = new InlineKeyboardMarkup(new[]
            {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("Выкл."),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Рестарт"),
                        }
                    });

                await Bot.SendTextMessageAsync(ChatId, "Что вы хотите сделать?", replyMarkup: inlineKeyboard2);
            }

            else if (callbackQuery.Data == "back")
            {
                startMenu("main");
            }

            else if (callbackQuery.Data == "Включить музыку" && musicFlag == false)
            {
                musicFlag = true;
                await Bot.SendTextMessageAsync(ChatId, "Введи название песни:");
                return;
            }

            else if (callbackQuery.Data == "Поменять фон раб. стола")
            {
                backgroundFlag = true;
                await Bot.SendTextMessageAsync(ChatId, "Отправьте фото (как документ) и оно установиться на твоем рабочем столе.");
            }
        }

        private async void startMenu(string menuPosition)
        {
            await Bot.SendChatActionAsync(ChatId, ChatAction.Typing);

            await Task.Delay(500); // simulate longer running task

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("Звук"),
                            InlineKeyboardButton.WithCallbackData("Сост. машины"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Поменять фон раб. стола"),
                            InlineKeyboardButton.WithCallbackData("Включить музыку"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Сделать скрин")
                        }
                    });

            if (menuPosition == "start")
            {
                await Bot.SendTextMessageAsync(ChatId, $" Мой Господин, ваш компьютер только что был запущен.\n " +
                                    $"Ваш IP:  {globalIp[0].Value}\n " +
                                    $"Напоминаю:  3389\n" +
                                    $"    Хорошего дня  =)\n\n" +
                                    $"Сегодня:   {DateTime.Now.ToLongDateString()}\n\n" +
                                    $"Что я могу для Вас сделать?",
                                    replyMarkup: inlineKeyboard);
            }
            else if (menuPosition == "main")
            {
                await Bot.SendTextMessageAsync(ChatId, $"Что я могу для Вас сделать?", replyMarkup: inlineKeyboard);
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

        private async void SendPhotoAsync(string path)
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
            startMenu("main");
        }

        private void screenAsync(string path)
        {
            path += @"\\printscreen.jpg";
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            printscreen.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

            SendPhotoAsync(path);
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
            Application.Exit();
        }
    }
}
