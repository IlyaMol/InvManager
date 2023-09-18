using System.ServiceProcess;

namespace Lanch.ClientUpdateService
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ClientUpdateService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
