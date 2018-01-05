﻿using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace AzLH.Models {
	internal sealed class SettingsStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		public static SettingsStore Instance { get; } = new SettingsStore();
		private SettingsStore(){ SetDefaultSettings(); }

		// Twitter用に加工するか？
		public bool ForTwitterFlg { get; set; }
		// メイン画面の位置・大きさ
		public double[] MainWindowRect { get; set; }
		// 資材記録画面の位置・大きさ
		public double[] SupplyWindowRect { get; set; }
		// 各種タイマー画面の位置・大きさ
		public double[] TimerWindowRect { get; set; }
		// ウィンドウの座標を記憶するか？
		public bool MemoryWindowPositionFlg { get; set; }
		// 常時座標を捕捉し続けるか？
		public bool AutoSearchPositionFlg { get; set; }
		// 資材記録画面を最初から表示するか？
		public bool AutoSupplyWindowFlg { get; set; }
		// 各種タイマー画面を最初から表示するか？
		public bool AutoTimerWindowFlg { get; set; }
		// 資材記録画面が表示されているか？
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool ShowSupplyWindowFlg { get; set; }
		// 各種タイマー画面が表示されているか？
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool ShowTimerWindowFlg { get; set; }
		// 資材記録時にスクショでロギングするか？
		public bool AutoSupplyScreenShotFlg { get; set; }
		// 資材記録時に画像処理結果を出力するか？
		public bool PutCharacterRecognitionFlg { get; set; }
		// ドラッグ＆ドロップでシーン認識するか？
		public bool DragAndDropPictureFlg { get; set; }

		// デフォルト設定
		private void SetDefaultSettings() {
			ForTwitterFlg = false;
			MainWindowRect = new double[] { double.NaN, double.NaN, 400.0, 300.0 };
			SupplyWindowRect = new double[] { double.NaN, double.NaN, 600.0, 400.0 };
			TimerWindowRect = new double[] { double.NaN, double.NaN, 400.0, 250.0 };
			MemoryWindowPositionFlg = true;
			AutoSearchPositionFlg = true;
			AutoSupplyWindowFlg = false;
			AutoTimerWindowFlg = false;
			AutoSupplyScreenShotFlg = false;
			PutCharacterRecognitionFlg = false;
			DragAndDropPictureFlg = false;
		}
		// JSONから読み込み
		public bool LoadSettings(string path) {
			try {
				using (var sr = new StreamReader(path, Encoding.UTF8)) {
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<SettingsStore>(json);
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
				}
				return true;
			}
			catch {
				SetDefaultSettings();
				return false;
			}
		}

		public bool LoadSettings() {
			return LoadSettings("settings.json");
		}

		// JSONに書き出し
		public bool SaveSettings(string path) {
			try {
				using (var sw = new StreamWriter(path, false, Encoding.UTF8)) {
					// Json.NETでstring形式に書き出す
					string json = JsonConvert.SerializeObject(this, Formatting.Indented);
					// 書き込み
					sw.Write(json);
				}
				return true;
			}
			catch {
				return false;
			}
		}

		public bool SaveSettings() {
			return SaveSettings("settings.json");
		}

		// 設定項目を初期化
		public bool Initialize() {
			if (LoadSettings()) {
				return true;
			}
			else {
				SaveSettings();
				return false;
			}
		}
	}
}
