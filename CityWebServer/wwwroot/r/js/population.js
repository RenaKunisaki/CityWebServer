class Population {
    constructor(app) {
        this.app = app;
        this.colors = {
            Children:    "#0040FF",
            Teens:       "#00FF00",
            YoungAdults: "#FF4040",
            Adults:      "#FFC000",
            Seniors:     "#800080",
        };
        this.prevYear  = null;
        this.prevMonth = null;
        this.prevDay   = null;
        this._makeCharts();
    }

    run() {
        this.app.registerMessageHandler("District", (data) => this._update(data));
        console.log("Population online.")
    }

    _makeCharts() {
        this._table = $('<table id="population-table">');
        this.rows   = {};

        const labels = [];
        const bgColors = [];
        for(const [name, color] of Object.entries(this.colors)) {
            labels.push(name);
            bgColors.push(color);

            const td  = $('<td>');
            this._table.append($('<tr>').append(
                $('<th>').append(
                    $('<div class="legend-box">')
                        .css('background-color', color),
                    $('<span class="label">').text(name),
                ), td,
            ));
            this.rows[name] = td;
        }

        $('#population .chart').append(this._table);

        //Make pie chart
        let ctx = $('#population-chart canvas')[0].getContext('2d');
        this.chart = new Chart(ctx, {
            type: 'pie',
            options: {
                layout: {
                    padding: { left: 0, right: 0, bottom: 0, top: 0 },
                },
                legend: { display: false },
                tooltips: {
                    callbacks: {
                        label: (item, data) => {
                            const dset  = data.datasets[item.datasetIndex];
                            const value = dset.data[item.index];
                            const total = dset.data.reduce((a,b) => a+b, 0);
                            const pct   = ((value/total)*100).toFixed(0);
                            const amount = value.toLocaleString();
                            return `${amount} (${pct}%)`;
                        },
                        title: (item, data) => {
                            return labels[item[0].index];
                        },
                    },
                },
            },
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

        //Make graph
        const datasets = [];
        for(let i=0; i<labels.length; i++) {
            const dataSet = {
                color: bgColors[i],
                label: labels[i],
            }
            datasets.push(dataSet);
        }
        this.graph = new TimeChart({
            app: this.app,
            element: $('#population-graph canvas')[0],
            datasets: datasets,
            options: {
                scales: {
                    yAxes: [{
                        stacked: true,
                    }],
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

        this.graph.add(time, district);

        const pieDataSet = [];
        for(const [name, color] of Object.entries(this.colors)) {
            this.rows[name].number(district[name]);
            pieDataSet.push(district[name]);
        }

        this.chart.data.datasets[0].data = pieDataSet;
        this.chart.update();
        this.graph.update();

        this.prevYear  = year;
        this.prevMonth = month;
        this.prevDay   = day;
    }
}
