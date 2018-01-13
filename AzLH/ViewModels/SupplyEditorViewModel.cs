using AzLH.Models;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AzLH.ViewModels
{
	class SupplyEditorViewModel
	{
		public ReactiveProperty<List<string>> SupplyNameList { get; }
		public ReactiveProperty<int> SupplyNameIndex { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<List<SupplyData>> SupplyDataList { get; }

		// コンストラクタ
		public SupplyEditorViewModel() {
			SupplyNameList = new ReactiveProperty<List<string>>(CharacterRecognition.SupplyParameters.Keys.Select(p => p).ToList());
			SupplyDataList = new ReactiveProperty<List<SupplyData>>(new List<SupplyData>());
			SupplyNameIndex.Subscribe(index => {
				SupplyDataList.Value.Clear();
				var supplyData = SupplyStore.GetSupplyData(SupplyNameList.Value[index]);
				foreach(var pair in supplyData) {
					var temp = new SupplyData();
					temp.Time = Utility.GetTimeStrLong(pair.Key);
					temp.Value = pair.Value;
					SupplyDataList.Value.Add(temp);
				}
			});
		}

		// 各行毎の資材データ
		public class SupplyData
		{
			public string Time { get; set; }
			public int Value { get; set; }
			public ReactiveCommand EditCommand { get; } = new ReactiveCommand();
			public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();
		}
	}
}
