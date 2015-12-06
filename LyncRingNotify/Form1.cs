// https://web.archive.org/web/20120224054715/http://www.codelync.com/2011/11/developing-screen-pop-applications
// https://github.com/sierrodc/Arduino-Lync-monitor
// https://msdn.microsoft.com/en-us/library/hh530042.aspx
// http://www.mztm.jp/2013/03/17/serialcommnunication/
using System;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using LyncRingNotify.Properties;
using System.Diagnostics;

namespace LyncRingNotify
{
    public partial class Form1 : Form
    {
        private bool _avaiableSerialAndLync = false;
        private Vibrator _vibrator;
        private LyncClient _lyncClient;

        public Form1()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

            try
            {
                _vibrator = new Vibrator(Settings.Default.ComPort);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show("Failed to open COM port: " + ex.Message,
                        "LyncRingNotify Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                return;
            }

            try
            {
                _lyncClient = LyncClient.GetClient();
                _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
                _avaiableSerialAndLync = true;
            }
            catch (ClientNotFoundException ex)
            {
                MessageBox.Show("Failed to get Lync client: " + ex.Message,
                        "LyncRingNotify Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!_avaiableSerialAndLync)
            {
                notifyIcon1.Visible = false;
                Close();
                return;
            }
            Hide();
        }

        void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            var conversation = e.Conversation;

            // Test conversation state. If inactive, then the new conversation
            // window was opened by the user, not a remote participant
            if (conversation.State == ConversationState.Inactive)
            {
                return;
            }

            bool hasInstantMessaging = false;
            if (ModalityIsNotified(conversation, ModalityTypes.InstantMessage))
            {
                hasInstantMessaging = true;
                _vibrator.Vibrate(128);
                conversation.Modalities[ModalityTypes.InstantMessage].ModalityStateChanged += IMModalityStateChanged;
                conversation.StateChanged += IMStateChanged;
                Debug.WriteLine("IM Notified");
            }
            bool hasAudioVideo = false;
            if (ModalityIsNotified(conversation, ModalityTypes.AudioVideo))
            {
                hasAudioVideo = true;
                _vibrator.Vibrate(512);
                conversation.Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += AVModalityStateChanged;
                conversation.StateChanged += AVStateChanged;
                Debug.WriteLine("AV Notified");
            }

            // Get the URI of the "Inviter" contact
            var remoteParticipant = ((Contact)conversation.Properties[ConversationProperty.Inviter]).Uri;

            Debug.WriteLine(
                    string.Format("Incoming Call\r\nCaller: {0}\r\nHas Instant Messaging: {1}\r\nHas Audio/Video: {2}",
                        remoteParticipant,
                        hasInstantMessaging,
                        hasAudioVideo)
                );
        }

        void IMModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ModalityState.Connected:
                    _vibrator.Off();
                    Debug.WriteLine("IM Modality Connected");
                    break;
                case ModalityState.Disconnected:
                    _vibrator.Off();
                    Debug.WriteLine("IM Modality Disconnected");
                    break;
                case ModalityState.Joining:
                    _vibrator.Off();
                    Debug.WriteLine("IM Modality Joining");
                    break;
            }
        }

        void IMStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ConversationState.Parked:
                    _vibrator.Off();
                    Debug.WriteLine("IM Parked");
                    break;
                case ConversationState.Terminated:
                    _vibrator.Off();
                    Debug.WriteLine("IM Terminated");
                    break;
            }
        }

        void AVModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ModalityState.Connected:
                    _vibrator.Off();
                    Debug.WriteLine("AV Modality Connected");
                    break;
                case ModalityState.Disconnected:
                    _vibrator.Off();
                    Debug.WriteLine("AV Modality Disconnected");
                    break;
                case ModalityState.Joining:
                    _vibrator.Off();
                    Debug.WriteLine("AV Modality Joining");
                    break;
            }
        }

        void AVStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            // current user didn't joined the conversation (timout/close/voice message)
            switch (e.NewState)
            {
                case ConversationState.Parked:
                    _vibrator.Off();
                    Debug.WriteLine("AV Parked");
                    break;
                case ConversationState.Terminated:
                    _vibrator.Off();
                    Debug.WriteLine("AV Terminated");
                    break;
            }
        }

        private bool ModalityIsNotified(Conversation conversation, ModalityTypes modalityType)
        {
            return conversation.Modalities.ContainsKey(modalityType) &&
                   conversation.Modalities[modalityType].State == ModalityState.Notified;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_vibrator != null)
            {
                _vibrator.Off();
                _vibrator.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (_vibrator != null)
            {
                _vibrator.Off();
            }
        }
    }
}
