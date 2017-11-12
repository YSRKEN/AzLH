using Prism.Mvvm;
using System;
using System.Drawing;
using System.Linq;

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
				var rectList = await ScreenShotProvider.GetGameWindowPositionAsync();
				// 候補数によって処理を分岐させる
				switch (rectList.Count) {
				case 0: {
						// 候補なしと表示する
						ScreenShotProvider.GameWindowRect = null;
						PutLog("座標取得 : 失敗");
						SaveScreenshotFlg = true;
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
				ScreenShotProvider.GetScreenshot().Save($"pic\\{fileName}");
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
		// 定期的にスクリーンショットを取得し、そこに起因する処理を行う
		public void HelperTaskF() {
			if (!SaveScreenshotFlg)
				return;
			using (var screenShot = ScreenShotProvider.GetScreenshot()) {
				// スクショが取得できるとscreenShotがnullにならない
				if (screenShot != null) {
					JudgedScene = SceneRecognition.JudgeGameScene(screenShot);
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
			// ズレ検出・修正処理
			if (SaveScreenshotFlg) {
				if (!ScreenShotProvider.CanGetScreenshot()) {
					// スクショが取得できなくなったのでその旨を通知する
					PutLog("エラー：ゲーム画面の位置ズレを検出しました");
					try {
						// 再び自動座標検出を行う
						var rectList = await ScreenShotProvider.GetGameWindowPositionAsync();
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
									int diff = diffX * diffX + diffY * diffY;
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
		}
	}
}
