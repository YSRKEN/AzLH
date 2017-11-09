using Prism.Mvvm;
using System.Windows.Media.Imaging;

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
		// 前の画像に移動
		public void PrevPage() {}
		// 次の画像に移動
		public void NextPage() { }
		// 決定ボタン
		public void SelectPage() { }
		// キャンセルボタン
		public void Cancel() { }
	}
}
