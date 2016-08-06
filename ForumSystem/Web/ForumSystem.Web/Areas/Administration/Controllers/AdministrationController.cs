﻿namespace ForumSystem.Web.Areas.Administration.Controllers
{
    using System.Web.Mvc;

    using ForumSystem.Common.Constants;
    using ForumSystem.Data.Models;
    using ForumSystem.Data.UnitOfWork;
    using ForumSystem.Web.Controllers;

    [Authorize(Roles = RoleConstants.Administrator)]
    public class AdministrationController : BaseController
    {
        protected AdministrationController(IForumSystemData data)
            : base(data)
        {
        }

        protected AdministrationController(IForumSystemData data, ApplicationUser userProfile)
            : base(data, userProfile)
        {
        }
    }
}