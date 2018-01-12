using System;
using System.Drawing;

namespace AzLH.Models {
	static class MainModel {
		// 戦闘中におけるタイマー調整
		public static void SetBattleBombTimer(Bitmap screenShot, ref double[] oldGauge, ref double[] remainTime) {
			// 読み取ったゲージから、フルチャージに必要な秒数を計算する
			var gauge = SceneRecognition.GetBattleBombGauge(screenShot);
			// 各種のゲージ毎に判定を行う
			for (int ti = 0; ti < SceneRecognition.GaugeTypeCount; ++ti) {
				if (gauge[ti] >= 0.0) {
					if (oldGauge[ti] >= 0.0) {
						// 前回のゲージ量が残っているので、チャージ完了に要する時間が計算できる
						// ただしゲージが変化していないようにみえる場合は無視する
						if (gauge[ti] > oldGauge[ti]) {
							remainTime[ti] = (1.0 - gauge[ti]) / (gauge[ti] - oldGauge[ti]);
							//
							switch (ti) {
							case 0:
								SettingsStore.BombChageTime1 = DateTime.Now.AddSeconds(remainTime[ti]);
								break;
							case 1:
								SettingsStore.BombChageTime2 = DateTime.Now.AddSeconds(remainTime[ti]);
								break;
							case 2:
								SettingsStore.BombChageTime3 = DateTime.Now.AddSeconds(remainTime[ti]);
								break;
							}
						} else {
							// 読み取り失敗した祭の処理
							if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
						}
					} else {
						// 読み取り失敗した祭の処理
						if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
					}
					// oldGaugeに今回読み取った量を上書きする
					oldGauge[ti] = gauge[ti];
				} else {
					// 読み取り失敗した祭の処理
					if (remainTime[ti] > 0.0) remainTime[ti] -= 1.0;
					oldGauge[ti] = -1.0;
				}
			}
		}
		// 委託画面におけるタイマー取得
		public static void SetConsignTimer(Bitmap screenShot) {
			// 委託時間を読み取って反映させる
			for (int ci = 0; ci < SceneRecognition.ConsignCount; ++ci) {
				// 残り時間を読み取る
				DateTime? finalTime = Utility.GetFinalTime(CharacterRecognition.GetTimeOCR(screenShot, $"委託{ci + 1}"));
				// 書き込み処理
				switch (ci) {
				case 0:
					SettingsStore.ConsignFinalTime1 = finalTime;
					break;
				case 1:
					SettingsStore.ConsignFinalTime2 = finalTime;
					break;
				case 2:
					SettingsStore.ConsignFinalTime3 = finalTime;
					break;
				case 3:
					SettingsStore.ConsignFinalTime4 = finalTime;
					break;
				}
			}
		}
		// 戦術教室画面におけるタイマー取得
		public static void SetLectureTimer(Bitmap screenShot) {
			// 残り時間を読み取って反映させる
			for (int ci = 0; ci < SceneRecognition.LectureCount; ++ci) {
				// 残り時間を読み取る
				DateTime? finalTime = Utility.GetFinalTime(CharacterRecognition.GetTimeOCR(screenShot, $"戦術教室{ci + 1}"));
				// 書き込み処理
				switch (ci) {
				case 0:
					SettingsStore.LectureFinalTime1 = finalTime;
					break;
				case 1:
					SettingsStore.LectureFinalTime2 = finalTime;
					break;
				}
			}
		}
		// 寮舎画面におけるタイマー取得
		public static void SetFoodTimer(Bitmap screenShot) {
			// 残り時間を読み取る
			DateTime? finalTime = Utility.GetFinalTime(CharacterRecognition.GetTimeOCR(screenShot, "食糧"));
			// 書き込み処理
			SettingsStore.FoodFinalTime = finalTime;
		}
	}
}
