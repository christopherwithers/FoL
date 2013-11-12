using System.Web.Mvc;
using System.Web.Security;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Mvc.FilterBindingSyntax;
using eMotive.FoL.Common.ActionFilters;

[assembly: WebActivator.PreApplicationStartMethod(typeof(eMotive.FoL.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(eMotive.FoL.App_Start.NinjectWebCommon), "RegisterRoles")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(eMotive.FoL.App_Start.NinjectWebCommon), "Stop")]

namespace eMotive.FoL.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        public static void RegisterRoles()
        {
            bootstrapper.Kernel.Inject(Roles.Provider);
        } 
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new eMotiveModule());
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="_kernel">The kernel.</param>
        private static void RegisterServices(IKernel _kernel)
        {
            //_kernel.Bind<eMotiveRoleProvider>().ToMethod(ctx => Roles.Provider); 
          //  _kernel.Inject(Roles.Provider);
           // _kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();

            _kernel.BindFilter<LogErrorsAttribute>(FilterScope.Last, 0).When((context, ad) => !string.IsNullOrEmpty(ad.ActionName) && ad.ControllerDescriptor.ControllerName.ToLower() != "navigation");
            _kernel.BindFilter<CriticalErrorAttribute>(FilterScope.Last, 1).When((context, ad) => !string.IsNullOrEmpty(ad.ActionName) && ad.ControllerDescriptor.ControllerName.ToLower() != "navigation" && ad.ActionName.ToLower() != "error");

         //   _kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);
          //  _kernel.Inject(Roles.Provider);

        }        
    }
}
