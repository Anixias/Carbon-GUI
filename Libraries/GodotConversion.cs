using System.Collections.Generic;

namespace Godot
{
	namespace Conversion
	{
		public static class CollectionExtensions
		{
			public static Dictionary<K, V> Convert<K, V>(this Godot.Collections.Dictionary dictionary)
			{
				var output = new Dictionary<K, V>();

				foreach (var key in dictionary.Keys)
				{
					if (key is K typedKey && dictionary[key] is V typedValue)
					{
						output[typedKey] = typedValue;
					}
				}

				return output;
			}

			public static Dictionary<K, V> Convert<K, V>(this Godot.Collections.Dictionary<K, V> dictionary)
			{
				var output = new Dictionary<K, V>();

				foreach (var key in dictionary.Keys)
				{
					output[key] = dictionary[key];
				}

				return output;
			}

			public static T[] ToArray<T>(this Godot.Collections.Array array, T defaultValue)
			{
				var output = new T[array.Count];

				for (var i = 0; i < array.Count; i++)
				{
					if (array[i] is T value)
					{
						output[i] = value;
					}
					else
					{
						output[i] = defaultValue;
					}
				}

				return output;
			}

			public static T[] ToArray<T>(this Godot.Collections.Array<T> array)
			{
				var output = new T[array.Count];

				for (var i = 0; i < array.Count; i++)
				{
					output[i] = array[i];
				}

				return output;
			}

			public static List<T> ToList<T>(this Godot.Collections.Array array, T defaultValue)
			{
				var output = new List<T>();

				foreach (var value in array)
				{
					if (value is T)
					{
						output.Add((T)value);
					}
					else
					{
						output.Add(defaultValue);
					}
				}

				return output;
			}

			public static List<T> ToList<T>(this Godot.Collections.Array<T> array)
			{
				var output = new List<T>();

				foreach (var value in array)
				{
					output.Add(value);
				}

				return output;
			}
		}
	}
}
