using FishNet;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionHandler : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        HandleLocalTesting();
#endif
    }

    void Update()
    {
        
    }
    
    void HandleLocalTesting()
    {
        if(ParrelSync.ClonesManager.IsClone())
        {
            InstanceFinder.ClientManager.StartConnection();
        }
        else
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        if(InstanceFinder.ClientManager)
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Stopping)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
#endif
}
