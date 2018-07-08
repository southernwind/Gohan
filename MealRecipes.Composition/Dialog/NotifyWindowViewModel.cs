using Livet;
using Livet.Messaging.Windows;

using Reactive.Bindings;

using System;
using System.Reactive;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Composition.Dialog {
	public class NotifyWindowViewModel : ViewModel {
		/// <summary>
		/// メッセージ
		/// </summary>
		public ReactiveProperty<string> Message {
			get;
		} = new ReactiveProperty<string>();

		/// <summary>
		/// 不透明度
		/// </summary>
		public ReactiveProperty<double> Opacity {
			get;
		}

		/// <summary>
		/// 閉じるコマンド
		/// </summary>
		public ReactiveCommand CloseCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="message">メッセージ</param>
		/// <param name="seconds">表示時間</param>
		public NotifyWindowViewModel(string message, int seconds) {
			this.Message.Value = message;

			var dueTime = seconds / 2d;
			var interval = seconds / 2d / 0xff;

			// 表示時間の1/2が経過した時点で透過がはじまり、表示時間が終了するまで少しずつ透過していく
			this.Opacity =
				Observable.Timer(
						TimeSpan.FromSeconds(dueTime),
						TimeSpan.FromSeconds(interval),
						UIDispatcherScheduler.Default
					)
					.Select(x => (0xff - x) / (double)0xff)
					.TakeWhile(x => x >= 0)
					.ToReactiveProperty(1);

			// 閉じるコマンド発行、もしくは完全に透明になった時点でCloseアクションを発行する
			this.Opacity
				.Where(x => x <= 0)
				.Select(_ => Unit.Default)
				.Merge(this.CloseCommand.Select(_ => Unit.Default))
				.Subscribe(_ => {
					this.Messenger.Raise(new WindowActionMessage("Close"));
				});
		}
	}
}
