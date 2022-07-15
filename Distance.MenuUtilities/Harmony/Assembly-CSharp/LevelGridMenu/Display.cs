using Distance.MenuUtilities.Scripts;
using HarmonyLib;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to create a manager for all <see cref="ModeCompleteStatusMenuLogic"/> components in
	/// the LevelGridMenu. Also restarts the animation timer every time the menu is displayed.
	/// </summary>
	/// <remarks>
	/// Required For: Mode Complete Status fix.
	/// </remarks>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.Display))]
	internal static class LevelGridMenu__Display
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelGridMenu __instance)
		{
			// Ensure our ModeCompleteStatusSyncLogic is created here, if this is the first call to Display.
			// Restart the global animation timer when entering the LevelGridMenu.
			var modeCompleteStatusSync = __instance.GetOrAddComponent<ModeCompleteStatusSyncLogic>();
			if (modeCompleteStatusSync)
			{
				modeCompleteStatusSync.RestartTimer();
			}
		}
	}
}
