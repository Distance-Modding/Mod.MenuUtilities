using Distance.MenuUtilities.Scripts;
using HarmonyLib;

namespace Distance.MenuUtilities.Harmony
{
	/// <summary>
	/// Patch to add handlers for slider page button logic to <see cref="UIExMoveSlider"/> components.
	/// </summary>
	/// <remarks>
	/// Required For: Slider Page Button Increments.
	/// </remarks>
	[HarmonyPatch(typeof(UIExMoveSlider), nameof(UIExMoveSlider.Start))]
	internal static class UIExMoveSlider__Start
	{
		[HarmonyPostfix]
		internal static void Postfix(UIExMoveSlider __instance)
		{
			// Ensure our script to handle page movement is created, so that its Update function can run.
			// Creating in the Awake function would have been prefered over Start (if the function existed).
			__instance.GetOrAddComponent<UIExMoveSliderPageLogic>();
		}
	}
}
