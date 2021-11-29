using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace mineSweep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int GRID_COLS = 10;           // 列数
        private const int GRID_ROWS = 10;           // 行数
        private const int GRID_NUMS = ((GRID_COLS) * (GRID_ROWS));    // 格子数 = 行数 * 列数
        private const int RANDOM_MINES_NUM = 10;    // 地雷数

        public List<Button> deepGridButton = new List<Button>();       // 底部按钮集合
        public List<Button> topGridButton = new List<Button>();        // 上层按钮集合

        private readonly DispatcherTimer timer = null;                           // 计时器

        readonly Dictionary<string, int> DeepButtonIndexDict = new Dictionary<string, int>();    // 存储底层按钮名字索引的字典
        readonly Dictionary<string, int> TopButtonIndexDict = new Dictionary<string, int>();    // 存储底层按钮名字索引的字典

        int innerMinesCounter = RANDOM_MINES_NUM;    // 内部剩余地雷数量，用于判断是否胜利

        private enum TimeState      // 计时器的三种状态
        {
            Start,
            Pause,
            End
        }

        TimeState timeState = TimeState.End;    // 初始化计时器状态
        private TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();
            Title = "扫雷";
            Panel.SetZIndex(GridBorder, 1);         // 设置边框的层级在其它控件之上

            // 计时器初始化
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += OnTimer;
            timer.IsEnabled = true;
            timer.Start();

            MinesCounter.Text = RANDOM_MINES_NUM.ToString();        // 剩余地雷数量初始化

            CreateDeepMinesAndNums(RANDOM_MINES_NUM);
            CreateTopGridButton(GRID_NUMS);
        }

        /// <summary>
        /// 创建上层按钮
        /// </summary>
        /// <param name="n">格子总数</param>
        private void CreateTopGridButton(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Button btn = new Button();
                btn.Name = "Top" + i.ToString();
                btn.Width = 40;
                btn.Height = 40;
                btn.FontSize = 30;
                btn.Content = "";
                btn.FontWeight = FontWeights.Normal;
                btn.Background = Brushes.LightBlue;
                btn.Foreground = Brushes.Red;
                btn.Click += Btn_MouseLeftButtonDown;
                btn.MouseDown += Btn_MouseRightButtonDown;
                TopButtonIndexDict.Add(btn.Name, i);
                topGridButton.Insert(i, btn);
                TopGrid.Children.Insert(i, btn);
            }
        }

        /// <summary>
        /// 上层鼠标右键点击事件
        /// </summary>
        private void Btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (timeState == TimeState.End)
            {
                timeState = TimeState.Start;
            }
            Button btn = sender as Button;
            int minesCounter = int.Parse(MinesCounter.Text);
            int index = TopButtonIndexDict[btn.Name];
            if (btn.Content.ToString() == "")
            {
                btn.Content = "🚩";
                minesCounter--;
                // 如果下面是雷，innerMinesCounter 减一
                if (deepGridButton[index].Content.ToString() == "💣")
                {
                    innerMinesCounter--;
                }
            }
            else
            {
                btn.Content = "";
                minesCounter++;
                // 如果下面是雷，拔了旗子之后 innerMinesCounter 加一
                if (deepGridButton[index].Content.ToString() == "💣")
                {
                    innerMinesCounter++;
                }
            }
            MinesCounter.Text = minesCounter.ToString();
            if ((innerMinesCounter == 0) && (minesCounter == 0))
            {
                WinText.Visibility = Visibility.Visible;
                timeState = TimeState.Pause;
                MessageBox.Show("你赢了！耗时" + TimerText.Text + "秒");
                DisableAllButton();
            }
        }

        /// <summary>
        /// 上层按钮鼠标左键点击事件
        /// </summary>
        private void Btn_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (timeState == TimeState.End)
            {
                timeState = TimeState.Start;
            }
            Button btn = sender as Button;
            int index = TopButtonIndexDict[btn.Name];
            // 如果点击的是雷，游戏结束
            if (deepGridButton[index].Content.ToString() == "💣")
            {
                FailText.Visibility = Visibility.Visible;
                deepGridButton[index].Content = "💥";
                timeState = TimeState.Pause;
                DisableAllButton();
            }
            // 如果点击的是空，则周围8格同时消除
            if (deepGridButton[index].Content.ToString() == "")
            {
                Clean8GridWithIndex(index);
            }
            btn.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 创建底层的地雷和数字
        /// </summary>
        /// <param name="n">地雷总数</param>
        private void CreateDeepMinesAndNums(int n)
        {
            List<int> randomList = new List<int>();     // 新建数组，用于生成指定个随机不相等的数
            for (int i = 0; i < n; i++)
            {
            tryAgain:
                Random random = new Random();
                int num = random.Next(0, GRID_NUMS);
                if (randomList.Count != 0 && randomList.Contains(num))
                {
                    goto tryAgain;
                }
                else
                {
                    randomList.Insert(i, num);
                }
            }

            // 创建底层按钮
            for (int i = 0; i < GRID_NUMS; i++)
            {
                Button btn = new Button();
                btn.Name = "Deep" + i.ToString();
                btn.Width = 40;
                btn.Height = 40;
                btn.FontSize = 30;
                btn.FontWeight = FontWeights.Normal;
                btn.Background = Brushes.AliceBlue;
                if (randomList.Contains(i))
                {
                    btn.Content = "💣";
                }
                btn.MouseDown += DeepButtonBothClick;
                //btn.MouseEnter += OnMouseEnter;
                deepGridButton.Insert(i, btn);
                DeepButtonIndexDict.Add(btn.Name, i);
                DeepGrid.Children.Add(btn);
            }

            // 随机数集合排序
            int temp;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = n + 1; j < n; j++)
                {
                    if (randomList[i] > randomList[j])
                    {
                        temp = randomList[i];
                        randomList[i] = randomList[j];
                        randomList[j] = temp;
                    }
                }
            }

            // 对生成的数字再次进行处理
            for (int i = 0; i < GRID_NUMS; i++)
            {
                if (!randomList.Contains(i))
                {
                    deepGridButton[i].Content = CreateNumBaseOnMinesAround(i, randomList);
                }
                if (deepGridButton[i].Content.ToString() == "0")
                {
                    deepGridButton[i].Content = "";
                }

                // 处理数字颜色
                switch (deepGridButton[i].Content.ToString())
                {
                    default: { } break;
                    case "1": deepGridButton[i].Foreground = Brushes.DeepSkyBlue; break;
                    case "2": deepGridButton[i].Foreground = Brushes.Green; break;
                    case "3": deepGridButton[i].Foreground = Brushes.OrangeRed; break;
                    case "4": deepGridButton[i].Foreground = Brushes.DarkBlue; break;
                    case "5": deepGridButton[i].Foreground = Brushes.DarkRed; break;
                    case "6": deepGridButton[i].Foreground = Brushes.LightSeaGreen; break;
                    case "7": deepGridButton[i].Foreground = Brushes.LightSteelBlue; break;
                }
            }
        }

        /// <summary>
        /// 根据周围的地雷生成数字
        /// </summary>
        /// <param name="n">格子号</param>
        /// <param name="randomList">炸弹的位置数组</param>
        /// <returns></returns>
        private string CreateNumBaseOnMinesAround(int n, List<int> randomList)
        {
            int count = 0;
            int[] indexArr = Around8Grid(n);

            for (int i = 0; i < indexArr.Length; i++)
            {
                if (randomList.Contains(indexArr[i]))
                {
                    count++;
                }
            }
            return count.ToString();
        }
        /// <summary>
        /// 传入一个格子的地址，用数组返回周围 8 个格子的索引号
        /// </summary>
        /// <param name="n">格子的索引</param>
        /// <returns>包含周围 8 个格子地址的数组</returns>
        private int[] Around8Grid(int n)
        {
            // 格子周围8个格子的索引
            int[] indexArr = {
                n - GRID_COLS - 1,  n - GRID_COLS,  n - GRID_COLS + 1,
                n - 1,                              n + 1,
                n + GRID_COLS - 1,  n + GRID_COLS,  n + GRID_COLS + 1
            };

            // 最左侧的格子
            if (n % GRID_ROWS == 0)
            {
                indexArr[0] = -1;
                indexArr[3] = -1;
                indexArr[5] = -1;
            }
            // 最右侧的格子
            else if (n % GRID_ROWS == (GRID_ROWS - 1))
            {
                indexArr[2] = -1;
                indexArr[4] = -1;
                indexArr[7] = -1;
            }
            // 最上面的格子
            if (n < GRID_ROWS)
            {
                indexArr[0] = -1;
                indexArr[1] = -1;
                indexArr[2] = -1;
            }
            // 最下面一行的格子
            else if (n >= (GRID_NUMS - GRID_ROWS))
            {
                indexArr[5] = -1;
                indexArr[6] = -1;
                indexArr[7] = -1;
            }
            return indexArr;
        }

        /// <summary>
        /// 鼠标左右键一起按下，触发清除事件
        /// </summary>
        private void DeepButtonBothClick(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            int n = DeepButtonIndexDict[btn.Name];
            // 判断左右键同时按下
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
            {
                if (deepGridButton[n].Content.ToString() == "")
                {
                    return;
                }
                int minesCount = int.Parse(deepGridButton[n].Content.ToString());   // 按下的按键的数字
                int flagCount = 0;
                int[] indexArr = Around8Grid(n);

                for (int i = 0; i < indexArr.Length; i++)
                {
                    if (indexArr[i] == -1)
                        continue;
                    if (topGridButton[indexArr[i]].Content.ToString() == "🚩")
                    {
                        flagCount++;
                    }
                }

                if (flagCount == minesCount)
                {
                    Clean8GridWithIndex(n);
                }
            }
        }

        /// <summary>
        /// 根据索引排除周围8格
        /// </summary>
        /// <param name="n"></param>
        private void Clean8GridWithIndex(int n)
        {
            int[] indexArr = Around8Grid(n);

            for (int i = 0; i < indexArr.Length; i++)
            {
                if (indexArr[i] == -1)
                    continue;
                if (topGridButton[indexArr[i]].Visibility == Visibility.Visible)
                {
                    if (deepGridButton[indexArr[i]].Content.ToString() == "💣")
                    {
                        continue;
                    }
                    topGridButton[indexArr[i]].Visibility = Visibility.Hidden;
                    if (deepGridButton[indexArr[i]].Content.ToString() == "")
                    {
                        Clean8GridWithIndex(indexArr[i]);
                    }
                }
            }
        }



        /// <summary>
        /// 时间回调方法
        /// </summary>
        private void OnTimer(object sender, EventArgs e)
        {
            switch (timeState)
            {
                case TimeState.Start: timeSpan += new TimeSpan(0, 0, 0, 1); break;
                case TimeState.Pause: { }; break;
                case TimeState.End: timeSpan = new TimeSpan(); break;
            }
            TimerText.Text = (timeSpan.Hours * 3600 + timeSpan.Minutes * 60 + timeSpan.Seconds).ToString();
        }

        /// <summary>
        /// 禁用所有按钮，在游戏结束的时候调用
        /// </summary>
        private void DisableAllButton()
        {
            for (int i = 0; i < GRID_NUMS; i++)
            {
                topGridButton[i].Visibility = Visibility.Hidden;
                deepGridButton[i].IsEnabled = false;
            }
        }

        private void PauseClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("你按了暂停");
        }

        private void RestartClick(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "mineSweep.exe";
            p.StartInfo.UseShellExecute = false;
            p.Start();
            Application.Current.Shutdown();
        }

        private void RangingClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("排行榜");
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("帮助");
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("关于");
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
