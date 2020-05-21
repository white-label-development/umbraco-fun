using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;

namespace AngUm.Composers
{
	// https://our.umbraco.com/Documentation/Reference/Events/Application-Startup
	public class ApplicationComposer : ComponentComposer<ApplicationComponent>, IUserComposer
	{
		public override void Compose(Composition composition)
		{
			// ApplicationStarting event in V7: add IContentFinders, register custom services and more here

			base.Compose(composition);
		}
	}

	public class ApplicationComponent : IComponent
	{
		public void Initialize()
		{
            // ApplicationStarted event in V7: add your events here
            RouteTable.Routes.MapRoute(
              "app",
              "app/{*.}",
              new { controller = "AngularApp", action = "AngularAppView" });
        }

		public void Terminate()
		{ }
	}
}