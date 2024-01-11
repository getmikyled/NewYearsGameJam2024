using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using IvoryIcicles.SwitchboardInternals;
using IvoryIcicles.Dialog;


// https://en.wikipedia.org/wiki/Telephone_switchboard
// https://en.wikipedia.org/wiki/Switchboard_operator

namespace IvoryIcicles
{
    public class Switchboard : MonoBehaviour
    {
        public static Switchboard instance { get; private set; }

        [SerializeField] private BoardButton[] boardButtons;
        [SerializeField] private BoardCable[] boardCables;
        [SerializeField] private BoardSocket[] boardSockets;

        DialogController dialogController;
        CallManager callManager;

		public IEnumerable<BoardButton> availableChannels => boardButtons.Where(b => b.activeCall == null);
		public int availableChannelsAmmount => availableChannels.Count();

        #region Unity Constructors
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(instance);
            }
            else
            {
                instance = this;
            }
        }
        private void Start()
        {
            dialogController = DialogController.controller;
            callManager = CallManager.manager;
        }
        #endregion //Unity Constructors

        public void AnswerCall(Call call)
        {
            if (!call.operatorAnswered)
            {
                call.operatorAnswered = true;
                call.callInfo.dialogType = DialogType.OPERATOR;
                dialogController.DisplayDialog(call.callInfo);
            }
            else
            {
                if (call.correctReceptorIsConnected)
                {
                    call.callInfo.dialogType = DialogType.RECEPTOR;
                    dialogController.DisplayDialog(call.callInfo);
                }
                else
                {
                    Debug.LogWarning("OOPSIES.. WRONG NUMBER");
                }
            }
        }

        public bool ConnectCall(Call call, int channelOutID)
        {
            if (call == null)
            {
                Debug.LogWarning("The connected cable doesn't have an active call.");
                return false;
            }
            if (call.channelInID == channelOutID)
            {
                Debug.LogWarning("The cable was connected to the same emisor.");
                return false;
            }
            call.channelOutID = channelOutID;
            boardSockets[channelOutID].ConnectCall(call);
            return true;
        }

        public void DisconnectCall(Call call)
        {
            call.channelOutID = -1;
            boardButtons[call.channelInID].DisconnectCall();
            boardCables[call.channelInID].DisconnectCall();
            boardSockets[call.channelOutID].DisconnectCall();
            callManager.ResetCallGenerator();

            dialogController.ForceStopDialog();
        }

        public void FinishCall(Call call)
        {
			call.finished = true;
			SetOperatorConnection(call, connect: false);
			boardButtons[call.channelInID].DisconnectCall();
			boardCables[call.channelInID].DisconnectCall();
			boardSockets[call.channelOutID].DisconnectCall();
            callManager.ResetCallGenerator();

            dialogController.ForceStopDialog();
        }

        public bool PublishConnectionRequest(Call incommingCall)
        {
            int availablesCount = availableChannelsAmmount;
            if (availablesCount == 0)
            {
                Debug.LogWarning("Switchboard channels full. Can't publish incomming call.");
                return false;
            }
            int targetChannel = availableChannels.ElementAt(Random.Range(0, availablesCount)).channelID;
            incommingCall.channelInID = targetChannel;
            boardButtons[targetChannel].ConnectCall(incommingCall);
            boardCables[targetChannel].SetActiveCall(incommingCall);
            return true;
        }

		public void SetOperatorConnection(Call call, bool connect)
		{
			call.operatorIsConnected = connect;
		}
	}
}