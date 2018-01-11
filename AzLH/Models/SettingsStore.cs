using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace AzLH.Models {
	internal class SettingStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		public static SettingStore This { get; } = new SettingStore();
		private SettingStore(){}

		#region ファイルに保存する設定
		// メイン画面の位置・大きさ
		public double[] MainWindowRect { get; set; }
			= new double[] { double.NaN, double.NaN, 400.0, 300.0 };
		// 資材記録画面の位置・大きさ
		public double[] SupplyWindowRect { get; set; }
			= new double[] { double.NaN, double.NaN, 600.0, 400.0 };
		// 各種タイマー画面の位置・大きさ
		public double[] TimerWindowRect { get; set; }
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
		#endregion

		#region staticに処理するためのラッパー
			
		#endregion

		// JSONから読み込み
		public static bool LoadSettings(string path = "settings.json") {
			try {
				using (var sr = new StreamReader(path, Encoding.UTF8)) {
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<SettingStore>(json);
					ForTwitterFlg = model.ForTwitterFlg;
					MainWindowRect = model.MainWindowRect;
					SupplyWindowRect = model.SupplyWindowRect;
					TimerWindowRect = model.TimerWindowRect;
					MemoryWindowPositionFlg = model.MemoryWindowPositionFlg;
					AutoSearchPositionFlg = model.AutoSearchPositionFlg;
					AutoSupplyWindowFlg = model.AutoSupplyWindowFlg;
					AutoTimerWindowFlg = model.AutoTimerWindowFlg;
					AutoSupplyScreenShotFlg = model.AutoSupplyScreenShotFlg;
					PutCharacterRecognitionFlg = model.PutCharacterRecognitionFlg;
					DragAndDropPictureFlg = model.DragAndDropPictureFlg;
					ConsignFinalTime1 = model.ConsignFinalTime1;
					ConsignFinalTime2 = model.ConsignFinalTime2;
					ConsignFinalTime3 = model.ConsignFinalTime3;
					ConsignFinalTime4 = model.ConsignFinalTime4;
					LectureFinalTime1 = model.LectureFinalTime1;
					LectureFinalTime2 = model.LectureFinalTime2;
					FoodFinalTime = model.FoodFinalTime;
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
				return true;
			} catch {
				return false;
			}
		}
		// 設定項目を初期化
		public static bool Initialize() {
			if (LoadSettings()) {
				return true;
			} else {
				SaveSettings();
				return false;
			}
		}
	}
}
