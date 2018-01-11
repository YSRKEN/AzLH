using AzLH.Models;
using Prism.Events;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Timers;
using System.Windows;

namespace AzLH.ViewModels {
	internal class MainViewModel {
		// modelのinstance
		private readonly MainModel mainModel;
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; }
		// 画像保存ボタンは有効か？
		public ReactiveProperty<bool> SaveScreenshotFlg { get; }
		// 実行ログ
		public ReactiveProperty<string> ApplicationLog { get; }
		// シーン表示
		public ReactiveProperty<string> JudgedScene { get; }
		// Twitter用に加工するか？
		public ReactiveProperty<bool> ForTwitterFlg { get; }
		// ソフトウェアのタイトル
		public ReactiveProperty<string> SoftwareTitle { get; }
		// メイン画面の位置
		public ReactiveProperty<double> MainWindowPositionLeft   { get; }
		public ReactiveProperty<double> MainWindowPositionTop    { get; }
		public ReactiveProperty<double> MainWindowPositionWidth  { get; }
		public ReactiveProperty<double> MainWindowPositionHeight { get; }
		// ウィンドウの座標を記憶するか？
		public ReactiveProperty<bool> MemoryWindowPositionFlg { get; }
		// 常時座標を捕捉し続けるか？
		public ReactiveProperty<bool> AutoSearchPositionFlg { get; }
		// 資材記録時にスクショでロギングするか？
		public ReactiveProperty<bool> AutoSupplyScreenShotFlg { get; }
		// 資材記録時に画像処理結果を出力するか？
		public ReactiveProperty<bool> PutCharacterRecognitionFlg { get; }
		// ドラッグ＆ドロップでシーン認識するか？
		public ReactiveProperty<bool> DragAndDropPictureFlg { get; }

		// 座標取得ボタン
		public ReactiveCommand GetGameWindowPositionCommand { get; }
		// 画像保存ボタン
		public ReactiveCommand SaveScreenshotCommand { get; }
		// 終了操作
		public ReactiveCommand CloseCommand { get; }
		// ソフトの情報を表示
		public ReactiveCommand SoftwareInfoCommand { get; }
		// 設定をインポート
		public ReactiveCommand ImportSettingsCommand { get; }
		// 設定をエクスポート
		public ReactiveCommand ExportSettingsCommand { get; }
		// スクショ保存フォルダを表示
		public ReactiveCommand OpenPicFolderCommand { get; }
		// 資材記録画面を表示
		public ReactiveCommand OpenSupplyViewCommand { get; }
		// 各種タイマー画面を表示
		public ReactiveCommand OpenTimerViewCommand { get; }
		// 資材のインポート機能
		public ReactiveCommand ImportMainSupplyCommand { get; }
		public ReactiveCommand ImportSubSupply1Command { get; }
		public ReactiveCommand ImportSubSupply2Command { get; }
		public ReactiveCommand ImportSubSupply3Command { get; }
		public ReactiveCommand ImportSubSupply4Command { get; }

