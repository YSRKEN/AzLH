using Prism.Mvvm;
using System.Windows;

namespace AzLH.Models {
	class MainModel : BindableBase {
		// コンストラクタ
		public MainModel() {}
		// サンプルコマンド
		public void Test() {
			MessageBox.Show("Test");
		}
	}
}
