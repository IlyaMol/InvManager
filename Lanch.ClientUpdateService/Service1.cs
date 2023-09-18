using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.ServiceProcess;

namespace Lanch.ClientUpdateService
{
    public partial class ClientUpdateService : ServiceBase
    {
        private HttpUpdater updater;
        private NamedPipeClientStream pipeClient;

        private const string parentPipeName = "ClientSuperHub";

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

        private async void Updater_OnUpdateFounded(long newVersion)
        {
            if (pipeClient == null)
            {
                pipeClient = new NamedPipeClientStream(".", parentPipeName, PipeDirection.Out, PipeOptions.Asynchronous);
            }

            if (!pipeClient.IsConnected)
            {
                try
                {
                    pipeClient = new NamedPipeClientStream(".", parentPipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                    await pipeClient.ConnectAsync(100);
                } 
                catch(System.TimeoutException)
                {

                }
            }

            if(pipeClient.IsConnected && pipeClient.CanWrite)
            {
                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteLineAsync($"New client version available: {newVersion}");

                    pipeClient.WaitForPipeDrain();

                    await writer.FlushAsync();
                }
                pipeClient.Close();
            }

            // Скачивание нового клиента
            // Если скачивание удачно и хеш md5 совпадает
            // Закрытие клиентского приложения
            Process[] processes = Process.GetProcessesByName("GraphicsClientSharp");
            foreach (Process process in processes)
            {
                process.Kill();
            }
            // 
        }

        protected override void OnStop()
        {
            if (pipeClient != null && pipeClient.IsConnected)
                pipeClient.Close();

            updater.OnUpdateFounded -= Updater_OnUpdateFounded;
            updater.Stop();
        }
    }
}
