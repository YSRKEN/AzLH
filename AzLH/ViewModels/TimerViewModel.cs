using AzLH.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Timers;
using System.Windows;

namespace AzLH.ViewModels
{
	class TimerViewModel : IDisposable
	{
		// modelのinstance
		private readonly TimerModel timerModel = new TimerModel();
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 起動時にこの画面を表示するか？
		public ReactiveProperty<bool> AutoOpenWindowFlg { get; } = new ReactiveProperty<bool>(false);
		// 画面の位置
		public ReactiveProperty<double> WindowPositionLeft { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionTop { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionWidth { get; } = new ReactiveProperty<double>(400.0);
		public ReactiveProperty<double> WindowPositionHeight { get; } = new ReactiveProperty<double>(250.0);
		// 軍事委託の残時間
		public ReactiveProperty<string> ConsignRemainTime1 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> ConsignRemainTime2 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> ConsignRemainTime3 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> ConsignRemainTime4 { get; } = new ReactiveProperty<string>("");
		// 戦術教室の残時間
		public ReactiveProperty<string> LectureRemainTime1 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> LectureRemainTime2 { get; } = new ReactiveProperty<string>("");
		// 食糧の残時間
		public ReactiveProperty<string> FoodRemainTime { get; } = new ReactiveProperty<string>("");
		// 各種ボムの残時間
		public ReactiveProperty<string> BombRemainTime1 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BombRemainTime2 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BombRemainTime3 { get; } = new ReactiveProperty<string>("");

		// タイマー表示の更新を行う
		private void RedrawTimerWindow() {
			var nowTime = DateTime.Now;
			// 軍事委託の残時間
			if (SettingsStore.ConsignFinalTime1.HasValue) {
				var remainTime = (SettingsStore.ConsignFinalTime1.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					ConsignRemainTime1.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					ConsignRemainTime1.Value = "00:00:00";
					SettingsStore.ConsignFinalTime1 = null;
				}
			} else {
				ConsignRemainTime1.Value = "00:00:00";
			}
			if (SettingsStore.ConsignFinalTime2.HasValue) {
				var remainTime = (SettingsStore.ConsignFinalTime2.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					ConsignRemainTime2.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					ConsignRemainTime2.Value = "00:00:00";
					SettingsStore.ConsignFinalTime2 = null;
				}
			} else {
				ConsignRemainTime2.Value = "00:00:00";
			}
			if (SettingsStore.ConsignFinalTime3.HasValue) {
				var remainTime = (SettingsStore.ConsignFinalTime3.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					ConsignRemainTime3.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					ConsignRemainTime3.Value = "00:00:00";
					SettingsStore.ConsignFinalTime3 = null;
				}
			} else {
				ConsignRemainTime3.Value = "00:00:00";
			}
			if (SettingsStore.ConsignFinalTime4.HasValue) {
				var remainTime = (SettingsStore.ConsignFinalTime4.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					ConsignRemainTime4.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					ConsignRemainTime4.Value = "00:00:00";
					SettingsStore.ConsignFinalTime1 = null;
				}
			} else {
				ConsignRemainTime4.Value = "00:00:00";
			}
			// 戦術教室の残時間
			if (SettingsStore.LectureFinalTime1.HasValue) {
				var remainTime = (SettingsStore.LectureFinalTime1.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					LectureRemainTime1.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					LectureRemainTime1.Value = "00:00:00";
					SettingsStore.LectureFinalTime1 = null;
				}
			} else {
				LectureRemainTime1.Value = "00:00:00";
			}
			if (SettingsStore.LectureFinalTime2.HasValue) {
				var remainTime = (SettingsStore.LectureFinalTime2.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					LectureRemainTime2.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					LectureRemainTime2.Value = "00:00:00";
					SettingsStore.LectureFinalTime2 = null;
				}
			} else {
				LectureRemainTime2.Value = "00:00:00";
			}
			// 食糧の残時間
			if (SettingsStore.FoodFinalTime.HasValue) {
				var remainTime = (SettingsStore.FoodFinalTime.Value - nowTime);
				if (remainTime.TotalSeconds >= 0.0) {
					FoodRemainTime.Value = remainTime.ToString(@"hh\:mm\:ss");
				} else {
					FoodRemainTime.Value = "00:00:00";
					SettingsStore.FoodFinalTime = null;
				}
			} else {
				FoodRemainTime.Value = "00:00:00";
			}
			// 各種ボムの残時間
			if (SettingsStore.BombChageTime1.HasValue) {
				double remainTime = (SettingsStore.BombChageTime1.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime1.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime1.Value = "--.--";
					SettingsStore.BombChageTime1 = null;
				}
			} else {
				BombRemainTime1.Value = "--.--";
			}
			if (SettingsStore.BombChageTime2.HasValue) {
				double remainTime = (SettingsStore.BombChageTime2.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime2.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime2.Value = "--.--";
					SettingsStore.BombChageTime2 = null;
				}
			} else {
				BombRemainTime2.Value = "--.--";
			}
			if (SettingsStore.BombChageTime3.HasValue) {
				double remainTime = (SettingsStore.BombChageTime3.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime3.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime3.Value = "--.--";
					SettingsStore.BombChageTime3 = null;
				}
			} else {
				BombRemainTime3.Value = "--.--";
			}
		}

		// コンストラクタ
		public TimerViewModel() {
			// 設定ファイルに記録していた情報を書き戻す
			{
				if (SettingsStore.MemoryWindowPositionFlg) {
					WindowPositionLeft.Value = SettingsStore.TimerWindowRect[0];
					WindowPositionTop.Value = SettingsStore.TimerWindowRect[1];
					WindowPositionWidth.Value = SettingsStore.TimerWindowRect[2];
					WindowPositionHeight.Value = SettingsStore.TimerWindowRect[3];
				}
				AutoOpenWindowFlg.Value = SettingsStore.AutoTimerWindowFlg;
			}
			// 画面の位置が変更された際、自動で設定ファイルに書き戻すようにする
			WindowPositionLeft.Subscribe(value => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.TimerWindowRect[0] = value;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionTop.Subscribe(value => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.TimerWindowRect[1] = value;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionWidth.Subscribe(value => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.TimerWindowRect[2] = value;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionHeight.Subscribe(value => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.TimerWindowRect[3] = value;
				SettingsStore.ChangeSettingFlg = true;
			});
			// 起動時にこの画面を表示するか？
			AutoOpenWindowFlg.Subscribe(value => {
				SettingsStore.AutoTimerWindowFlg = value;
			});
			// タイマーを初期化し、定時タスクを登録して実行する
			// http://takachan.hatenablog.com/entry/2017/09/09/225342
			var timer = new Timer(100);
			timer.Elapsed += (sender, e) => {
				try {
					timer.Stop();
					RedrawTimerWindow();
				} finally { timer.Start(); }
			};
			timer.Start();
			//
			RedrawTimerWindow();
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
}
