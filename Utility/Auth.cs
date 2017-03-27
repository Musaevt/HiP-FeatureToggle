using System;
using System.Security.Claims;
using System.Security.Principal;

namespace de.uni_paderborn.si_lab.hip.featuretoggles.utility
{
	public static class Auth
	{
		// Adds function to get User Id from Context.User.Identity
		public static string GetUserIdentity(this IIdentity identity)
		{
			var claimsIdentity = identity as ClaimsIdentity;
			if (claimsIdentity == null)
			{
				throw new InvalidOperationException("identity not found");
			}

			return claimsIdentity.FindFirst(ClaimTypes.Name).Value;
		}
	}
}
