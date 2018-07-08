
using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// ネットワーク設定
	/// </summary>
	public interface INetworkSettings : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// IPv4アドレス
		/// XamlServicesでIPAddress型をシリアライズできないのでstring型とする
		/// </summary>
		string IpV4Address {
			get;
			set;
		}

		/// <summary>
		/// IPv6アドレス
		/// XamlServicesでIPAddress型をシリアライズできないのでstring型とする
		/// </summary>
		string IpV6Address {
			get;
			set;
		}

		/// <summary>
		/// IPv4ポート
		/// </summary>
		int IpV4Port {
			get;
			set;
		}

		/// <summary>
		/// IPv6ポート
		/// </summary>
		int IpV6Port {
			get;
			set;
		}

	}
}
