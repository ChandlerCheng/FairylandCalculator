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

            int initialPetValue;
            if (!int.TryParse(textMainValue.Text, out initialPetValue))
            {
                MessageBox.Show(string.Format("主要數值解析錯誤"));                
                return;
            }
            double addValue;
            if (!double.TryParse(textAddValue.Text, out addValue))
            {
                MessageBox.Show(string.Format("成長模擬解析錯誤"));
                return;
            }

            var allEntries = new List<Tuple<int, int>>(); // (level, petLevel)
            // 展平成一筆一筆的序列
            foreach (var kv in levelData)
            {
                int level = kv.Key;
                List<int> levels = kv.Value.Item2;

                foreach (var petLevel in levels)
                {
                    allEntries.Add(Tuple.Create(level, petLevel));
                }
            }

            int currentValue = initialPetValue;
            int? lastNextPetLevel = null;

            for (int i = 0; i < allEntries.Count; i++)
            {
                int level = allEntries[i].Item1;
                int petLevel = allEntries[i].Item2;

                int result;
                bool success = PetCalculator.PetCalculator_Level(true, level, petLevel, currentValue, out result);

                if (!success)
                {
                    MessageBox.Show(string.Format("計算失敗：Level {0}, PetLevel {1}", level, petLevel));
                    return;
                }

                int usedNextPetLevel = -1;

                if (i + 1 < allEntries.Count)
                {
                    int nextPetLevel = allEntries[i + 1].Item2;
                    usedNextPetLevel = nextPetLevel;
                    currentValue = result + (int)((nextPetLevel - 1) * addValue);
                    lastNextPetLevel = nextPetLevel; // 記錄最後一次使用的 nextPetLevel
                }
                else if (lastNextPetLevel.HasValue)
                {
                    // 最後一筆 → 用上一筆的 nextPetLevel 來算一次 final currentValue
                    currentValue = result + (int)((lastNextPetLevel.Value - 1) * addValue);
                    usedNextPetLevel = lastNextPetLevel.Value;
                }

                sb.AppendLine(string.Format("第 {0} 次：{1}, 降等等級 = {2}, 降後初始 = {3}, 最終 = {4}",
                    i + 1, PetString.getLevelItemName(level), petLevel,  result, currentValue));
            }




            MessageBox.Show("計算完成：\n\n" + sb.ToString());

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

    class PetString
    {
        public static string getLevelItemName(int level)
        {
            string result = "";
            switch (level)
            {
                case 1:
                    result = "咖啡";
                    break;
                case 2:
                    result = "汽水";
                    break;
                case 3:
                    result = "可樂";
                    break;
                case 4:
                    result = "香檳";
                    break;
                case 5:
                    result = "開水";
                    break;
                case 6:
                    result = "雞精";
                    break;
                case 7:
                    result = "蔘湯";
                    break;
            }
            return result;
        }
    }
}
