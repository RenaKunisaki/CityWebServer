using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using CityWebServer.Callbacks;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Allows client to use reflection to access game objects.
	/// </summary>
	public class ReflectionHandler: SocketHandlerBase {
		protected float totalTimeDelta;

		protected class Response {
			public string[] targetName;
			public string action;
			public object result;
			public string error;
		}

		public ReflectionHandler(SocketRequestHandler handler) :
		base(handler, "Reflection") {
			handler.RegisterMessageHandler("Reflection", OnClientMessage);
		}

		/// <summary>
		/// Handle "Reflection" message from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// TODO
		/// </remarks>
		public void OnClientMessage(ClientMessage msg) {
			string[] targetName;
			string parentName = "<none>", objectName = "<none>";
			string action;
			Response resp;
			try {
				targetName = msg.GetStringArray("object");
				action = msg.GetString("action");
				resp = new Response {
					targetName = targetName,
					action = action,
				};
			}
			catch(KeyNotFoundException) {
				SendErrorResponse(HttpStatusCode.BadRequest);
				return;
			}

			try {
				object target, parent = null;
				try {
					objectName = targetName[0];
					target = GetManager(targetName[0]);
					for(int i = 1; i < targetName.Length; i++) {
						parent = target;
						parentName = objectName;
						objectName = targetName[i];
						//Log($"[{i}] Object={objectName} parent={parentName}");
						target = target.GetType().GetField(targetName[i]).GetValue(target);
					}
					if(target == null) throw new NullReferenceException();
				}
				catch(NullReferenceException) {
					resp.error = $"Not found '{objectName}' in '{parentName}'";
					SendJson(resp);
					return;
				}

				Log($"Object={objectName} parent={parentName} action={action}");
				switch(action) {
					case null:
						SendErrorResponse(HttpStatusCode.BadRequest);
						return;
					case "GetFields": {
							var fields = target.GetType().GetFields(
								BindingFlags.FlattenHierarchy | BindingFlags.Instance |
								BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Static);
							var dict = new Dictionary<string, Dictionary<string, object>>();
							foreach(FieldInfo field in fields) {
								dict[field.Name] = new Dictionary<string, object> {
									{ "IsAssembly", field.IsAssembly },
									{ "IsFamily", field.IsFamily },
									{ "IsInitOnly", field.IsInitOnly },
									{ "IsLiteral", field.IsLiteral },
									{ "IsPrivate", field.IsPrivate },
									{ "IsPublic", field.IsPublic },
									{ "IsStatic", field.IsStatic },
									{ "Type", field.GetType().FullName },
									{ "DeclaringType", field.DeclaringType.FullName },
								};
							}
							resp.result = dict;
							break;
						}
					case "GetMethods": {
							var methods = target.GetType().GetMethods(
								BindingFlags.FlattenHierarchy | BindingFlags.Instance |
								BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Static);
							var dict = new Dictionary<string, Dictionary<string, object>>();
							foreach(MethodInfo method in methods) {
								var paramList = new List<Dictionary<string, object>>();
								foreach(var paramInfo in method.GetParameters()) {
									paramList.Add(GetParamInfo(paramInfo));
								}

								dict[method.Name] = new Dictionary<string, object> {
									{ "ContainsGenericParameters", method.ContainsGenericParameters},
									{ "IsAbstract", method.IsAbstract },
									{ "IsAssembly", method.IsAssembly },
									{ "IsConstructor", method.IsConstructor },
									{ "IsFamily", method.IsFamily },
									{ "IsFinal", method.IsFinal },
									{ "IsGenericMethod", method.IsGenericMethod },
									{ "IsPrivate", method.IsPrivate },
									{ "IsPublic", method.IsPublic },
									{ "IsStatic", method.IsStatic },
									{ "IsVirtual", method.IsVirtual },
									{ "Parameters", paramList },
									{ "ReturnParameter", method.ReturnParameter },
									{ "ReturnType", method.ReturnType.FullName },
									//{ "Type", method.GetType().FullName },
									{ "DeclaringType", method.DeclaringType.FullName },
								};
							}
							resp.result = dict;
							break;
						}
					case "GetProperties": {
							var props = target.GetType().GetProperties(
								BindingFlags.FlattenHierarchy | BindingFlags.Instance |
								BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Static);
							var dict = new Dictionary<string, Dictionary<string, object>>();
							foreach(PropertyInfo prop in props) {
								dict[prop.Name] = new Dictionary<string, object> {
									{ "CanRead", prop.CanRead },
									{ "CanWrite", prop.CanWrite },
									{ "Type", prop.GetType().FullName },
									{ "DeclaringType", prop.DeclaringType.FullName },
								};
							}
							resp.result = dict;
							break;
						}
					case "GetType":
						resp.result = target.GetType(); //.FullName;
						break;
					case "GetValue": {
							string field = msg.GetString("field");
							object[] index = msg.GetObjectArray("index");
							var prop = target.GetType().GetProperty(field);
							if(prop == null) {
								resp.error = $"No field '{field}' in object '{objectName}'";
							}
							else resp.result = prop.GetValue(target, index);
							break;
						}
					case "Invoke":
						//XXX cast args to appropriate types...
						resp.result = target.GetType().GetMethod(
							msg.GetString("field")).Invoke(
							target, msg.GetObjectArray("args"));
						break;
					case "SetValue": {
							string field = msg.GetString("field");
							object value = msg.GetObject("value");
							object[] index = msg.GetObjectArray("index");
							Log($"Set field '{field}' of '{objectName}' to value '{value}'");
							var prop = target.GetType().GetProperty(field);
							if(prop == null) {
								resp.error = $"No field '{field}' in object '{objectName}'";
							}
							else prop.SetValue(target, value, index);
							break;
						}
					default:
						throw new ArgumentException($"Invalid method {action}");
				}
				SendJson(resp);
			}
			catch(Exception ex) {
				resp.error = ex.ToString();
				SendJson(resp);
			}
		}

		public Dictionary<string, object> GetParamInfo(ParameterInfo param) {
			return new Dictionary<string, object> {
				{ "Name", param.Name },
				{ "DefaultValue", param.DefaultValue },
				//{ "Type", param.GetType().FullName },
				{ "IsIn", param.IsIn },
				{ "IsLcid", param.IsLcid },
				{ "IsOptional", param.IsOptional },
				{ "IsOut", param.IsOut },
				{ "IsRetval", param.IsRetval },
				{ "ParameterType", param.ParameterType.FullName },
				{ "Position", param.Position },
			};
		}

		public object GetManager(string name) {
			switch(name) {
				//Not using IManagers because it doesn't have all of them...
				case "AudioManager": return Singleton<AudioManager>.instance;
				case "BuildingManager": return Singleton<BuildingManager>.instance;
				case "CitizenManager": return Singleton<CitizenManager>.instance;
				case "DisasterManager": return Singleton<DisasterManager>.instance;
				case "DistrictManager": return Singleton<DistrictManager>.instance;
				case "EconomyManager": return Singleton<EconomyManager>.instance;
				case "ElectricityManager": return Singleton<ElectricityManager>.instance;
				case "EventManager": return Singleton<EventManager>.instance;
				case "GameAreaManager": return Singleton<GameAreaManager>.instance;
				case "ImmaterialResourceManager": return Singleton<ImmaterialResourceManager>.instance;
				case "MessageManager": return Singleton<MessageManager>.instance;
				case "NaturalResourceManager": return Singleton<NaturalResourceManager>.instance;
				case "NetManager": return Singleton<NetManager>.instance;
				case "PathManager": return Singleton<PathManager>.instance;
				case "PropManager": return Singleton<PropManager>.instance;
				case "RenderManager": return Singleton<RenderManager>.instance;
				case "SimulationManager": return Singleton<SimulationManager>.instance;
				case "TerrainManager": return Singleton<TerrainManager>.instance;
				case "TransferManager": return Singleton<TransferManager>.instance;
				case "TransportManager": return Singleton<TransportManager>.instance;
				case "TreeManager": return Singleton<TreeManager>.instance;
				case "VehicleManager": return Singleton<VehicleManager>.instance;
				case "WeatherManager": return Singleton<WeatherManager>.instance;
				case "WaterManager": return Singleton<WaterManager>.instance;
				case "ZoneManager": return Singleton<ZoneManager>.instance;
				default: return null;
			}
		}
	}
}