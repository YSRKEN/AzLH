using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace AzLH.ViewModels {
	class MainViewModel {
		// modelのinstance
		private MainModel mainModel;
		// 画像保存ボタンは有効か？
		public ReactiveProperty<bool> SaveScreenshotFlg { get; }
		// 実行ログ
		public ReactiveProperty<string> ApplicationLog { get; }
		// 座標取得ボタン
		public ReactiveCommand GetGameWindowPositionCommand { get; }
		// 画像保存ボタン
		public ReactiveCommand SaveScreenshotCommand { get; }
		// コンストラクタ
		public MainViewModel() {
			// 初期化
			mainModel = new MainModel();
			// プロパティを設定
			SaveScreenshotFlg = mainModel.ObserveProperty(x => x.SaveScreenshotFlg).ToReactiveProperty();
			ApplicationLog = mainModel.ObserveProperty(x => x.ApplicationLog).ToReactiveProperty();
			// コマンドを設定
			GetGameWindowPositionCommand = new ReactiveCommand();
			SaveScreenshotCommand = new ReactiveCommand();
			//voidを返すメソッドならこれだけで良いらしい
			//https://qiita.com/pierusan2010/items/76b7a406b3f064193c88
			GetGameWindowPositionCommand.Subscribe(mainModel.GetGameWindowPosition);
			SaveScreenshotCommand.Subscribe(mainModel.SaveScreenshot);
		}
	}
}
