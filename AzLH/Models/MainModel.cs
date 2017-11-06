using Prism.Mvvm;
using System.Windows;

namespace AzLH.Models {
	class MainModel : BindableBase {
		// コンストラクタ
		public MainModel() {}
		// サンプルコマンド
		public void Test() {
			if (true) {
				var sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				var rectList = ScreenShotProvider.GetGameWindowPosition();
				sw.Stop();
				string output = $"{sw.ElapsedMilliseconds}[ms]\n";
				var bitmap = ScreenShotProvider.GetScreenBitmap();
				bitmap.Save("hoge-1.png");
				using (var bitmapGraphics = System.Drawing.Graphics.FromImage(bitmap)) {
					foreach (var rect in rectList) {
						bitmapGraphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Blue, 10.0f), rect);
						output += $"({rect.X},{rect.Y}) - {rect.Width}x{rect.Height}\n";
					}
				}
				MessageBox.Show(output);
				bitmap.Save("hoge-2.png");
			}
			else {
				int count = 10;
				var sw = new System.Diagnostics.Stopwatch();
				// 呼び出すオーバーヘッドが18[ms/回]程度あることに注意
				sw.Start();
				for (int i = 0; i < count; ++i) {
					var rectList = ScreenShotProvider.GetGameWindowPosition(new System.Drawing.Bitmap("benchmark2.png"));
				}
				sw.Stop();
				System.GC.Collect();
				string output = $"{1.0 * sw.ElapsedMilliseconds / count}[ms]\n";
				MessageBox.Show(output);
			}
		}
	}
}
