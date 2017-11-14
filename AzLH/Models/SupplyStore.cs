using System;
using System.Data.SQLite;
using static AzLH.Models.CharacterRecognition;
using System.Data.SQLite.Linq;

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
		}

		// ある資材について、その最新書き込み日時を知る
		public static DateTime LastWriteDateTime(string supplyType) {
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
	}
}
