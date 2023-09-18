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
        public delegate void UpdateFounded(long newVersion);
        public event UpdateFounded OnUpdateFounded; 

        private long lastVersion = 0;

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
                period: 10000,
                callback: tm);
        }

        private void OnTimerTick(object state)
        {
            HttpResponseMessage response = null;
            
            try
            {
                response = _httpClient.GetAsync("api/ClientUpdate").Result;
            }
            catch (Exception) { }

            if(response != null && response.IsSuccessStatusCode) {
                string jsonString = response.Content.ReadAsStringAsync().Result;
                List<ClientUpdateModel> models = Json.Deserialize<List<ClientUpdateModel>>(jsonString);

                if (models.Count == 0) return;

                if (models.Max(m => m.ChangeNumber) > lastVersion)
                {
                    OnUpdateFounded?.Invoke(models.Max(m => m.ChangeNumber));
                    lastVersion = models.Max(m => m.ChangeNumber);
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
