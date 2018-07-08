using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xaml;

namespace SandBeige.MealRecipes.Models.Notifier {
	public class DbChangeNotifier : IDbChangeNotifier {
		private readonly string _identifier;
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly int _ipv4Port;
		private readonly int _ipv6Port;
		private readonly IPAddress _ipv4Address;
		private readonly IPAddress _ipv6Address;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		private Subject<Exception> _error = new Subject<Exception>();
		public IObservable<Exception> Error {
			get {
				return this._error.AsObservable();
			}
		}

		private Subject<DbChangeArgs> _received = new Subject<DbChangeArgs>();
		public IObservable<DbChangeArgs> Received {
			get {
				return this._received.AsObservable();
			}
		}

		private IEnumerable<UnicastIPAddressInformation> _nicAddresses {
			get {
				return
					NetworkInterface
						.GetAllNetworkInterfaces()
						.Where(x => x.OperationalStatus == OperationalStatus.Up)
						.SelectMany(x => x.GetIPProperties().UnicastAddresses)
						.Where(x => x.Address.ToString() != "127.0.0.1" && x.Address.ToString() != "::1");
			}
		}


		public DbChangeNotifier(ILogger logger, ISettings settings) {
			// 今の作りだとIP、ポートを変更するチャンスは変更通知受信時だけなので、コンストラクタで設定して以後変更しない
			// もし設定変更時に変更を反映する場合、IP、ポートからの変更通知を受けてUDPクライアントを作り直すこと
			this._settings = settings;
			this._logger = logger;
			this._ipv4Port = this._settings.NetworkSettings.IpV4Port;
			this._ipv6Port = this._settings.NetworkSettings.IpV6Port;
			this._ipv4Address = IPAddress.Parse(this._settings.NetworkSettings.IpV4Address);
			this._ipv6Address = IPAddress.Parse(this._settings.NetworkSettings.IpV6Address);
			this._identifier = Guid.NewGuid().ToString();
			this._received.AddTo(this._disposable);
			this._error.AddTo(this._disposable);
			Listen();
		}

		// 変更通知送信
		public void Notify(string[] tables) {
			var args = new DbChangeArgs(this._identifier, tables);
			this._logger.Log(LogLevel.Notice, $"変更通知送信 {args.Source} : [{string.Join(", ", args.TableNames)}]");
			using (var ms = new MemoryStream()) {
				XamlServices.Save(ms, args);
				foreach (var address in this._nicAddresses) {
					using (var udpClient = new UdpClient(new IPEndPoint(address.Address, 0))) {
						try {
							if (address.Address.AddressFamily == AddressFamily.InterNetwork) {
								var remoteAddress = this._ipv4Address;
								udpClient.Send(ms.ToArray(), (int)ms.Length, new IPEndPoint(remoteAddress, this._ipv4Port));
							} else if (address.Address.AddressFamily == AddressFamily.InterNetworkV6) {
								var remoteAddress = this._ipv6Address;
								udpClient.Send(ms.ToArray(), (int)ms.Length, new IPEndPoint(remoteAddress, this._ipv6Port));
							} else {
								continue;
							}
						} catch (Exception e) {
							this._logger.Log(LogLevel.Warning, $"変更通知送信失敗", e);
							Console.WriteLine(e);
						}
					}
				}
			}
		}

		// 変更通知受信開始
		private void Listen() {
			void func(IAsyncResult result) {
				var udpClient = ((UdpState)(result.AsyncState)).UdpClient;
				var ipEndPoint = ((UdpState)(result.AsyncState)).IpEndPoint;
				try {
					var data = udpClient.EndReceive(result, ref ipEndPoint);
					var receivedObject = XamlServices.Load(new MemoryStream(data));
					if (receivedObject is DbChangeArgs args) {
						if (args.Source != this._identifier) {
							this._logger.Log(LogLevel.Notice, $"変更通知受信 {args.Source} : [{string.Join(", ", args.TableNames)}]");
							this._received.OnNext(args);
						}
					}
				} catch (Exception e) {
					this._logger.Log(LogLevel.Warning, $"変更通知受信失敗", e);
					this._error.OnNext(e);
				}
				udpClient.BeginReceive(func, new UdpState(udpClient, ipEndPoint));
			}

			foreach (var address in this._nicAddresses) {
				IPAddress remoteAddress = null;
				var port = 0;
				if (address.Address.AddressFamily == AddressFamily.InterNetwork) {
					port = this._ipv4Port;
					remoteAddress = this._ipv4Address;
				} else if (address.Address.AddressFamily == AddressFamily.InterNetworkV6) {
					port = this._ipv6Port;
					remoteAddress = this._ipv6Address;
				} else {
					continue;
				}
				var ipEndPoint = new IPEndPoint(address.Address, port);
				var client = new UdpClient(ipEndPoint);
				client.JoinMulticastGroup(remoteAddress);
				client.BeginReceive(func, new UdpState(client, ipEndPoint));
			}
		}

		public void Dispose() {
			this._disposable.Dispose();
		}

		public class UdpState {
			public UdpState(UdpClient udpClient, IPEndPoint ipEndPoint) {
				this.UdpClient = udpClient;
				this.IpEndPoint = ipEndPoint;
			}
			public UdpClient UdpClient {
				get;
				set;
			}
			public IPEndPoint IpEndPoint {
				get;
				set;
			}
		}
	}
}
