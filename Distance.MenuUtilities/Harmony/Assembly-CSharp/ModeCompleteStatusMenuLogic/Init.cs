using Distance.MenuUtilities.Scripts;
using HarmonyLib;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to handle setup of the timers for <see cref="ModeCompleteStatusMenuLogic"/> components,
	/// so that their initial time on first appearance in the <see cref="MenuButtonList"/> will be synced
	/// with others in the same list.
	/// <para/>
	/// NOTE: This patch is effectively unneeded, because there are no scenarios where scrolling happens to
	/// cause the janky behavior in the main menu. But it's kept here in-case mods decided the Main Menu needs
	/// 50+ arcade modes.
	/// </summary>
	/// <remarks>
	/// Required For: Mode Complete Status fix (part 1/3).
	/// </remarks>
	[HarmonyPatch(typeof(ModeCompleteStatusMenuLogic), nameof(ModeCompleteStatusMenuLogic.Init))]
	internal static class ModeCompleteStatusMenuLogic__Init
	{
		[HarmonyPrefix]
		internal static void Prefix(ModeCompleteStatusMenuLogic __instance)
		{
			// Ensure our ModeCompleteStatusSyncLogic is created here for the parent MenuButtonList.
			// This function is only called by MenuButtonList.Init, so we know we're a button in the Main Menu.
			// Setup individual global status timers per submenu.
			// NOTE: This fix isn't exactly necessary, since there's never enough submenu buttons to require scrolling,
			//       but we should handle it just in-case another mod decides we need 50+ game modes. :)
			var menuButtonList = __instance.GetComponentInParents<MenuButtonList>();
			if (menuButtonList)
			{
				/*var modeCompleteStatusSync =*/
				menuButtonList.GetOrAddComponent<ModeCompleteStatusSyncLogic>();
				// Timer does not need to be restarted, since this function is only called on Main Menu setup.
				// Also Main Menu mode complete status progress bars ONLY reset when the Main Menu is recreated.
				//  i.e. After exiting a level or the level editor.
				/*if (modeCompleteStatusSync)
				{
					modeCompleteStatusSync.RestartTimer();
				}*/
			}
		}
	}
}
