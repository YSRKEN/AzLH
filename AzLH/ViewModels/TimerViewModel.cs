using AzLH.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;

namespace AzLH.ViewModels
{
	class TimerViewModel
	{
		// modelのinstance
		private readonly TimerModel timerModel = new TimerModel();
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 起動時にこの画面を表示するか？
		public ReactiveProperty<bool> AutoOpenWindowFlg { get; } = new ReactiveProperty<bool>(false);
		// 画面の位置
		public ReactiveProperty<double> WindowPositionLeft { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionTop { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionWidth { get; } = new ReactiveProperty<double>(400.0);
		public ReactiveProperty<double> WindowPositionHeight { get; } = new ReactiveProperty<double>(250.0);
		// 各種ボムの残時間
		public ReactiveProperty<string> BombRemainTime1 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BombRemainTime2 { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BombRemainTime3 { get; } = new ReactiveProperty<string>("");

		// タイマー表示の更新を行う
		private void RedrawTimerWindow() {
			var setting = SettingsStore.Instance;
			var nowTime = DateTime.Now;
			if (setting.BombChageTime1.HasValue) {
				double remainTime = (setting.BombChageTime1.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime1.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime1.Value = "--.--";
					setting.BombChageTime1 = null;
				}
			} else {
				BombRemainTime1.Value = "--.--";
			}
			if (setting.BombChageTime2.HasValue) {
				double remainTime = (setting.BombChageTime2.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime2.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime2.Value = "--.--";
					setting.BombChageTime2 = null;
				}
			} else {
				BombRemainTime2.Value = "--.--";
			}
			if (setting.BombChageTime3.HasValue) {
				double remainTime = (setting.BombChageTime3.Value - nowTime).TotalSeconds;
				if (remainTime >= 0.0) {
					BombRemainTime3.Value = remainTime.ToString("00.0");
				} else {
					BombRemainTime3.Value = "--.--";
					setting.BombChageTime3 = null;
				}
			} else {
				BombRemainTime3.Value = "--.--";
			}
		}

		// コンストラクタ
		public TimerViewModel() {
			// 設定ファイルに記録していた情報を書き戻す
			{
				var settings = SettingsStore.Instance;
				if (settings.MemoryWindowPositionFlg) {
					WindowPositionLeft.Value = settings.TimerWindowRect[0];
					WindowPositionTop.Value = settings.TimerWindowRect[1];
					WindowPositionWidth.Value = settings.TimerWindowRect[2];
					WindowPositionHeight.Value = settings.TimerWindowRect[3];
				}
				AutoOpenWindowFlg.Value = settings.AutoSupplyWindowFlg;
			}
			// 画面の位置が変更された際、自動で設定ファイルに書き戻すようにする
			WindowPositionLeft.Subscribe(value => {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				settings.TimerWindowRect[0] = value;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			});
			WindowPositionTop.Subscribe(value => {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				settings.TimerWindowRect[1] = value;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			});
			WindowPositionWidth.Subscribe(value => {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				settings.TimerWindowRect[2] = value;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			});
			WindowPositionHeight.Subscribe(value => {
				var settings = SettingsStore.Instance;
				if (!settings.MemoryWindowPositionFlg)
					return;
				settings.TimerWindowRect[3] = value;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			});
			// 起動時にこの画面を表示するか？
			AutoOpenWindowFlg.Subscribe(value => {
				var settings = SettingsStore.Instance;
				settings.AutoTimerWindowFlg = value;
				if (!settings.SaveSettings()) {
					MessageBox.Show("設定を保存できませんでした。", Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
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
		}
	}
}
