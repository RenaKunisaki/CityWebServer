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
        this.oldData = {};
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
            this.oldData[name] = [];

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
                backgroundColor: bgColors[i],
                //hoverBackgroundColor: bgColors[i],
                borderWidth: 1,
                borderColor: bgColors[i],
                data: this.oldData[labels[i]],
                fill: '-1',
            }
            datasets.push(dataSet);
        }
        ctx = $('#population-graph canvas')[0].getContext('2d');
        this.graph = new Chart(ctx, {
            type: 'line',
            options: {
                layout: {
                    padding: { left: 0, right: 0, bottom: 0, top: 0 },
                },
                legend: { display: false },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]
                },
                tooltips: {
                    callbacks: {
                        label: (item, data) => {
                            const dset  = data.datasets[item.datasetIndex];
                            const value = dset.data[item.index];
                            const time  = new Date(value.x);
                            let year    = time.getFullYear();
                            let month   = this.app.monthNames[time.getMonth()];
                            let day     = String(time.getDate()).padStart(2, "0");
                            const amount = value.y.toLocaleString();
                            return `${amount} on ${year} ${month} ${day}`;
                        },
                        title: (item, data) => {
                            return labels[item[0].datasetIndex];
                        },
                    },
                },
            },
            data: {
                labels: [],
                datasets: datasets,
            },
        });
    }

    _update(district) {
        if(district.ID != 0) return;
        if(!this.app.data.Tick) return;
        const time = new Date(this.app.data.Tick.Time);
        let year  = time.getFullYear();
        let month = time.getMonth();
        let day   = time.getDate();

        if(day   == this.prevDay
        && month == this.prevMonth
        && year  == this.prevYear) return;
        const dayS   = String(day).padStart(2, "0");
        const monthS = String(month+1).padStart(2, "0");

        if(year != this.prevYear) {
            this.graph.data.labels.push(`${year}/${monthS}/${dayS}`);
        }
        else if(month != this.prevMonth) {
            this.graph.data.labels.push(`${monthS}/${dayS}`);
        }
        else {
            this.graph.data.labels.push(`${dayS}`);
        }


        this.prevYear  = year;
        this.prevMonth = month;
        this.prevDay   = day;

        const pieDataSet = [];
        for(const [name, color] of Object.entries(this.colors)) {
            this.rows[name].number(district[name]);
            pieDataSet.push(district[name]);
            this.oldData[name].push({x:time, y:district[name]});
            if(this.oldData[name].length > 31) {
                this.oldData[name].unshift();
            }
        }

        this.chart.data.datasets[0].data = pieDataSet;
        this.chart.update();
        this.graph.update();
    }
}
