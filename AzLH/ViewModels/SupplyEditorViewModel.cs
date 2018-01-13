using AzLH.Models;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows;

namespace AzLH.ViewModels
{
	class SupplyEditorViewModel
	{
		public delegate void Action1(string oldTime, int oldValue, string newTime, int newValue);
		public delegate void Action2(string oldTime, int oldValue);

		public ReactiveProperty<List<string>> SupplyNameList { get; }
		public ReactiveProperty<int> SupplyNameIndex { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<List<SupplyData>> SupplyDataList { get; }

		// データを更新する
		private void EditSupplyData(string oldTime, int oldValue, string newTime, int newValue) {
			// 更新していない場合は無視する
			if (oldTime == newTime && oldValue == newValue)
				return;
			// 更新するかを確認する
			var result = MessageBox.Show($"以下のデータを更新します。よろしいですか？\n種類：{SupplyNameList.Value[SupplyNameIndex.Value]}\n旧データ：{oldTime}　{oldValue}\n新データ：{newTime}　{newValue}", Utility.SoftwareName, MessageBoxButton.YesNo);
			if(result != MessageBoxResult.Yes)
				return;
			// 更新操作を行う(スタブ)
			// リストを再作成する
			RemakeSupplyDataList();
			return;
		}
		// データを削除する
		private void DeleteSupplyData(string oldTime, int oldValue) {
			// 更新するかを確認する
			var result = MessageBox.Show($"以下のデータを削除します。よろしいですか？\n種類：{SupplyNameList.Value[SupplyNameIndex.Value]}\nデータ：{oldTime}　{oldValue}", Utility.SoftwareName, MessageBoxButton.YesNo);
			if (result != MessageBoxResult.Yes)
				return;
			// 削除操作を行う(スタブ)
			// リストを再作成する
			RemakeSupplyDataList();
			return;
		}
		// リストを再作成する
		private void RemakeSupplyDataList() {
			var newSupplyDataList = new List<SupplyData>();
			var supplyData = SupplyStore.GetSupplyData(SupplyNameList.Value[SupplyNameIndex.Value]);
			foreach (var pair in supplyData) {
				var temp = new SupplyData(pair.Key, pair.Value, EditSupplyData, DeleteSupplyData);
				newSupplyDataList.Add(temp);
			}
			SupplyDataList.Value = newSupplyDataList;
		}

		// コンストラクタ
		public SupplyEditorViewModel() {
			SupplyNameList = new ReactiveProperty<List<string>>(CharacterRecognition.SupplyParameters.Keys.Select(p => p).ToList());
			SupplyDataList = new ReactiveProperty<List<SupplyData>>(new List<SupplyData>());
			SupplyNameIndex.Subscribe(index => {
				var newSupplyDataList = new List<SupplyData>();
				var supplyData = SupplyStore.GetSupplyData(SupplyNameList.Value[index]);
				foreach(var pair in supplyData) {
					var temp = new SupplyData(pair.Key, pair.Value, EditSupplyData, DeleteSupplyData);
					newSupplyDataList.Add(temp);
				}
				SupplyDataList.Value = newSupplyDataList;
			});
		}

		// 各行毎の資材データ
		public class SupplyData
		{
			// 旧データ
			private string oldTime;
			private int oldValue;
			// 現データ
			public string Time { get; set; }
			public int Value { get; set; }
			// delegate
			private Action1 action1;
			private Action2 action2;
			// 編集・削除コマンド
			public ReactiveCommand EditCommand { get; } = new ReactiveCommand();
			public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();
			// コンストラクタ
			public SupplyData(DateTime time, int value, Action1 action1, Action2 action2) {
				// 数値を記録する
				oldTime = Time = Utility.GetTimeStrSQLite(time);
				oldValue = Value = value;
				// コマンドを登録する
				EditCommand.Subscribe(_ => action1(oldTime, oldValue, Time, Value));
				DeleteCommand.Subscribe(_ => action2(oldTime, oldValue));
			}
		}
	}
}
