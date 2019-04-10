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
        this._makeCharts();
    }

    run() {
        this.app.registerMessageHandler("District", (data) => this._update(data));
        console.log("Population online.")
    }

    _makeCharts() {
        this._table = $('<table id="population-table">');
        this.rows   = {};

        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: { display: false },
        };
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

        $('#population .body').append(this._table);

        let ctx = $('#population canvas')[0].getContext('2d');
        this.chart = new Chart(ctx, {
            type: 'pie',
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

    _update(district) {
        if(district.ID != 0) return;
        const dataSet = [];
        for(const [name, color] of Object.entries(this.colors)) {
            this.rows[name].number(district[name]);
            dataSet.push(district[name]);
        }

        this.chart.data.datasets[0].data = dataSet;
        this.chart.update();
    }
}
