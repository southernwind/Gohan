using System;
using System.Collections.Generic;
using System.Linq;

namespace SandBeige.MealRecipes.Extensions {
	public static class EnumerableEx {
		/// <summary>
		/// 実行
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<T> action) {
			foreach (var item in source) {
				action(item);
				yield return item;
			}
		}

		/// <summary>
		/// SQLのLEFT OUTER JOINのような動きをするLINQ拡張
		/// outer側に値が存在すればinner側に値が存在しない場合でもinner側はデフォルト値で結合する。
		/// defaultがnull以外の型ではデフォルト値が結合されるため、値型などでは扱いに要注意
		/// </summary>
		/// <typeparam name="TOuter">最初のシーケンスの要素の型。</typeparam>
		/// <typeparam name="TInner">2 番目のシーケンスの要素の型。</typeparam>
		/// <typeparam name="TKey">キー セレクター関数によって返されるキーの型。</typeparam>
		/// <typeparam name="TResult">結果の要素の型。</typeparam>
		/// <param name="outer">結合する最初のシーケンス。</param>
		/// <param name="inner">最初のシーケンスに結合するシーケンス。</param>
		/// <param name="outerKeySelector">最初のシーケンスの各要素から結合キーを抽出する関数。</param>
		/// <param name="innerKeySelector">
		///   2 番目のシーケンスの各要素から結合キーを抽出する関数。
		/// </param>
		/// <param name="resultSelector">
		///   最初のシーケンスの要素と、2 番目のシーケンスの一致する要素のコレクションから結果の要素を作成する関数。
		/// </param>
		/// <returns>
		///    2 つのシーケンスを左外部結合した結果の <see cref="T:System.Collections.Generic.IEnumerable`1" />
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="outer" /> または <paramref name="inner" /> または <paramref name="outerKeySelector" /> または <paramref name="innerKeySelector" /> または <paramref name="resultSelector" /> は <see langword="null" />です。
		/// </exception>
		public static IEnumerable<TResult> LeftOuterJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector) {
			return outer
				.GroupJoin(
					inner,
					outerKeySelector,
					innerKeySelector,
					(o, i) => new {
						Out = o,
						Ins = i.DefaultIfEmpty()
					}).SelectMany(
					g => g.Ins,
					(g, i) => resultSelector(g.Out, i));
		}
	}
}
