using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestfulRouting
{
	public class ResourcesMapper
	{
		private RouteCollection _routeCollection;
		private string _basePath;
		private RouteConfiguration _configuration;

		public ResourcesMapper(RouteCollection routeCollection) : this(routeCollection, RouteConfiguration.Default())
		{
		}

		public ResourcesMapper(RouteCollection routeCollection, RouteConfiguration configuration)
		{
			_configuration = configuration;
			_basePath = VirtualPathUtility.RemoveTrailingSlash(configuration.PathPrefix);
			if (!string.IsNullOrEmpty(_basePath))
			{
				_basePath = _basePath + "/";
			}
			_routeCollection = routeCollection;
		}

		public void Map<TModel>()
		{
			var resource = Inflector.Net.Inflector.Pluralize(typeof (TModel).Name).ToLower();
			Map(resource);
		}

		public void Map(string resource)
		{
			var controller = string.IsNullOrEmpty(_configuration.Controller) ? resource : _configuration.Controller;

			// GET /blogs => Index
			_routeCollection.Add(new Route(
									_basePath + resource,
									new RouteValueDictionary(new { action = "Index", controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("GET") }),
			                     	new MvcRouteHandler()));

			// POST /blogs => Create
			_routeCollection.Add(new Route(
									_basePath + resource,
									new RouteValueDictionary(new { action = "create", controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("POST") }),
			                     	new MvcRouteHandler()));

			// GET /blogs/new => New
			_routeCollection.Add(new Route(
									_basePath + resource + "/new",
									new RouteValueDictionary(new { action = "new", controller }),
			                     	new MvcRouteHandler()));

			// GET /blogs/show => Show
			// GET /blogs/edit => Edit
			// GET /blogs/delete => Delete
			_routeCollection.Add(new Route(
									_basePath + resource + "/{id}/{action}",
									new RouteValueDictionary(new { action = "show", controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("GET"), action = "show|edit|delete" }),
			                     	new MvcRouteHandler()));

			// PUT /blogs/1 => Update
			_routeCollection.Add(new Route(
									_basePath + resource + "/{id}",
									new RouteValueDictionary(new { action = "update", controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("PUT") }),
			                     	new MvcRouteHandler()));

			// DELETE /blogs/1 => Delete
			_routeCollection.Add(new Route(
									_basePath + resource + "/{id}",
									new RouteValueDictionary(new { action = "destroy", controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("DELETE") }),
			                     	new MvcRouteHandler()));

			// HTML forms can only GET or POST... this allows us to place an override value in the form to simulate a PUT or DELETE
			// The route handler then translates the overrides to the appropriate action
			// POST /blogs/1 => Update or Delete
			_routeCollection.Add(new Route(
									_basePath + resource + "/{id}",
									new RouteValueDictionary(new { controller }),
			                     	new RouteValueDictionary(new { httpMethod = new HttpMethodConstraint("POST") }),
			                     	new PostOverrideRouteHandler()));
		}

		public void Map<TModel>(Action<RestfulRouteMapper> map)
		{
			var resource = Inflector.Net.Inflector.Pluralize(typeof(TModel).Name).ToLower();
			Map(resource, map);
		}

		public void Map(string resource, Action<RestfulRouteMapper> map)
		{
			Map(resource);
			var singular = Inflector.Net.Inflector.Singularize(resource).ToLower();
			var configuration = RouteConfiguration.Default();
			configuration.PathPrefix = _basePath + resource + "/{" + singular + "Id}";
			var mapper = new RestfulRouteMapper(_routeCollection, configuration);
			map(mapper);
		}
	}
}