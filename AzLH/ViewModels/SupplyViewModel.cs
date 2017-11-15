﻿using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Windows.Media;

namespace AzLH.ViewModels {
	internal class SupplyViewModel {
		// modelのinstance
		private readonly SupplyModel supplyModel;
		// 表示する期間
		public ReactiveProperty<int> GraphPeriodIndex { get; }
		// 表示する期間の一覧
		public ReactiveProperty<List<string>> GraphPeriodList { get; }
		// 表示する資材のモードに対応してボタンの色・表示内容が変わる
		public ReactiveProperty<Brush> SupplyModeButtonColor { get; }
		public ReactiveProperty<string> SupplyModeButtonContent { get; }

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; }
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; }

		// コンストラクタ
		public SupplyViewModel() {
			// 初期化
			supplyModel = new SupplyModel();
			// プロパティを設定
			GraphPeriodIndex = supplyModel.ToReactivePropertyAsSynchronized(x => x.GraphPeriodIndex);
			GraphPeriodList = supplyModel.ObserveProperty(x => x.GraphPeriodList).ToReactiveProperty();
			SupplyModeButtonColor = supplyModel.ObserveProperty(x => x.SupplyModeButtonColor).ToReactiveProperty();
			SupplyModeButtonContent = supplyModel.ObserveProperty(x => x.SupplyModeButtonContent).ToReactiveProperty();
			// コマンドを設定
			ChangeSupplyModeCommand = new ReactiveCommand();
			SaveSupplyGraphCommand = new ReactiveCommand();
			//
			ChangeSupplyModeCommand.Subscribe(supplyModel.ChangeSupplyMode);
			SaveSupplyGraphCommand.Subscribe(supplyModel.SaveSupplyGraph);
		}
	}
}
