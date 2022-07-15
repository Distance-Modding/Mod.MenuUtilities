#pragma warning disable IDE0051
using UnityEngine;

namespace Distance.MenuUtilities.Scripts
{
	/// <summary>
	/// Stores the timer field needed for <see cref="UICameraEx.GetRepeatedInput"/>,
	/// and handles updating the page buttons for moving the slider in larger increments.
	/// <para/>
	/// This page increment behavior is only for integer sliders.
	/// </summary>
	/// <remarks>
	/// Required For: Slider Page Button Increments.
	/// </remarks>
	public class UIExMoveSliderPageLogic : MonoBehaviour
	{
		private UIExMoveSlider moveSlider_;

		private float pageInputRepeatTimer_;


		private void Awake()
		{
			this.moveSlider_ = this.GetComponentInParent<UIExMoveSlider>();
			this.ResetPageTimer();
		}

		private void Update()
		{
			if (!Mod.Instance.Config.EnableSliderPageButtons || !this.moveSlider_ || this.moveSlider_.slider_ == null)
			{
				return;
			}

			if (!this.moveSlider_.useController_)
			{
				this.ResetPageTimer();
				return;
			}

			// Additional check (only seen used by integer sliders in OnNavigate). Is this necessary?
			if (!this.moveSlider_.enabled)
			{
				this.ResetPageTimer();
				return;
			}

			// This ensure that we're the focused control before handling input.
			if (UICamera.selectedObject != this.gameObject) // same as this.moveSlider_.gameObject
			{
				this.ResetPageTimer();
				return;
			}

			// Handle page input for integer sliders.
			if (this.moveSlider_.slider_.numberOfSteps != 0)
			{
				//bool isVertical, isNegative;
				this.moveSlider_.GetDirection(out bool isVertical, out bool isNegative);

				var actionPair = (!isVertical) ? InputActionPair.MenuPageHorizontal : InputActionPair.MenuPageVertical;

				var dualState = G.Sys.InputManager_.GetDualState(actionPair, UICameraEx.GetDeviceIndex());
				if (dualState.pressedDir_ == 0)
				{
					// If neither (or both) inputs are down, then reset the repeat timer.
					this.ResetPageTimer();
				}
				else
				{
					// Use repeated-style input (like typing), so that we don't need to hammer the button over and over.
					int direction = UICameraEx.GetRepeatedInput(ref this.pageInputRepeatTimer_, actionPair);

					if (isNegative)
					{
						direction = -direction;
					}

					// Add increment to slider based on directional buttons pressed.
					if (direction > 0)
					{
						this.moveSlider_.slider_.value += this.GetPageIncrement();
					}
					else if (direction < 0)
					{
						this.moveSlider_.slider_.value -= this.GetPageIncrement();
					}
				}
			}
		}


		private void ResetPageTimer()
		{
			this.pageInputRepeatTimer_ = 0f;
		}

		private float GetPageIncrement()
		{
			if (this.moveSlider_ && this.moveSlider_.slider_ != null)
			{
				int steps = this.moveSlider_.slider_.numberOfSteps;
				if (steps != 0)
				{
					// Arbitrarily increase increment amount based on total number of steps.
					// Each amount of steps has a small amount subtracted to account for options with higher minimum values.
					float increment = 1f / (float)(steps - 1);
					if (steps < 20 - 2)
					{
						increment *= 2f;
					}
					else if (steps < 100 - 10)
					{
						increment *= 5f;
					}
					else if (steps < 200 - 20)
					{
						increment *= 10f;
					}
					else if (steps < 10000 - 1000)
					{
						// We should stop here...
						// Anything higher than 20 would get slower to adjust minutely with normal slider input.
						increment *= 20f;
					}
					else
					{
						// However...
						// Special case for VERY LARGE slider values, where it would take 200 movements to reach max.
						increment *= 50f;
					}
					return increment;
				}
			}
			return 0f;
		}
	}
}
