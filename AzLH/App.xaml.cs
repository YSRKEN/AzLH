using System.Windows;

namespace AzLH {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			// アプリの起動
			var bootstrapper = new Bootstrapper();
			bootstrapper.Run();

			// デバッグ用コード
			int fuel = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "燃料", true);
			int money = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "資金", true);
			int diamond = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "ダイヤ", true);
			return;
		}
	}
}
