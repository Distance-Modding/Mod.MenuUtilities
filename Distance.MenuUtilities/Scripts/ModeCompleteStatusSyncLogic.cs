#pragma warning disable IDE0051
using UnityEngine;

namespace Distance.MenuUtilities.Scripts
{
	/// <summary>
	/// A simple timer behavior used to help sync multiple <see cref="ModeCompleteStatusMenuLogic"/> components.
	/// </summary>
	/// <remarks>
	/// Required For: Mode Complete Status fix.
	/// </remarks>
	public class ModeCompleteStatusSyncLogic : MonoBehaviour
	{
		// Single timer for all `ModeCompleteStatusMenuLogic`s of the same grouping.
		private float timer_;
		// Used to track if we have already updated this frame or not.
		private int lastFrameCount_;

		public float Timer_
		{
			get => this.timer_;
			set => this.timer_ = value;
		}

		
		private void Awake()
		{
			this.RestartTimer();
		}

		public void RestartTimer()
		{
			this.timer_ = 0f;
			// Set the last frame count to *last frame*, meaning we can update on the same frame as restarting.
			this.lastFrameCount_ = Time.frameCount - 1;
		}

		public void UpdateTimer()
		{
			// Don't increment the timer if we've already updated this frame.
			if (this.lastFrameCount_ < Time.frameCount)
			{
				this.lastFrameCount_ = Time.frameCount;
				this.timer_ += Time.deltaTime;
			}
		}
	}
}
