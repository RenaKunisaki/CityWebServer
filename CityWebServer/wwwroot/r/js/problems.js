const problemTypes = [
    "Crime",
	"Death",
	"DepotNotConnected",
	"DirtyWater",
	"Electricity",
	"ElectricityNotConnected",
	"Emptying",
	"EmptyingFinished",
	"Evacuating",
	//"FatalProblem", //this is a total
	"Fire",
	"Flood",
	"Garbage",
	"Heating",
	"HeatingNotConnected",
	"LandfillFull",
	"LandValueLow",
	"LineNotConnected",
	//"MajorProblem",
	"NoCustomers",
	"NoEducatedWorkers",
	"NoFood",
	"NoFuel",
	"NoGoods",
	"NoInputProducts",
	"Noise",
	"NoMainGate",
	"NoNaturalResources",
	"NoPark",
	"NoPlaceForGoods",
	"NoResources",
	"NotInIndustryArea",
	"NoWorkers",
	"PathNotConnected",
	"Pollution",
	"ResourceNotSelected",
	"RoadNotConnected",
	"Sewage",
	"Snow",
	"StructureDamaged",
	"StructureVisited",
	"StructureVisitedService",
	"TaxesTooHigh",
	"TooFewServices",
	"TooLong",
	"TrackNotConnected",
	"TurnedOff",
	"Water",
	"WaterNotConnected",
	"WrongAreaType",
];

class Problems {
    constructor(app) {
        this.app = app;
        this._makeIcons();
    }

    _makeIcons() {
        this.icons = {};
        const elems = [];
        for(const type of problemTypes) {
            let icon = $(`<div class="problem-icon" id="problem-icon-${type}">`)
            .append(
                $(`<div class="problem-label" id="problem-label-${type}">`)
            ).attr('title', type)
            .css('background-image', `url(/r/img/problems/${type}.png)`)
            .on('click', event => this._onIconClick(type));
            this.icons[type] = icon;
            elems.push(icon);
        }
        $('#problem-list').append(elems);
    }

    run() {
        this.app.registerMessageHandler("ProblemCounts", (data) => this._update(data));
        console.log("Problems online.")
    }

    _update(ProblemCounts) {
        for(const type of problemTypes) {
            let count = ProblemCounts[type];
            if(count == undefined) count = 0;
            $(`#problem-label-${type}`).number(count);
            $(`#problem-icon-${type}`).toggleClass('zero', count == 0);
        }
    }

    async _onIconClick(problem) {
        let data = await this.app.query({
            "Building": {
                "action": "getByProblem",
                "problem": problem,
            }
        }, "ProblemBuildings");
        console.log("Buildings with problem", problem, data);

        const list = $('<table class="list">');
        for(const building of data) {
            list.append($('<tr class="building">').append(
                $('<td class="name">').text(building.name),
                $('<td class="type">').text(building.category),
                $('<td class="buttons">').append(
                    $('<button class="camera-goto">').text("Go There")
                    .on('click', e => {
                        //add a small delay so that we can get back
                        //into the game window so it won't scroll away
                        setTimeout(() => {
                            this.app.query({"Camera":{
                                "action":"lookAt",
                                "building":building.ID,
                            }})
                        }, 500);
                    }),
                    $('<button class="building-destroy">').text("Demolish")
                    .on('click', e => {
                        if(confirm(`Demolish ${building.name}?`)) {
                            this.app.query({"Building":{
                                "action": "destroy",
                                "id": building.ID,
                            }})
                        }
                    })
                )
            ));
        }

        new Popup({
            title: "Buildings with problem: "+problem,
            body: list,
        }).show();
    }
}
