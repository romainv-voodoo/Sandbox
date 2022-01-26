using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{
	public static class JsonHelper
	{
		/// <summary>
		/// Save the item _item to a json file at path _filePath
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_filePath"></param>
		/// <param name="_item"></param>
		/// <param name="append"></param>
		public static void Save<T>(string _filePath, T _item, bool append = false)
		{
			string json = JsonUtility.ToJson(_item, true);

			if (append)
				File.AppendAllText(_filePath, json);
			else
				File.WriteAllText(_filePath, json);
		}

		/// <summary>
		/// Save the item _item to a json array file at path _filePath
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_filePath"></param>
		/// <param name="_item"></param>
		/// <param name="append"></param>
		public static void SaveArray<T>(string _filePath, T[] _item, bool append = false)
		{
			string json = ToJsonArray(_item);

			if (append)
				File.AppendAllText(_filePath, json);
			else
				File.WriteAllText(_filePath, json);
		}

		/// <summary>
		/// Load a json file at path _filePath and extract the class from it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_filePath"></param>
		/// <returns></returns>
		public static T Load<T>(string _filePath)
		{
			if (!File.Exists(_filePath))
				return default;

			string json = File.ReadAllText(_filePath);

			return (T)JsonUtility.FromJson(json, typeof(T));
		}

		/// <summary>
		/// Load a json array file at path _filePath and extract the class from it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_filePath"></param>
		/// <returns></returns>
		public static List<T> LoadArray<T>(string _filePath)
		{
			if (!File.Exists(_filePath))
				return new List<T>();

			string json = File.ReadAllText(_filePath);

			return FromJsonArrayWithWrapper<T>(json).ToList();
		}

		//Usage:
		//YouObject[] objects = JsonHelper.FromJsonArray<YouObject> (jsonString);
		public static T[] FromJsonArray<T>(string json)
		{
			string newJson = "{ \"array\": " + json + "}";
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			return wrapper.array;
		}

		//Usage:
		//YouObject[] objects = JsonHelper.FromJsonArrayWithWrapper<YouObject> (jsonString);
		public static T[] FromJsonArrayWithWrapper<T>(string json)
		{
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
			return wrapper.array;
		}

		//Usage:
		//string jsonString = JsonHelper.ToJsonArray<YouObject>(objects);
		public static string ToJsonArray<T>(T[] array)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.array = array;
			return JsonUtility.ToJson(wrapper, true);
		}

		[System.Serializable]
		private class Wrapper<T>
		{
			public T[] array;
		}
	}
}