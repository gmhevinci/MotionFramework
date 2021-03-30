//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public interface IAssetEncrypter
	{
		/// <summary>
		/// 检测是否需要加密
		/// </summary>
		bool Check(string filePath);

		/// <summary>
		/// 加密方法
		/// </summary>
		/// <param name="fileData">要加密的文件数据</param>
		/// <returns>返回加密后的字节数据</returns>
		byte[] Encrypt(byte[] fileData);
	}
}