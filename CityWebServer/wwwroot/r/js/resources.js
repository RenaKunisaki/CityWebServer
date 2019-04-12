class Resources {
    constructor(app) {
        this.app = app;
    }

    run() {
        this._makeCharts();
        this.app.registerMessageHandler("District", (data) => this._update());
    }

    _makeCharts() {
        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: { display: false },
            scales: {
                xAxes: [{
                    gridLines: {
                        color: 'rgba(255, 255, 255, 0.2)',
                        zeroLineColor: 'rgba(255, 255, 255, 0.2)',
                        display: true,
                        lineWidth: 1,
                    },
                }],
                yAxes: [{
                    barPercentage: 1.0,
                    categoryPercentage: 1.0,
                    min: 0, max: 100,
                    suggestedMin: 0, suggestedMax: 100,
                    gridLines: {
                        color: 'rgba(255, 255, 255, 0.2)',
                        zeroLineColor: 'rgba(255, 255, 255, 0.2)',
                        display: true,
                        lineWidth: 1,
                    },
                }],
            },
            tooltips: {
                callbacks: {
                    label: (item, data) => {
                        const label = data.labels[item.index];
                        const field = this.fields[label];
                        const dset  = data.datasets[item.datasetIndex];
                        let value = field.get(this._lastData);
                        let max = field.max;
                        if(max instanceof Function) max = max(this._lastData);
                        let pct = Math.round((value / max) * 100).toLocaleString();
                        const scale = field.scale;
                        if(scale != undefined) {
                            value *= scale;
                            max *= scale;
                        }
                        value = Math.round(value).toLocaleString();
                        max = Math.round(max).toLocaleString();
                        let unit = field.unit;
                        if(unit == undefined) unit = '';
                        return `${label}: ${value} / ${max}${unit} (${pct}%)`;
                    },
                },
            },
        };

        this.fields = {
            "Traffic Flow": {
                max: 100,
                get: data => data.Tick.trafficFlow,
                unit: '%',
            },
            "Electricity Usage": {
                max: data => data.District.ElectricityCapacity,
                get: data => data.District.ElectricityConsumption,
                scale: 1/1000, //data is given in KW but game shows in MW
                unit: " MW",
            },
            "Water Usage": {
                max: data => data.District.WaterCapacity,
                get: data => data.District.WaterConsumption,
                unit: " m³"
            },
            "Sewer Usage": {
                max: data => data.District.SewageCapacity,
                get: data => data.District.SewageAccumulation,
                unit: " m³"
            },
            "Heat Usage": {
                max: data => data.District.HeatingCapacity,
                get: data => data.District.HeatingConsumption,
                scale: 1/1000, //data is given in KW but game shows in MW
                unit: " MW",
            },
            "Landfill Usage": {
                max: data => data.District.GarbageCapacity,
                get: data => data.District.GarbageAmount,
            },
            "Hospital Usage": {
                max: data => data.District.HealCapacity,
                get: data => data.District.SickCount,
                unit: " people",
            },
            "Cemetery Usage": {
                max: data => data.District.DeadCapacity,
                get: data => data.District.DeadAmount,
                unit: "people",
            },
            "Water Storage": {
                max: data => data.District.WaterStorageCapacity,
                get: data => data.District.WaterStorageAmount,
                unit: " m³"
            },
            "Shelter Usage": {
                max: data => data.District.ShelterCitizenCapacity,
                get: data => data.District.ShelterCitizenNumber,
                unit: " people",
            },
            "E.School Usage": {
                max: data => data.District.Education1Capacity,
                get: data => data.District.Education1Need,
                unit: " people",
            },
            "H.School Usage": {
                max: data => data.District.Education2Capacity,
                get: data => data.District.Education2Need,
                unit: " people",
            },
            "University Usage": {
                max: data => data.District.Education3Capacity,
                get: data => data.District.Education3Need,
                unit: " people",
            },
            "Jail Usage": {
                max: data => data.District.CriminalCapacity,
                get: data => data.District.CriminalAmount,
                unit: " people",
            },
            "Job Usage": {
                max: data => data.District.WorkplaceCount,
                get: data => data.District.WorkerCount,
                unit: " jobs",
            },
            "Unemployment": {
                max: 100,
                get: data => data.District.Unemployment,
                unit: '%',
            },
            "Water Pollution": {
                max: 100,
                get: data => data.District.WaterPollution,
                unit: '%',
            },
            "Ground Pollution": {
                max: 100,
                get: data => data.District.GroundPollution,
                unit: '%',
            },
        };

        const labels = [];
        const bgColors = [];
        for(const [name, field] of Object.entries(this.fields)) {
            labels.push(name);
            bgColors.push(this.app.makeNameColor(name));
        }

        let ctx = $('#resources canvas')[0].getContext('2d');
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

    _update() {
        const data = this.app.data;
        this._lastData = this.app.data;
        const dataSet = [];
        for(const [name, field] of Object.entries(this.fields)) {
            let val = field.get(data);
            let max = field.max;
            if(max instanceof Function) max = max(data);
            dataSet.push((val / max) * 100);
        }
        this.chart.data.datasets[0].data = dataSet;
        this.chart.update();
    }
}
