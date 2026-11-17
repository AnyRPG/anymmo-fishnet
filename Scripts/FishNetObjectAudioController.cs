using FishNet.Object;
using UnityEngine;


namespace AnyRPG {
    public class FishNetObjectAudioController : NetworkBehaviour {

        private ObjectAudioController objectAudioController = null;

        private bool eventRegistrationComplete = false;

        // game manager references
        protected SystemGameManager systemGameManager = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected AudioManager audioManager = null;

        public ObjectAudioController ObjectAudioController { get => objectAudioController; }

        protected virtual void Awake() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.Awake() position: { gameObject.transform.position}");
        }

        protected virtual void Configure() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.Configure()");

            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            audioManager = systemGameManager.AudioManager;

            objectAudioController = GetComponent<ObjectAudioController>();
            if (objectAudioController != null) {
                objectAudioController.SetServerModeActive(networkManagerServer.ServerModeActive);
                // host mode: the server object is also the host's client object, so it must be audible when there is a local player
                objectAudioController.SetMuteCheck(() => systemGameManager.HasLocalPlayer == false);
            } else {
                Debug.LogError($"{gameObject.name}.FishNetObjectAudioController.Configure(): ObjectAudioController is null");
            }
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.OnStartClient()");

            base.OnStartClient();

            Configure();
            if (systemGameManager == null) {
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            //objectAudioController.AutoConfigure(systemGameManager);

            //SubscribeToClientInteractableEvents();
        }

        public override void OnStopClient() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.OnStopClient()");

            base.OnStopClient();
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            //UnsubscribeFromClientInteractableEvents();
            //systemGameManager.NetworkManagerClient.ProcessStopClient(unitController);
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.OnStartServer()");

            base.OnStartServer();

            Configure();
            if (systemGameManager == null) {
                //Debug.LogWarning($"{gameObject.name}.FishNetObjectAudioController.OnStartServer(): systemGameManager is null");
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToServerInteractableEvents();
        }

        public override void OnStopServer() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.OnStopServer()");

            base.OnStopServer();

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            UnsubscribeFromServerInteractableEvents();
            //systemGameManager.NetworkManagerServer.ProcessStopServer(unitController);
        }

        public void SubscribeToServerInteractableEvents() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.SubscribeToServerInteractableEvents()");

            if (eventRegistrationComplete == true) {
                //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.SubscribeToServerInteractableEvents(): already registered");
                return;
            }

            if (objectAudioController == null) {
                Debug.LogWarning($"{gameObject.name}.FishNetObjectAudioController.SubscribeToServerInteractableEvents(): interactable is null");
                // something went wrong
                return;
            }

            objectAudioController.OnPlayAudioClip += HandlePlayAudioClip;
            objectAudioController.OnPlayOneShot += HandlePlayOneShot;
            objectAudioController.OnStopAudio += HandleStopAudio;
            objectAudioController.OnPauseAudio += HandlePauseAudio;
            objectAudioController.OnUnPauseAudio += HandleUnPauseAudio;

            eventRegistrationComplete = true;
        }

        public void HandleInteractableDisableServer() {
            UnsubscribeFromServerInteractableEvents();
        }

        public void UnsubscribeFromServerInteractableEvents() {
            if (objectAudioController == null) {
                return;
            }
            if (eventRegistrationComplete == false) {
                //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.UnsubscribeFromServerInteractableEvents(): not registered");
                return;
            }
            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement -= HandleAnimatedObjectChooseMovementServer;
            objectAudioController.OnPlayAudioClip -= HandlePlayAudioClip;
            objectAudioController.OnPlayOneShot -= HandlePlayOneShot;
            objectAudioController.OnStopAudio -= HandleStopAudio;
            objectAudioController.OnPauseAudio -= HandlePauseAudio;
            objectAudioController.OnUnPauseAudio -= HandleUnPauseAudio;

            eventRegistrationComplete = false;
        }

        private void HandleUnPauseAudio() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandleUnPauseAudio()");

            HandleUnPauseAudioClient();
        }

        // ExcludeServer: on a host the server already played this on the shared object; re-running it on the host client
        // would re-raise the event the server forwards, sending the RPC again every tick
        [ObserversRpc(ExcludeServer = true)]
        public void HandleUnPauseAudioClient() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandleUnPauseAudioClient()");

            objectAudioController.UnPauseAudio();
        }

        private void HandlePauseAudio() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePauseAudio()");

            HandlePauseAudioClient();
        }

        // ExcludeServer: on a host the server already played this on the shared object; re-running it on the host client
        // would re-raise the event the server forwards, sending the RPC again every tick
        [ObserversRpc(ExcludeServer = true)]
        public void HandlePauseAudioClient() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePauseAudioClient()");

            objectAudioController.PauseAudio();
        }

        private void HandleStopAudio() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandleStopAudio()");

            HandleStopAudioClient();
        }

        // ExcludeServer: on a host the server already played this on the shared object; re-running it on the host client
        // would re-raise the event the server forwards, sending the RPC again every tick
        [ObserversRpc(ExcludeServer = true)]
        public void HandleStopAudioClient() {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandleStopAudioClient()");

            objectAudioController.StopAudio();
        }

        private void HandlePlayOneShot(AudioClip clip) {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePlayOneShot({clip.name})");

            HandlePlayOneShotClient(clip.name);
        }

        // ExcludeServer: on a host the server already played this on the shared object; re-running it on the host client
        // would re-raise the event the server forwards, sending the RPC again every tick
        [ObserversRpc(ExcludeServer = true)]
        public void HandlePlayOneShotClient(string clipName) {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePlayOneShotClient({clipName})");

            AudioClip clip = audioManager.GetAudioClip(clipName);
            if (clip != null) {
                objectAudioController.PlayOneShot(clip);
            }
        }

        private void HandlePlayAudioClip(AudioClip clip) {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePlayAudioClip({clip.name})");

            HandlePlayAudioClipClient(clip.name);
        }

        // ExcludeServer: on a host the server already played this on the shared object; re-running it on the host client
        // would re-raise the event the server forwards, sending the RPC again every tick
        [ObserversRpc(ExcludeServer = true)]
        public void HandlePlayAudioClipClient(string clipName) {
            //Debug.Log($"{gameObject.name}.FishNetObjectAudioController.HandlePlayAudioClipClient({clipName})");

            AudioClip clip = audioManager.GetAudioClip(clipName);
            if (clip != null) {
                objectAudioController.PlayAudioClip(clip);
            }
        }

    }
}

