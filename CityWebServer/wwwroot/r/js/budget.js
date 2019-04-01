class Budget {
    constructor(app) {
        this.app = app;
        this.updateInterval = 5000; //msec
        this.txtIncome = $('<span class="money income">');
        this.txtExpenses = $('<span class="money expenses">');
        this.txtDelta = $('<span class="money delta">');
        this.element = $('<div class="budget">').append(
            this.txtIncome,
            this.txtExpenses,
            this.txtDelta,
        );
    }

    run() {
        window.setInterval(() => {this._refresh()}, this.updateInterval);
        console.log("Budget online.")
    }

    _refresh() {
        $.getJSON('/Budget', (data) => {
            this.txtIncome.number(data.income / 100);
            this.txtExpenses.number(data.expenses / 100);
            this.txtDelta.number((data.income - data.expenses) / 100);
            console.log("Budget data:", data);
        });
    }
}
