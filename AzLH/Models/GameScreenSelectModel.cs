using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;
using static AzLH.Models.MainModel;

namespace AzLH.Models {
	class GameScreenSelectModel : BindableBase {
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
			PageInfoStr = $"{rectIndex + 1} / {rectList.Count} {Utility.GetRectStr(rectList[rectIndex])}";
		}
		// コンストラクタ
		public GameScreenSelectModel(List<Rectangle> rectList, SelectGameWindowAction dg) {
			this.rectList = rectList;
			this.dg = dg;
			rectIndex = 0;
		}
		// 前の画像に移動
		public void PrevPage() {
			rectIndex = Math.Max(rectIndex - 1, 0);
			RedrawPage();
		}
		// 次の画像に移動
		public void NextPage() {
			rectIndex = Math.Min(rectIndex + 1, rectList.Count - 1);
			RedrawPage();
		}
		// 決定ボタン
		public void SelectPage() { dg(rectList[rectIndex]); }
		// キャンセルボタン
		public void Cancel() { dg(null); }
	}
}
