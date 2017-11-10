﻿using AzLH.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Timers;

namespace AzLH.ViewModels {
	class MainViewModel {
		// modelのinstance
		private MainModel mainModel;
		// 画像保存ボタンは有効か？
		public ReactiveProperty<bool> SaveScreenshotFlg { get; }
		// 実行ログ
		public ReactiveProperty<string> ApplicationLog { get; }
		// 座標取得ボタン
		public ReactiveCommand GetGameWindowPositionCommand { get; }
		// 画像保存ボタン
		public ReactiveCommand SaveScreenshotCommand { get; }
		// シーン表示
		public ReactiveProperty<string> JudgedScene { get; }
		// コンストラクタ
		public MainViewModel() {
			// 初期化
			mainModel = new MainModel();
			// プロパティを設定
			SaveScreenshotFlg = mainModel.ObserveProperty(x => x.SaveScreenshotFlg).ToReactiveProperty();
			ApplicationLog = mainModel.ObserveProperty(x => x.ApplicationLog).ToReactiveProperty();
			JudgedScene = mainModel.ObserveProperty(x => x.JudgedScene).ToReactiveProperty();
			// コマンドを設定
			GetGameWindowPositionCommand = new ReactiveCommand();
			SaveScreenshotCommand = new ReactiveCommand();
			//voidを返すメソッドならこれだけで良いらしい
			//https://qiita.com/pierusan2010/items/76b7a406b3f064193c88
			GetGameWindowPositionCommand.Subscribe(mainModel.GetGameWindowPosition);
			SaveScreenshotCommand.Subscribe(mainModel.SaveScreenshot);
			// タイマーを初期化し、定時タスクを登録して実行する
			// http://takachan.hatenablog.com/entry/2017/09/09/225342
			var timer = new Timer(200);
			timer.Elapsed += (sender, e) => {
				try{
					timer.Stop();
					mainModel.HelperTask();
				}
				finally{timer.Start();}
			};
			timer.Start();
		}
	}
}
