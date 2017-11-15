using System;
using System.Data.SQLite;
using static AzLH.Models.CharacterRecognition;
using System.Data.SQLite.Linq;
using System.Collections.Generic;
using System.Drawing;

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

		// 初期化
		public static void Initialize() {
			// テーブルが存在しない場合、テーブルを作成する
			foreach (var supplyInfo in SupplyParameters) {
				try {
					using (var con = new SQLiteConnection(connectionString)) {
						con.Open();
						using (var cmd = con.CreateCommand()) {
							string sql = $"CREATE TABLE [{supplyInfo.Value.Name}]([datetime] DATETIME, [value] INTEGER, PRIMARY KEY(datetime))";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
					}
				}
				catch { }
			}
			// 最終更新日時のキャッシュを準備する
			foreach (var supplyInfo in SupplyParameters) {
				lastWriteDateTime[supplyInfo.Key] = GetLastWriteDateTime(supplyInfo.Key);
			}
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
		public static bool UpdateSupplyValue(Bitmap bitmap, string supplyType) {
			var nowDateTime = DateTime.Now;
			if ((nowDateTime - lastWriteDateTime[supplyType]).TotalMinutes < updateInterval)
				return false;
			int value = GetValueOCR(bitmap, supplyType);
			if (value < 0)
				return false;
			try {
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						string sql = $"INSERT INTO [{SupplyParameters[supplyType].Name}] VALUES ('{nowDateTime.ToString("yyyy-MM-dd HH:mm:ss")}', {value})";
						cmd.CommandText = sql;
						cmd.ExecuteNonQuery();
					}
				}
				lastWriteDateTime[supplyType] = nowDateTime;
				return true;
			}
			catch {
				return false;
			}
		}
	}
}
