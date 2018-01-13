using AzLH.Models;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace AzLH.ViewModels {
	internal class MainViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public event PropertyChangedEventHandler PropertyChanged;
		public delegate void SelectGameWindowAction(Rectangle? rect);
		// 各種ボムにおける1秒前のゲージ量・残時間
		private double[] oldGauge = new double[] { -1.0, -1.0, -1.0 };
		private double[] remainTime = new double[] { 0.0, 0.0, 0.0 };

		#region 各種ReactiveProperty
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 画像保存ボタンは有効か？
		public ReactiveProperty<bool> SaveScreenshotFlg { get; } = new ReactiveProperty<bool>(false);
		// 実行ログ
		public ReactiveProperty<string> ApplicationLog { get; } = new ReactiveProperty<string>("");
		// シーン表示
		public ReactiveProperty<string> JudgedScene { get; } = new ReactiveProperty<string>("");
		// Twitter用に加工するか？
		public ReactiveProperty<bool> ForTwitterFlg { get; } = new ReactiveProperty<bool>(false);
		// ソフトウェアのタイトル
		public ReactiveProperty<string> SoftwareTitle { get; }
			= new ReactiveProperty<string>($"{Utility.SoftwareName} Ver.{Utility.SoftwareVer}");
		// メイン画面の位置
		public ReactiveProperty<double> MainWindowPositionLeft { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> MainWindowPositionTop { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> MainWindowPositionWidth { get; } = new ReactiveProperty<double>(400.0);
		public ReactiveProperty<double> MainWindowPositionHeight { get; } = new ReactiveProperty<double>(300.0);
		// ウィンドウの座標を記憶するか？
		public ReactiveProperty<bool> MemoryWindowPositionFlg { get; } = new ReactiveProperty<bool>(true);
		// 常時座標を捕捉し続けるか？
		public ReactiveProperty<bool> AutoSearchPositionFlg { get; } = new ReactiveProperty<bool>(true);
		// 資材記録時にスクショでロギングするか？
		public ReactiveProperty<bool> AutoSupplyScreenShotFlg { get; } = new ReactiveProperty<bool>(false);
		// 資材記録時に画像処理結果を出力するか？
		public ReactiveProperty<bool> PutCharacterRecognitionFlg { get; } = new ReactiveProperty<bool>(false);
		// ドラッグ＆ドロップでシーン認識するか？
		public ReactiveProperty<bool> DragAndDropPictureFlg { get; } = new ReactiveProperty<bool>(false);
		#endregion

		#region 各種ReactiveCommand
		// 座標取得ボタン
		public ReactiveCommand GetGameWindowPositionCommand { get; } = new ReactiveCommand();
		// 画像保存ボタン
		public ReactiveCommand SaveScreenshotCommand { get; } = new ReactiveCommand();
		// 終了操作
		public ReactiveCommand CloseCommand { get; } = new ReactiveCommand();
		// ソフトの情報を表示
		public ReactiveCommand SoftwareInfoCommand { get; } = new ReactiveCommand();
		// 設定をインポート
		public ReactiveCommand ImportSettingsCommand { get; } = new ReactiveCommand();
		// 設定をエクスポート
		public ReactiveCommand ExportSettingsCommand { get; } = new ReactiveCommand();
		// スクショ保存フォルダを表示
		public ReactiveCommand OpenPicFolderCommand { get; } = new ReactiveCommand();
		// 資材記録画面を表示
		public ReactiveCommand OpenSupplyViewCommand { get; } = new ReactiveCommand();
		// 各種タイマー画面を表示
		public ReactiveCommand OpenTimerViewCommand { get; } = new ReactiveCommand();
		// 資材のインポート機能
		public ReactiveCommand ImportMainSupplyCommand { get; } = new ReactiveCommand();
		public ReactiveCommand ImportSubSupply1Command { get; } = new ReactiveCommand();
		public ReactiveCommand ImportSubSupply2Command { get; } = new ReactiveCommand();
		public ReactiveCommand ImportSubSupply3Command { get; } = new ReactiveCommand();
		public ReactiveCommand ImportSubSupply4Command { get; } = new ReactiveCommand();
		#endregion

		// 実行ログに追記する
		private void PutLog(string message) {
			ApplicationLog.Value += $"{Utility.GetTimeStrShort()} {message}\n";
		}
		// 複数ウィンドウから選択した際の結果を処理する
		private void SelectGameWindow(Rectangle? rect) {
			if (rect == null) {
				ScreenShotProvider.GameWindowRect = null;
				PutLog("座標取得 : 失敗");
				SaveScreenshotFlg.Value = false;
			} else {
				ScreenShotProvider.GameWindowRect = rect;
				PutLog("座標取得 : 成功");
				PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
				SaveScreenshotFlg.Value = true;
			}
		}
		// 選択画面を表示する
		private void ShowSelectGameWindow(List<Rectangle> rectList) {
			var dg = new SelectGameWindowAction(SelectGameWindow);
			var vm = new GameScreenSelectViewModel(rectList, dg);
			var view = new Views.GameScreenSelectView { DataContext = vm };
			view.ShowDialog();
		}
		// ゲーム画面の座標を取得する
		private async void GetGameWindowPosition() {
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
						SaveScreenshotFlg.Value = false;
					}
					break;
				case 1: {
						// 即座にその候補で確定させる
						ScreenShotProvider.GameWindowRect = rectList[0];
						PutLog("座標取得 : 成功");
						PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
						SaveScreenshotFlg.Value = true;
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
								SaveScreenshotFlg.Value = true;
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
			} catch (Exception) {
				PutLog($"座標取得 : 失敗");
			}
		}
		// 設定内容を画面に反映する
		private void SetSettings() {
			ForTwitterFlg.Value = SettingsStore.ForTwitterFlg;
			MemoryWindowPositionFlg.Value = SettingsStore.MemoryWindowPositionFlg;
			if (MemoryWindowPositionFlg.Value) {
				MainWindowPositionLeft.Value = SettingsStore.MainWindowRect[0];
				MainWindowPositionTop.Value = SettingsStore.MainWindowRect[1];
				MainWindowPositionWidth.Value = SettingsStore.MainWindowRect[2];
				MainWindowPositionHeight.Value = SettingsStore.MainWindowRect[3];
			}
			AutoSearchPositionFlg.Value = SettingsStore.AutoSearchPositionFlg;
			AutoSupplyScreenShotFlg.Value = SettingsStore.AutoSupplyScreenShotFlg;
			PutCharacterRecognitionFlg.Value = SettingsStore.PutCharacterRecognitionFlg;
			DragAndDropPictureFlg.Value = SettingsStore.DragAndDropPictureFlg;
		}
		// 資材のインポート機能(index=0～3、0から順にキューブ・ドリル・勲章・家具コイン)
		private async Task ImportSubSupplyAsync(int index) {
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
				} else {
					PutLog("エラー：資材データを読み込めませんでした");
				}
			}
		}
		// ソフトウェアが最新かを調べる
		private async void CheckSoftwareVer() {
			string nowSoftwareVer = Utility.SoftwareVer;
			string lastSoftwareVer = await Utility.NewestSoftwareVerAsync();
			PutLog("最新版チェック...");
			if (lastSoftwareVer == "") {
				PutLog("警告：バージョンチェックできませんでした");
			} else {
				if (nowSoftwareVer != lastSoftwareVer) {
					PutLog("警告：このソフトウェアは最新ではありません");
					var result = MessageBox.Show($"より新しいVerが存在するようです。\n\n　現在のVer.→{nowSoftwareVer}\n　最新のVer.→{lastSoftwareVer}\n\nダウンロードページを開きますか？", Utility.SoftwareName, MessageBoxButton.YesNo, MessageBoxImage.Information);
					if (result == MessageBoxResult.Yes) {
						System.Diagnostics.Process.Start(@"https://github.com/YSRKEN/AzLH/releases");
					}
				} else {
					PutLog("このソフトウェアは最新です");
				}
			}
		}
		// 定期的にスクリーンショットを取得し、そこに起因する処理を行う
		private void HelperTaskF() {
			if (!SaveScreenshotFlg.Value)
				return;
			using (var screenShot = ScreenShotProvider.GetScreenshot()) {
				// スクショが取得できるとscreenShotがnullにならない
				if (screenShot != null) {
					// シーン文字列を取得し、表示する
					string judgedScene = SceneRecognition.JudgeGameScene(screenShot);
					JudgedScene.Value = $"シーン判定 : {judgedScene}";
					// 資材量を取得する
					// (戦闘中なら各種ボムの分量と残り秒数を読み取る)
					if (SupplyStore.SupplyListEachScene.ContainsKey(judgedScene)) {
						foreach (string supplyName in SupplyStore.SupplyListEachScene[judgedScene]) {
							if (SupplyStore.UpdateSupplyValue(screenShot, supplyName, AutoSupplyScreenShotFlg.Value, PutCharacterRecognitionFlg.Value))
								PutLog($"資材量追記：{supplyName}");
						}
					}
					// 戦闘中でなくなった場合、速やかにボムタイマーをリセットする
					if (judgedScene != "戦闘中" && judgedScene != "不明") {
						SettingsStore.BombChageTime1 = null;
						SettingsStore.BombChageTime2 = null;
						SettingsStore.BombChageTime3 = null;
					}
				} else {
					// スクショが取得できなくなったのでその旨を通知する
					PutLog("エラー：スクショが取得できなくなりました");
					SaveScreenshotFlg.Value = false;
				}
			}
		}
		// 位置ズレ自動調整
		private async Task AutoGetGameWindowPositionAsync() {
			try {
				// 再び自動座標検出を行う
				var rectList = await ScreenShotProvider.GetGameWindowPositionAsync().ConfigureAwait(false);
				// その結果によって処理を分岐させる
				switch (rectList.Count) {
				case 0: {
						// 候補なしと表示する
						ScreenShotProvider.GameWindowRect = null;
						PutLog($"位置ズレ自動修正 : 失敗");
						SaveScreenshotFlg.Value = false;
					}
					break;
				case 1: {
						// 即座にその候補で確定させる
						ScreenShotProvider.GameWindowRect = rectList[0];
						PutLog("位置ズレ自動修正 : 成功");
						PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
						SaveScreenshotFlg.Value = true;
					}
					break;
				default: {
						// 元の座標に最も近いものに合わせる
						int distance = int.MaxValue;
						Rectangle? nearestRect = null;
						foreach (var rect in rectList) {
							int diffX = ScreenShotProvider.GameWindowRect.Value.X - rect.X;
							int diffY = ScreenShotProvider.GameWindowRect.Value.Y - rect.Y;
							int diff = (diffX * diffX) + (diffY * diffY);
							if (distance > diff) {
								distance = diff;
								nearestRect = rect;
							}
						}
						ScreenShotProvider.GameWindowRect = nearestRect;
						PutLog("位置ズレ自動修正 : 成功");
						PutLog($"ゲーム座標 : {Utility.GetRectStr((Rectangle)ScreenShotProvider.GameWindowRect)}");
						SaveScreenshotFlg.Value = true;
					}
					break;
				}
			} catch {
				ScreenShotProvider.GameWindowRect = null;
				PutLog($"位置ズレ自動修正 : 失敗");
				SaveScreenshotFlg.Value = false;
			}
		}
		// 毎秒ごとの処理を行う
		private async void HelperTaskS() {
			if (SaveScreenshotFlg.Value) {
				// ズレ検出・修正処理
				if (!ScreenShotProvider.CanGetScreenshot()) {
					// スクショが取得できなくなったのでその旨を通知する
					PutLog("エラー：ゲーム画面の位置ズレを検出しました");
					await AutoGetGameWindowPositionAsync();
					return;
				}
				// スクショから得られる情報を用いた修正
				using (var screenShot = ScreenShotProvider.GetScreenshot()) {
					switch (SceneRecognition.JudgeGameScene(screenShot)) {
					case "戦闘中":
						MainModel.SetBattleBombTimer(screenShot, ref oldGauge, ref remainTime);
						break;
					case "委託":
						MainModel.SetConsignTimer(screenShot);
						break;
					case "戦術教室":
						MainModel.SetLectureTimer(screenShot);
						break;
					case "寮舎":
						MainModel.SetFoodTimer(screenShot);
						break;
					}
				}
			} else {
				// 常時座標認識
				if (AutoSearchPositionFlg.Value) {
					GetGameWindowPosition();
				}
			}
		}
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
			}
			// 資材データベースを初期化する
			SupplyStore.Initialize();
			// 設定内容を画面に反映する
			SetSettings();

			// プロパティを設定
			ForTwitterFlg.Subscribe(x => { SettingsStore.ForTwitterFlg = x; });
			MainWindowPositionLeft.Subscribe(x => {
				SettingsStore.MainWindowRect[0] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			MainWindowPositionTop.Subscribe(x => {
				SettingsStore.MainWindowRect[1] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			MainWindowPositionWidth.Subscribe(x => {
				SettingsStore.MainWindowRect[2] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			MainWindowPositionHeight.Subscribe(x => {
				SettingsStore.MainWindowRect[3] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			MemoryWindowPositionFlg.Subscribe(x => { SettingsStore.MemoryWindowPositionFlg = x; });
			AutoSearchPositionFlg.Subscribe(x => { SettingsStore.AutoSearchPositionFlg = x; });
			AutoSupplyScreenShotFlg.Subscribe(x => { SettingsStore.AutoSupplyScreenShotFlg = x; });
			PutCharacterRecognitionFlg.Subscribe(x => { SettingsStore.PutCharacterRecognitionFlg = x; });
			DragAndDropPictureFlg.Subscribe(x => { SettingsStore.DragAndDropPictureFlg = x; });

			// コマンドを設定
			GetGameWindowPositionCommand.Subscribe(_ => GetGameWindowPosition());
			SaveScreenshotCommand.Subscribe(_ => {
				try {
					string fileName = $"{Utility.GetTimeStrLong()}.png";
					ScreenShotProvider.GetScreenshot(ForTwitterFlg.Value).Save($"pic\\{fileName}");
					PutLog($"スクリーンショット : 成功");
					PutLog($"ファイル名 : {fileName}");
				} catch (Exception) {
					PutLog($"スクリーンショット : 失敗");
				}
			});
			CloseCommand.Subscribe(_ => {
				CloseWindow.Value = true;
			});
			SoftwareInfoCommand.Subscribe(_ => {
				// ソフトの情報を返す
				// 参考→http://dobon.net/vb/dotnet/file/myversioninfo.html
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
				MessageBox.Show(
					$"{asmttl} Ver.{asmver}\n{asmcpy}\n{asmprd}",
					Utility.SoftwareName,
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			});
			ImportSettingsCommand.Subscribe(_ => {
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
					} else {
						PutLog("設定を読み込みました");
					}
					SetSettings();
				}
			});
			ExportSettingsCommand.Subscribe(_ => {
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
					} else {
						PutLog("設定を保存しました");
					}
				}
			});
			OpenPicFolderCommand.Subscribe(_ => System.Diagnostics.Process.Start(@"pic\"));
			OpenSupplyViewCommand.Subscribe(_ => {
				if (SettingsStore.ShowSupplyWindowFlg)
					return;
				var vm = new SupplyViewModel();
				var view = new Views.SupplyView { DataContext = vm };
				view.Show();
				SettingsStore.ShowSupplyWindowFlg = true;
			}).AddTo(Disposable);
			OpenTimerViewCommand.Subscribe(_ => {
				if (SettingsStore.ShowTimerWindowFlg)
					return;
				var vm = new TimerViewModel();
				var view = new Views.TimerView { DataContext = vm };
				view.Show();
				SettingsStore.ShowTimerWindowFlg = true;
			}).AddTo(Disposable);
			ImportMainSupplyCommand.Subscribe(async _ => {
				// 資材のインポート機能(燃料・資金・ダイヤ)
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
					} else {
						PutLog("エラー：資材データを読み込めませんでした");
					}
				}
			});
			ImportSubSupply1Command.Subscribe(async _ => await ImportSubSupplyAsync(0));
			ImportSubSupply2Command.Subscribe(async _ => await ImportSubSupplyAsync(1));
			ImportSubSupply3Command.Subscribe(async _ => await ImportSubSupplyAsync(2));
			ImportSubSupply4Command.Subscribe(async _ => await ImportSubSupplyAsync(3));

			// タイマーを初期化し、定時タスクを登録して実行する
			// http://takachan.hatenablog.com/entry/2017/09/09/225342
			var timer = new Timer(200);
			timer.Elapsed += (sender, e) => {
				try{
					timer.Stop();
					HelperTaskF();
				}
				finally{timer.Start();}
			};
			timer.Start();
			var timer2 = new Timer(1000);
			timer2.Elapsed += (sender, e) => {
				try {
					timer2.Stop();
					HelperTaskS();
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
			CheckSoftwareVer();
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
