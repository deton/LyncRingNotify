using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace LyncRingNotify
{
    public partial class Form1 : Form
    {
        private SerialPort _serial;
        private LyncClient _lyncClient;
        private delegate void DelegateWrite(string data);

        public Form1()
        {
            InitializeComponent();
            _serial = new SerialPort("COM10", 115200);
            _serial.Open();

            _lyncClient = LyncClient.GetClient();
            _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
        }

        void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            var conversation = e.Conversation;

            // Test conversation state. If inactive, then the new conversation window was opened by the user, not a remote participant
            if (conversation.State == ConversationState.Inactive)
            {
                return;
            }

            // Register for the ParticipantAdded event on the conversation
            conversation.ParticipantAdded += Conversation_ParticipantAdded;

            bool hasInstantMessaging = false;
            if (ModalityIsNotified(conversation, ModalityTypes.InstantMessage))
            {
                hasInstantMessaging = true;
                _serial.Write("v128.");
                conversation.Modalities[ModalityTypes.InstantMessage].ModalityStateChanged += IMModalityStateChanged;
                conversation.StateChanged += IMStateChanged;
                Invoke(new DelegateWrite(Write), new Object[] {"IM Notified"});
            }
            bool hasAudioVideo = false;
            if (ModalityIsNotified(conversation, ModalityTypes.AudioVideo))
            {
                hasAudioVideo = true;
                _serial.Write("v512.");
                conversation.Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += AVModalityStateChanged;
                conversation.StateChanged += AVStateChanged;
                Invoke(new DelegateWrite(Write), new Object[] {"AV Notified"});
            }

            // Get the URI of the "Inviter" contact
            var remoteParticipant = ((Contact)conversation.Properties[ConversationProperty.Inviter]).Uri;

            Invoke(new DelegateWrite(Write), new Object[] {
                    string.Format("Incoming Call\r\nCaller: {0}\r\nHas Instant Messaging: {1}\r\nHas Audio/Video: {2}",
                        remoteParticipant,
                        hasInstantMessaging,
                        hasAudioVideo)
                });
        }

        void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            var participant = e.Participant;

            // We're not interested in notifying on our own IM's, so return
            if (participant.IsSelf)
            {
                return;
            }

            // Get the InstantMessage modality, and register to the InstantMessageReceived event
            var imModality = (InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage];
            imModality.InstantMessageReceived += ImModality_InstantMessageReceived;
        }

        void ImModality_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            // Get the instant mesage modality that raised this event, and the contact that it belongs to
            var imModality = (InstantMessageModality)sender;
            var contact = imModality.Participant.Contact;

            var instantMessageText = e.Text;

            Invoke(new DelegateWrite(Write), new Object[] {
                    string.Format("Caller: {0}\r\nText: {1}",
                        contact.Uri,
                        instantMessageText)
                });
        }

        void IMModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ModalityState.Connected:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"IM Modality Connected"});
                    break;
                case ModalityState.Disconnected:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"IM Modality Disconnected"});
                    break;
                case ModalityState.Joining:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"IM Modality Joining"});
                    break;
            }
        }

        void IMStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ConversationState.Parked:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"IM Parked"});
                    break;
                case ConversationState.Terminated:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"IM Terminated"});
                    break;
            }
        }

        void AVModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ModalityState.Connected:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"AV Modality Connected"});
                    break;
                case ModalityState.Disconnected:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"AV Modality Disconnected"});
                    break;
                case ModalityState.Joining:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"AV Modality Joining"});
                    break;
            }
        }

        void AVStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            // current user didn't joined the conversation (timout/close/voice message)
            switch (e.NewState)
            {
                case ConversationState.Parked:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"AV Parked"});
                    break;
                case ConversationState.Terminated:
                    _serial.Write("v0.");
                    Invoke(new DelegateWrite(Write), new Object[] {"AV Terminated"});
                    break;
            }
        }

        private bool ModalityIsNotified(Conversation conversation, ModalityTypes modalityType)
        {
            return conversation.Modalities.ContainsKey(modalityType) &&
                   conversation.Modalities[modalityType].State == ModalityState.Notified;
        }

        /*!
         * logTextBoxに受信内容を書き込みます。
         */
        private void Write(string data)
        {
            if (data != null)
            {
                logTextBox.AppendText(data + "\n");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serial.IsOpen)
            {
                _serial.Write("v0.");
                _serial.Close();
            }
        }
    }
}
