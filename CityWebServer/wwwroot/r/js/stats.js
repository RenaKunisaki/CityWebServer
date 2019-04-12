class Stats {
    constructor(app) {
        this.app = app;
        this.prevYear  = null;
        this.prevMonth = null;
        this.prevDay   = null;
        this._makeCharts();
    }

    run() {
        this.app.registerMessageHandler("District", (data) => this._update(data));
        console.log("Stats online.")
    }

    _makeCharts(data) {
        this.fields = {
            "Birth Rate": {
                get: data => data.District.Births,
                color: '#009DF3',
            },
            "Death Rate": {
                get: data => data.District.Deaths,
                color: '#804040',
            },
            "Traffic Flow": {
                max: 100,
                get: data => data.Tick.trafficFlow,
                unit: '%',
            },
            "E.School Grads": {
                max: 100,
                get: data => data.District.Education1Rate,
                unit: "%",
            },
            "H.School Grads": {
                max: 100,
                get: data => data.District.Education2Rate,
                unit: "%",
            },
            "University Grads": {
                max: 100,
                get: data => data.District.Education3Rate,
                unit: "%",
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

        this.legend = $('#stats .legend');
        this.legendRows = {};
        const datasets = [];
        for(const [name, field] of Object.entries(this.fields)) {
            const color = field.color || this.app.makeNameColor(name);
            const dataSet = {
                color: color,
                label: name,
            }
            datasets.push(dataSet);

            const td = $('<td class="number">').text("...");
            this.legend.append($('<tr>').append(
                $('<th>').append(
                    $('<div class="legend-box">')
                        .css('background-color', color),
                    $('<span class="label">').text(name),
                ), td,
            ));
            this.legendRows[name] = td;
        }
        this.graph = new TimeChart({
            app: this.app,
            element: $('#stats canvas')[0],
            datasets: datasets,
            options: {
                layout: {
                    padding: { left: 0, right: 0, bottom: 0, top: 10 },
                },
            },
        });
    }

    _update(district) {
        if(district.ID != 0) return;
        if(!this.app.data.Tick) return;

        const time = new Date(this.app.data.Tick.Time);
        let year   = time.getFullYear();
        let month  = time.getMonth();
        let day    = time.getDate();
        if(day    == this.prevDay
        && month  == this.prevMonth
        && year   == this.prevYear) return;

        const data = {};
        for(const [name, field] of Object.entries(this.fields)) {
            let val = field.get(this.app.data);
            let max = field.max;
            if(max instanceof Function) max = max(this.app.data);
            if(max != undefined) val = (val / max) * 100;
            let unit = field.unit || '';
            data[name] = val;
            this.legendRows[name].text(val.toLocaleString()+unit);
        }
        this.graph.add(time, data);
        //for(const [name, color] of Object.entries(this.colors)) {
        //    this.rows[name].number(district[name]);
        //}

        this.graph.update();

        this.prevYear  = year;
        this.prevMonth = month;
        this.prevDay   = day;
    }
}
