[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DhruvEnterprises.API.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(DhruvEnterprises.API.App_Start.NinjectWebCommon), "Stop")]

namespace DhruvEnterprises.API.App_Start
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using DhruvEnterprises.Data;
    using DhruvEnterprises.Repo;
    using DhruvEnterprises.Service;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;


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
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<DhruvEnterprisesDBEntities>().ToSelf().InRequestScope();
            kernel.Bind(typeof(IRepository<>)).To(typeof(Repository<>));
            kernel.Bind(typeof(IUserService)).To(typeof(UserService));
            kernel.Bind(typeof(IApiService)).To(typeof(ApiService));
            kernel.Bind(typeof(IRequestResponseService)).To(typeof(RequestResponseService));
            kernel.Bind(typeof(IRechargeService)).To(typeof(RechargeService));
            kernel.Bind(typeof(IWalletService)).To(typeof(WalletService));
            kernel.Bind(typeof(IDMTReportService)).To(typeof(DMTReportService));
            kernel.Bind(typeof(IPackageService)).To(typeof(PackageService));

        }


        public class NinjectDependencyResolver : IDependencyResolver
        {
            private readonly IKernel kernel;

            public NinjectDependencyResolver(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public object GetService(Type serviceType)
            {
                return this.kernel.TryGet(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                try
                {
                    return this.kernel.GetAll(serviceType);
                }
                catch (Exception)
                {
                    return new List<object>();
                }
            }
        }
    }
}