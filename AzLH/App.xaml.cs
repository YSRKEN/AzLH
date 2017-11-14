using AzLH.Models;
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
			SupplyStore.Initialize();
			var time = SupplyStore.LastWriteDateTime("燃料");
/*			int fuel = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "燃料");
			int money = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "資金");
			int diamond = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample.png"), "ダイヤ");
			int cube = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample2.png"), "キューブ");
			int drill = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample3.png"), "ドリル");
			int medal = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample4.png"), "勲章");
			int furniture = Models.CharacterRecognition.GetValueOCR((System.Drawing.Bitmap)System.Drawing.Image.FromFile("pic\\sample5.png"), "家具コイン");
*/
			return;
		}
	}
}
