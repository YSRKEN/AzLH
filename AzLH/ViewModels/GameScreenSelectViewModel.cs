using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using static AzLH.Models.MainModel;

namespace AzLH.ViewModels {
	class GameScreenSelectViewModel {
		// modelのinstance
		private GameScreenSelectModel gameScreenSelectModel;
		// ページ情報を表示
		public ReactiveProperty<string> PageInfoStr { get; }
		// 選択しているrectに基づくスクショのプレビュー
		public ReactiveProperty<BitmapSource> GameWindowPage { get; }
		// 前の画像に移動
		public ReactiveCommand PrevPageCommand { get; }
		// 次の画像に移動
		public ReactiveCommand NextPageCommand { get; }
		// 決定ボタン
		public ReactiveCommand SelectPageCommand { get; }
		// キャンセルボタン
		public ReactiveCommand CancelCommand { get; }
		// コンストラクタ
		public GameScreenSelectViewModel(List<Rectangle> rectList, SelectGameWindowAction dg) {
			// 初期化
			gameScreenSelectModel = new GameScreenSelectModel(rectList, dg);
			// プロパティを設定
			PageInfoStr = gameScreenSelectModel.ObserveProperty(x => x.PageInfoStr).ToReactiveProperty();
			GameWindowPage = gameScreenSelectModel.ObserveProperty(x => x.GameWindowPage).ToReactiveProperty();
			// コマンドを設定
			PrevPageCommand = new ReactiveCommand();
			NextPageCommand = new ReactiveCommand();
			SelectPageCommand = new ReactiveCommand();
			CancelCommand = new ReactiveCommand();
			PrevPageCommand.Subscribe(gameScreenSelectModel.PrevPage);
			NextPageCommand.Subscribe(gameScreenSelectModel.NextPage);
			SelectPageCommand.Subscribe(gameScreenSelectModel.SelectPage);
			CancelCommand.Subscribe(gameScreenSelectModel.Cancel);
		}
	}
}
