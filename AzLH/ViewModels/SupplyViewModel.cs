using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Windows.Media;

namespace AzLH.ViewModels {
	internal class SupplyViewModel {
		// modelのinstance
		private readonly SupplyModel supplyModel;
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
		// 表示する資材のモードに対応して色・表示内容が変わる
		public ReactiveProperty<Brush> SupplyModeButtonColor { get; }
		public ReactiveProperty<string> SupplyModeButtonContent { get; }
		public ReactiveProperty<string> AxisYStr { get; }
		public ReactiveProperty<string> AxisY2Str { get; }

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; }
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; }

		// コンストラクタ
		public SupplyViewModel() {
			// 初期化
			supplyModel = new SupplyModel();
			// プロパティを設定
			CloseWindow = supplyModel.ObserveProperty(x => x.CloseWindow).ToReactiveProperty();
			WindowPositionLeft = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionLeft);
			WindowPositionTop = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionTop);
			WindowPositionWidth = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionWidth);
			WindowPositionHeight = supplyModel.ToReactivePropertyAsSynchronized(x => x.WindowPositionHeight);
			AutoOpenWindowFlg = supplyModel.ToReactivePropertyAsSynchronized(x => x.AutoOpenWindowFlg);
			GraphPeriodIndex = supplyModel.ToReactivePropertyAsSynchronized(x => x.GraphPeriodIndex);
			GraphPeriodList = supplyModel.ObserveProperty(x => x.GraphPeriodList).ToReactiveProperty();
			SupplyModeButtonColor = supplyModel.ObserveProperty(x => x.SupplyModeButtonColor).ToReactiveProperty();
			SupplyModeButtonContent = supplyModel.ObserveProperty(x => x.SupplyModeButtonContent).ToReactiveProperty();
			AxisYStr = supplyModel.ObserveProperty(x => x.AxisYStr).ToReactiveProperty();
			AxisY2Str = supplyModel.ObserveProperty(x => x.AxisY2Str).ToReactiveProperty();
			// コマンドを設定
			ChangeSupplyModeCommand = new ReactiveCommand();
			SaveSupplyGraphCommand = new ReactiveCommand();
			//
			ChangeSupplyModeCommand.Subscribe(supplyModel.ChangeSupplyMode);
			SaveSupplyGraphCommand.Subscribe(supplyModel.SaveSupplyGraph);
		}
	}
}
