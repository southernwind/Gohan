
using Reactive.Bindings;

using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// ウィンドウ状態
	/// </summary>
	public interface IWindowState : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// 幅
		/// </summary>
		IReactiveProperty<double> Width {
			get;
			set;
		}

		/// <summary>
		/// 高さ
		/// </summary>
		IReactiveProperty<double> Height {
			get;
			set;
		}

		/// <summary>
		/// 縦位置
		/// </summary>
		IReactiveProperty<double> Top {
			get;
			set;
		}

		/// <summary>
		/// 横位置
		/// </summary>
		IReactiveProperty<double> Left {
			get;
			set;
		}
	}
}
