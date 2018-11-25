using System;
namespace FileManagerLib.Extensions.Collections
{
    /// <summary>
    /// 配列関連の拡張メソッドを提供します。
    /// </summary>
	public static class ArrayExtension
	{
        /// <summary>
        /// 配列の各要素に対して、指定されたデリゲートの処理を実行します。
        /// </summary>
        /// <typeparam name="T">配列の総称型</typeparam>
        /// <param name="array">対象の配列</param>
        /// <param name="action">実行するデリゲート</param>
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			foreach (var val in array)
				action(val);
		}
	}
}
