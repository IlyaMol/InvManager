using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Lanch.ClientUpdateService
{
    [RunInstaller(true)]
    public partial class UpdaterServiceInstaller : System.Configuration.Install.Installer
    {
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;

        public UpdaterServiceInstaller()
        {
            InitializeComponent();

            serviceInstaller = new ServiceInstaller();
            processInstaller = new ServiceProcessInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "ClientUpdateService";
            serviceInstaller.DisplayName = "Client Update Service";
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
