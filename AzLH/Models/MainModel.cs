using Prism.Mvvm;
using System.Windows;

namespace AzLH.Models {
	class MainModel : BindableBase {
		// コンストラクタ
		public MainModel() {}
		// サンプルコマンド
		public void Test() {
			if (false) {
				var sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				var rectList = ScreenShotProvider.GetGameWindowPosition();
				sw.Stop();
				string output = $"{sw.ElapsedMilliseconds}[ms]\n";
				foreach (var rect in rectList) {
					output += $"({rect.X},{rect.Y}) - {rect.Width}x{rect.Height}\n";
				}
				MessageBox.Show(output);
			}
			else {
				int count = 10;
				var sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				for (int i = 0; i < count; ++i) {
					var rectList = ScreenShotProvider.GetGameWindowPosition(new System.Drawing.Bitmap("hoge.png"));
				}
				sw.Stop();
				System.GC.Collect();
				string output = $"{1.0 * sw.ElapsedMilliseconds / count}[ms]\n";
				MessageBox.Show(output);
			}
		}
	}
}
