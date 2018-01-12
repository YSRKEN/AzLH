using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using static AzLH.Models.MainModel;

namespace AzLH.ViewModels {
	internal class GameScreenSelectViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public event PropertyChangedEventHandler PropertyChanged;
		// rect一覧
		private readonly List<Rectangle> rectList;
		// 選択結果を返すdelegate
		private readonly SelectGameWindowAction dg;

		// 選択しているrectのindex
		private ReactiveProperty<int> RectIndex { get; } = new ReactiveProperty<int>(0);

		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// ページ情報を表示
		public ReadOnlyReactiveProperty<string> PageInfoStr { get; }
		// 選択しているrectに基づくスクショのプレビュー
		public ReadOnlyReactiveProperty<BitmapSource> GameWindowPage { get; }

		// 前の画像に移動
		public ReactiveCommand PrevPageCommand { get; } = new ReactiveCommand();
		// 次の画像に移動
		public ReactiveCommand NextPageCommand { get; } = new ReactiveCommand();
		// 決定ボタン
		public ReactiveCommand SelectPageCommand { get; } = new ReactiveCommand();
		// キャンセルボタン
		public ReactiveCommand CancelCommand { get; } = new ReactiveCommand();

		// コンストラクタ
		public GameScreenSelectViewModel() { }
		public GameScreenSelectViewModel(List<Rectangle> rectList, SelectGameWindowAction dg) {
			this.rectList = rectList;
			this.dg = dg;
			// プロパティを設定
			PageInfoStr = RectIndex.Select(x => $"[{x + 1}/{rectList.Count}] {Utility.GetRectStr(rectList[x])}").ToReadOnlyReactiveProperty();
			GameWindowPage = RectIndex.Select(x => (BitmapSource)ScreenShotProvider.GetScreenBitmap(rectList[x]).ToImageSource()).ToReadOnlyReactiveProperty();
			// コマンドを設定
			PrevPageCommand.Subscribe(_ => {
				RectIndex.Value = (rectList.Count + RectIndex.Value - 1) % rectList.Count;
			});
			NextPageCommand.Subscribe(_ => {
				RectIndex.Value = (rectList.Count + RectIndex.Value + 1) % rectList.Count;
			});
			SelectPageCommand.Subscribe(_ => {
				dg(rectList[RectIndex.Value]);
				CloseWindow.Value = true;
			});
			CancelCommand.Subscribe(_ => {
				dg(null);
				CloseWindow.Value = true;
			});
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
