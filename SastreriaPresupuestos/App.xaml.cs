using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using SastreriaPresupuestos.Data;

namespace SastreriaPresupuestos
{
    
    //rebase
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var db = new AppDbContext())
            {
                db.Database.Migrate();
            }

            base.OnStartup(e);
        }
    }
}
