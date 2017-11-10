using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;

namespace AzLH.Models {
	internal class MainModel : BindableBase {
		public delegate void SelectGameWindowAction(Rectangle? rect);
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
		// 実行ログに追記する
		private void PutLog(string message) {
			ApplicationLog += $"{Utility.GetTimeStrShort()} {message}\n";
		}
		// 複数ウィンドウから選択した際の結果を処理する
		private void SelectGameWindow(Rectangle? rect) {
			if (rect == null) {
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
		public void GetGameWindowPosition() {
			try {
				// ゲーム画面の座標候補を検出する
				var rectList = ScreenShotProvider.GetGameWindowPosition();
				// 候補数によって処理を分岐させる
				switch (rectList.Count) {
				case 0: {
						// 候補なしと表示する
						ScreenShotProvider.GameWindowRect = null;
						PutLog("座標取得 : 失敗");
						SaveScreenshotFlg = true;
					}
					break;
				/*case 1: {
						// 即座にその候補で確定させる
						ScreenShotProvider.GameWindowRect = rectList[0];
						PutLog("座標取得 : 成功");
						PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
						SaveScreenshotFlg = true;
					}
					break;*/
				default: {
						// 選択画面を表示する
						var dg = new SelectGameWindowAction(SelectGameWindow);
						var vm = new ViewModels.GameScreenSelectViewModel(rectList, dg);
						var view = new Views.GameScreenSelectView { DataContext = vm };
						view.ShowDialog();
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
				ScreenShotProvider.GetScreenshot().Save($"pic\\{fileName}");
				PutLog($"スクリーンショット : 成功");
				PutLog($"ファイル名 : {fileName}");
			}
			catch (Exception) {
				PutLog($"スクリーンショット : 失敗");
			}
		}
		// そこから定期的な処理を実行する
		public void HelperTask() {
			// スクショが取得可能な際の処理
			if (SaveScreenshotFlg) {
				using(var screenShot = ScreenShotProvider.GetScreenshot()) {
					JudgedScene = SceneRecognition.JudgeGameScene(screenShot);
				}
			}
		}
	}
}
