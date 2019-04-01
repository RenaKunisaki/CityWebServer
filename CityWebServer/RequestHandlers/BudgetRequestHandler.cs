using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.Extensibility.Responses;
using CityWebServer.Models;
using ColossalFramework;

namespace CityWebServer.RequestHandlers {
	public class BudgetRequestHandler: RequestHandlerBase {
		public BudgetRequestHandler(IWebServer server)
			: base(server, new Guid("87205a0d-1b53-47bd-91fa-9cddf0a3bd9e"),
				"Budget", "Rychard", 100, "/Budget") {
		}

		public override IResponseFormatter Handle(HttpListenerRequest request) {
			// TODO: Expand upon this to expose substantially more information.
			var economyManager = Singleton<EconomyManager>.instance;
			BudgetInfo budget = new BudgetInfo();

			economyManager.GetIncomeAndExpenses(new ItemClass(),
				out budget.income, out budget.expenses);

			List<TaxRate> taxRates = new List<TaxRate>();
			taxRates.Add(new TaxRate {
				GroupName = "Residential Low",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Residential,
				ItemClass.SubService.ResidentialLow, ItemClass.Level.None)
			});
			taxRates.Add(new TaxRate {
				GroupName = "Residential High",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Residential,
				ItemClass.SubService.ResidentialHigh, ItemClass.Level.None)
			});
			taxRates.Add(new TaxRate {
				GroupName = "Commercial Low",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Commercial,
				ItemClass.SubService.CommercialLow, ItemClass.Level.None)
			});
			taxRates.Add(new TaxRate {
				GroupName = "Commercial High",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Commercial,
				ItemClass.SubService.CommercialHigh, ItemClass.Level.None)
			});
			taxRates.Add(new TaxRate {
				GroupName = "Industrial",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Industrial,
				ItemClass.SubService.IndustrialGeneric, ItemClass.Level.None)
			});
			taxRates.Add(new TaxRate {
				GroupName = "Office",
				Rate = economyManager.GetTaxRate(ItemClass.Service.Office,
				ItemClass.SubService.OfficeGeneric, ItemClass.Level.None)
			});
			budget.taxRates = taxRates.ToArray();

			return JsonResponse(budget);
		}
	}
}