using Livet;
using Livet.Messaging.Windows;

using Reactive.Bindings;

using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SandBeige.MealRecipes.Composition.Dialog {
	public class DialogWindowViewModel : ViewModel {
		public enum DialogResult {
			Yes,
			No,
			Cancel,
			Ok
		}

		/// <summary>
		/// 有効な選択肢一覧
		/// </summary>
		public ReactiveCollection<Option> EnabledOptions {
			get;
		}

		/// <summary>
		/// メッセージ
		/// </summary>
		public ReactiveProperty<string> Message {
			get;
		} = new ReactiveProperty<string>();

		/// <summary>
		/// YESコマンド
		/// </summary>
		public ReactiveCommand YesCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// OKコマンド
		/// </summary>
		public ReactiveCommand OkCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// NOコマンド
		/// </summary>
		public ReactiveCommand NoCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// CANCELコマンド
		/// </summary>
		public ReactiveCommand CancelCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 選択結果
		/// </summary>
		public DialogResult? Result {
			get;
			private set;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="title">メッセージウィンドウタイトル</param>
		/// <param name="message">メッセージ文章</param>
		/// <param name="enableOptions">選択肢一覧</param>
		public DialogWindowViewModel(string title, string message, params DialogResult[] enableOptions) {
			this.Message.Value = message;

			var options = new[] {
				new Option("Yes", DialogResult.Yes, this.YesCommand,false,true),
				new Option("Ok", DialogResult.Ok, this.OkCommand,false,true),
				new Option("No", DialogResult.No, this.NoCommand),
				new Option("Cancel", DialogResult.Cancel, this.CancelCommand, true)
			};

			this.EnabledOptions = new ReactiveCollection<Option>(options.Where(x => enableOptions.Contains(x.DialogResult)).ToObservable());

			this.YesCommand.Subscribe(() => {
				this.Result = DialogResult.Yes;
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			});
			this.OkCommand.Subscribe(() => {
				this.Result = DialogResult.Ok;
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			});
			this.NoCommand.Subscribe(() => {
				this.Result = DialogResult.No;
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			});
			this.CancelCommand.Subscribe(() => {
				this.Result = DialogResult.Cancel;
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			});
		}

		public class Option {
			public DialogResult DialogResult {
				get;
			}

			public string Name {
				get;
			}

			public ICommand Command {
				get;
			}

			public bool IsCancel {
				get;
			}

			public bool IsDefault {
				get;
			}

			public Option(string name, DialogResult dialogResult, ICommand command, bool isCancel = false, bool isDefault = false) {
				this.Name = name;
				this.DialogResult = dialogResult;
				this.Command = command;
				this.IsCancel = isCancel;
				this.IsDefault = isDefault;
			}

		}
	}
}
