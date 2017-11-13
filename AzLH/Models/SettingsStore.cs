using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace AzLH.Models {
	sealed class SettingsStore {
		// シングルトンパターン
		// 参考→https://qiita.com/rohinomiya/items/6bca22211d1bddf581c4
		private static SettingsStore instance = new SettingsStore();
		public static SettingsStore Instance => instance;
		private SettingsStore(){ SetDefaultSettings(); }

		// Twitter用に加工するか？
		public bool ForTwitterFlg { get; set; }
		// メイン画面の位置・大きさ
		public double[] MainWindowRect { get; set; }
		// ウィンドウの座標を記憶するか？
		public bool MemoryWindowPositionFlg { get; set; }
		// 常時座標を捕捉し続けるか？
		public bool AutoSearchPositionFlg { get; set; }

		// デフォルト設定
		private void SetDefaultSettings() {
			ForTwitterFlg = false;
			MainWindowRect = new double[] { double.NaN, double.NaN, 400.0, 300.0 };
			MemoryWindowPositionFlg = true;
			AutoSearchPositionFlg = true;
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
					MemoryWindowPositionFlg = model.MemoryWindowPositionFlg;
					AutoSearchPositionFlg = model.AutoSearchPositionFlg;
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
		public bool initialize() {
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
