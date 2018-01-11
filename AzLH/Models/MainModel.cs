﻿using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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
				SettingsStore.ForTwitterFlg = forTwitterFlg;
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
				SettingsStore.MainWindowRect[0] = mainWindowPositionLeft;
				SettingsStore.ChangeSettingFlg = true;
			}
		}
		private double mainWindowPositionTop = double.NaN;
		public double MainWindowPositionTop {
			get => mainWindowPositionTop;
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionTop, value);
				SettingsStore.MainWindowRect[1] = mainWindowPositionTop;
				SettingsStore.ChangeSettingFlg = true;
			}
		}
		private double mainWindowPositionWidth = 400.0;
		public double MainWindowPositionWidth {
			get => mainWindowPositionWidth;
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionWidth, value);
				SettingsStore.MainWindowRect[2] = mainWindowPositionWidth;
				SettingsStore.ChangeSettingFlg = true;
			}
		}
		private double mainWindowPositionHeight = 300.0;
		public double MainWindowPositionHeight {
			get { return mainWindowPositionHeight; }
			set {
				if (!MemoryWindowPositionFlg)
					return;
				SetProperty(ref mainWindowPositionHeight, value);
				SettingsStore.MainWindowRect[3] = mainWindowPositionHeight;
				SettingsStore.ChangeSettingFlg = true;
			}
		}
		// ウィンドウの座標を記憶するか？
		private bool memoryWindowPositionFlg = false;
		public bool MemoryWindowPositionFlg {
			get => memoryWindowPositionFlg;
			set {
				SetProperty(ref memoryWindowPositionFlg, value);
				SettingsStore.MemoryWindowPositionFlg = memoryWindowPositionFlg;
			}
		}
		// 常時座標を捕捉し続けるか？
		private bool autoSearchPositionFlg = false;
		public bool AutoSearchPositionFlg {
			get => autoSearchPositionFlg;
			set {
				SetProperty(ref autoSearchPositionFlg, value);
				SettingsStore.AutoSearchPositionFlg = autoSearchPositionFlg;
			}
		}
		// 資材記録時にスクショでロギングするか？
		private bool autoSupplyScreenShotFlg = false;
		public bool AutoSupplyScreenShotFlg {
			get => autoSupplyScreenShotFlg;
			set {
				SetProperty(ref autoSupplyScreenShotFlg, value);
				SettingsStore.AutoSupplyScreenShotFlg = autoSupplyScreenShotFlg;
			}
		}
		// 資材記録時に画像処理結果を出力するか？
		private bool putCharacterRecognitionFlg = false;
		public bool PutCharacterRecognitionFlg {
			get => putCharacterRecognitionFlg;
			set {
				SetProperty(ref putCharacterRecognitionFlg, value);
				SettingsStore.PutCharacterRecognitionFlg = putCharacterRecognitionFlg;
			}
		}
		// ドラッグ＆ドロップでシーン認識するか？
		private bool dragAndDropPictureFlg = false;
		public bool DragAndDropPictureFlg {
			get => dragAndDropPictureFlg;
			set {
				SetProperty(ref dragAndDropPictureFlg, value);
				SettingsStore.DragAndDropPictureFlg = dragAndDropPictureFlg;
			}
		}

		// 1秒前のゲージ量
		double[] oldGauge = new double[] { -1.0, -1.0, -1.0 };
		double[] remainTime = new double[] { 0.0, 0.0, 0.0 };

		// コンストラクタ
		public MainModel() {
			SetSettings();
		}

		// 設定内容を画面に反映する
		private void SetSettings() {
			ForTwitterFlg = SettingsStore.ForTwitterFlg;
			MemoryWindowPositionFlg = SettingsStore.MemoryWindowPositionFlg;
			if (MemoryWindowPositionFlg) {
				MainWindowPositionLeft   = SettingsStore.MainWindowRect[0];
				MainWindowPositionTop    = SettingsStore.MainWindowRect[1];
				MainWindowPositionWidth  = SettingsStore.MainWindowRect[2];
				MainWindowPositionHeight = SettingsStore.MainWindowRect[3];
			}
			AutoSearchPositionFlg = SettingsStore.AutoSearchPositionFlg;
			AutoSupplyScreenShotFlg = SettingsStore.AutoSupplyScreenShotFlg;
			PutCharacterRecognitionFlg = SettingsStore.PutCharacterRecognitionFlg;
			DragAndDropPictureFlg = SettingsStore.DragAndDropPictureFlg;
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
		// 選択画面を表示する
		private void ShowSelectGameWindow(List<Rectangle> rectList) {
			var dg = new SelectGameWindowAction(SelectGameWindow);
			var vm = new ViewModels.GameScreenSelectViewModel(rectList, dg);
			var view = new Views.GameScreenSelectView { DataContext = vm };
			view.ShowDialog();
		}
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
								// http://blogs.wankuma.com/naka/archive/2009/02/12/168020.aspx
								var dispatcher = Application.Current.Dispatcher;
								if (dispatcher.CheckAccess()) {
									ShowSelectGameWindow(rectList);
								} else {
									dispatcher.Invoke(() => {
										ShowSelectGameWindow(rectList);
									});
								}
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
								// http://blogs.wankuma.com/naka/archive/2009/02/12/168020.aspx
								var dispatcher = Application.Current.Dispatcher;
								if (dispatcher.CheckAccess()) {
									ShowSelectGameWindow(rectList2);
								} else {
									dispatcher.Invoke(() => {
										ShowSelectGameWindow(rectList2);
									});
								}
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
				if (!SettingsStore.LoadSettings(ofd.FileName)) {
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
				if (!SettingsStore.SaveSettings(sfd.FileName)) {
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
			if (SettingsStore.ShowSupplyWindowFlg)
				return;
			var vm = new ViewModels.SupplyViewModel();
			var view = new Views.SupplyView { DataContext = vm };
			view.Show();
			SettingsStore.ShowSupplyWindowFlg = true;
		}
		// 各種タイマー画面を表示
		public void OpenTimerView() {
			if (SettingsStore.ShowTimerWindowFlg)
				return;
			var vm = new ViewModels.TimerViewModel();
			var view = new Views.TimerView { DataContext = vm };
			view.Show();
			SettingsStore.ShowTimerWindowFlg = true;
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
		// ソフトウェアが最新かを調べる
		public async void CheckSoftwareVer() {
			string nowSoftwareVer = Utility.SoftwareVer;
			string lastSoftwareVer = await Utility.NewestSoftwareVerAsync();
			PutLog("最新版チェック...");
			if (lastSoftwareVer == "") {
				PutLog("警告：バージョンチェックできませんでした");
			}
			else {
				if (nowSoftwareVer != lastSoftwareVer) {
					PutLog("警告：このソフトウェアは最新ではありません");
					var result = MessageBox.Show($"より新しいVerが存在するようです。\n\n　現在のVer.→{nowSoftwareVer}\n　最新のVer.→{lastSoftwareVer}\n\nダウンロードページを開きますか？", Utility.SoftwareName, MessageBoxButton.YesNo, MessageBoxImage.Information);
					if (result == MessageBoxResult.Yes) {
						System.Diagnostics.Process.Start(@"https://github.com/YSRKEN/AzLH/releases");
					}
				}
				else {
					PutLog("このソフトウェアは最新です");
				}
			}
		}

		// 定期的にスクリーンショットを取得し、そこに起因する処理を行う
		public void HelperTaskF() {
			if (!SaveScreenshotFlg)
				return;
			using(var screenShot = ScreenShotProvider.GetScreenshot()) {
				// スクショが取得できるとscreenShotがnullにならない
				if (screenShot != null) {
					// シーン文字列を取得し、表示する
					JudgedScene = SceneRecognition.JudgeGameScene(screenShot);
					// 資材量を取得する
					// (戦闘中なら各種ボムの分量と残り秒数を読み取る)
					switch (JudgedScene) {
					case "シーン判定 : 母港": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "燃料", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：燃料");
							if (SupplyStore.UpdateSupplyValue(screenShot, "資金", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：資金");
							if (SupplyStore.UpdateSupplyValue(screenShot, "ダイヤ", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：ダイヤ");
						}
						break;
					case "シーン判定 : 建造": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "キューブ", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：キューブ");
						}
						break;
					case "シーン判定 : 建造中": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "ドリル", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：ドリル");
						}
						break;
					case "シーン判定 : 支援": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "勲章", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：勲章");
						}
						break;
					case "シーン判定 : 家具屋": {
							if (SupplyStore.UpdateSupplyValue(screenShot, "家具コイン", AutoSupplyScreenShotFlg, PutCharacterRecognitionFlg))
								PutLog("資材量追記：家具コイン");
						}
						break;
					}
					// 戦闘中でなくなった場合、速やかにボムタイマーをリセットする
					if(JudgedScene != "シーン判定 : 戦闘中" && JudgedScene != "シーン判定 : 不明") {
						SettingsStore.BombChageTime1 = null;
						SettingsStore.BombChageTime2 = null;
						SettingsStore.BombChageTime3 = null;
					}
				} else {
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
					return;
				}
				// スクショから得られる情報を用いた修正
				using(var screenShot = ScreenShotProvider.GetScreenshot()) {
					switch (SceneRecognition.JudgeGameScene(screenShot)) {
					case "戦闘中": {
							// 読み取ったゲージから、フルチャージに必要な秒数を計算する
							var gauge = SceneRecognition.GetBattleBombGauge(screenShot);
							// 各種のゲージ毎に判定を行う
							//string output = "残りチャージ時間：";
							//var label = new string[] { "空撃", "雷撃", "砲撃" };
							for (int ti = 0; ti < SceneRecognition.GaugeTypeCount; ++ti) {
								if (gauge[ti] >= 0.0) {
									if (oldGauge[ti] >= 0.0) {
										// 前回のゲージ量が残っているので、チャージ完了に要する時間が計算できる
										// ただしゲージが変化していないようにみえる場合は無視する
										if (gauge[ti] > oldGauge[ti]) {
											remainTime[ti] = (1.0 - gauge[ti]) / (gauge[ti] - oldGauge[ti]);
											//
											switch (ti) {
											case 0:
												SettingsStore.BombChageTime1 = DateTime.Now.AddSeconds(remainTime[ti]);
												break;
											case 1:
												SettingsStore.BombChageTime2 = DateTime.Now.AddSeconds(remainTime[ti]);
												break;
											case 2:
												SettingsStore.BombChageTime3 = DateTime.Now.AddSeconds(remainTime[ti]);
												break;
											}
										} else {
											// 読み取り失敗した祭の処理
											if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
										}
									} else {
										// 読み取り失敗した祭の処理
										if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
									}
									// oldGaugeに今回読み取った量を上書きする
									oldGauge[ti] = gauge[ti];
								} else {
									// 読み取り失敗した祭の処理
									if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
									oldGauge[ti] = -1.0;
								}
							}
						}
						break;
					case "委託": {
							// 委託時間を読み取って反映させる
							for (int ci = 0; ci < SceneRecognition.ConsignCount; ++ci) {
								// 読み取り
								long remainTime = CharacterRecognition.GetTimeOCR(screenShot, $"委託{ci + 1}");
								// 委託完了時刻を逆算
								DateTime? finalTime = (remainTime > 0 ? DateTime.Now.AddSeconds(remainTime) : (DateTime?)null);
								// 書き込み処理
								switch (ci) {
								case 0:
									SettingsStore.ConsignFinalTime1 = finalTime;
									break;
								case 1:
									SettingsStore.ConsignFinalTime2 = finalTime;
									break;
								case 2:
									SettingsStore.ConsignFinalTime3 = finalTime;
									break;
								case 3:
									SettingsStore.ConsignFinalTime4 = finalTime;
									break;
								}
							}
						}
						break;
					case "戦術教室": {
							// 残り時間を読み取って反映させる
							for (int ci = 0; ci < SceneRecognition.LectureCount; ++ci) {
								// 読み取り
								long remainTime = CharacterRecognition.GetTimeOCR(screenShot, $"戦術教室{ci + 1}");
								// 完了時刻を逆算
								DateTime? finalTime = (remainTime > 0 ? DateTime.Now.AddSeconds(remainTime) : (DateTime?)null);
								// 書き込み処理
								switch (ci) {
								case 0:
									SettingsStore.LectureFinalTime1 = finalTime;
									break;
								case 1:
									SettingsStore.LectureFinalTime2 = finalTime;
									break;
								}
							}
						}
						break;
					case "寮舎": {
							// 残り時間を読み取って反映させる
							// 読み取り
							long remainTime = CharacterRecognition.GetTimeOCR(screenShot, "食糧");
							// 完了時刻を逆算
							DateTime? finalTime = (remainTime > 0 ? DateTime.Now.AddSeconds(remainTime) : (DateTime?)null);
							// 書き込み処理
							SettingsStore.FoodFinalTime = finalTime;
						}
						break;
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
