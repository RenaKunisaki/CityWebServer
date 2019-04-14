class TimeChart {
    /** A line chart displaying values over in-game time.
     */
    constructor(params) {
        this.app = params.app;
        this._data = {};
        this._maxLength = params.maxLength || 31;
        this._element = params.element;
        const ctx = this._element.getContext('2d');

        const datasets = [];
        this._labels = [];
        for(let i=0; i<params.datasets.length; i++) {
            const param = params.datasets[i];
            this._data[param.label] = [];
            this._labels.push(param.label);
            const dataSet = {
                backgroundColor: param.color,
                //hoverBackgroundColor: bgColors[i],
                borderWidth: 0.5,
                pointRadius: 0.5,
                borderColor: param.color,
                data: this._data[param.label],
                fill: param.fill == undefined ? false : param.fill,
            }
            datasets.push(dataSet);
        }

        this._chart = new Chart(ctx, merge({
            type: 'line',
            options: {
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
                            pointRadius: 1,
                        },
                    }],
                    yAxes: [{
                        gridLines: {
                            color: 'rgba(255, 255, 255, 0.2)',
                            zeroLineColor: 'rgba(255, 255, 255, 0.2)',
                            display: true,
                            lineWidth: 1,
                            pointRadius: 1,
                        },
                    }],
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
                            return this._labels[item[0].datasetIndex];
                        },
                    },
                },
            },
            data: {
                labels: [],
                datasets: datasets,
            },
        }, params));
    }

    add(time, values) {
        let year  = time.getFullYear();
        let month = time.getMonth();
        let day   = time.getDate();
        const dayS   = String(day).padStart(2, "0");
        const monthS = String(month+1).padStart(2, "0");

        if(year != this.prevYear) {
            this._chart.data.labels.push(`${year}/${monthS}/${dayS}`);
        }
        else if(month != this.prevMonth) {
            this._chart.data.labels.push(`${monthS}/${dayS}`);
        }
        else {
            this._chart.data.labels.push(`${dayS}`);
        }

        for(const name of this._labels) {
            this._data[name].push({x:time, y:values[name]});
            if(this._data[name].length > this.maxLength) {
                this._data[name].unshift();
            }
        }
    }

    update() {
        this._chart.update();
    }
}
