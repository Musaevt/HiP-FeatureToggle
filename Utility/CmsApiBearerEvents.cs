using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace de.uni_paderborn.si_lab.hip.featuretoggles.utility
{
	public class CmsApiBearerEvents: JwtBearerEvents
{
	public override Task AuthenticationFailed(AuthenticationFailedContext context)
	{
		return Task.FromResult(0);
	}

	public override Task TokenValidated(TokenValidatedContext context)
	{
		var identity = context.Ticket.Principal.Identity as ClaimsIdentity;
		if (identity == null)
			return Task.FromException<ArgumentNullException>(new ArgumentNullException());

		// Adding Claims for the current request user.
		identity.AddClaim(new Claim(ClaimTypes.Name, identity.Name));
		return Task.FromResult(0);
	}
}
}
