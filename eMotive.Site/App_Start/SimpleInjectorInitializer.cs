using System.Configuration;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;

[assembly: WebActivator.PostApplicationStartMethod(typeof(eMotive.Site.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace eMotive.Site.App_Start
{
    using System.Reflection;
    using System.Web.Mvc;

    using SimpleInjector;
    using SimpleInjector.Integration.Web.Mvc;
    
    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as MVC3 Dependency Resolver.</summary>
        public static void Initialize()
        {
            // Did you know the container can diagnose your configuration? Go to: http://bit.ly/YE8OJj.
            var container = new Container();
            
            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            
            container.RegisterMvcAttributeFilterProvider();
       
            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
     
        private static void InitializeContainer(Container container)
        {
            container.RegisterPerWebRequest<IUserManager, UserManager>();
            container.RegisterPerWebRequest<ISiteSearchManager, SiteSearchManager>();
            container.RegisterPerWebRequest<IUserRepository>(() => new MySqlUserRepository(ConfigurationManager.ConnectionStrings["live"].ConnectionString ?? string.Empty));
            container.RegisterPerWebRequest<IRoleRepository>(() => new MySqlRoleRepository(ConfigurationManager.ConnectionStrings["live"].ConnectionString ?? string.Empty));

            container.RegisterSingle<ISearchManager>(() => new SearchManager(ConfigurationManager.AppSettings["LuceneIndex"]));
            container.RegisterSingle<ISearchDocumentManager>(() => new SearchDocumentManager(ConfigurationManager.AppSettings["LuceneIndex"]));
        }
    }
}