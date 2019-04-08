using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about game limits.
	/// </summary>
	public class LimitsHandler: SocketHandlerBase {

		public LimitsHandler(SocketRequestHandler handler) :
		base(handler, "Limits") {
			Log("Sending limits...");
			Update();
			Log("Sent limits.");
		}

		protected void Update() {
			Dictionary<String, Dictionary<String, float>> limits =
			new Dictionary<String, Dictionary<String, float>> {
				["AudioManager"] = new Dictionary<String, float> {
					["BYTES_PER_CHANNEL"] = AudioManager.BYTES_PER_CHANNEL,
					["MAX_RADIO_CHANNEL_COUNT"] = AudioManager.MAX_RADIO_CHANNEL_COUNT,
					["MAX_RADIO_CONTENT_COUNT"] = AudioManager.MAX_RADIO_CONTENT_COUNT,
					["STREAM_BUFFER_LIMIT"] = AudioManager.STREAM_BUFFER_LIMIT,
					["STREAM_BUFFER_SIZE"] = AudioManager.STREAM_BUFFER_SIZE,
				},

				["BuildingManager"] = new Dictionary<String, float> {
					["MAX_ASSET_BUILDINGS"] = BuildingManager.MAX_ASSET_BUILDINGS,
					["MAX_BUILDING_COUNT"] = BuildingManager.MAX_BUILDING_COUNT,
					["MAX_MAP_BUILDINGS"] = BuildingManager.MAX_MAP_BUILDINGS,
					["MAX_OUTSIDE_CONNECTIONS"] = BuildingManager.MAX_OUTSIDE_CONNECTIONS,
				},

				["CitizenManager"] = new Dictionary<String, float> {
					["CITIZENGRID_CELL_SIZE"] = CitizenManager.CITIZENGRID_CELL_SIZE,
					["CITIZENGRID_RESOLUTION"] = CitizenManager.CITIZENGRID_RESOLUTION,
					["MAX_CITIZEN_COUNT"] = CitizenManager.MAX_CITIZEN_COUNT,
					["MAX_INSTANCE_COUNT"] = CitizenManager.MAX_INSTANCE_COUNT,
					["MAX_UNIT_COUNT"] = CitizenManager.MAX_UNIT_COUNT,
				},

				["DisasterManager"] = new Dictionary<String, float> {
					["EVACUATIONMAP_CELL_SIZE"] = DisasterManager.EVACUATIONMAP_CELL_SIZE,
					["EVACUATIONMAP_RESOLUTION"] = DisasterManager.EVACUATIONMAP_RESOLUTION,
					["HAZARDMAP_CELL_SIZE"] = DisasterManager.HAZARDMAP_CELL_SIZE,
					["HAZARDMAP_RESOLUTION"] = DisasterManager.HAZARDMAP_RESOLUTION,
					["MAX_DISASTER_COUNT"] = DisasterManager.MAX_DISASTER_COUNT,
					["MAX_SCENARIO_DISASTERS"] = DisasterManager.MAX_SCENARIO_DISASTERS,
				},

				["DistrictManager"] = new Dictionary<String, float> {
					["CELL_AREA_TO_SQUARE"] = DistrictManager.CELL_AREA_TO_SQUARE,
					["DISTRICTGRID_CELL_SIZE"] = DistrictManager.DISTRICTGRID_CELL_SIZE,
					["DISTRICTGRID_RESOLUTION"] = DistrictManager.DISTRICTGRID_RESOLUTION,
					["MAX_DISTRICT_COUNT"] = DistrictManager.MAX_DISTRICT_COUNT,
				},

				["EconomyManager"] = new Dictionary<String, float> {
					["MAX_LOANS"] = EconomyManager.MAX_LOANS,
				},

				["ElectricityManager"] = new Dictionary<String, float> {
					["CONDUCTIVITY_LIMIT"] = ElectricityManager.CONDUCTIVITY_LIMIT,
					["ELECTRICITYGRID_CELL_SIZE"] = ElectricityManager.ELECTRICITYGRID_CELL_SIZE,
					["ELECTRICITYGRID_RESOLUTION"] = ElectricityManager.ELECTRICITYGRID_RESOLUTION,
					["MAX_PULSE_GROUPS"] = ElectricityManager.MAX_PULSE_GROUPS,
				},

				["EventManager"] = new Dictionary<String, float> {
					["MAX_EVENT_COUNT"] = EventManager.MAX_EVENT_COUNT,
				},

				["GameAreaManager"] = new Dictionary<String, float> {
					["AREAGRID_CELL_SIZE"] = GameAreaManager.AREAGRID_CELL_SIZE,
					["AREAGRID_RESOLUTION"] = GameAreaManager.AREAGRID_RESOLUTION,
					["AREA_MAP_RESOLUTION"] = GameAreaManager.AREA_MAP_RESOLUTION,
					["DEFAULT_START_X"] = GameAreaManager.DEFAULT_START_X,
					["DEFAULT_START_Z"] = GameAreaManager.DEFAULT_START_Z,
					["TOTAL_AREA_RESOLUTION"] = GameAreaManager.TOTAL_AREA_RESOLUTION,
				},

				["ImmaterialResourceManager"] = new Dictionary<String, float> {
					["RESOURCE_COUNT"] = ImmaterialResourceManager.RESOURCE_COUNT,
					["RESOURCEGRID_CELL_SIZE"] = ImmaterialResourceManager.RESOURCEGRID_CELL_SIZE,
					["RESOURCEGRID_RESOLUTION"] = ImmaterialResourceManager.RESOURCEGRID_RESOLUTION,
				},

				["MessageManager"] = new Dictionary<String, float> {
					["RANDOM_MESSAGE_COUNT"] = MessageManager.RANDOM_MESSAGE_COUNT,
					["RECENT_MESSAGE_COUNT"] = MessageManager.RECENT_MESSAGE_COUNT,
				},

				["NaturalResourceManager"] = new Dictionary<String, float> {
					["RESOURCEGRID_CELL_SIZE"] = NaturalResourceManager.RESOURCEGRID_CELL_SIZE,
					["RESOURCEGRID_RESOLUTION"] = NaturalResourceManager.RESOURCEGRID_RESOLUTION,
				},

				["NetManager"] = new Dictionary<String, float> {
					["MAX_ASSET_LANES"] = NetManager.MAX_ASSET_LANES,
					["MAX_ASSET_NODES"] = NetManager.MAX_ASSET_NODES,
					["MAX_ASSET_SEGMENTS"] = NetManager.MAX_ASSET_SEGMENTS,
					["MAX_LANE_COUNT"] = NetManager.MAX_LANE_COUNT,
					["MAX_MAP_LANES"] = NetManager.MAX_MAP_LANES,
					["MAX_MAP_NODES"] = NetManager.MAX_MAP_NODES,
					["MAX_MAP_SEGMENTS"] = NetManager.MAX_MAP_SEGMENTS,
					["MAX_NODE_COUNT"] = NetManager.MAX_NODE_COUNT,
					["MAX_SEGMENT_COUNT"] = NetManager.MAX_SEGMENT_COUNT,
					["NODEGRID_CELL_SIZE"] = NetManager.NODEGRID_CELL_SIZE,
					["NODEGRID_RESOLUTION"] = NetManager.NODEGRID_RESOLUTION,
				},

				["PathManager"] = new Dictionary<String, float> {
					["MAX_PATHUNIT_COUNT"] = PathManager.MAX_PATHUNIT_COUNT,
				},

				["PropManager"] = new Dictionary<String, float> {
					["MAX_ASSET_PROPS"] = PropManager.MAX_ASSET_PROPS,
					["MAX_MAP_PROPS"] = PropManager.MAX_MAP_PROPS,
					["MAX_PROP_COUNT"] = PropManager.MAX_PROP_COUNT,
					["PROPGRID_CELL_SIZE"] = PropManager.PROPGRID_CELL_SIZE,
					["PROPGRID_RESOLUTION"] = PropManager.PROPGRID_RESOLUTION,
				},

				["RenderManager"] = new Dictionary<String, float> {
					["GROUP_CELL_SIZE"] = RenderManager.GROUP_CELL_SIZE,
					["GROUP_RESOLUTION"] = RenderManager.GROUP_RESOLUTION,
					["MAX_INSTANCE_HOLDERS"] = RenderManager.MAX_INSTANCE_HOLDERS,
					["MEGA_GROUP_CELL_SIZE"] = RenderManager.MEGA_GROUP_CELL_SIZE,
					["MEGA_GROUP_RESOLUTION"] = RenderManager.MEGA_GROUP_RESOLUTION,
					["OBJECT_COLORMAP_WIDTH"] = RenderManager.OBJECT_COLORMAP_WIDTH,
					["OBJECT_COLORMAP_HEIGHT"] = RenderManager.OBJECT_COLORMAP_HEIGHT,
				},

				["SimulationManager"] = new Dictionary<String, float> {
					["DAYTIME_FRAME_TO_HOUR"] = SimulationManager.DAYTIME_FRAME_TO_HOUR,
					["DAYTIME_FRAMES"] = SimulationManager.DAYTIME_FRAMES,
					["DAYTIME_HOUR_TO_FRAME"] = SimulationManager.DAYTIME_HOUR_TO_FRAME,
					["RELATIVE_DAY_LENGTH"] = SimulationManager.RELATIVE_DAY_LENGTH,
					["RELATIVE_NIGHT_LENGTH"] = SimulationManager.RELATIVE_NIGHT_LENGTH,
					["SIMULATION_PRIORITY"] = (float)SimulationManager.SIMULATION_PRIORITY,
					["SUNRISE_HOUR"] = SimulationManager.SUNRISE_HOUR,
					["SUNSET_HOUR"] = SimulationManager.SUNSET_HOUR,
					["SYNCHRONIZE_TIMEOUT"] = SimulationManager.SYNCHRONIZE_TIMEOUT,
				},

				["TerrainManager"] = new Dictionary<String, float> {
					["DETAIL_CELL_SIZE"] = TerrainManager.DETAIL_CELL_SIZE,
					["DETAIL_MAP_RESOLUTION"] = TerrainManager.DETAIL_MAP_RESOLUTION,
					["DETAIL_RESOLUTION"] = TerrainManager.DETAIL_RESOLUTION,
					["DETAIL_SHIFT"] = TerrainManager.DETAIL_SHIFT,
					["DIRT_BUFFER_SIZE"] = TerrainManager.DIRT_BUFFER_SIZE,
					["MAX_SUBPATCH_LEVELS"] = TerrainManager.MAX_SUBPATCH_LEVELS,
					["MIN_WATER_AMOUNT"] = TerrainManager.MIN_WATER_AMOUNT,
					["PATCH_RESOLUTION"] = TerrainManager.PATCH_RESOLUTION,
					["RAW_CELL_SIZE"] = TerrainManager.RAW_CELL_SIZE,
					["RAW_MAP_RESOLUTION"] = TerrainManager.RAW_MAP_RESOLUTION,
					["RAW_RESOLUTION"] = TerrainManager.RAW_RESOLUTION,
					["TERRAIN_HEIGHT"] = TerrainManager.TERRAIN_HEIGHT,
					["TERRAIN_LEVEL"] = TerrainManager.TERRAIN_LEVEL,
				},

				["TransferManager"] = new Dictionary<String, float> {
					["LARGE_POS_LIMIT"] = TransferManager.LARGE_POS_LIMIT,
					["REASON_CELL_SIZE_LARGE"] = TransferManager.REASON_CELL_SIZE_LARGE,
					["REASON_CELL_SIZE_SMALL"] = TransferManager.REASON_CELL_SIZE_SMALL,
					["TRANSFER_OFFER_COUNT"] = TransferManager.TRANSFER_OFFER_COUNT,
					["TRANSFER_PRIORITY_COUNT"] = TransferManager.TRANSFER_PRIORITY_COUNT,
					["TRANSFER_REASON_COUNT"] = TransferManager.TRANSFER_REASON_COUNT,
				},

				["TransportManager"] = new Dictionary<String, float> {
					["MAX_LINE_COUNT"] = TransportManager.MAX_LINE_COUNT,
				},

				["TreeManager"] = new Dictionary<String, float> {
					["MAX_ASSET_TREES"] = TreeManager.MAX_ASSET_TREES,
					["MAX_MAP_TREES"] = TreeManager.MAX_MAP_TREES,
					["MAX_TREE_COUNT"] = TreeManager.MAX_TREE_COUNT,
					["TREEGRID_CELL_SIZE"] = TreeManager.TREEGRID_CELL_SIZE,
					["TREEGRID_RESOLUTION"] = TreeManager.TREEGRID_RESOLUTION,
				},

				["VehicleManager"] = new Dictionary<String, float> {
					["MAX_PARKED_COUNT"] = VehicleManager.MAX_PARKED_COUNT,
					["MAX_VEHICLE_COUNT"] = VehicleManager.MAX_VEHICLE_COUNT,
					["VEHICLEGRID_CELL_SIZE"] = VehicleManager.VEHICLEGRID_CELL_SIZE,
					["VEHICLEGRID_CELL_SIZE2"] = VehicleManager.VEHICLEGRID_CELL_SIZE2,
					["VEHICLEGRID_RESOLUTION"] = VehicleManager.VEHICLEGRID_RESOLUTION,
					["VEHICLEGRID_RESOLUTION2"] = VehicleManager.VEHICLEGRID_RESOLUTION2,
				},

				["WeatherManager"] = new Dictionary<String, float> {
					["WINDGRID_CELL_SIZE"] = WeatherManager.WINDGRID_CELL_SIZE,
					["WINDGRID_RESOLUTION"] = WeatherManager.WINDGRID_RESOLUTION,
				},

				["WaterManager"] = new Dictionary<String, float> {
					["CONDUCTIVITY_LIMIT"] = WaterManager.CONDUCTIVITY_LIMIT,
					["MAX_PULSE_GROUPS"] = WaterManager.MAX_PULSE_GROUPS,
					["WATERGRID_CELL_SIZE"] = WaterManager.WATERGRID_CELL_SIZE,
					["WATERGRID_RESOLUTION"] = WaterManager.WATERGRID_RESOLUTION,
				},

				["ZoneManager"] = new Dictionary<String, float> {
					["MAX_ASSET_BLOCKS"] = ZoneManager.MAX_ASSET_BLOCKS,
					["MAX_BLOCK_COUNT"] = ZoneManager.MAX_BLOCK_COUNT,
					["MAX_MAP_BLOCKS"] = ZoneManager.MAX_MAP_BLOCKS,
					["ZONEGRID_CELL_SIZE"] = ZoneManager.ZONEGRID_CELL_SIZE,
					["ZONEGRID_RESOLUTION"] = ZoneManager.ZONEGRID_RESOLUTION,
				},
			};
			SendJson(limits);
		}
	}
}