using System.ServiceProcess;

namespace Lanch.ClientUpdateService
{
    public partial class ClientUpdateService : ServiceBase
    {
        private HttpUpdater updater;

        public ClientUpdateService()
        {
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            updater = new HttpUpdater("https://localhost:7094");
            updater.OnUpdateFounded += Updater_OnUpdateFounded;
            updater.Start();
        }

        private void Updater_OnUpdateFounded(System.Version newVersion)
        {
            notifyIcon1.ShowBalloonTip(1000, "Update founded", $"New version is: {newVersion}", System.Windows.Forms.ToolTipIcon.Info);
        }

        protected override void OnStop()
        {
            updater.OnUpdateFounded -= Updater_OnUpdateFounded;
            updater.Stop();
        }

    }
}
