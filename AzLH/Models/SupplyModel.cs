using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows;

namespace AzLH.Models {
	class SupplyModel : BindableBase {
		// 燃料：～10000　通常資材左
		// 資金：～50000　通常資材右
		// キューブ：～100　特殊資材左
		// ドリル：～100　特殊資材左
		// 勲章：～100　特殊資材左
		// ダイヤ：～1000　特殊資材右
		// 家具コイン：～1000　特殊資材右

		// trueにすると画面を閉じる
		private bool closeWindow;
		public bool CloseWindow {
			get { return closeWindow; }
			set { SetProperty(ref closeWindow, value); }
		}
		// 画面の位置
		private double windowPositionLeft = double.NaN;
		public double WindowPositionLeft {
			get => windowPositionLeft;
			set {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				SetProperty(ref windowPositionLeft, value);
				settings.SupplyWindowRect[0] = windowPositionLeft;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double windowPositionTop = double.NaN;
		public double WindowPositionTop {
			get => windowPositionTop;
			set {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				SetProperty(ref windowPositionTop, value);
				settings.SupplyWindowRect[1] = windowPositionTop;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double windowPositionWidth = 600.0;
		public double WindowPositionWidth {
			get => windowPositionWidth;
			set {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				SetProperty(ref windowPositionWidth, value);
				settings.SupplyWindowRect[2] = windowPositionWidth;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double windowPositionHeight = 400.0;
		public double WindowPositionHeight {
			get { return windowPositionHeight; }
			set {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				SetProperty(ref windowPositionHeight, value);
				settings.SupplyWindowRect[3] = windowPositionHeight;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		// 表示する期間
		private int graphPeriodIndex = 2;
		public int GraphPeriodIndex {
			get => graphPeriodIndex;
			set { SetProperty(ref graphPeriodIndex, value); }
		}
		// 表示する期間の一覧(Keyが名称、Valueが期間(単位は"日"))
		private Dictionary<string, double> graphPeriodDic = new Dictionary<string, double> {
				{ "6時間", 0.25 },
				{ "12時間", 0.5 },
				{ "1日", 1 },
				{ "3日", 3 },
				{ "1週間", 7 },
				{ "1ヶ月", 30 },
				{ "3ヶ月", 90 },
				{ "6ヶ月", 180 },
				{ "1年", 365 },
			};
		public List<string> GraphPeriodList => graphPeriodDic.Keys.Select(p => p).ToList();
		// 表示する資材のモード
		private int showSupplyMode = 0;
		// 表示する資材のモードに対応してボタンの色・表示内容が変わる
		public Brush SupplyModeButtonColor => (showSupplyMode == 0 ? new SolidColorBrush(Colors.Pink) : new SolidColorBrush(Colors.SkyBlue));
		public string SupplyModeButtonContent => (showSupplyMode == 0 ? "通常資材" : "特殊資材");

		// コンストラクタ
		public SupplyModel() {
			SetSettings();
		}

		// 設定内容を画面に反映する
		private void SetSettings() {
			var settings = SettingsStore.Instance;
			if (settings.MemoryWindowPositionFlg) {
				WindowPositionLeft = settings.SupplyWindowRect[0];
				WindowPositionTop = settings.SupplyWindowRect[1];
				WindowPositionWidth = settings.SupplyWindowRect[2];
				WindowPositionHeight = settings.SupplyWindowRect[3];
			}
		}
		// 表示する資材のモードを切り替える
		public void ChangeSupplyMode() {
			if (showSupplyMode == 0)
				showSupplyMode = 1;
			else
				showSupplyMode = 0;
			RaisePropertyChanged("SupplyModeButtonColor");
			RaisePropertyChanged("SupplyModeButtonContent");
		}
		// グラフ画像を保存する
		public void SaveSupplyGraph() {
			// スタブ
		}
	}
}
