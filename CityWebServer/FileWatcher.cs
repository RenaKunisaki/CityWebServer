using System;
using System.IO;
using System.Linq;
using System.Threading;
using ColossalFramework.Plugins;

namespace CityWebServer {
	public class FileWatcher {
		/** Watches for changes to the mod's files, and reloads it.
		 *  (Actually, doesn't do anything when a change is detected,
		 *  because I can't find the proper way to reload.)
		 */
		protected FileSystemWatcher watcher;

		public FileWatcher() {
			var modPaths = PluginManager.instance.GetPluginsInfo().Select(
				obj => obj.modPath);
			String myPath = null;

			foreach(var path in modPaths) {
				var testPath = Path.Combine(path, "wwwroot");
				if(Directory.Exists(testPath)) {
					myPath = path;
					break;
				}
			}

			if(myPath == null) {
				WebServer.Log("FileWatcher: no wwwroot found!");
				return;
			}
			WebServer.Log($"Watching filesystem path: '{myPath}");
			watcher = new FileSystemWatcher {
				Path = myPath,
				NotifyFilter = NotifyFilters.LastWrite,
				Filter = "*.dll",
			};
			watcher.Changed += OnChanged;
			watcher.Created += OnCreated;
			watcher.EnableRaisingEvents = true; //start watching
		}

		private static void OnChanged(object source, FileSystemEventArgs e) {
			WebServer.Log("File changed!");
			//PluginManager.instance.ForcePluginsChanged();
		}

		private static void OnCreated(object source, FileSystemEventArgs e) {
			WebServer.Log("File created!");
			ThreadPool.QueueUserWorkItem(o => {
				Thread.Sleep(1000);
				//PluginManager.instance.ForcePluginsChanged();
			});
		}
	}
}
