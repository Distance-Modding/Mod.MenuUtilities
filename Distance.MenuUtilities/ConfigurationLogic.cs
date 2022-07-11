using Reactor.API.Configuration;
using System;
using UnityEngine;

namespace Distance.MenuUtilities
{
	public class ConfigurationLogic : MonoBehaviour
	{
		#region Properties
		private const string EnableDeletePlaylistButton_ID = "EnableDeletePlaylistButton";
		public bool EnableDeletePlaylistButton
		{
			get => Get<bool>(EnableDeletePlaylistButton_ID);
			set => Set(EnableDeletePlaylistButton_ID, value);
		}

		private const string EnableHexColorInput_ID = "EnableHexColorInput";
		public bool EnableHexColorInput
		{
			get => Get<bool>(EnableHexColorInput_ID);
			set => Set(EnableHexColorInput_ID, value);
		}

		private const string EnableSliderPageButtons_ID = "EnableSliderPageButtons";
		public bool EnableSliderPageButtons
		{
			get => Get<bool>(EnableSliderPageButtons_ID);
			set => Set(EnableSliderPageButtons_ID, value);
		}

		private const string EnableCompletionProgressBarFix_ID = "EnableCompletionProgressBarFix";
		public bool EnableCompletionProgressBarFix
		{
			get => Get<bool>(EnableCompletionProgressBarFix_ID);
			set => Set(EnableCompletionProgressBarFix_ID, value);
		}
		#endregion

		internal Settings Config;

		public event Action<ConfigurationLogic> OnChanged;

		private void Load()
		{
			Config = new Settings("Config");
		}

		public void Awake()
		{
			Load();

			Get(EnableDeletePlaylistButton_ID, true);
			Get(EnableHexColorInput_ID, true);
			Get(EnableSliderPageButtons_ID, true);
			Get(EnableCompletionProgressBarFix_ID, true);

			Save();
		}

		public T Get<T>(string key, T @default = default)
		{
			return Config.GetOrCreate(key, @default);
		}

		public void Set<T>(string key, T value)
		{
			Config[key] = value;
			Save();
		}

		public void Save()
		{
			Config?.Save();
			OnChanged?.Invoke(this);
		}
	}
}