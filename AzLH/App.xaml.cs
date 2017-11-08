using System.IO;
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
			// picフォルダが存在しない場合は作成する
			if (!Directory.Exists(@"pic\"))
				Directory.CreateDirectory(@"pic\");
		}
	}
}
