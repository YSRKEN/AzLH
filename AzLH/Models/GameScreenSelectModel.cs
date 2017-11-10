using Prism.Mvvm;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using static AzLH.Models.MainModel;

namespace AzLH.Models {
	class GameScreenSelectModel : BindableBase {
		// trueにすると画面を閉じる
		private bool closeWindow;
		public bool CloseWindow {
			get { return closeWindow; }
			set { SetProperty(ref closeWindow, value); }
		}
		// ページ情報を表示
		private string pageInfoStr = "";
		public string PageInfoStr {
			get => pageInfoStr;
			set { SetProperty(ref pageInfoStr, value); }
		}
		// 選択しているrectに基づくスクショのプレビュー
		private BitmapSource gameWindowPage = null;
		public BitmapSource GameWindowPage {
			get => gameWindowPage;
			set { SetProperty(ref gameWindowPage, value); }
		}
		// rect一覧
		private List<Rectangle> rectList;
		// 選択しているrectのindex
		private int rectIndex;
		// 選択結果を返すdelegate
		private SelectGameWindowAction dg;

		// プレビューを書き換える
		private void RedrawPage() {
			PageInfoStr = $"[{rectIndex + 1}/{rectList.Count}] {Utility.GetRectStr(rectList[rectIndex])}";
			GameWindowPage = (BitmapSource)ScreenShotProvider.GetScreenBitmap(rectList[rectIndex]).ToImageSource();
		}
		// コンストラクタ
		public GameScreenSelectModel(List<Rectangle> rectList, SelectGameWindowAction dg) {
			this.rectList = rectList;
			this.dg = dg;
			rectIndex = 0;
			RedrawPage();
		}
		// 前の画像に移動
		public void PrevPage() {
			rectIndex = (rectList.Count + rectIndex - 1) % rectList.Count;
			RedrawPage();
		}
		// 次の画像に移動
		public void NextPage() {
			rectIndex = (rectList.Count + rectIndex + 1) % rectList.Count;
			RedrawPage();
		}
		// 決定ボタン
		public void SelectPage() {
			dg(rectList[rectIndex]);
			CloseWindow = true;
		}
		// キャンセルボタン
		public void Cancel() {
			dg(null);
			CloseWindow = true;
		}
	}
}
