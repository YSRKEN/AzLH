using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;
using static AzLH.Models.MainModel;

namespace AzLH.ViewModels {
	internal class GameScreenSelectViewModel : IDisposable
	{
		// modelのinstance
		private readonly GameScreenSelectModel gameScreenSelectModel;
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; }
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
		public GameScreenSelectViewModel() { }
		public GameScreenSelectViewModel(List<Rectangle> rectList, SelectGameWindowAction dg) {
			// 初期化
			gameScreenSelectModel = new GameScreenSelectModel(rectList, dg);
			// プロパティを設定
			CloseWindow = gameScreenSelectModel.ObserveProperty(x => x.CloseWindow).ToReactiveProperty().AddTo(Disposable);
			PageInfoStr = gameScreenSelectModel.ObserveProperty(x => x.PageInfoStr).ToReactiveProperty().AddTo(Disposable);
			GameWindowPage = gameScreenSelectModel.ObserveProperty(x => x.GameWindowPage).ToReactiveProperty().AddTo(Disposable);
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

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
