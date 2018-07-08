using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xaml;

namespace SandBeige.MealRecipes.Extensions {
	/// <summary>
	/// シリアライズ拡張
	/// </summary>
	public static class SerializeExtension {
		/// <summary>
		/// シリアライズ
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte[] Serialize<T>(this T value) {
			if (value.GetType().IsValueType) {
				switch (value) {
					case int _:
					case long _:
					case bool _:
					case char _:
					case short _:
					case ushort _:
					case uint _:
					case ulong _:
					case float _:
					case double _:
						return BitConverter.GetBytes((dynamic)value);
					case byte[] val:
						return val;
					default:
						var size = Marshal.SizeOf(value);
						var bytes = new byte[size];
						var ptr = Marshal.AllocHGlobal(size);
						Marshal.StructureToPtr(value, ptr, false);

						Marshal.Copy(ptr, bytes, 0, size);

						Marshal.FreeHGlobal(ptr);
						return bytes;
				}
			}

			switch (value) {
				case string s:
					return Encoding.UTF8.GetBytes(s);
				default:
					return Encoding.UTF8.GetBytes(XamlServices.Save(value));
			}
		}

		/// <summary>
		/// デシリアライズ
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static T Deserialize<T>(this byte[] bytes) {
			if (bytes == null) {
				return (dynamic)null;
			}
			if (typeof(T).IsValueType) {
				if (typeof(T) == typeof(int)) {
					return (dynamic)BitConverter.ToInt32(bytes, 0);
				} else if (typeof(T) == typeof(long)) {
					return (dynamic)BitConverter.ToInt64(bytes, 0);
				} else if (typeof(T) == typeof(bool)) {
					return (dynamic)BitConverter.ToBoolean(bytes, 0);
				} else if (typeof(T) == typeof(char)) {
					return (dynamic)BitConverter.ToChar(bytes, 0);
				} else if (typeof(T) == typeof(short)) {
					return (dynamic)BitConverter.ToInt16(bytes, 0);
				} else if (typeof(T) == typeof(ushort)) {
					return (dynamic)BitConverter.ToUInt16(bytes, 0);
				} else if (typeof(T) == typeof(uint)) {
					return (dynamic)BitConverter.ToUInt32(bytes, 0);
				} else if (typeof(T) == typeof(ulong)) {
					return (dynamic)BitConverter.ToUInt64(bytes, 0);
				} else if (typeof(T) == typeof(float)) {
					return (dynamic)BitConverter.ToSingle(bytes, 0);
				} else if (typeof(T) == typeof(double)) {
					return (dynamic)BitConverter.ToDouble(bytes, 0);
				} else if (typeof(T) == typeof(byte[])) {
					return (dynamic)bytes;
				} else {
					var size = bytes.Length;
					var ptr = Marshal.AllocHGlobal(size);

					Marshal.Copy(bytes, 0, ptr, size);
					var val = Marshal.PtrToStructure(ptr, typeof(T));
					Marshal.FreeHGlobal(ptr);
					return (dynamic)val;
				}
			} else {
				if (typeof(T) == typeof(string)) {
					return (dynamic)Encoding.UTF8.GetString(bytes);
				} else {
					using (var sr = new StreamReader(new MemoryStream(bytes), Encoding.UTF8)) {
						return (T)XamlServices.Load(sr);
					}
				}
			}
		}
	}
}
