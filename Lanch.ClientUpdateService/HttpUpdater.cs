using Lanch.ClassLib.Helpers;
using Lanch.ClassLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace Lanch.ClientUpdateService
{
    public class HttpUpdater
    {
        public delegate void UpdateFounded(Version newVersion);
        public event UpdateFounded OnUpdateFounded; 

        private Version lastVersion = new Version();

        private string _url;
        private HttpClient _httpClient;
        private MessageProcessingHandler _handler;

        private bool _isApiServerAccessable = false;
        private bool _isActive = true;

        private Timer timer;
        private Thread timerThread;

        public HttpUpdater(string url)
        {
            _httpClient = new HttpClient();

            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;

            _httpClient = new HttpClient(httpClientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.BaseAddress = new Uri(url);
        }

        public void Start()
        {
            TimerCallback tm = new TimerCallback(OnTimerTick);
            timer = new Timer(
                state: null,
                dueTime: 10000,
                period: 15000,
                callback: tm);
        }

        private void OnTimerTick(object state)
        {
            var result = _httpClient.GetAsync("api/ClientUpdate").Result;

            if(result != null && result.IsSuccessStatusCode) {
                string jsonString = result.Content.ReadAsStringAsync().Result;
                List<ClientUpdateModel> models = Json.Deserialize<List<ClientUpdateModel>>(jsonString);

                if (models.Count == 0) return;

                if (models.Max(m => m.Version) > lastVersion)
                {
                    OnUpdateFounded?.Invoke(models.Max(m => m.Version));
                    lastVersion = models.Max(m => m.Version);
                }
            }
        }

        public void Stop()
        {
            if (timerThread != null && timerThread.IsAlive)
                timerThread.Join();
        }
    }
}
