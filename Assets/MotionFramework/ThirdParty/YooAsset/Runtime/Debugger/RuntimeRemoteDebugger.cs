using System;
using UnityEngine.Networking.PlayerConnection;

namespace YooAsset.Editor
{
	public class RuntimeRemoteDebugger
	{
		private static Guid EditorConnectionGuid;

        public static Guid PlayerConnectionGuid
        {
            get
            {
                if (EditorConnectionGuid == Guid.Empty)
                    EditorConnectionGuid = new Guid(1, 2, 3, new byte[] { 20, 1, 32, 32, 4, 9, 6, 44 });
                return EditorConnectionGuid;
            }
        }
    }
}