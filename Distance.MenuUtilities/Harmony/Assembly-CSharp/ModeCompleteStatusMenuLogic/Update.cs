using Distance.MenuUtilities.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to handle updating the timers for <see cref="ModeCompleteStatusMenuLogic"/> components,
	/// so that they only update on first appearance in any list, and won't restart from 0% when
	/// scrolling into view.
	/// </summary>
	/// <remarks>
	/// Required For: Mode Complete Status fix (part 3/3).
	/// </remarks>
	[HarmonyPatch(typeof(ModeCompleteStatusMenuLogic), nameof(ModeCompleteStatusMenuLogic.Update))]
	internal static class ModeCompleteStatusMenuLogic__Update
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//this.timer_ += UnityEngine.Time.deltaTime;
			// -to-
			//this.timer_ = UpdateModeCompleteStatusTimer_(this);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 4; i < codes.Count; i++)
			{
				if ((codes[i - 4].opcode == OpCodes.Ldarg_0 || codes[i - 4].opcode == OpCodes.Dup) &&
					(codes[i - 3].opcode == OpCodes.Ldfld && ((FieldInfo) codes[i - 3].operand).Name == "timer_") &&
					(codes[i - 2].opcode == OpCodes.Call  && ((MethodInfo)codes[i - 2].operand).Name == "get_deltaTime") &&
					(codes[i - 1].opcode == OpCodes.Add) &&
					(codes[i    ].opcode == OpCodes.Stfld && ((FieldInfo) codes[i    ].operand).Name == "timer_"))
				{
					Mod.Instance.Logger.Info($"stfld timer_ @ {i}");

					// Replace: ldarg.0/dup
					// Replace: ldfld timer_
					// Replace: call get_deltaTime
					// Replace: add
					// With:    ldarg.0
					// With:    call UpdateModeCompleteStatusTimer_
					// Before:  stfld timer_
					codes.RemoveRange(i - 4, 4);

					codes.InsertRange(i - 4, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(ModeCompleteStatusMenuLogic__Update).GetMethod(nameof(UpdateModeCompleteStatusTimer_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// Trailing underscore added since there's no HarmonyIgnore attribute.
		public static float UpdateModeCompleteStatusTimer_(ModeCompleteStatusMenuLogic modeCompleteStatus)
		{
			if (Mod.Instance.Config.EnableCompletionProgressBarFix)
			{
				// Get the component managing the synced timer.
				var modeCompleteStatusSync = modeCompleteStatus.GetComponentInParents<ModeCompleteStatusSyncLogic>();
				if (modeCompleteStatusSync)
				{
					modeCompleteStatusSync.UpdateTimer();
					return modeCompleteStatusSync.Timer_;
				}
			}

			// Normal behavior when disabled.
			return modeCompleteStatus.timer_ + Time.deltaTime;
		}

		#endregion
	}
}
