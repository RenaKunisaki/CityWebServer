class Population {
    constructor(app) {
        this.app = app;
        this.colors = {
            Children:   "#0040FF",
            Teen:       "#00FF00",
            YoungAdult: "#FF4040",
            Adult:      "#FFC000",
            Senior:     "#800080",
        };
        this._makeCharts();
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
        const labels = ["Children", "Teen", "YoungAdult", "Adult", "Senior"];
        const bgColors = [];

        for(const name of labels) {
            const color = this.colors[name];
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

    //This class updates when called from App, since all the data is there.
    update(data) {
        const dataSet = [];
        for(let item of data.GlobalDistrict.PopulationData) {
            this.rows[item.Name].number(item.Amount);
            dataSet.push(item.Amount);
        }
        $('.population.number').number(data.GlobalDistrict.TotalPopulationCount);
        this.chart.data.datasets[0].data = dataSet;
        this.chart.update();
    }
}
