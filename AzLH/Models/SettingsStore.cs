using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzLH.Models {
	sealed class SettingsStore {
		// シングルトンパターン
		private static SettingsStore instance = new SettingsStore();
		public static SettingsStore Instance => instance;
		private SettingsStore(){}

		// Twitter用に加工するか？
		public bool ForTwitterFlg { get; set; }

		// デフォルト設定
		private void SetDefaultSettings() {
			ForTwitterFlg = false;
		}
		// JSONから読み込み
		private void LoadSettings() {
			try {
				using (var sr = new StreamReader("settings.json", Encoding.UTF8)) {
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<SettingsStore>(json);
					ForTwitterFlg = model.ForTwitterFlg;
				}
			}
			catch {
				SetDefaultSettings();
				throw;
			}
		}
		// JSONに書き出し
		public void SaveSettings() {
			try {
				using (var sw = new StreamWriter("settings.json", false, Encoding.UTF8)) {
					// Json.NETでstring形式に書き出す
					string json = JsonConvert.SerializeObject(this);
					// 書き込み
					sw.Write(json);
				}
			}
			catch {
				SetDefaultSettings();
				throw;
			}
		}
		// 設定項目を初期化
		public bool initialize() {
			try {
				LoadSettings();
			}
			catch {
				SaveSettings();
				return false;
			}
			return true;
		}
	}
}
