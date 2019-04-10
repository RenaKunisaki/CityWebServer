class Limits {
    constructor(app) {
        this.app = app;
    }

    async _makeCharts() {
        const limits = await this.app.query("Limits", "Limits");
        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: { display: false },
            scales: {
                yAxes: [{
                    barPercentage: 1.0,
                    categoryPercentage: 1.0,
                    min: 0, max: 100,
                    suggestedMin: 0, suggestedMax: 100,
                }],
            },
            tooltips: {
                callbacks: {
                    label: (item, data) => {
                        const label = data.labels[item.index];
                        const field = this.fields[label];
                        const dset  = data.datasets[item.datasetIndex];
                        const value = field.get(this._lastData);
                        return `${label}: ${value} / ${field.max}`;
                    },
                },
            },
        };

        const L = limits;
        this.fields = {
            "Buildings": {
                max: L.BuildingManager.MAX_BUILDING_COUNT,
                get: (data) => data.BuildingManager.m_buildingCount,
            },
            "Citizens": {
                max: L.CitizenManager.MAX_CITIZEN_COUNT,
                get: (data) => data.CitizenManager.m_citizenCount,
            },
            "Citizen Instances": {
                max: L.CitizenManager.MAX_INSTANCE_COUNT,
                get: (data) => data.CitizenManager.m_instanceCount,
            },
            "Districts": {
                max: L.DistrictManager.MAX_DISTRICT_COUNT,
                get: (data) => data.DistrictManager.m_districtCount,
            },
            "Road Lanes": {
                max: L.NetManager.MAX_LANE_COUNT,
                get: (data) => data.NetManager.m_laneCount,
            },
            "Road Nodes": {
                max: L.NetManager.MAX_NODE_COUNT,
                get: (data) => data.NetManager.m_nodeCount,
            },
            "Road Segments": {
                max: L.NetManager.MAX_SEGMENT_COUNT,
                get: (data) => data.NetManager.m_segmentCount,
            },
            "Path Units": {
                max: L.PathManager.MAX_PATHUNIT_COUNT,
                get: (data) => data.PathManager.m_pathUnitCount,
            },
            "Props": {
                max: L.PropManager.MAX_PROP_COUNT,
                get: (data) => data.PathManager.m_propCount,
            },
            "Transit Lines": {
                max: L.TransportManager.MAX_LINE_COUNT,
                get: (data) => data.TransportManager.m_lineCount,
            },
            "Trees": {
                //XXX this gives the vanilla max, even with mods to increase it
                max: L.TreeManager.MAX_TREE_COUNT,
                get: (data) => data.TreeManager.m_treeCount,
            },
            "Vehicles": {
                max: L.VehicleManager.MAX_VEHICLE_COUNT,
                get: (data) => data.VehicleManager.m_vehicleCount,
            },
            "Vehicles Parked": {
                max: L.VehicleManager.MAX_PARKED_COUNT,
                get: (data) => data.VehicleManager.m_parkedCount,
            },
            "Zone Blocks": {
                max: L.ZoneManager.MAX_BLOCK_COUNT,
                get: (data) => data.ZoneManager.m_blockCount,
            },
        };
        const labels = [];
        const bgColors = [];
        for(const [name, field] of Object.entries(this.fields)) {
            labels.push(name);
            bgColors.push(this.app.makeNameColor(name));
        }

        let ctx = $('#limits canvas')[0].getContext('2d');
        this.chart = new Chart(ctx, {
            type: 'horizontalBar',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    hoverBackgroundColor: bgColors,
                    borderWidth: 1,
                    borderColor: '#000',
                    data: [],
                }],
            },
        });
    }

    async run() {
        await this._makeCharts();
        this.app.registerMessageHandler("Instances", (data) => this._update(data));
        console.log("Limits online.")
    }

    _update(instances) {
        this._lastData = instances;
        const dataSet = [];
        for(const [name, field] of Object.entries(this.fields)) {
            let val = field.get(instances);
            dataSet.push((val / field.max) * 100);
        }
        this.chart.data.datasets[0].data = dataSet;
        this.chart.update();
    }
}
