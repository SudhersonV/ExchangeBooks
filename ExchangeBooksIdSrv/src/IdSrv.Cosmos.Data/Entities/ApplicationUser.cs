using IdSrv.Infra;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;

namespace IdSrv.Cosmos.Data.Entities
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public override Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string ProviderName {get; set; } = Constants.ExchangeBooks;
		public string ProviderSubjectId { get; set; }
		public bool HasAcceptedEula { get; set; }
	}
}
