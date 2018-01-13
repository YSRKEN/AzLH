using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AzLH.Models.CharacterRecognition;

namespace AzLH.Models {
	internal static class SupplyStore {
		// 燃料：～10000　通常資材左
		// 資金：～50000　通常資材右
		// キューブ：～100　特殊資材左
		// ドリル：～100　特殊資材左
		// 勲章：～100　特殊資材左
		// ダイヤ：～1000　特殊資材右
		// 家具コイン：～1000　特殊資材右
		private const string connectionString = @"Data Source=supply.db";
		// 最終更新日時のキャッシュ
		private static Dictionary<string, DateTime> lastWriteDateTime = new Dictionary<string, DateTime>();
		// 更新間隔(分)
		private static int updateInterval = 10;

		// 各シーンに対応した資材
		public static Dictionary<string, string[]> SupplyListEachScene { get; } = new Dictionary<string, string[]>{
			{ "母港", new string[]{ "燃料", "資金", "ダイヤ" } },
			{ "建造", new string[]{ "キューブ" } },
			{ "建造中", new string[]{ "ドリル" } },
			{ "支援", new string[]{ "勲章" } },
			{ "家具屋", new string[]{ "家具コイン" } }
		};
		// 初期化
		public static void Initialize() {
			// テーブルが存在しない場合、テーブルを作成する
			foreach (var supplyInfo in SupplyParameters) {
				// テーブルの存在を確認(SQLite流)
				bool hasTableFlg = false;
				try {
					using (var con = new SQLiteConnection(connectionString)) {
						con.Open();
						using (var cmd = con.CreateCommand()) {
							string sql = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{supplyInfo.Value.Name}';";
							cmd.CommandText = sql;
							using (var reader = cmd.ExecuteReader()) {
								if (reader.Read() && reader.GetInt32(0) == 1) {
									hasTableFlg = true;
								}
							}
						}
					}
				} catch { }
				// テーブルを作成
				if (!hasTableFlg) {
					try {
						using (var con = new SQLiteConnection(connectionString)) {
							con.Open();
							using (var cmd = con.CreateCommand()) {
								string sql = $"CREATE TABLE [{supplyInfo.Value.Name}]([datetime] DATETIME, [value] INTEGER, PRIMARY KEY(datetime))";
								cmd.CommandText = sql;
								cmd.ExecuteNonQuery();
							}
						}
					} catch { }
				}
			}
			// 最終更新日時のキャッシュを準備する
			foreach (var supplyInfo in SupplyParameters) {
				lastWriteDateTime[supplyInfo.Key] = GetLastWriteDateTime(supplyInfo.Key);
			}
			// SupplyNameListを初期化する

		}
		// ある資材について、その最新書き込み日時を知る
		private static DateTime GetLastWriteDateTime(string supplyType) {
			var output = new DateTime();
			try {
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						string sql = $"SELECT MAX(datetime) FROM [{SupplyParameters[supplyType].Name}]";
						cmd.CommandText = sql;
						using (var reader = cmd.ExecuteReader()) {
							if (reader.Read()) {
								output = reader.GetDateTime(0);
							}
						}
					}
				}
			}
			catch { }
			return output;
		}
		// 資材量を更新できれば更新する
		public static bool UpdateSupplyValue(Bitmap bitmap, string supplyType, bool autoSupplyScreenShotFlg, bool putCharacterRecognitionFlg) {
			// 最終更新日時(lastWriteDateTime)～現在時刻(nowDateTime)の間が、
			// 更新間隔(updateInterval)より短ければ更新しない
			var nowDateTime = DateTime.Now;
			if ((nowDateTime - lastWriteDateTime[supplyType]).TotalMinutes < updateInterval)
				return false;
			// OCRを行い、結果がなぜか負数になった場合も更新しない
			int value = GetValueOCR(bitmap, supplyType, putCharacterRecognitionFlg);
			if (value < 0)
				return false;
			// 更新操作
			try {
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						string sql = $"INSERT INTO [{SupplyParameters[supplyType].Name}] VALUES ('{nowDateTime.ToString("yyyy-MM-dd HH:mm:ss")}', {value})";
						cmd.CommandText = sql;
						cmd.ExecuteNonQuery();
					}
				}
				// 更新に成功すれば、最終更新日時を書き換える
				lastWriteDateTime[supplyType] = nowDateTime;
				// フラグが立っていれば、読み取った瞬間のスクショを数値とともに保存する
				if(autoSupplyScreenShotFlg)
					bitmap.Save($"debug\\{Utility.GetTimeStrLong(nowDateTime)} {supplyType} {value}.png");
				return true;
			}
			catch {
				return false;
			}
		}
		// ある資材のデータを収集する
		public static Dictionary<DateTime, int> GetSupplyData(string supplyType) {
			var output = new Dictionary<DateTime, int>();
			try {
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						string sql = $"SELECT datetime, value FROM [{SupplyParameters[supplyType].Name}] ORDER BY datetime";
						cmd.CommandText = sql;
						using (var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								// reader.GetInt32(1)とは、「1(引数の数字)列目をint型で読み取る」ということ
								output[reader.GetDateTime(0)] = reader.GetInt32(1);
							}
						}
					}
				}
			}
			catch {}
			return output;
		}
		// メイン資材のデータをインポートする
		public static bool ImportOldMainSupplyData(string fileName) {
			// インポート用にデータを読み込む
			var mainSupplyData = new Dictionary<string, Dictionary<DateTime, int>>() {
				{ "燃料", new Dictionary<DateTime, int>() },
				{ "資金", new Dictionary<DateTime, int>()  },
				{ "ダイヤ", new Dictionary<DateTime, int>() },
			};
			try {
				using (var sr = new StreamReader(fileName)) {
					while (!sr.EndOfStream) {
						// 1行を読み込む
						string line = sr.ReadLine();
						// マッチさせてから各数値を取り出す
						string pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Fuel>\d+),(?<Money>\d+),(?<Diamond>\d+)";
						var match = Regex.Match(line, pattern);
						if (!match.Success) {
							continue;
						}
						// 取り出した数値を元に、mainSupplyDataに入力する
						try {
							// 読み取り
							var supplyDateTime = new DateTime(
								int.Parse(match.Groups["Year"].Value),
								int.Parse(match.Groups["Month"].Value),
								int.Parse(match.Groups["Day"].Value),
								int.Parse(match.Groups["Hour"].Value),
								int.Parse(match.Groups["Minute"].Value),
								int.Parse(match.Groups["Second"].Value));
							int[] supplyData = {
							int.Parse(match.Groups["Fuel"].Value),
							int.Parse(match.Groups["Money"].Value),
							int.Parse(match.Groups["Diamond"].Value)};
							// データベースに入力
							mainSupplyData["燃料"][supplyDateTime] = supplyData[0];
							mainSupplyData["資金"][supplyDateTime] = supplyData[1];
							mainSupplyData["ダイヤ"][supplyDateTime] = supplyData[2];
						}
						catch {}
					}
				}
			}
			catch {
				return false;
			}
			// 読み込んだデータをデータベースに入力していく
			foreach(var supplyDataPair in mainSupplyData) {
				string supplyType = supplyDataPair.Key;
				var supplyData = supplyDataPair.Value;
				foreach(var supply in supplyData) {
					var date = supply.Key;
					int value = supply.Value;
					if (value < 0)
						continue;
					try {
						using (var con = new SQLiteConnection(connectionString)) {
							con.Open();
							using (var cmd = con.CreateCommand()) {
								string sql = $"INSERT INTO [{SupplyParameters[supplyType].Name}] VALUES ('{date.ToString("yyyy-MM-dd HH:mm:ss")}', {value})";
								cmd.CommandText = sql;
								cmd.ExecuteNonQuery();
							}
						}
					}
					catch {}
				}
				lastWriteDateTime[supplyType] = GetLastWriteDateTime(supplyType);
			}
			return true;
		}
		public static Task<bool> ImportOldMainSupplyDataAsync(string fileName) {
			return Task.Run(() => ImportOldMainSupplyData(fileName));
		}
		// サブ資材のデータをインポートする
		public static bool ImportOldSubSupplyData(string fileName, int index) {
			if (index < 0 || index >= 4)
				return false;
			// インポート用にデータを読み込む
			var subSupplyNameList = new string[] { "キューブ", "ドリル", "勲章", "家具コイン" };
			string subSupplyType = subSupplyNameList[index];
			var subSupplyData = new Dictionary<DateTime, int>();
			try {
				using (var sr = new StreamReader(fileName)) {
					while (!sr.EndOfStream) {
						// 1行を読み込む
						string line = sr.ReadLine();
						// マッチさせてから各数値を取り出す
						string pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Supply>\d+)";
						var match = Regex.Match(line, pattern);
						if (!match.Success) {
							continue;
						}
						// 取り出した数値を元に、subSupplyDataに入力する
						try {
							// 読み取り
							// 読み取り
							var supplyDateTime = new DateTime(
								int.Parse(match.Groups["Year"].Value),
								int.Parse(match.Groups["Month"].Value),
								int.Parse(match.Groups["Day"].Value),
								int.Parse(match.Groups["Hour"].Value),
								int.Parse(match.Groups["Minute"].Value),
								int.Parse(match.Groups["Second"].Value));
							int supplyData = int.Parse(match.Groups["Supply"].Value);
							// データベースに入力
							subSupplyData[supplyDateTime] = supplyData;
						}
						catch { }
					}
				}
			}
			catch {
				return false;
			}
			// 読み込んだデータをデータベースに入力していく
			foreach (var supply in subSupplyData) {
				var date = supply.Key;
				int value = supply.Value;
				if (value < 0)
					continue;
				try {
					using (var con = new SQLiteConnection(connectionString)) {
						con.Open();
						using (var cmd = con.CreateCommand()) {
							string sql = $"INSERT INTO [{SupplyParameters[subSupplyType].Name}] VALUES ('{date.ToString("yyyy-MM-dd HH:mm:ss")}', {value})";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
					}
				}
				catch { }
			}
			lastWriteDateTime[subSupplyType] = GetLastWriteDateTime(subSupplyType);
			return true;
		}
		public static Task<bool> ImportOldSubSupplyDataAsync(string fileName, int index) {
			return Task.Run(() => ImportOldSubSupplyData(fileName, index));
		}
	}
}
