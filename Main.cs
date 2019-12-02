using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rory_Mercury
{
    public partial class Main : Form
    {
        protected static readonly TelegramBotClient Bot = new TelegramBotClient("956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc");
        protected static Telegram.Bot.Types.Message message;
        private static Process process = new Process();
        private static DateTime StartSession;
        private static string UserMessage;
        private static string globalPath;
        private static bool musicFlag = false;
        private static bool cmdFlag = false;
        private static bool chatFlag = false;
        private static bool backgroundFlag = false;
        private static MatchCollection globalIp;
        private readonly string path = "states.txt";
        private static long ChatId;

        //https://api.telegram.org/bot956177251:AAE65NwO-j2Rf8H_J70FiSew4gaCgOT0yyc/getUpdates

        private const int KEYEVENTF_EXTENTEDKEY = 1;
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;
        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const int VK_MEDIA_PREV_TRACK = 0xB1;
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

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

        private void Main_Load(object sender, EventArgs e)
        {
            StartSession = DateTime.Now;
            if (File.Exists("states.txt"))
            {
                using (FileStream fstream = File.OpenRead(path))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    ChatId = Convert.ToInt64(textFromFile);
                    label5.Text = ChatId.ToString();
                    startMenu("start");
                }
            }

            notifyIcon1.BalloonTipText = $"Ваш IP:  {globalIp[0].Value}";
            notifyIcon1.BalloonTipTitle = "Rory Mercury";
            //notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(700);
        }

        private void NI_BalloonTipClosed(Object sender, EventArgs e)
        {
            //notifyIcon1.Visible = false;
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
            if (message.Text == "/exit")
            {
                cmdFlag = false;
                startMenu("main");
            }
            if (cmdFlag)
            {
                string command = message.Text;

                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();

                process.StandardInput.WriteLine("cls");
                process.StandardInput.Flush();
            }
            if (chatFlag)
            {
                richTextBox1.Text += $"{message.Chat.FirstName}:\n\t" + message.Text + "\n";
            }
            if (message.Text == "/menu")
            {
                startMenu("main");
                chatFlag = cmdFlag = musicFlag = false;
                Visible = false;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        private void changeVolume(int APPCOMMAND_VOLUME)
        {
            Form main = new Form();
            SendMessageW(main.Handle, WM_APPCOMMAND, main.Handle, (IntPtr)APPCOMMAND_VOLUME);
            main.Close();
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

            else if (callbackQuery.Data == "Cmd")
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = @"C:\";
                process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");


                process.StartInfo.RedirectStandardInput = true;

                process.OutputDataReceived += ProcessOutputDataHandler;
                process.ErrorDataReceived += ProcessErrorDataHandler;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                cmdFlag = true;
                return;
            }

            else if (callbackQuery.Data == "Chat")
            {
                Visible = Visible ? false : true;
                chatFlag = true;
            }

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
                            InlineKeyboardButton.WithCallbackData("Рестарт")
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Cmd"),
                            InlineKeyboardButton.WithCallbackData("Chat"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("back")
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

            else if (callbackQuery.Data == "\u23EA") // back
            {
                keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            }

            else if (callbackQuery.Data == "\u25B6") // pause/play
            {
                keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
            }

            else if (callbackQuery.Data == "\u23E9") // next
            {
                keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
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
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("\u23EA"),
                            InlineKeyboardButton.WithCallbackData("\u25B6"),
                            InlineKeyboardButton.WithCallbackData("\u23E9")
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

        public static void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                Bot.SendTextMessageAsync(ChatId, outLine.Data);
                Thread.Sleep(500);
            }                
        }

        public static void ProcessErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
                Bot.SendTextMessageAsync(ChatId, outLine.Data);
        }

        private static void stateMachine(string Act)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
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
            Thread.Sleep(1000);
            Bot.StopReceiving();
            Application.Exit();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            Thread.Sleep(1000);
            TimeSpan span = DateTime.Now - StartSession;
            string str;

            if (span.Minutes <= 1)
                str = $"{span.Seconds} c.";
            else if (span.Hours <= 1)
                str = $"{span.Minutes} м. {span.Seconds} c.";
            else if (span.Days <= 1)
                str = $"{span.Hours} ч. {span.Minutes} м. {span.Seconds} c.";
            else
                str = $"{span.Days} д. {span.Hours} ч. {span.Minutes} м. {span.Seconds} c.";

            Bot.SendTextMessageAsync(ChatId, $" Мой Господин, ваш компьютер только что был выключен.\n " +
                $"Ваш компьютер был в сети:   {str}");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Visible = false;
            notifyIcon1.BalloonTipText = $"Окно свернуто.\nНажмите на иконку два раза чтобы розвернуть.";
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = Visible ? false : true;
        }

        private void chatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = Visible ? false : true;
            chatFlag = true;
        }

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                yOffset = -e.Y - SystemInformation.CaptionHeight -
                    SystemInformation.FrameBorderSize.Height;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void Send_Click(object sender, EventArgs e)
        {
            Bot.SendTextMessageAsync(ChatId, richTextBox2.Text);
            richTextBox1.Text += "User:\n\t" + richTextBox2.Text + "\n";
            richTextBox2.Clear();
        }

        private void Main_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                Bot.SendTextMessageAsync(ChatId, "Чат включен.");
            else
                Bot.SendTextMessageAsync(ChatId, "Чат выключен.");
        }
    }
}