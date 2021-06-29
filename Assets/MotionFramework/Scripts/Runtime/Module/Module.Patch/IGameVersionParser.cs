//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	/// <summary>
	/// 游戏版本解析接口
	/// </summary>
	public interface IGameVersionParser
	{
		/// <summary>
		/// 当前游戏版本号
		/// </summary>
		System.Version GameVersion { get; }

		/// <summary>
		/// 当前资源版本
		/// </summary>
		int ResourceVersion { get; }

		/// <summary>
		/// 是否发现了新的安装包
		/// </summary>
		bool FoundNewApp { get; }

		/// <summary>
		/// 是否需要强制用户安装
		/// </summary>
		bool ForceInstall { get; }

		/// <summary>
		/// App安装的地址
		/// </summary>
		string AppURL { get; }


		/// <summary>
		/// 解析WEB服务器反馈的内容
		/// </summary>
		bool ParseContent(string content);
	}
}