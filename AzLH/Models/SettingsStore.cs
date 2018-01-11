using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;

namespace AzLH.Models {
	internal class SettingsStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		private static SettingsStore This { get; } = new SettingsStore();
		private SettingsStore(){}

		#region ファイルに保存する設定
		// メイン画面の位置・大きさ
		[JsonProperty("MainWindowRect")]
		private double[] mainWindowRect = new double[] { double.NaN, double.NaN, 400.0, 300.0 };
		// 資材記録画面の位置・大きさ
		[JsonProperty("SupplyWindowRect")]
		private double[] supplyWindowRect = new double[] { double.NaN, double.NaN, 600.0, 400.0 };
		// 各種タイマー画面の位置・大きさ
		[JsonProperty("TimerWindowRect")]
		private double[] timerWindowRect = new double[] { double.NaN, double.NaN, 400.0, 250.0 };

		// Twitter用に加工するか？
		[JsonProperty("ForTwitterFlg")]
		private bool forTwitterFlg;
		// ウィンドウの座標を記憶するか？
		[JsonProperty("MemoryWindowPositionFlg")]
		private bool memoryWindowPositionFlg = true;
		// 常時座標を捕捉し続けるか？
		[JsonProperty("AutoSearchPositionFlg")]
		private bool autoSearchPositionFlg = true;
		// 資材記録画面を最初から表示するか？
		[JsonProperty("AutoSupplyWindowFlg")]
		private bool autoSupplyWindowFlg;
		// 各種タイマー画面を最初から表示するか？
		[JsonProperty("AutoTimerWindowFlg")]
		private bool autoTimerWindowFlg;
		// 資材記録時にスクショでロギングするか？
		[JsonProperty("AutoSupplyScreenShotFlg")]
		private bool autoSupplyScreenShotFlg;
		// 資材記録時に画像処理結果を出力するか？
		[JsonProperty("PutCharacterRecognitionFlg")]
		private bool putCharacterRecognitionFlg;
		// ドラッグ＆ドロップでシーン認識するか？
		[JsonProperty("DragAndDropPictureFlg")]
		private bool dragAndDropPictureFlg;

		// 軍事委託の完了時刻をメモする
		[JsonProperty("ConsignFinalTime1")]
		private DateTime? consignFinalTime1;
		[JsonProperty("ConsignFinalTime2")]
		private DateTime? consignFinalTime2;
		[JsonProperty("ConsignFinalTime3")]
		private DateTime? consignFinalTime3;
		[JsonProperty("ConsignFinalTime4")]
		private DateTime? consignFinalTime4;
		// 戦術教室の完了時刻をメモする
		[JsonProperty("LectureFinalTime1")]
		private DateTime? lectureFinalTime1;
		[JsonProperty("LectureFinalTime2")]
		private DateTime? lectureFinalTime2;
		// 食糧の枯渇時刻をメモする
		[JsonProperty("FoodFinalTime")]
		private DateTime? foodFinalTime;
		#endregion

		#region ファイルに保存しない設定
		// 資材記録画面が表示されているか？
		[JsonIgnore]
		private bool showSupplyWindowFlg;
		// 各種タイマー画面が表示されているか？
		[JsonIgnore]
		private bool showTimerWindowFlg;

		// 各種ボムのチャージ完了時刻をメモする
		[JsonIgnore]
		private DateTime? bombChageTime1;
		[JsonIgnore]
		private DateTime? bombChageTime2;
		[JsonIgnore]
		private DateTime? bombChageTime3;
		#endregion

		#region staticに処理するためのラッパー
		// メイン画面の位置・大きさ
		[JsonIgnore]
		public static double[] MainWindowRect {
			get => This.mainWindowRect;
			set {
				This.mainWindowRect = value;
				ChangeSettingFlg = true;
			}
		}
		// 資材記録画面の位置・大きさ
		[JsonIgnore]
		public static double[] SupplyWindowRect {
			get => This.supplyWindowRect;
			set {
				This.supplyWindowRect = value;
				ChangeSettingFlg = true;
			}
		}
		// 各種タイマー画面の位置・大きさ
		[JsonIgnore]
		public static double[] TimerWindowRect {
			get => This.timerWindowRect;
			set {
				This.timerWindowRect = value;
				ChangeSettingFlg = true;
			}
		}

