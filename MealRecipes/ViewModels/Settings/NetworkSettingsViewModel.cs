using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	class NetworkSettingsViewModel : SettingsPageViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// 設定名
		/// </summary>
		public override string Name {
			get;
		} = "ネットワーク設定";

		/// <summary>
		/// 選択状態
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 値検証結果
		/// </summary>
		public override ReactiveProperty<bool> IsValidated {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// IPv4アドレス
		/// </summary>
		public ReactiveProperty<string> IpV4Address {
			get;
		}

		/// <summary>
		/// IPv6アドレス
		/// </summary>
		public ReactiveProperty<string> IpV6Address {
			get;
		}

		/// <summary>
		/// IPv4アドレス
		/// </summary>
		[Range(49152, 65535)]
		public ReactiveProperty<int> IpV4Port {
			get;
		}

		/// <summary>
		/// IPv6アドレス
		/// </summary>
		[Range(49152, 65535)]
		public ReactiveProperty<int> IpV6Port {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public NetworkSettingsViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.IpV6Address =
				this._settings
					.NetworkSettings
					.ToReactivePropertyAsSynchronized(x => x.IpV6Address)
					.SetValidateNotifyError(x =>
						IPAddress.TryParse(x, out var ip) && ip.GetAddressBytes().First() == 0b1111_1111 && ip.AddressFamily == AddressFamily.InterNetworkV6
							? null
							: "IPv6のマルチキャストアドレスではありません。"
					).AddTo(this.CompositeDisposable);

			this.IpV4Address =
				this._settings
					.NetworkSettings
					.ToReactivePropertyAsSynchronized(x => x.IpV4Address)
					.SetValidateNotifyError(x =>
						IPAddress.TryParse(x, out var ip) && ip.GetAddressBytes().First() >> 4 == 0b1110 && ip.AddressFamily == AddressFamily.InterNetwork
						? null
						: "IPv4のマルチキャストアドレスではありません。"
					).AddTo(this.CompositeDisposable);


			this.IpV6Port =
				this._settings
					.NetworkSettings
					.ToReactivePropertyAsSynchronized(x =>
					x.IpV6Port
					)
					.SetValidateAttribute(() => this.IpV6Port)
					.AddTo(this.CompositeDisposable);

			this.IpV4Port =
				this._settings
					.NetworkSettings
					.ToReactivePropertyAsSynchronized(x =>
					x.IpV4Port)
					.SetValidateAttribute(() => this.IpV4Port)
					.AddTo(this.CompositeDisposable);

			this.IsValidated = new[] {
					this.IpV4Address.ObserveHasErrors,
					this.IpV6Address.ObserveHasErrors,
					this.IpV4Port.ObserveHasErrors,
					this.IpV6Address.ObserveHasErrors,
				}.CombineLatestValuesAreAllFalse()
				.ToReactiveProperty()
				.AddTo(this.CompositeDisposable);
		}
	}
}
