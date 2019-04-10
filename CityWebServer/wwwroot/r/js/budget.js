class Budget {
    constructor(app) {
        this.app = app;
        this._isInit = false;

        //for some reason the in-game bank names
        //just return BankA, BankB, BankC. XXX fix this.
        this.bankNames = [
            "Silver Sunset Bank",
            "Global Credit Inc",
            "Pyramid Capital",
        ];

        this.groups = {
            Residential:     {color:'#80FF00'},
            Commercial:      {color:'#0080FF'},
            Industrial:      {color:'#FF8000'},
            Office:          {color:'#00C0C0'},
            Transit:         {},
            Parks:           {},
            Roads:           {},
            Electricity:     {},
            Water:           {},
            Garbage:         {},
            UniqueBuildings: {},
            Health:          {},
            Education:       {},
            Police:          {},
            Fire:            {},
            Disaster:        {},
            LoanPayments:    {},
            Policies:        {},
            Unknown:         {color: '#FF0000'},
        };
    }

    run() {
        this.app.registerMessageHandler("Budget", (data) => this._update(data));
        console.log("Budget online.")
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
                        const amount = Math.round(value / 100).toLocaleString();
                        return `${label}: ₡${amount}`;
                    },
                },
            },
        };
        const labels      = [];
        const bgColors    = [];
        const incomeData  = [];
        const expenseData = [];

        data.groups = {};

        const items = Object.entries(data.economy.incomesAndExpenses);
        let totalIncome=0, totalExpense=0;
        for(const [name, item] of items) {
            totalIncome += item.Income;
            totalExpense += item.Expense;
        }
        //Add a row for any discrepancies if totals don't match up
        items.push(["Unknown", {
            Income:  data.totalIncome   - totalIncome,
            Expense: data.totalExpenses - totalExpense,
        }]);

        for(const [name, item] of items) {
            let groupName = name.substring(0, name.indexOf('_'));
            if(groupName == '') groupName = name;
            let group = this.groups[groupName];
            if(data.groups[groupName] == undefined) {
                group.income  = item.Income;
                group.expense = item.Expense;
                data.groups[groupName] = group;
            }
            else {
                group.income  += item.Income;
                group.expense += item.Expense;
            }
        }

        for(const [name, group] of Object.entries(data.groups)) {
            labels.push(name);
            if(group.color == undefined) group.color = this.makeNameColor(name);
            bgColors.push(group.color);

            const cellIncome = $(`<td id="income-${name}" class="money income">`);
            const cellCost = $(`<td id="cost-${name}" class="money expense">`);
            const cellNet = $(`<td id="net-${name}" class="money net">`);
            cellIncome.money(group.income);
            cellCost.money(group.expense);
            const dispIn = Math.round(group.income/100).toLocaleString();
            const dispEx = Math.round(group.expense/100).toLocaleString();
            cellNet.money(group.income - group.expense).attr(
                'title', `Income: ₡${dispIn}\nExpense: ₡${dispEx}`
            );

            let row = $(`<tr id="legend-row-${name}">`).append(
                $('<td>').append(
                    $('<div class="legend-box">')
                        .css('background-color', group.color),
                    $('<span class="label">').text(name),
                ),
                cellIncome, cellCost, cellNet,
            );
            if(group.income == 0 && group.expense == 0) {
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
        data.groups = {};
        for(const [name, item] of Object.entries(data.economy.incomesAndExpenses)) {
            //XXX are we sure these will always be in the same order?
            let groupName = name.substring(0, name.indexOf('_'));
            if(groupName == '') groupName = name;
            let group = this.groups[groupName];
            if(data.groups[groupName] == undefined) {
                group.income  = item.Income;
                group.expense = item.Expense;
                data.groups[groupName] = group;
            }
            else {
                group.income  += item.Income;
                group.expense += item.Expense;
            }
        }

        for(const [name, group] of Object.entries(data.groups)) {
            incomeData.push(group.income);
            expenseData.push(group.expense);
            $(`#income-${name}`).number(group.income / 100);
            $(`#cost-${name}`).number(group.expense / 100);
            $(`#net-${name}`).number((group.income - group.expense) / 100)
                .toggleClass('negative', group.income < group.expense);
            $(`#legend-row-${name}`).toggleClass('zero',
                (group.income == 0 && group.expense == 0));
        }

        this.chartIncome.data.datasets[0].data = incomeData;
        this.chartExpenses.data.datasets[0].data = expenseData;
        this.chartIncome.update();
        this.chartExpenses.update();
    }

    _update(data) {
        if(!this._isInit) {
            this._init(data);
            this._isInit = true;
        }

        data.netIncome = data.totalIncome - data.totalExpenses;
        this._updateCharts(data);
    }
}
