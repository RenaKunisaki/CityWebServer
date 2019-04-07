using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ApacheMimeTypes;
using CityWebServer.Extensibility;
using CityWebServer.Models;
using ColossalFramework;
using UnityEngine;

namespace CityWebServer.RequestHandlers {
	public class DefaultRequestHandler: RequestHandlerBase {
		public DefaultRequestHandler()
			: base(new Guid("9f2a91e2-8174-4592-8876-b2ceb9c77ddc"),
				"Default", "Rena", 999, "/") {
		}

		public DefaultRequestHandler(WebServer server, HttpRequest request)
		: base(server, request) { }

		public override Boolean ShouldHandle(HttpRequest request) {
			//This is the fallback handler when no others can handle,
			//so it should handle everything.
			return true;
		}

		public override void Handle() {
			if(request.method == "GET") HandleGet();
			else SendErrorResponse(HttpStatusCode.MethodNotAllowed);
		}

		public void HandleGet() {
			/** Handle a GET request.
			 *  We'll do so by trying to send a file from wwwroot.		
			 */
			String path = request.path;
			Log($"Handling GET path '{path}");
			//Sanitize and make relative
			path = path.Replace("..", "%2E%2E");
			while(path.StartsWith("/", StringComparison.Ordinal)) {
				path = path.Substring(1);
			}
			if(path == "") path = "index.html";

			path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
			var absolutePath = Path.Combine(WebServer.GetWebRoot(), path);
			SendFile(request, absolutePath);
		}

		public void SendFile(HttpRequest request, String absolutePath) {
			Log($"Sending file: {absolutePath}");

			try {
				var extension = Path.GetExtension(absolutePath);
				var contentType = Apache.GetMime(extension);
				var resp = new HttpResponse(request.stream);
				if(contentType != null) resp.AddHeader("Content-Type", contentType);

				using(FileStream fileReader = File.OpenRead(absolutePath)) {
					byte[] buffer = new byte[4096];
					int read;
					while((read = fileReader.Read(buffer, 0, buffer.Length)) > 0) {
						resp.SendPartialBody(buffer, 0, read);
					}
				}
			}
			catch(System.IO.FileNotFoundException) {
				Log($"Not found: '{absolutePath}'");
				SendErrorResponse(HttpStatusCode.NotFound);
			}
			catch(Exception ex) {
				Log($"Error sending '{absolutePath}': {ex}");
				SendErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
			}
		}
	}
}