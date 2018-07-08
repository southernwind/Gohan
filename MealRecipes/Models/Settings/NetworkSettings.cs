using Livet;

using SandBeige.MealRecipes.Composition.Settings;

using System.Reactive.Disposables;

namespace SandBeige.MealRecipes.Models.Settings {
	/// <summary>
	/// ネットワーク設定
	/// </summary>
	public class NetworkSettings : NotificationObject, INetworkSettings {
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		/// <summary>
		/// IPv4アドレス
		/// </summary>
		public string IpV4Address {
			get;
			set;
		} = "224.0.0.201";

		/// <summary>
		/// IPv6アドレス
		/// </summary>
		public string IpV6Address {
			get;
			set;
		} = "FF02::3333";

		/// <summary>
		/// IPv4ポート
		/// </summary>
		public int IpV4Port {
			get;
			set;
		} = 54126;

		/// <summary>
		/// IPv6ポート
		/// </summary>
		public int IpV6Port {
			get;
			set;
		} = 54126;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public NetworkSettings() {
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
