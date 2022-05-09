using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace YooAsset.Editor
{
	public class EditorRemoteDebugger
	{
		public void Create()
		{
			EditorConnection.instance.Initialize();
			//EditorConnection.instance.Register(RuntimeRemoteDebugger.PlayerConnectionGuid, OnPlayerConnectionMessage);
			//EditorConnection.instance.RegisterConnection(OnPlayerConnection);
			//EditorConnection.instance.RegisterDisconnection(OnPlayerDisconnection);
		}
		public void Destroy()
		{
			//EditorConnection.instance.Unregister(RuntimeRemoteDebugger.PlayerConnectionGuid, OnPlayerConnectionMessage);
			//RegisterEventHandler(false);
			//EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;
		}
	}
}