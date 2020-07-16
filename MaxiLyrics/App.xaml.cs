using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace MaxiLyrics
{
    /// <summary>
    /// Logic for App.xaml.
    /// Host for MEF - starts composition.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// MainWindow override for MEF purposes.
        /// </summary>
        
        [Import(typeof(MainWindow))]
        public new Window MainWindow
        {
            get { return base.MainWindow; }
            set { base.MainWindow = value; }
        }

        CompositionContainer _container;
        
        /// <summary>
        /// OnStartup override - starts composition.
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (Compose())
                MainWindow.Show();
            else
                Shutdown();
        }

        /// <summary>
        /// OnExit override - cleares resources used by CompositionContainer
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if(_container != null)
                _container.Dispose();
        }

        /// <summary>
        /// Tries to load every plugins found in this assembly or "Extensions" folder
        /// </summary>
        private bool Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(MainWindow).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(@"Extensions\"));

            _container = new CompositionContainer(catalog);
            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                MessageBox.Show(ex.Errors.ToString());
                return false;
            }
            return true;
        }
    }
}
