//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Console
{
	public class FPSCounter
	{
		private const float UpdateInterval = 1.0f;

		private bool _isStart = false;
		private float _lastInterval;
		private int _frames;
		private float _ms;
		private float _fps;

		public void Update()
		{
			if (_isStart == false)
			{
				_isStart = true;
				_lastInterval = Time.realtimeSinceStartup;
			}

			++_frames;
			float timeNow = Time.realtimeSinceStartup;
			if (timeNow > _lastInterval + UpdateInterval)
			{
				_fps = _frames / (timeNow - _lastInterval);
				_ms = 1000.0f / Mathf.Max(_fps, 0.00001f);
				_frames = 0;
				_lastInterval = timeNow;
			}
		}
		public int GetFPS()
		{
			return Mathf.CeilToInt(_fps);
		}
		public float GetMS()
		{
			return _ms;
		}
	}
}