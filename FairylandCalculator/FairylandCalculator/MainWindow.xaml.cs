using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FairylandCalculator
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCalculator_Click(object sender, RoutedEventArgs e)
        {
            // 改用 Tuple 作為資料格式
            Dictionary<int, Tuple<int, List<int>>> levelData = new Dictionary<int, Tuple<int, List<int>>>();

            for (int i = 1; i <= 7; i++)
            {
                TextBox countBox = (TextBox)this.FindName($"textCount_Level{i}");
                TextBox levelBox = (TextBox)this.FindName($"textLevel_Level{i}");

                string countText = countBox.Text.Trim();

                if (!string.IsNullOrEmpty(countText))
                {
                    int countValue;
                    if (!int.TryParse(countText, out countValue))
                    {
                        MessageBox.Show(string.Format("Level {0} 的數量格式錯誤，請輸入整數", i));
                        countBox.Focus();
                        return;
                    }

                    string levelText = levelBox.Text.Trim();

                    if (string.IsNullOrEmpty(levelText))
                    {
                        MessageBox.Show(string.Format("Level {0} 的等級尚未輸入", i));
                        levelBox.Focus();
                        return;
                    }

                    string[] parts = levelText.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> levelList = new List<int>();

                    foreach (string part in parts)
                    {
                        int levelVal;
                        if (!int.TryParse(part.Trim(), out levelVal))
                        {
                            MessageBox.Show(string.Format("Level {0} 的等級格式錯誤：{1}", i, part));
                            levelBox.Focus();
                            return;
                        }
                        levelList.Add(levelVal);
                    }

                    // 儲存結果
                    levelData[i] = new Tuple<int, List<int>>(countValue, levelList);
                }
            }

            // 成功：顯示資料
            StringBuilder sb = new StringBuilder();
            foreach (var kv in levelData)
            {
                sb.AppendLine(string.Format("Level {0}：數量 {1}，等級為 {2}",
                    kv.Key, kv.Value.Item1, string.Join(", ", kv.Value.Item2)));
            }

            MessageBox.Show("輸入正確，資料如下：\n\n" + sb.ToString());

            double addValue = 3.8;
            int initialPetValue = 760;
            int finalValue = initialPetValue;

            StringBuilder calcLog = new StringBuilder();
            calcLog.AppendLine(string.Format("初始值：{0}", finalValue));
        }
    }
    class PetCalculator
    {
        public static bool PetCalculator_Level(bool isMainValue, int Level, int PetLevel, int PetValue, out int Value)
        {
            Value = 0;
            double final;

            int calTotal = PetLevel + PetValue;

            // 主屬性等級對應資料：Level → (LevelVal, extraValue)
            Dictionary<int, Tuple<double, double>> mainLevelMap = new Dictionary<int, Tuple<double, double>>
            {
                { 1, Tuple.Create(6.0,  8.0) },
                { 2, Tuple.Create(5.0, 10.0) },
                { 3, Tuple.Create(4.0, 12.0) },
                { 4, Tuple.Create(3.0, 14.0) },
                { 5, Tuple.Create(2.0, 16.0) },
                { 6, Tuple.Create(1.5, 22.0) },
                { 7, Tuple.Create(0.75, 32.0) },
            };

            // 副屬性等級對應資料：Level → extraValue（LevelVal 固定 20）
            Dictionary<int, double> subLevelMap = new Dictionary<int, double>
            {
                { 1, 16 },
                { 2, 20 },
                { 3, 28 },
                { 4, 41 },
                { 5, 54 },
                { 6, 83 },
                { 7, 102 },
            };

            if (isMainValue)
            {
                if (!mainLevelMap.ContainsKey(Level))
                    return false;

                double levelVal = mainLevelMap[Level].Item1;
                double extraValue = mainLevelMap[Level].Item2;

                if (Level == 7)
                    final = (calTotal * levelVal) + extraValue;
                else
                    final = (calTotal / levelVal) + extraValue;
            }
            else
            {
                if (!subLevelMap.ContainsKey(Level))
                    return false;

                double levelVal = 20;
                double extraValue = subLevelMap[Level];

                final = (calTotal / levelVal) + extraValue;
            }

            Value = (int)Math.Floor(final); // 無條件捨去
            return true;
        }

    }
}
