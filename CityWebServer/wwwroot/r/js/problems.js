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

const problemFlagTypes = [
    "Abandoned",
    "BurnedDown",
    //"CapacityFull",
    "Collapsed",
    "RateReduced",
];

const rebuildFailMessages = {
    "WrongType": "Not a city building",
    "NotDestroyed": "Building is not destroyed",
    "NotReady": "Building is not ready to rebuild",
    "NoMoney": "Not enough money to rebuild",
};

//Which building flags should be shown in UI
const flagsToShow = ["Abandoned", "BurnedDown", "CapacityStep1",
    "CapacityStep2", "Collapsed", "Demolishing", "Downgrading",
    "Evacuating", "Flooded", "HighDensity", "Historical",
    "RateReduced", "RoadAccessFailed", "Upgrading"];

class Problems {
    constructor(app) {
        this.app = app;
        this._makeIcons();
    }

    _makeIcons() {
        this.icons = {};
        const elems = [];
        for(const type of problemTypes.concat(problemFlagTypes)) {
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
        for(const type of problemTypes.concat(problemFlagTypes)) {
            let count = ProblemCounts[type];
            if(count == undefined) count = 0;
            $(`#problem-label-${type}`).number(count);
            $(`#problem-icon-${type}`).toggleClass('zero', count == 0);
        }
    }

    async _doGoto(building) {
        //add a small delay so that we can get back
        //into the game window so it won't scroll away
        setTimeout(() => {
            this.app.query({"Camera":{
                "action":"lookAt",
                "building":building.ID,
            }})
        }, 500);
    }

    async _doDemolish(building) {
        if(confirm(`Demolish ${building.name}?`)) {
            this.app.query({"Building":{
                "action": "destroy",
                "id": building.ID,
            }})
        }
    }

    async _doRebuild(building) {
        let reply = await this.app.query({"Building":{
            "action": "rebuild",
            "id": building.ID,
        }}, "rebuild");
        if(reply.rebuild.toLowerCase() != "true") {
            let msg = rebuildFailMessages[reply.status] || reply.status;
            alert(msg);
        }
    }

    buildingFlagsToImages(flags) {
        const result = $('<span class="flags">');
        for(const flag of flagsToShow) {
            const val = this.app.data.Flags.Building[flag];
            if(flags & val) {
                result.append($(`<img src="/r/img/BuildingFlags/${flag}.png" title="${flag}">`));
            }
        }
        return result;
    }

    async _onIconClick(problem) {
        let data = await this.app.query({
            "Building": {
                "action": "getByProblem",
                "problem": problem,
            }
        }, "ProblemBuildings");
        console.log("Buildings with problem", problem, data);

        const list = $('<table class="list">').append(
            $('<th>').text("Name"),
            $('<th>').text("Category"),
            $('<th>').text("District"),
            $('<th>').text("Flags"),
            $('<th>').text("Actions"),
        );
        for(const building of data) {
            //Get district name
            //It might not be in the list if it's new
            let tdDistrict = $('<td class="district">');
            if(building.district != 0) {
                let district = this.app.data.District[building.district];
                if(district == undefined) {
                    this.app.query({"District":{
                        "action":"get","id":building.district
                    }}).then(result => {
                        tdDistrict.text(result.Name);
                    })
                }
                else {
                    tdDistrict.text(district.Name);
                }
            }
            else tdDistrict.text("-");

            list.append($('<tr class="building">').append(
                $('<td class="name">').text(building.name),
                $('<td class="type">').text(building.category),
                tdDistrict,
                $('<td class="flags">').append(
                    this.buildingFlagsToImages(building.flags)),
                $('<td class="buttons">').append(
                    $('<button class="camera-goto">').text("Show")
                    .on('click', e => this._doGoto(building)),
                    $('<button class="building-destroy">').text("Demolish")
                    .on('click', e => this._doDemolish(building)),
                    $('<button class="building-rebuild">').text("Rebuild")
                    .on('click', e => this._doRebuild(building))
                )
            ));
        }

        new Popup({
            title: "Buildings with problem: "+problem,
            classes: "problems",
            body: list,
        }).show();
    }
}
