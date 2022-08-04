using Distance.MenuUtilities.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to handle starting the timers for <see cref="ModeCompleteStatusMenuLogic"/> components,
	/// so that their initial time in any list won't be set to 0% when scrolling into view.
	/// </summary>
	/// <remarks>
	/// Required For: Mode Complete Status fix (part 2/3).
	/// </remarks>
	[HarmonyPatch(typeof(ModeCompleteStatusMenuLogic), nameof(ModeCompleteStatusMenuLogic.StartAnimation))]
	internal static class ModeCompleteStatusMenuLogic__StartAnimation
	{
		// Prefix patch implementation:
		/*[HarmonyPrefix]
		internal static bool Prefix(ModeCompleteStatusMenuLogic __instance)
		{
			Mod.Instance.Logger.Info($"ModeCompleteStatusMenuLogic__StartAnimation");
			__instance.Clear();
			if (!G.Sys.OptionsManager_.General_.menuAnimations_)
			{
				__instance.SetValues(1f);
				return false;
			}

			__instance.timer_ = StartModeCompleteStatusTimer_(__instance);
			__instance.animate_ = true;
			return false;
		}*/

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//this.timer_ = 0f;
			// -to-
			//this.timer_ = StartModeCompleteStatusTimer_(this);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 1; i < codes.Count; i++)
			{
				if ((codes[i - 1].opcode == OpCodes.Ldc_R4 && (float)codes[i - 1].operand == 0f) &&
					(codes[i    ].opcode == OpCodes.Stfld  && ((FieldInfo)codes[i].operand).Name == "timer_"))
				{
					Mod.Instance.Logger.Info($"stfld timer_ @ {i}");

					// Replace: ldc.r4 0f
					// With:    ldarg.0
					// With:    call StartModeCompleteStatusTimer_
					// Before:  stfld timer_
					codes.RemoveAt(i - 1);

					codes.InsertRange(i - 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(ModeCompleteStatusMenuLogic__StartAnimation).GetMethod(nameof(StartModeCompleteStatusTimer_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// Trailing underscore added since there's no HarmonyIgnore attribute.
		public static float StartModeCompleteStatusTimer_(ModeCompleteStatusMenuLogic modeCompleteStatus)
		{
			if (Mod.Instance.Config.EnableCompletionProgressBarFix)
			{
				// Get the component managing the synced timer.
				var modeCompleteStatusSync = modeCompleteStatus.GetComponentInParents<ModeCompleteStatusSyncLogic>();
				if (modeCompleteStatusSync)
				{
					return modeCompleteStatusSync.Timer_;
				}
			}

			// Normal behavior when disabled.
			return 0f;
		}

		#endregion
	}
}