		// コンストラクタ
		public MainViewModel() {
			// picフォルダが存在しない場合は作成する
			if (!Directory.Exists(@"pic\"))
				Directory.CreateDirectory(@"pic\");
			// debugフォルダが存在しない場合は作成する
			if (!Directory.Exists(@"debug\"))
				Directory.CreateDirectory(@"debug\");
			// 設定項目を初期化する
			if (!SettingsStore.Initialize()) {
				MessageBox.Show("設定を読み込めませんでした。\nデフォルトの設定で起動します。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				if (!SettingsStore.SaveSettings()) {
					MessageBox.Show("デフォルト設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
			// 資材データベースを初期化する
			SupplyStore.Initialize();

			// 初期化
			mainModel = new MainModel();
			// プロパティを設定
			CloseWindow = mainModel.ObserveProperty(x => x.CloseWindow).ToReactiveProperty();
			SaveScreenshotFlg = mainModel.ObserveProperty(x => x.SaveScreenshotFlg).ToReactiveProperty();
			ApplicationLog = mainModel.ObserveProperty(x => x.ApplicationLog).ToReactiveProperty();
			JudgedScene = mainModel.ObserveProperty(x => x.JudgedScene).ToReactiveProperty();
			ForTwitterFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.ForTwitterFlg);
			SoftwareTitle = mainModel.ObserveProperty(x => x.SoftwareTitle).ToReactiveProperty();
			MainWindowPositionLeft   = mainModel.ToReactivePropertyAsSynchronized(x => x.MainWindowPositionLeft  );
			MainWindowPositionTop    = mainModel.ToReactivePropertyAsSynchronized(x => x.MainWindowPositionTop   );
			MainWindowPositionWidth  = mainModel.ToReactivePropertyAsSynchronized(x => x.MainWindowPositionWidth );
			MainWindowPositionHeight = mainModel.ToReactivePropertyAsSynchronized(x => x.MainWindowPositionHeight);
			MemoryWindowPositionFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.MemoryWindowPositionFlg);
			AutoSearchPositionFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.AutoSearchPositionFlg);
			AutoSupplyScreenShotFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.AutoSupplyScreenShotFlg);
			PutCharacterRecognitionFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.PutCharacterRecognitionFlg);
			DragAndDropPictureFlg = mainModel.ToReactivePropertyAsSynchronized(x => x.DragAndDropPictureFlg);
			// コマンドを設定
			GetGameWindowPositionCommand = new ReactiveCommand();
			SaveScreenshotCommand = new ReactiveCommand();
			CloseCommand = new ReactiveCommand();
			SoftwareInfoCommand = new ReactiveCommand();
			ImportSettingsCommand = new ReactiveCommand();
			ExportSettingsCommand = new ReactiveCommand();
			OpenPicFolderCommand = new ReactiveCommand();
			OpenSupplyViewCommand = new ReactiveCommand();
			OpenTimerViewCommand = new ReactiveCommand();
			ImportMainSupplyCommand = new ReactiveCommand();
			ImportSubSupply1Command = new ReactiveCommand();
			ImportSubSupply2Command = new ReactiveCommand();
			ImportSubSupply3Command = new ReactiveCommand();
			ImportSubSupply4Command = new ReactiveCommand();
			//voidを返すメソッドならこれだけで良いらしい
			//https://qiita.com/pierusan2010/items/76b7a406b3f064193c88
			GetGameWindowPositionCommand.Subscribe(mainModel.GetGameWindowPosition);
			SaveScreenshotCommand.Subscribe(mainModel.SaveScreenshot);
			CloseCommand.Subscribe(mainModel.Close);
			ImportSettingsCommand.Subscribe(mainModel.ImportSettings);
			ExportSettingsCommand.Subscribe(mainModel.ExportSettings);
			OpenPicFolderCommand.Subscribe(mainModel.OpenPicFolder);
			OpenSupplyViewCommand.Subscribe(mainModel.OpenSupplyView);
			OpenTimerViewCommand.Subscribe(mainModel.OpenTimerView);
			ImportMainSupplyCommand.Subscribe(mainModel.ImportMainSupply);
			ImportSubSupply1Command.Subscribe(mainModel.ImportSubSupply1);
			ImportSubSupply2Command.Subscribe(mainModel.ImportSubSupply2);
			ImportSubSupply3Command.Subscribe(mainModel.ImportSubSupply3);
			ImportSubSupply4Command.Subscribe(mainModel.ImportSubSupply4);
			// 値を返すメソッドなのでView側でも対応する
			SoftwareInfoCommand.Subscribe(
				() => Messenger.Instance.GetEvent<PubSubEvent<string>>()
				.Publish(mainModel.GetSoftwareInfo()));

			// タイマーを初期化し、定時タスクを登録して実行する
			// http://takachan.hatenablog.com/entry/2017/09/09/225342
			var timer = new Timer(200);
			timer.Elapsed += (sender, e) => {
				try{
					timer.Stop();
					mainModel.HelperTaskF();
				}
				finally{timer.Start();}
			};
			timer.Start();
			var timer2 = new Timer(1000);
			timer2.Elapsed += (sender, e) => {
				try {
					timer2.Stop();
					mainModel.HelperTaskS();
				}
				finally { timer2.Start(); }
			};
			timer2.Start();

			// ウィンドウ表示関係
			if (SettingsStore.AutoSupplyWindowFlg) {
				OpenSupplyViewCommand.Execute();
			}
			if (SettingsStore.AutoTimerWindowFlg) {
				OpenTimerViewCommand.Execute();
			}

			// 最新版をチェックする
			mainModel.CheckSoftwareVer();
		}
	}
}
