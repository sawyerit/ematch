using eMatch.Data.Mongo;
using eMatch.Engine.Data;
using eMatch.Engine.Services;
using eMatch.Engine.Services.Interfaces;

[assembly: WebActivator.PreApplicationStartMethod(typeof(eMatch.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(eMatch.Web.App_Start.NinjectWebCommon), "Stop")]

namespace eMatch.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using System.Web.Http;
    using eMatch.Web.Infrastructure;

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
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver(kernel);

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IOfferService>().To<OfferService>();
            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<IUserRepository>().To<MongoUserRepo>();
            kernel.Bind<IOfferRepository>().To<MongoOfferRepo>();
            kernel.Bind<IMatchService>().To<MatchService>();
            kernel.Bind<ISessionService>().To<SessionService>();
        }
    }
}
