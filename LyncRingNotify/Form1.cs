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
        private LyncClient _lyncClient;

        public Form1()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

            try
            {
                _lyncClient = LyncClient.GetClient();
                _lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
            }
            catch (ClientNotFoundException ex)
            {
                MessageBox.Show("Failed to get Lync client: " + ex.Message,
                        "LyncRingNotify Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                Load += (s, e) =>
                {
                    notifyIcon1.Visible = false;
                    Close();
                };
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            string notifyParam = "";
            bool hasInstantMessaging = false;
            if (ModalityIsNotified(conversation, ModalityTypes.InstantMessage))
            {
                hasInstantMessaging = true;
                notifyParam = "im";
                conversation.Modalities[ModalityTypes.InstantMessage].ModalityStateChanged += IMModalityStateChanged;
                conversation.StateChanged += IMStateChanged;
                Debug.WriteLine("IM Notified");
            }
            bool hasAudioVideo = false;
            if (ModalityIsNotified(conversation, ModalityTypes.AudioVideo))
            {
                hasAudioVideo = true;
                notifyParam = "audio";
                conversation.Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += AVModalityStateChanged;
                conversation.StateChanged += AVStateChanged;
                Debug.WriteLine("AV Notified");
            }

            // Get the URI of the "Inviter" contact
            var remoteParticipant = ((Contact)conversation.Properties[ConversationProperty.Inviter]).Uri;
            notifyParam += " " + remoteParticipant;

            CallNotifier(notifyParam);
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
                    CallNotifier("off");
                    Debug.WriteLine("IM Modality Connected");
                    break;
                case ModalityState.Disconnected:
                    CallNotifier("off");
                    Debug.WriteLine("IM Modality Disconnected");
                    break;
                case ModalityState.Joining:
                    CallNotifier("off");
                    Debug.WriteLine("IM Modality Joining");
                    break;
            }
        }

        void IMStateChanged(object sender, ConversationStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ConversationState.Parked:
                    CallNotifier("off");
                    Debug.WriteLine("IM Parked");
                    break;
                case ConversationState.Terminated:
                    CallNotifier("off");
                    Debug.WriteLine("IM Terminated");
                    break;
            }
        }

        void AVModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ModalityState.Connected:
                    CallNotifier("off");
                    Debug.WriteLine("AV Modality Connected");
                    break;
                case ModalityState.Disconnected:
                    CallNotifier("off");
                    Debug.WriteLine("AV Modality Disconnected");
                    break;
                case ModalityState.Joining:
                    CallNotifier("off");
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
                    CallNotifier("off");
                    Debug.WriteLine("AV Parked");
                    break;
                case ConversationState.Terminated:
                    CallNotifier("off");
                    Debug.WriteLine("AV Terminated");
                    break;
            }
        }

        private bool ModalityIsNotified(Conversation conversation, ModalityTypes modalityType)
        {
            return conversation.Modalities.ContainsKey(modalityType) &&
                   conversation.Modalities[modalityType].State == ModalityState.Notified;
        }

        private void CallNotifier(string param)
        {
            string notifierPath = Application.StartupPath + "\\notifier.bat";
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = string.Format("/c {0} {1}", notifierPath, param);
            Process.Start(startInfo);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CallNotifier("close");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            CallNotifier("off");
        }
    }
}
