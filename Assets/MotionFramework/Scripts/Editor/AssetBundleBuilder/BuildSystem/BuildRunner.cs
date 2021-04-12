//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Editor
{
	public class BuildRunner
	{
		public static void Run(List<IBuildTask> pipeline, BuildContext context)
		{
			if (pipeline == null)
				throw new ArgumentNullException("pipeline");
			if (context == null)
				throw new ArgumentNullException("context");

			for (int i = 0; i < pipeline.Count; i++)
			{
				IBuildTask task = pipeline[i];
				try
				{
					task.Run(context);
				}
				catch (Exception e)
				{
					throw new Exception($"Build task {task.GetType().Name} failed : {e}");
				}
			}

			BuildLogger.Log($"构建完成！");
		}
	}
}