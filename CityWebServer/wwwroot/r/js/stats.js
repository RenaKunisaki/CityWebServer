class Stats {
    constructor(app) {
        this.app = app;
    }

    run() {
        this.app.registerMessageHandler("District", (data) => this._onDistrict(data));
    }

    _onDistrict(data) {
        //compute some helpful stats for the UI
        data.CemeteryUsage = (data.DeadAmount / data.DeadCapacity) * 100;
        data.Education1Usage = (data.Education1Need / data.Education1Capacity) * 100;
        data.Education2Usage = (data.Education2Need / data.Education2Capacity) * 100;
        data.Education3Usage = (data.Education3Need / data.Education3Capacity) * 100;
        data.ElectricityUsage = (data.ElectricityConsumption / data.ElectricityCapacity) * 100;
        data.JailUsage = (data.CriminalAmount / data.CriminalCapacity) * 100;
        data.LandfillUsage = (data.GarbageAmount / data.GarbageCapacity) * 100;
        data.HospitalUsage = (data.SickCount / data.HealCapacity) * 100;
        data.HeatingUsage = (data.HeatingConsumption / data.HeatingCapacity) * 100;
        data.SewageUsage = (data.SewageAccumulation / data.SewageCapacity) * 100;
        data.ShelterUsage = (data.ShelterCitizenNumber / data.ShelterCitizenCapacity) * 100;
        data.WaterUsage = (data.WaterConsumption / data.WaterCapacity) * 100;
        data.WaterStorageUsage = (data.WaterStorageAmount / data.WaterStorageCapacity) * 100;
        data.WorkplaceUsage = (data.WorkerCount / data.WorkplaceCount) * 100;
    }
}
