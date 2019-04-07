using System;
using System.Net;

namespace CityWebServer {
	public class RequestHandlerBase: IRequestHandler {
		protected readonly WebServer _server;
		protected Guid _handlerID;
		protected int _priority;
		protected String _name;
		protected String _author;
		protected String _mainPath;
		protected HttpRequest request;

		protected RequestHandlerBase(WebServer server, HttpRequest request, String name) {
			/** This constructor is used when we're creating a new instance
			 *  to actually handle a request.
			 */
			this._server = server;
			this.request = request;
			this._name = name;
		}

		protected RequestHandlerBase(Guid handlerID, String name, String author, int priority, String mainPath) {
			/** This constructor is used when the server is starting up and
			 *  enumerating all available handlers.
			 *  The instance created here won't be used to serve requests.
			 *  This is a bit hackish. I'd rather this info be static members,
			 *  but I'm not sure how to make that work.		 
			 */
			_handlerID = handlerID;
			_name = name;
			_author = author;
			_priority = priority;
			_mainPath = mainPath;
		}

		protected void SendJson<T>(T body, HttpStatusCode statusCode = HttpStatusCode.OK) {
			HttpResponse response = new HttpResponse(
				request.stream, statusCode: statusCode);
			response.SendJson<T>(body);
		}

		protected void SendBinary(byte[] body, String contentType = "application/octet-stream", HttpStatusCode statusCode = HttpStatusCode.OK) {
			HttpResponse response = new HttpResponse(request.stream,
				contentType: contentType,
				statusCode: statusCode);
			response.SendBody(body);
		}

		public void SendErrorResponse(HttpStatusCode status, String message = null) {
			/** Send an error resposne to the client.
			 */
			var resp = new HttpResponse(request.stream,
				contentType: "text/plain",
				statusCode: status);
			if(message == null) {
				message = HttpResponse.StatusCodeToName(status);
			}
			resp.SendBody(message);
		}

		protected void Log(string msg) {
			WebServer.Log($"{Name}: {msg}");
		}

		/// <summary>
		/// Gets the server that is currently servicing this instance.
		/// </summary>
		public virtual IWebServer Server { get { return _server; } }

		/// <summary>
		/// Gets a unique identifier for this handler.  Only one handler can be loaded with a given identifier.
		/// </summary>
		public virtual Guid HandlerID { get { return _handlerID; } }

		/// <summary>
		/// Gets the priority of this request handler.  A request will be handled by the request handler with the lowest priority.
		/// </summary>
		public virtual int Priority { get { return _priority; } }

		/// <summary>
		/// Gets the display name of this request handler.
		/// </summary>
		public virtual String Name { get { return _name; } }

		/// <summary>
		/// Gets the author of this request handler.
		/// </summary>
		public virtual String Author { get { return _author; } }

		/// <summary>
		/// Gets the absolute path to the main page for this request handler.  Your class is responsible for handling requests at this path.
		/// </summary>
		/// <remarks>
		/// When set to a value other than <c>null</c>, the Web Server will show this url as a link on the home page.
		/// </remarks>
		public virtual String MainPath { get { return _mainPath; } }

		/// <summary>
		/// Returns a value that indicates whether this handler is capable of servicing the given request.
		/// </summary>
		public virtual Boolean ShouldHandle(HttpRequest request) {
			return (request.method.Equals("GET") &&
				request.path.Equals(_mainPath, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Handles the specified request.  The method should not close the stream.
		/// </summary>
		/// <exception cref="T:System.NotImplementedException"></exception>
		public virtual void Handle() {
			throw new NotImplementedException();
		}
	}
}