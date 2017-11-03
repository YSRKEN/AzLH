using AzLH.Models;
using Reactive.Bindings;

namespace AzLH.ViewModels {
	class MainViewModel {
		// modelのinstance
		private MainModel mainModel;
		// 最適化ボタン
		public ReactiveCommand TestCommand { get; }
		// コンストラクタ
		public MainViewModel() {
			// 初期化
			mainModel = new MainModel();

			// ボタン操作を設定
			TestCommand = new ReactiveCommand();
			//voidを返すメソッドならこれだけで良いらしい
			//https://qiita.com/pierusan2010/items/76b7a406b3f064193c88
			TestCommand.Subscribe(mainModel.Test);
		}
	}
}
