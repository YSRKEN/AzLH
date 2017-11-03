using Prism.Events;

namespace AzLH.Models {
	class Messenger : EventAggregator {
		private static Messenger instance;

		public static Messenger Instance {
			get => instance ?? (instance = new Messenger());
		}
	}
}
