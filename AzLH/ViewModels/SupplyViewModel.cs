using AzLH.Models;
using OxyPlot;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Timers;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AzLH.ViewModels {
	// 表示する期間の情報
	public struct GraphPeriodInfo
	{
		public double Days;
		public string StringFormat;
		public double MajorStep;
		public double MinorStep;
	}

	internal class SupplyViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public event PropertyChangedEventHandler PropertyChanged;
		// 表示する期間の一覧(Keyが名称、Valueが期間(単位は"日"))
		private readonly Dictionary<string, GraphPeriodInfo> graphPeriodDic
			= new Dictionary<string, GraphPeriodInfo> {
			{ "6時間",  new GraphPeriodInfo{Days = 0.25,  StringFormat = "HH:mm", MajorStep = 1.0 / 24,  MinorStep = 0.5 / 24} },
			{ "12時間", new GraphPeriodInfo{Days = 0.5 ,  StringFormat = "HH:mm", MajorStep = 2.0 / 24,  MinorStep = 1.0 / 24 } },
			{ "1日",    new GraphPeriodInfo{Days = 1.0 ,  StringFormat = "HH:mm", MajorStep = 4.0 / 24,  MinorStep = 2.0 / 24 } },
			{ "3日",    new GraphPeriodInfo{Days = 3.0,   StringFormat = "MM/dd", MajorStep = 12.0 / 24, MinorStep = 6.0 / 24 } },
			{ "1週間",  new GraphPeriodInfo{Days = 7.0,   StringFormat = "MM/dd", MajorStep = 1.0,       MinorStep = 12.0 / 24 } },
			{ "1ヶ月",  new GraphPeriodInfo{Days = 30.0,  StringFormat = "MM/dd", MajorStep = 3.0,       MinorStep = 1.0 } },
			{ "3ヶ月",  new GraphPeriodInfo{Days = 90.0,  StringFormat = "MM/dd", MajorStep = 10.0,      MinorStep = 5.0 } },
			{ "6ヶ月",  new GraphPeriodInfo{Days = 180.0, StringFormat = "MM/dd", MajorStep = 30.0,      MinorStep = 15.0 } },
			{ "1年",    new GraphPeriodInfo{Days = 365,   StringFormat = "MM/dd", MajorStep = 60.0,      MinorStep = 30.0 } },
		};
		// 現在の表示する期間
		private GraphPeriodInfo NowGraphPeriodInfo
			=> graphPeriodDic[GraphPeriodList.Value[GraphPeriodIndex.Value]];
		// 表示する資材のモード
		private ReactiveProperty<int> ShowSupplyMode { get; } = new ReactiveProperty<int>(0);

		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 画面の位置
		public ReactiveProperty<double> WindowPositionLeft { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionTop { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionWidth { get; } = new ReactiveProperty<double>(600.0);
		public ReactiveProperty<double> WindowPositionHeight { get; } = new ReactiveProperty<double>(400.0);
		// 起動時にこの画面を表示するか？
		public ReactiveProperty<bool> AutoOpenWindowFlg { get; } = new ReactiveProperty<bool>(false);
		// 表示する期間
		public ReactiveProperty<int> GraphPeriodIndex { get; } = new ReactiveProperty<int>(2);
		// 表示する期間の一覧
		public ReactiveProperty<List<string>> GraphPeriodList { get; }
		// 表示する資材のモードに対応してボタンの色・表示内容が変わる
		public ReadOnlyReactiveProperty<Brush> SupplyModeButtonColor { get; }
		public ReadOnlyReactiveProperty<string> SupplyModeButtonContent { get; }
		// グラフデータ
		public ReactiveProperty<PlotModel> SupplyGraphModel { get; } = new ReactiveProperty<PlotModel>();

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; } = new ReactiveCommand();
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; } = new ReactiveCommand();
		// エディタを起動する
		public ReactiveCommand ShowEditorCommand { get; } = new ReactiveCommand();

		// グラフを再描画する
		private PlotModel RedrawSupplyGraph() {
			return SupplyModel.RedrawSupplyGraph(ShowSupplyMode.Value, NowGraphPeriodInfo);
		}
		// コンストラクタ
		public SupplyViewModel() {
			// 設定ファイルに記録していた情報を書き戻す
			{
				if (SettingsStore.MemoryWindowPositionFlg) {
					WindowPositionLeft.Value = SettingsStore.SupplyWindowRect[0];
					WindowPositionTop.Value = SettingsStore.SupplyWindowRect[1];
					WindowPositionWidth.Value = SettingsStore.SupplyWindowRect[2];
					WindowPositionHeight.Value = SettingsStore.SupplyWindowRect[3];
				}
				AutoOpenWindowFlg.Value = SettingsStore.AutoSupplyWindowFlg;
			}
			// 画面の位置が変更された際、自動で設定ファイルに書き戻すようにする
			WindowPositionLeft.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[0] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionTop.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[1] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionWidth.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[2] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionHeight.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[3] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			// 起動時にこの画面を表示するか？
			AutoOpenWindowFlg.Subscribe(value => {
				SettingsStore.AutoSupplyWindowFlg = value;
			});
			// 表示する資材のモードに対応してボタンの色・表示内容を変える
			SupplyModeButtonColor = ShowSupplyMode.Select(x => {
				return (Brush)(x == 0 ? new SolidColorBrush(Colors.Pink) : new SolidColorBrush(Colors.SkyBlue));
			}).ToReadOnlyReactiveProperty();
			SupplyModeButtonContent = ShowSupplyMode.Select(x => {
				return (x == 0 ? "通常資材" : "特殊資材");
			}).ToReadOnlyReactiveProperty();
			// その他staticな初期化
			GraphPeriodList = new ReactiveProperty<List<string>>(graphPeriodDic.Keys.Select(p => p).ToList());
			// 表示期間・資材モードが変更された際、グラフを再描画する
			SupplyGraphModel = GraphPeriodIndex.CombineLatest(ShowSupplyMode, (_, mode) => RedrawSupplyGraph()).ToReactiveProperty();
			// コマンドを設定
			ChangeSupplyModeCommand.Subscribe(_ => {
				ShowSupplyMode.Value = (ShowSupplyMode.Value == 0 ? 1 : 0);
				RedrawSupplyGraph();
			});
			SaveSupplyGraphCommand.Subscribe(_ => SupplyModel.SaveSupplyGraph(SupplyGraphModel.Value));
			ShowEditorCommand.Subscribe(_ => {
				var vm = new SupplyEditorViewModel();
				var view = new Views.SupplyEditorView { DataContext = vm };
				view.ShowDialog();
			});
			// タイマーを指定してグラフ更新を定期実行する
			var timer = new Timer(1000 * 60 * 5);
			timer.Elapsed += (sender, e) => {
				try {
					timer.Stop();
					RedrawSupplyGraph();
				}
				finally { timer.Start(); }
			};
			timer.Start();
			// まず最初の画面更新を掛ける
			SupplyGraphModel.Value = RedrawSupplyGraph();
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
