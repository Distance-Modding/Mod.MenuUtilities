using Distance.MenuUtilities.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to add the DELETE PLAYLIST button to the bottom-left button list when entering
	/// a Level Set grid view.
	/// </summary>
	/// <remarks>
	/// Required For: Delete Playlist Button (part 1/3).
	/// </remarks>
	[HarmonyPatch(typeof(LevelGridGrid), nameof(LevelGridGrid.PushGrid))]
	internal static class LevelGridGrid__PushGrid
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Hook just before `menuPanel.Push()` so that we can add additional bottom-left buttons.
			//menuPanel.Push();
			//this.GridPushChange();
			// -to-
			//AddMenuPanelButtons_(this);
			//menuPanel.Push();
			//this.GridPushChange();

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 2].operand).Name == "Push") &&
					(codes[i    ].opcode == OpCodes.Call     && ((MethodInfo)codes[i    ].operand).Name == "GridPushChange"))
				{
					Mod.Instance.Logger.Info($"call MenuPanel.Push @ {i-2}");

					// Insert:  ldarg.0
					// Insert:  call AddMenuPanelButtons_
					// Before:  ldloc.
					// Before:  callvirt MenuPanel.Push
					// NOTE: (i - 3) to insert before the ldloc used to call the MenuPanel.Push instance method.
					codes.InsertRange(i - 3, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelGridGrid__PushGrid).GetMethod(nameof(AddMenuPanelButtons_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// Trailing underscore added since there's no HarmonyIgnore attribute.
		public static void AddMenuPanelButtons_(LevelGridGrid levelGridGrid)
		{
			if (!Mod.Instance.Config.EnableDeletePlaylistButton)
			{
				return;
			}

			LevelPlaylist playlist = levelGridGrid.playlist_;

			LevelPlaylistCompoundData data = playlist.GetComponent<LevelPlaylistCompoundData>();

			if (data && !playlist.IsResourcesPlaylist())
			{
				// Inserting the button into the MenuPanel (before Push is called) is more stable,
				//  and will prevent the button from sometimes disappearing.
				//  (i.e. when Pressing Advanced view or Leaderboards, then going back)
				MenuPanel menuPanel = levelGridGrid.gridPanel_.GetComponent<MenuPanel>();

				menuPanel.SetBottomLeftButton(InternalResources.Constants.INPUT_DELETE_PLAYLIST, "DELETE PLAYLIST");
				//G.Sys.MenuPanelManager_.SetBottomLeftActionButton(InternalResources.Constants.INPUT_DELETE_PLAYLIST, "DELETE PLAYLIST");
			}
		}

		#endregion
	}
}
