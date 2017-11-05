using Prism.Mvvm;
using System.Windows;

namespace AzLH.Models {
	class MainModel : BindableBase {
		// コンストラクタ
		public MainModel() {}
		// サンプルコマンド
		public void Test() {
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			var rectList = ScreenShotProvider.GetGameWindowPosition();
			sw.Stop();
			string output = $"{sw.ElapsedMilliseconds}[ms]\n";
			foreach(var rect in rectList) {
				output += $"({rect.X},{rect.Y}) - {rect.Width}x{rect.Height}\n";
			}
			MessageBox.Show(output);
		}
	}
}
