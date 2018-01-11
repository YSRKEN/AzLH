using AzLH.Models;
using OxyPlot;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Timers;
using System.Windows.Media;
using System;

namespace AzLH.ViewModels {
	internal class SupplyViewModel : IDisposable
	{
		// modelのinstance
		private readonly SupplyModel supplyModel;
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; }
		// 画面の位置
		public ReactiveProperty<double> WindowPositionLeft { get; }
		public ReactiveProperty<double> WindowPositionTop { get; }
		public ReactiveProperty<double> WindowPositionWidth { get; }
		public ReactiveProperty<double> WindowPositionHeight { get; }
		// 起動時にこの画面を表示するか？
		public ReactiveProperty<bool> AutoOpenWindowFlg { get; }
		// 表示する期間
		public ReactiveProperty<int> GraphPeriodIndex { get; }
		// 表示する期間の一覧
		public ReactiveProperty<List<string>> GraphPeriodList { get; }
		// 表示する資材のモードに対応してボタンの色・表示内容が変わる
		public ReactiveProperty<Brush> SupplyModeButtonColor { get; }
		public ReactiveProperty<string> SupplyModeButtonContent { get; }
		// グラフデータ
		public ReactiveProperty<PlotModel> SupplyGraphModel { get; }

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; }
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; }

		// コンストラクタ
		public SupplyViewModel() {
			// 初期化
			supplyModel = new SupplyModel();
			// プロパティを設定
			CloseWindow = supplyModel.ObserveProperty(x => x.CloseWindow).ToReactiveProperty().AddTo(Disposable);
			WindowPositionLeft = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionLeft).AddTo(Disposable);
			WindowPositionTop = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionTop).AddTo(Disposable);
			WindowPositionWidth = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionWidth).AddTo(Disposable);
			WindowPositionHeight = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionHeight).AddTo(Disposable);
			AutoOpenWindowFlg = supplyModel.ToReactivePropertyAsSynchronized(x => x.AutoOpenWindowFlg).AddTo(Disposable);
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
