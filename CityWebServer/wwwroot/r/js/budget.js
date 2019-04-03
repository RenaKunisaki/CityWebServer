class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this._isInit = false;

        //for some reason the in-game bank names
        //just return BankA, BankB, BankC. XXX fix this.
        this.bankNames = [
            "Silver Sunset Bank",
            "Global Credit Inc",
            "Pyramid Capital",
        ];
    }

    _init(data) {
        this._makeCharts(data);
    }

    makeNameColor(name) {
        //Try to match game colors
        if(name.startsWith('Residential_Low'))  return '#80FF00';
        if(name.startsWith('Residential_High')) return '#40C000';
        if(name.startsWith('Commercial_Low'))   return '#0080FF';
        if(name.startsWith('Commercial_High'))  return '#0040C0';
        if(name.startsWith('Industrial'))       return '#FF8000';
        if(name.startsWith('Office'))           return '#00C0C0';
        return this.app.makeNameColor(name);
    }

    _makeCharts(data) {
        const legend = $('<table class="legend">').append(
            $('<tr>').append(
                $('<th class="name">').text("Service"),
                $('<th class="income">').text("Income ₡"),
                $('<th class="expense">').text("Cost ₡"),
                $('<th class="net">').text("Net ₡"),
            ),
        );
        const options = {
            layout: {
                padding: { left: 0, right: 0, bottom: 0, top: 0 },
            },
            legend: { display: false },
            tooltips: {
                callbacks: {
                    label: (item, data) => {
                        const label = data.labels[item.index];
                        const dset  = data.datasets[item.datasetIndex];
                        const value = dset.data[item.index];
                        const amount = (value / 100).toFixed(0);
                        //XXX format amount as number (commas)
                        return `${label}: ₡${amount}`;
                    },
                },
            },
        };
        const labels      = [];
        const bgColors    = [];
        const incomeData  = [];
        const expenseData = [];

        const items = Object.entries(data.economy.incomesAndExpenses);
        let totalIncome=0, totalExpense=0;
        for(const [name, item] of items) {
            totalIncome += item.Income;
            totalExpense += item.Expense;
        }
        items.push(["Unknown", {
            Income:  data.totalIncome   - totalIncome,
            Expense: data.totalExpenses - totalExpense,
        }]);

        for(const [name, item] of items) {
            labels.push(name);
            const color = this.makeNameColor(name);
            bgColors.push(color);
            incomeData.push(item.Income);
            expenseData.push(item.Expense);

            const cellIncome = $(`<td id="income-${name}" class="money income">`);
            const cellCost = $(`<td id="cost-${name}" class="money expense">`);
            const cellNet = $(`<td id="net-${name}" class="money net">`);
            cellIncome.number(item.Income / 100);
            cellCost.number(item.Expense / 100);
            cellNet.number((item.Income - item.Expense) / 100);
            cellNet.toggleClass('negative', item.Income < item.Expense);

            let row = $(`<tr id="legend-row-${name}">`).append(
                $('<td>').append(
                    $('<div class="legend-box">')
                        .css('background-color', color),
                    $('<span class="label">').text(name),
                ),
                cellIncome, cellCost, cellNet,
            );
            if(item.Income == 0 && item.Expense == 0) {
                row.addClass('zero');
            }
            legend.append(row);

        }


        let ctx = $('#income-chart canvas')[0].getContext('2d');
        this.chartIncome = new Chart(ctx, {
            type: 'pie',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    hoverBackgroundColor: bgColors,
                    borderWidth: 0,
                    borderColor: '#000',
                    data: incomeData,
                }],
            },
        });

        ctx = $('#expense-chart canvas')[0].getContext('2d');
        this.chartExpenses = new Chart(ctx, {
            type: 'pie',
            options: options,
            data: {
                labels: labels,
                datasets: [{
                    backgroundColor: bgColors,
                    hoverBackgroundColor: bgColors,
                    borderWidth: 0,
                    borderColor: '#000',
                    data: expenseData,
                }],
            },
        });
        $('#budget-legend').append(legend);
    }

    _updateCharts(data) {
        const incomeData  = [];
        const expenseData = [];
        for(const [name, item] of Object.entries(data.economy.incomesAndExpenses)) {
            //XXX are we sure these will always be in the same order?
            incomeData.push(item.Income);
            expenseData.push(item.Expense);
            $(`#income-${name}`).number(item.Income / 100);
            $(`#cost-${name}`).number(item.Expense / 100);
            $(`#net-${name}`).number((item.Income - item.Expense) / 100)
                .toggleClass('negative', item.Income < item.Expense);
            $(`#legend-row-${name}`).toggleClass('zero',
                (item.Income == 0 && item.Expense == 0));
        }

        this.chartIncome.data.datasets[0].data = incomeData;
        this.chartExpenses.data.datasets[0].data = expenseData;
        this.chartIncome.update();
        this.chartExpenses.update();
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Budget online.")
        this._refresh();
    }

    _refresh() {
        $.getJSON('/Budget', (data) => {
            //console.log("Budget data:", data);
            if(!this._isInit) {
                this._init(data);
                this._isInit = true;
            }

            const net = data.totalIncome - data.totalExpenses;

            $('#income-chart .total').number(data.totalIncome / 100)
                .toggleClass('negative', data.totalIncome <= 0);

            $('#expense-chart .total').number(data.totalExpenses / 100)
                .toggleClass('negative', data.totalExpenses <= 0);

            $('#net-income').number(net / 100)
                .toggleClass('negative', net <= 0);

            $('#current-cash').number(data.currentCash / 100)
                .toggleClass('negative', data.currentCash <= 0);

            const taxes = Object.entries(data.economy.taxRates);
            for(const [name, rate] of taxes) {
                $(`#tax-${name}`).number(rate);
            }

            for(let i=0; i < data.loans.length; i++) {
                const loan = data.loans[i];
                $(`#loan${i}`).empty().append(
                    //$('<td>').text(loan.BankName),
                    $('<td>').text(this.bankNames[i]),
                    $('<td class="money">').number(loan.PaymentLeft / 100),
                    $('<td class="number">').text('XXX'), //time left
                    $('<td class="number">').text(
                        (loan.InterestRate / 100).toFixed(0) + '%'),
                    $('<td class="money">').text('XXX'), //weekly cost

                    //Length: number of weeks of repayment total
                    //No idea how to get number of weeks left or start date,
                    //or cost per week
                    //Should be 408 and 442.31
                );
            }

            this._updateCharts(data);
        });
    }
}
