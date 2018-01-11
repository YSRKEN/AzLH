using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Timers;

namespace AzLH.Models {
	internal class SettingStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		public static SettingStore This { get; } = new SettingStore();
		private SettingStore(){}

		#region ファイルに保存する設定
		// メイン画面の位置・大きさ
		[JsonProperty("MainWindowRect")]
		private double[] mainWindowRect { get; set; }
			= new double[] { double.NaN, double.NaN, 400.0, 300.0 };
		// 資材記録画面の位置・大きさ
		[JsonProperty("SupplyWindowRect")]
		private double[] supplyWindowRect { get; set; }
			= new double[] { double.NaN, double.NaN, 600.0, 400.0 };
		// 各種タイマー画面の位置・大きさ
		[JsonProperty("TimerWindowRect")]
		private double[] timerWindowRect { get; set; }
			= new double[] { double.NaN, double.NaN, 400.0, 250.0 };

		// Twitter用に加工するか？
		public bool ForTwitterFlg { get; set; }
		// ウィンドウの座標を記憶するか？
		public bool MemoryWindowPositionFlg { get; set; } = true;
		// 常時座標を捕捉し続けるか？
		public bool AutoSearchPositionFlg { get; set; } = true;
		// 資材記録画面を最初から表示するか？
		public bool AutoSupplyWindowFlg { get; set; }
		// 各種タイマー画面を最初から表示するか？
		public bool AutoTimerWindowFlg { get; set; }
		// 資材記録時にスクショでロギングするか？
		public bool AutoSupplyScreenShotFlg { get; set; }
		// 資材記録時に画像処理結果を出力するか？
		public bool PutCharacterRecognitionFlg { get; set; }
		// ドラッグ＆ドロップでシーン認識するか？
		public bool DragAndDropPictureFlg { get; set; }

		// 軍事委託の完了時刻をメモする
		public DateTime? ConsignFinalTime1 { get; set; }
		public DateTime? ConsignFinalTime2 { get; set; }
		public DateTime? ConsignFinalTime3 { get; set; }
		public DateTime? ConsignFinalTime4 { get; set; }
		// 戦術教室の完了時刻をメモする
		public DateTime? LectureFinalTime1 { get; set; }
		public DateTime? LectureFinalTime2 { get; set; }
		// 食糧の枯渇時刻をメモする
		public DateTime? FoodFinalTime { get; set; }
		#endregion

		#region ファイルに保存しない設定
		// 資材記録画面が表示されているか？
		[JsonIgnore]
		public bool ShowSupplyWindowFlg { get; set; }
		// 各種タイマー画面が表示されているか？
		[JsonIgnore]
		public bool ShowTimerWindowFlg { get; set; }

		// 各種ボムのチャージ完了時刻をメモする
		[JsonIgnore]
		public DateTime? BombChageTime1 { get; set; }
		[JsonIgnore]
		public DateTime? BombChageTime2 { get; set; }
		[JsonIgnore]
		public DateTime? BombChageTime3 { get; set; }

		// ファイルが変更されたか？
		[JsonIgnore]
		private static bool ChangeSettingFlg { get; set; }
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
		#endregion

		// JSONから読み込み
		public static bool LoadSettings(string path = "settings.json") {
			try {
				using (var sr = new StreamReader(path, Encoding.UTF8)) {
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var This = JsonConvert.DeserializeObject<SettingStore>(json);
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
					string json = JsonConvert.SerializeObject(This, Formatting.Indented);
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
			if (!ChangeSettingFlg)
				return false;
			ChangeSettingFlg = false;
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