		// Twitter用に加工するか？
		[JsonIgnore]
		public static bool ForTwitterFlg {
			get => This.forTwitterFlg;
			set {
				This.forTwitterFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// ウィンドウの座標を記憶するか？
		[JsonIgnore]
		public static bool MemoryWindowPositionFlg {
			get => This.memoryWindowPositionFlg;
			set {
				This.memoryWindowPositionFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// 常時座標を捕捉し続けるか？
		[JsonIgnore]
		public static bool AutoSearchPositionFlg {
			get => This.autoSearchPositionFlg;
			set {
				This.autoSearchPositionFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// 資材記録画面を最初から表示するか？
		[JsonIgnore]
		public static bool AutoSupplyWindowFlg {
			get => This.autoSupplyWindowFlg;
			set {
				This.autoSupplyWindowFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// 各種タイマー画面を最初から表示するか？
		[JsonIgnore]
		public static bool AutoTimerWindowFlg {
			get => This.autoTimerWindowFlg;
			set {
				This.autoTimerWindowFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// 資材記録時にスクショでロギングするか？
		[JsonIgnore]
		public static bool AutoSupplyScreenShotFlg {
			get => This.autoSupplyScreenShotFlg;
			set {
				This.autoSupplyScreenShotFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// 資材記録時に画像処理結果を出力するか？
		[JsonIgnore]
		public static bool PutCharacterRecognitionFlg {
			get => This.putCharacterRecognitionFlg;
			set {
				This.putCharacterRecognitionFlg = value;
				ChangeSettingFlg = true;
			}
		}
		// ドラッグ＆ドロップでシーン認識するか？
		[JsonIgnore]
		public static bool DragAndDropPictureFlg {
			get => This.dragAndDropPictureFlg;
			set {
				This.dragAndDropPictureFlg = value;
				ChangeSettingFlg = true;
			}
		}

		// 軍事委託の完了時刻をメモする
		[JsonIgnore]
		public static DateTime? ConsignFinalTime1 {
			get => This.consignFinalTime1;
			set {
				This.consignFinalTime1 = value;
				ChangeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime2 {
			get => This.consignFinalTime2;
			set {
				This.consignFinalTime2 = value;
				ChangeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime3 {
			get => This.consignFinalTime3;
			set {
				This.consignFinalTime3 = value;
				ChangeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime4 {
			get => This.consignFinalTime4;
			set {
				This.consignFinalTime4 = value;
				ChangeSettingFlg = true;
			}
		}
		// 戦術教室の完了時刻をメモする
		[JsonIgnore]
		public static DateTime? LectureFinalTime1 {
			get => This.lectureFinalTime1;
			set {
				This.lectureFinalTime1 = value;
				ChangeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? LectureFinalTime2 {
			get => This.lectureFinalTime2;
			set {
				This.lectureFinalTime2 = value;
				ChangeSettingFlg = true;
			}
		}
		// 食糧の枯渇時刻をメモする
		[JsonIgnore]
		public static DateTime? FoodFinalTime {
			get => This.foodFinalTime;
			set {
				This.foodFinalTime = value;
				ChangeSettingFlg = true;
			}
		}

		// 資材記録画面が表示されているか？
		[JsonIgnore]
		public static bool ShowSupplyWindowFlg {
			get => This.showSupplyWindowFlg;
			set {
				This.showSupplyWindowFlg = value;
			}
		}
		// 各種タイマー画面が表示されているか？
		[JsonIgnore]
		public static bool ShowTimerWindowFlg {
			get => This.showTimerWindowFlg;
			set {
				This.showTimerWindowFlg = value;
			}
		}

		// 各種ボムのチャージ完了時刻をメモする
		[JsonIgnore]
		public static DateTime? BombChageTime1 {
			get => This.bombChageTime1;
			set {
				This.bombChageTime1 = value;
			}
		}
		[JsonIgnore]
		public static DateTime? BombChageTime2 {
			get => This.bombChageTime2;
			set {
				This.bombChageTime2 = value;
			}
		}
		[JsonIgnore]
		public static DateTime? BombChageTime3 {
			get => This.bombChageTime3;
			set {
				This.bombChageTime3 = value;
			}
		}
		#endregion

		// ファイルが変更されたか？
		[JsonIgnore]
		public static bool ChangeSettingFlg { get; set; }

		// JSONから読み込み
		public static bool LoadSettings(string path = "settings.json") {
			try {
				using (var sr = new StreamReader(path, Encoding.UTF8)) {
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var This = JsonConvert.DeserializeObject<SettingsStore>(json);
				}
				return true;
			} catch {
				return false;
			}
		}
		// JSONに書き出し
		public static bool SaveSettings(string path) {
			try {
				using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
					// Json.NETでstring形式に書き出す
					string json = JsonConvert.SerializeObject(This, Formatting.Indented);
					// 書き込み
					sw.Write(json);
				}
				Console.WriteLine("Save Success.");
				return true;
			} catch {
				Console.WriteLine("Save Failed.");
				MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return false;
			}
		}
		// 設定に変更点があった場合、JSONに書き出し
		public static bool SaveSettingsAuto() {
			if (!ChangeSettingFlg)
				return false;
			ChangeSettingFlg = false;
			return SaveSettings("settings.json");
		}
		// 設定項目を初期化
		public static bool Initialize() {
			// 規定の位置から設定ファイルを読み込む
			bool successflg = LoadSettings();
			// 正常に読み込めなかった場合、新規に設定ファイルを作成する
			if (!successflg)
				SaveSettings("settings.json");
			// 定期的に設定を保存するようにする
			var timer = new Timer(1000);
			timer.Elapsed += (sender, e) => {
				try {
					timer.Stop();
					SaveSettingsAuto();
				} finally { timer.Start(); }
			};
			timer.Start();
			return successflg;
		}
	}
}
