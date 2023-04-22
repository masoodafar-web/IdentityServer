// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerHost.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser User { get; set; }
    public virtual ApplicationRole Role { get; set; }
}