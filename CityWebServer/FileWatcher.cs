using System;
using System.IO;
using System.Linq;
using ColossalFramework.Plugins;

namespace CityWebServer {
	public class FileWatcher {
		/** Watches for changes to the mod's files, and reloads it.
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
		}

		private static void OnChanged(object source, FileSystemEventArgs e) {
			WebServer.Log("File changed!");
			PluginManager.instance.ForcePluginsChanged();
		}
	}
}
