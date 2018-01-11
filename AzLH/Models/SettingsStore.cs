using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Timers;

namespace AzLH.Models {
	internal class SettingsStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		private static SettingsStore @this { get; } = new SettingsStore();
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

		// ファイルが変更されたか？
		[JsonIgnore]
		private static bool changeSettingFlg;
		#endregion

		#region staticに処理するためのラッパー
		// メイン画面の位置・大きさ
		[JsonIgnore]
		public static double[] MainWindowRect {
			get => @this.mainWindowRect;
			set {
				@this.mainWindowRect = value;
				changeSettingFlg = true;
			}
		}
		// 資材記録画面の位置・大きさ
		[JsonIgnore]
		public static double[] SupplyWindowRect {
			get => @this.supplyWindowRect;
			set {
				@this.supplyWindowRect = value;
				changeSettingFlg = true;
			}
		}
		// 各種タイマー画面の位置・大きさ
		[JsonIgnore]
		public static double[] TimerWindowRect {
			get => @this.timerWindowRect;
			set {
				@this.timerWindowRect = value;
				changeSettingFlg = true;
			}
		}

		// Twitter用に加工するか？
		[JsonIgnore]
		public static bool ForTwitterFlg {
			get => @this.forTwitterFlg;
			set {
				@this.forTwitterFlg = value;
				changeSettingFlg = true;
			}
		}
		// ウィンドウの座標を記憶するか？
		[JsonIgnore]
		public static bool MemoryWindowPositionFlg {
			get => @this.memoryWindowPositionFlg;
			set {
				@this.memoryWindowPositionFlg = value;
				changeSettingFlg = true;
			}
		}
		// 常時座標を捕捉し続けるか？
		[JsonIgnore]
		public static bool AutoSearchPositionFlg {
			get => @this.autoSearchPositionFlg;
			set {
				@this.autoSearchPositionFlg = value;
				changeSettingFlg = true;
			}
		}
		// 資材記録画面を最初から表示するか？
		[JsonIgnore]
		public static bool AutoSupplyWindowFlg {
			get => @this.autoSupplyWindowFlg;
			set {
				@this.autoSupplyWindowFlg = value;
				changeSettingFlg = true;
			}
		}
		// 各種タイマー画面を最初から表示するか？
		[JsonIgnore]
		public static bool AutoTimerWindowFlg {
			get => @this.autoTimerWindowFlg;
			set {
				@this.autoTimerWindowFlg = value;
				changeSettingFlg = true;
			}
		}
		// 資材記録時にスクショでロギングするか？
		[JsonIgnore]
		public static bool AutoSupplyScreenShotFlg {
			get => @this.autoSupplyScreenShotFlg;
			set {
				@this.autoSupplyScreenShotFlg = value;
				changeSettingFlg = true;
			}
		}
		// 資材記録時に画像処理結果を出力するか？
		[JsonIgnore]
		public static bool PutCharacterRecognitionFlg {
			get => @this.putCharacterRecognitionFlg;
			set {
				@this.putCharacterRecognitionFlg = value;
				changeSettingFlg = true;
			}
		}
		// ドラッグ＆ドロップでシーン認識するか？
		[JsonIgnore]
		public static bool DragAndDropPictureFlg {
			get => @this.dragAndDropPictureFlg;
			set {
				@this.dragAndDropPictureFlg = value;
				changeSettingFlg = true;
			}
		}

		// 軍事委託の完了時刻をメモする
		[JsonIgnore]
		public static DateTime? ConsignFinalTime1 {
			get => @this.consignFinalTime1;
			set {
				@this.consignFinalTime1 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime2 {
			get => @this.consignFinalTime2;
			set {
				@this.consignFinalTime2 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime3 {
			get => @this.consignFinalTime3;
			set {
				@this.consignFinalTime3 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? ConsignFinalTime4 {
			get => @this.consignFinalTime4;
			set {
				@this.consignFinalTime4 = value;
				changeSettingFlg = true;
			}
		}
		// 戦術教室の完了時刻をメモする
		[JsonIgnore]
		public static DateTime? LectureFinalTime1 {
			get => @this.lectureFinalTime1;
			set {
				@this.lectureFinalTime1 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? LectureFinalTime2 {
			get => @this.lectureFinalTime2;
			set {
				@this.lectureFinalTime2 = value;
				changeSettingFlg = true;
			}
		}
		// 食糧の枯渇時刻をメモする
		[JsonIgnore]
		public static DateTime? FoodFinalTime {
			get => @this.foodFinalTime;
			set {
				@this.foodFinalTime = value;
				changeSettingFlg = true;
			}
		}

		// 資材記録画面が表示されているか？
		[JsonIgnore]
		public static bool ShowSupplyWindowFlg {
			get => @this.showSupplyWindowFlg;
			set {
				@this.showSupplyWindowFlg = value;
				changeSettingFlg = true;
			}
		}
		// 各種タイマー画面が表示されているか？
		[JsonIgnore]
		public static bool ShowTimerWindowFlg {
			get => @this.showTimerWindowFlg;
			set {
				@this.showTimerWindowFlg = value;
				changeSettingFlg = true;
			}
		}

		// 各種ボムのチャージ完了時刻をメモする
		[JsonIgnore]
		public static DateTime? BombChageTime1 {
			get => @this.bombChageTime1;
			set {
				@this.bombChageTime1 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? BombChageTime2 {
			get => @this.bombChageTime2;
			set {
				@this.bombChageTime2 = value;
				changeSettingFlg = true;
			}
		}
		[JsonIgnore]
		public static DateTime? BombChageTime3 {
			get => @this.bombChageTime3;
			set {
				@this.bombChageTime3 = value;
				changeSettingFlg = true;
			}
		}
		#endregion

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
		public static bool SaveSettings(string path = "settings.json") {
			try {
				using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
					// Json.NETでstring形式に書き出す
					string json = JsonConvert.SerializeObject(@this, Formatting.Indented);
					// 書き込み
					sw.Write(json);
				}
				Console.WriteLine("Save Success.");
				return true;
			} catch {
				Console.WriteLine("Save Failed.");
				return false;
			}
		}
		// 設定に変更点があった場合、JSONに書き出し
		public static bool SaveSettingsAuto() {
			if (!changeSettingFlg)
				return false;
			changeSettingFlg = false;
			return SaveSettings();
		}
		// 設定項目を初期化
		public static bool Initialize() {
			// 規定の位置から設定ファイルを読み込む
			bool successflg = LoadSettings();
			// 正常に読み込めなかった場合、新規に設定ファイルを作成する
			if (!successflg)
				SaveSettings();
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
