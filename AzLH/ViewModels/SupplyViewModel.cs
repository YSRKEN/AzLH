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

namespace AzLH.ViewModels {
	internal class SupplyViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public event PropertyChangedEventHandler PropertyChanged;
		// 表示する期間の情報
		private class GraphPeriodInfo {
			public double Days { get; set; }
			public string StringFormat { get; set; }
			public double MajorStep { get; set; }
			public double MinorStep { get; set; }
			public GraphPeriodInfo(double days, string stringFormat, double majorStep, double minorStep) {
				Days = days;
				StringFormat = stringFormat;
				MajorStep = majorStep;
				MinorStep = minorStep;
			}
		}
		// 表示する期間の一覧(Keyが名称、Valueが期間(単位は"日"))
		private readonly Dictionary<string, GraphPeriodInfo> graphPeriodDic
			= new Dictionary<string, GraphPeriodInfo> {
			{ "6時間",  new GraphPeriodInfo(0.25, "HH:mm"   , 1.0 / 24 , 0.5 / 24)},
			{ "12時間", new GraphPeriodInfo(0.5 , "HH:mm"   , 2.0 / 24 , 1.0 / 24)},
			{ "1日",    new GraphPeriodInfo(1.0 , "HH:mm"   , 4.0 / 24 , 2.0 / 24)},
			{ "3日",    new GraphPeriodInfo(3.0, "MM/dd" , 12.0 / 24, 6.0 / 24)},
			{ "1週間",  new GraphPeriodInfo(7.0, "MM/dd" , 1.0      , 12.0 / 24)},
			{ "1ヶ月", new GraphPeriodInfo(30.0, "MM/dd" , 3.0      , 1.0)},
			{ "3ヶ月", new GraphPeriodInfo(90.0, "MM/dd" , 10.0     , 5.0)},
			{ "6ヶ月", new GraphPeriodInfo(180.0, "MM/dd", 30.0     , 15.0)},
			{ "1年", new GraphPeriodInfo(365, "MM/dd"    , 60.0     , 30.0)},
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
		public ReactiveProperty<PlotModel> SupplyGraphModel { get; }

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; }
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; }

		// グラフを再描画する
		private void RedrawSupplyGraph() {
			// スタブ
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
			// 表示期間が変更された際、グラフを再描画する
			GraphPeriodIndex.Subscribe(_ => RedrawSupplyGraph());
			// 表示する資材のモードに対応してボタンの色・表示内容を変える
			SupplyModeButtonColor = ShowSupplyMode.Select(x => {
				return (Brush)(x == 0 ? new SolidColorBrush(Colors.Pink) : new SolidColorBrush(Colors.SkyBlue));
			}).ToReadOnlyReactiveProperty();
			SupplyModeButtonContent = ShowSupplyMode.Select(x => {
				return (x == 0 ? "通常資材" : "特殊資材");
			}).ToReadOnlyReactiveProperty();
			// その他staticな初期化
			GraphPeriodList = new ReactiveProperty<List<string>>(graphPeriodDic.Keys.Select(p => p).ToList());

			// プロパティを設定
			GraphPeriodIndex = supplyModel.ToReactivePropertyAsSynchronized(x => x.GraphPeriodIndex).AddTo(Disposable);
			GraphPeriodList = supplyModel.ObserveProperty(x => x.GraphPeriodList).ToReactiveProperty().AddTo(Disposable);
			SupplyModeButtonColor = supplyModel.ObserveProperty(x => x.SupplyModeButtonColor).ToReactiveProperty().AddTo(Disposable);
			SupplyModeButtonContent = supplyModel.ObserveProperty(x => x.SupplyModeButtonContent).ToReactiveProperty().AddTo(Disposable);
			SupplyGraphModel = supplyModel.ObserveProperty(x => x.SupplyGraphModel).ToReactiveProperty().AddTo(Disposable);
			// コマンドを設定
			ChangeSupplyModeCommand = new ReactiveCommand();
			SaveSupplyGraphCommand = new ReactiveCommand();
			//
			ChangeSupplyModeCommand.Subscribe(supplyModel.ChangeSupplyMode);
			SaveSupplyGraphCommand.Subscribe(supplyModel.SaveSupplyGraph);
			// タイマーを指定してグラフ更新を定期実行する
			var timer = new Timer(1000 * 60 * 5);
			timer.Elapsed += (sender, e) => {
				try {
					timer.Stop();
					supplyModel.RedrawSupplyGraph();
				}
				finally { timer.Start(); }
			};
			timer.Start();
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
