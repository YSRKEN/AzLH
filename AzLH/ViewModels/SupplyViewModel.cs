using AzLH.Models;

namespace AzLH.ViewModels {
	internal class SupplyViewModel {
		// modelのinstance
		private readonly SupplyModel supplyModel;
		// コンストラクタ
		public SupplyViewModel() {
			// 初期化
			supplyModel = new SupplyModel();
		}
	}
}
