using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace AzLH.Models {
	internal class MainModel : BindableBase {
		public delegate void SelectGameWindowAction(Rectangle? rect);
		// trueにすると画面を閉じる
		private bool closeWindow;
		public bool CloseWindow {
			get { return closeWindow; }
			set { SetProperty(ref closeWindow, value); }
		}
		// 画像保存ボタンは有効か？
		private bool saveScreenshotFlg = false;
		public bool SaveScreenshotFlg {
			get { return saveScreenshotFlg; }
			set { SetProperty(ref saveScreenshotFlg, value); }
		}
		// 実行ログ
		private string applicationLog = "";
		public string ApplicationLog {
			get { return applicationLog; }
			set { SetProperty(ref applicationLog, value); }
		}
		// シーン表示
		private string judgedScene = "不明";
		public string JudgedScene {
			get { return $"シーン判定 : {judgedScene}"; }
			set { SetProperty(ref judgedScene, value); }
		}
		// Twitter用に加工するか？
		private bool forTwitterFlg = false;
		public bool ForTwitterFlg {
			get => forTwitterFlg;
			set {
				SetProperty(ref forTwitterFlg, value);
				var settings = SettingsStore.Instance;
				settings.ForTwitterFlg = forTwitterFlg;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		// ソフトウェアのタイトル
		private string softwareTitle = $"{Utility.SoftwareName} Ver.{Utility.SoftwareVer}";
		public string SoftwareTitle {
			get { return softwareTitle; }
			set { SetProperty(ref softwareTitle, value); }
		}
		// メイン画面の位置
		private double mainWindowPositionLeft = double.NaN;
		public double MainWindowPositionLeft {
			get => mainWindowPositionLeft;
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionLeft, value);
				var settings = SettingsStore.Instance;
				settings.MainWindowRect[0] = mainWindowPositionLeft;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double mainWindowPositionTop = double.NaN;
		public double MainWindowPositionTop {
			get => mainWindowPositionTop;
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionTop, value);
				var settings = SettingsStore.Instance;
				settings.MainWindowRect[1] = mainWindowPositionTop;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double mainWindowPositionWidth = 400.0;
		public double MainWindowPositionWidth {
			get => mainWindowPositionWidth;
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionWidth, value);
				var settings = SettingsStore.Instance;
				settings.MainWindowRect[2] = mainWindowPositionWidth;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		private double mainWindowPositionHeight = 300.0;
		public double MainWindowPositionHeight {
			get { return mainWindowPositionHeight; }
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionHeight, value);
				var settings = SettingsStore.Instance;
				settings.MainWindowRect[3] = mainWindowPositionHeight;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		// ウィンドウの座標を記憶するか？
		private bool memoryWindowPositionFlg = false;
		public bool MemoryWindowPositionFlg {
			get => memoryWindowPositionFlg;
			set {
				SetProperty(ref memoryWindowPositionFlg, value);
				var settings = SettingsStore.Instance;
				settings.MemoryWindowPositionFlg = memoryWindowPositionFlg;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}
		// 常時座標を捕捉し続けるか？
		private bool autoSearchPositionFlg = false;
		public bool AutoSearchPositionFlg {
			get => autoSearchPositionFlg;
			set {
				SetProperty(ref autoSearchPositionFlg, value);
				var settings = SettingsStore.Instance;
				settings.AutoSearchPositionFlg = autoSearchPositionFlg;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
		}

		// コンストラクタ
		public MainModel() {
			SetSettings();
		}

		// 設定内容を画面に反映する
		private void SetSettings() {
			var settings = SettingsStore.Instance;
			ForTwitterFlg = settings.ForTwitterFlg;
			MemoryWindowPositionFlg = settings.MemoryWindowPositionFlg;
			if (MemoryWindowPositionFlg) {
				MainWindowPositionLeft   = settings.MainWindowRect[0];
				MainWindowPositionTop    = settings.MainWindowRect[1];
				MainWindowPositionWidth  = settings.MainWindowRect[2];
				MainWindowPositionHeight = settings.MainWindowRect[3];
			}
			AutoSearchPositionFlg = settings.AutoSearchPositionFlg;
		}
		// 実行ログに追記する
		private void PutLog(string message) {
			ApplicationLog += $"{Utility.GetTimeStrShort()} {message}\n";
		}
		// 複数ウィンドウから選択した際の結果を処理する
		private void SelectGameWindow(Rectangle? rect) {
			if (rect == null) {
				ScreenShotProvider.GameWindowRect = null;
				PutLog("座標取得 : 失敗");
				SaveScreenshotFlg = false;
			}
			else {
				ScreenShotProvider.GameWindowRect = rect;
				PutLog("座標取得 : 成功");
				PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
				SaveScreenshotFlg = true;
			}
		}
		// サンプルコマンド
		/*public void Test() {
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
		}*/
		// ゲーム画面の座標を取得する
		public async void GetGameWindowPosition() {
			PutLog("座標取得開始...");
			try {
				// ゲーム画面の座標候補を検出する
				var rectList = await ScreenShotProvider.GetGameWindowPositionAsync().ConfigureAwait(false);
				// 候補数によって処理を分岐させる
				switch (rectList.Count) {
				case 0: {
						// 候補なしと表示する
						ScreenShotProvider.GameWindowRect = null;
						PutLog("座標取得 : 失敗");
						SaveScreenshotFlg = false;
					}
					break;
				case 1: {
						// 即座にその候補で確定させる
						ScreenShotProvider.GameWindowRect = rectList[0];
						PutLog("座標取得 : 成功");
						PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
						SaveScreenshotFlg = true;
					}
					break;
				default: {
						// 各座標についてシーン認識を行い、認識可能だった座標が
						// 0個→従来通りの選択画面を表示する
						// 1個→その結果で確定させる
						// 2個以上→認識可能だった座標だけで選択画面を表示する
						var rectList2 = rectList.Where(rect => {
							var bitmap = ScreenShotProvider.GetScreenBitmap(rect);
							string scene = SceneRecognition.JudgeGameScene(bitmap);
							return (scene != "不明");
						}).ToList();
						switch (rectList2.Count) {
						case 0: {
								// 選択画面を表示する
								var dg = new SelectGameWindowAction(SelectGameWindow);
								var vm = new ViewModels.GameScreenSelectViewModel(rectList, dg);
								var view = new Views.GameScreenSelectView { DataContext = vm };
								view.ShowDialog();
							}
							break;
						case 1: {
								// 即座にその候補で確定させる
								ScreenShotProvider.GameWindowRect = rectList2[0];
								PutLog("座標取得 : 成功");
								PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
								SaveScreenshotFlg = true;
							}
							break;
						default: {
								// 選択画面を表示する
								var dg = new SelectGameWindowAction(SelectGameWindow);
								var vm = new ViewModels.GameScreenSelectViewModel(rectList2, dg);
								var view = new Views.GameScreenSelectView { DataContext = vm };
								view.ShowDialog();
							}
							break;
						}
					}
					break;
				}
			}
			catch (Exception) {
				PutLog($"座標取得 : 失敗");
			}
		}
		// ゲーム画面のスクリーンショットを保存する
		public void SaveScreenshot() {
			try {
				string fileName = $"{Utility.GetTimeStrLong()}.png";
				ScreenShotProvider.GetScreenshot(ForTwitterFlg).Save($"pic\\{fileName}");
				PutLog($"スクリーンショット : 成功");
				PutLog($"ファイル名 : {fileName}");
			}
			catch (Exception) {
				PutLog($"スクリーンショット : 失敗");
			}
		}
		// ソフトを終了させる
		public void Close() {
			CloseWindow = true;
		}
		// ソフトの情報を返す
		// 参考→http://dobon.net/vb/dotnet/file/myversioninfo.html
		public string GetSoftwareInfo() {
			var assembly = Assembly.GetExecutingAssembly();
			// AssemblyTitle
			string asmttl = ((AssemblyTitleAttribute)
				Attribute.GetCustomAttribute(assembly,
				typeof(AssemblyTitleAttribute))).Title;
			// AssemblyCopyright
			string asmcpy = ((AssemblyCopyrightAttribute)
				Attribute.GetCustomAttribute(assembly,
				typeof(AssemblyCopyrightAttribute))).Copyright;
			// AssemblyProduct
			string asmprd = ((AssemblyProductAttribute)
				Attribute.GetCustomAttribute(assembly,
				typeof(AssemblyProductAttribute))).Product;
			// AssemblyVersion
			var asmver = assembly.GetName().Version;
			return $"{asmttl} Ver.{asmver}\n{asmcpy}\n{asmprd}";
		}
		// 設定をインポート
		public void ImportSettings() {
			// インスタンスを作成
			var ofd = new OpenFileDialog {
				// ファイルの種類を設定
				Filter = "設定ファイル(*.json)|*.json|全てのファイル (*.*)|*.*"
			};
			// ダイアログを表示
			if ((bool)ofd.ShowDialog()) {
				// 設定をインポート
				var settings = SettingsStore.Instance;
				if (!settings.LoadSettings(ofd.FileName)) {
					PutLog("エラー：設定を読み込めませんでした");
				}
				else {
					PutLog("設定を読み込みました");
				}
				SetSettings();
			}
		}
		// 設定をエクスポート
		public void ExportSettings() {
			// インスタンスを作成
			var sfd = new SaveFileDialog {
				// ファイルの種類を設定
				Filter = "設定ファイル(*.json)|*.json|全てのファイル (*.*)|*.*"
			};
			// ダイアログを表示
			if ((bool)sfd.ShowDialog()) {
				// 設定をエクスポート
				var settings = SettingsStore.Instance;
				if (!settings.SaveSettings(sfd.FileName)) {
					PutLog("エラー：設定を保存できませんでした");
				}
				else {
					PutLog("設定を保存しました");
				}
			}
		}
		// スクショ保存フォルダを表示
		public void OpenPicFolder() {
			System.Diagnostics.Process.Start(@"pic\");
		}
		// 資材記録画面を表示
		public void OpenSupplyView() {
			var settings = SettingsStore.Instance;
			if (settings.ShowSupplyWindowFlg)
				return;
			var vm = new ViewModels.SupplyViewModel();
			var view = new Views.SupplyView { DataContext = vm };
			view.Show();
			settings.ShowSupplyWindowFlg = true;
		}
		// 資材のインポート機能(燃料・資金・ダイヤ)
		public async void ImportMainSupply() {
			// インスタンスを作成
			var ofd = new OpenFileDialog {
				// ファイルの種類を設定
				Filter = "設定ファイル(*.csv)|*.csv|全てのファイル (*.*)|*.*",
				FileName = "MainSupply.csv"
			};
			// ダイアログを表示
			if ((bool)ofd.ShowDialog()) {
				// 資材をインポート
				PutLog("資材データを読み込み開始...");
				if (await SupplyStore.ImportOldMainSupplyDataAsync(ofd.FileName)) {
					PutLog("資材データを読み込みました");
				}
				else {
					PutLog("エラー：資材データを読み込めませんでした");
				}
			}
		}
		// 資材のインポート機能(index=0～3、0から順にキューブ・ドリル・勲章・家具コイン)
		public async void ImportSubSupply(int index) {
			// インスタンスを作成
			var ofd = new OpenFileDialog {
				// ファイルの種類を設定
				Filter = "設定ファイル(*.csv)|*.csv|全てのファイル (*.*)|*.*",
				FileName = $"SubSupply{(index + 1)}.csv"
			};
			// ダイアログを表示
			if ((bool)ofd.ShowDialog()) {
				// 資材をインポート
				PutLog("資材データを読み込み開始...");
				if (await SupplyStore.ImportOldSubSupplyDataAsync(ofd.FileName, index)) {
					PutLog("資材データを読み込みました");
				}
				else {
					PutLog("エラー：資材データを読み込めませんでした");
				}
			}
		}
		public void ImportSubSupply1() {
			ImportSubSupply(0);
		}
		public void ImportSubSupply2() {
			ImportSubSupply(1);
		}
		public void ImportSubSupply3() {
			ImportSubSupply(2);
		}
		public void ImportSubSupply4() {
			ImportSubSupply(3);
		}

		// 定期的にスクリーンショットを取得し、そこに起因する処理を行う
		public void HelperTaskF() {
			if (!SaveScreenshotFlg)
				return;
			using (var screenShot = ScreenShotProvider.GetScreenshot()) {
				// スクショが取得できるとscreenShotがnullにならない
				if (screenShot != null) {
					// シーン文字列を取得し、表示する
					JudgedScene = SceneRecognition.JudgeGameScene(screenShot);
					// 資材量を取得する
					switch (JudgedScene) {
					case "シーン判定 : 母港": {
							if(SupplyStore.UpdateSupplyValue(screenShot, "燃料"))
								PutLog("資材量追記：燃料");
							if (SupplyStore.UpdateSupplyValue(screenShot, "資金"))
								PutLog("資材量追記：資金");
							if (SupplyStore.UpdateSupplyValue(screenShot, "ダイヤ"))
								PutLog("資材量追記：ダイヤ");
						}
						break;
					case "シーン判定 : 建造": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "キューブ"))
								PutLog("資材量追記：キューブ");
						}
						break;
					case "シーン判定 : 建造中": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "ドリル"))
								PutLog("資材量追記：ドリル");
						}
						break;
					case "シーン判定 : 支援": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "勲章"))
								PutLog("資材量追記：勲章");
						}
						break;
					case "シーン判定 : 家具屋": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "家具コイン"))
								PutLog("資材量追記：家具コイン");
						}
						break;
					}
				}
				else {
					// スクショが取得できなくなったのでその旨を通知する
					PutLog("エラー：スクショが取得できなくなりました");
					SaveScreenshotFlg = false;
				}
			}
		}
		// 毎秒ごとの処理を行う
		public async void HelperTaskS() {
			if (SaveScreenshotFlg) {
				// ズレ検出・修正処理
				if (!ScreenShotProvider.CanGetScreenshot()) {
					// スクショが取得できなくなったのでその旨を通知する
					PutLog("エラー：ゲーム画面の位置ズレを検出しました");
					try {
						// 再び自動座標検出を行う
						var rectList = await ScreenShotProvider.GetGameWindowPositionAsync().ConfigureAwait(false);
						// その結果によって処理を分岐させる
						switch (rectList.Count) {
						case 0: {
								// 候補なしと表示する
								ScreenShotProvider.GameWindowRect = null;
								PutLog($"位置ズレ自動修正 : 失敗");
								SaveScreenshotFlg = false;
							}
							break;
						case 1: {
								// 即座にその候補で確定させる
								ScreenShotProvider.GameWindowRect = rectList[0];
								PutLog("位置ズレ自動修正 : 成功");
								PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
								SaveScreenshotFlg = true;
							}
							break;
						default: {
								// 元の座標に最も近いものに合わせる
								int distance = int.MaxValue;
								Rectangle? nearestRect = null;
								foreach(var rect in rectList) {
									int diffX = ScreenShotProvider.GameWindowRect.Value.X - rect.X;
									int diffY = ScreenShotProvider.GameWindowRect.Value.Y - rect.Y;
									int diff = (diffX * diffX) + (diffY * diffY);
									if(distance > diff) {
										distance = diff;
										nearestRect = rect;
									}
								}
								ScreenShotProvider.GameWindowRect = nearestRect;
								PutLog("位置ズレ自動修正 : 成功");
								PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
								SaveScreenshotFlg = true;
							}
							break;
						}
					}
					catch {
						ScreenShotProvider.GameWindowRect = null;
						PutLog($"位置ズレ自動修正 : 失敗");
						SaveScreenshotFlg = false;
					}
				}
			}
			else {
				// 常時座標認識
				if (AutoSearchPositionFlg) {
					GetGameWindowPosition();
				}
			}
		}
	}
}
