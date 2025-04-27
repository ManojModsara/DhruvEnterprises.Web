[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DhruvEnterprises.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(DhruvEnterprises.App_Start.NinjectWebCommon), "Stop")]
namespace DhruvEnterprises.App_Start
{
    using System;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using DhruvEnterprises.Data;
    using DhruvEnterprises.Repo;
    using DhruvEnterprises.Service;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;
    using System.Web.Mvc;
    using System.Collections.Generic;
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
                DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
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
            kernel.Bind(typeof(ILoginService)).To(typeof(LoginService));
            kernel.Bind(typeof(IActivityLogService)).To(typeof(ActivityLogService));
            kernel.Bind(typeof(IRoleService)).To(typeof(RoleService));
            kernel.Bind(typeof(IUserService)).To(typeof(UserService));
            kernel.Bind(typeof(IMenuService)).To(typeof(MenuService));
            kernel.Bind(typeof(IWalletService)).To(typeof(WalletService));
            kernel.Bind(typeof(IPackageService)).To(typeof(PackageService));
            kernel.Bind(typeof(IApiService)).To(typeof(ApiService));
            kernel.Bind(typeof(IRequestResponseService)).To(typeof(RequestResponseService));
            kernel.Bind(typeof(ITagValueService)).To(typeof(TagValueService));
            kernel.Bind(typeof(IRechargeReportService)).To(typeof(RechargeReportService));
            kernel.Bind(typeof(IApiWalletService)).To(typeof(ApiWalletService));
            kernel.Bind(typeof(IOperatorSwitchService)).To(typeof(OperatorSwitchService));
            kernel.Bind(typeof(ICommonSwitchService)).To(typeof(CommonSwitchService));
            kernel.Bind(typeof(IRechargeService)).To(typeof(RechargeService));
            kernel.Bind(typeof(IBankAccountService)).To(typeof(BankAccountService));
            kernel.Bind(typeof(IUserKycService)).To(typeof(UserKycService));
            kernel.Bind(typeof(ISmsApiService)).To(typeof(SmsApiService));
            kernel.Bind(typeof(IEmailApiService)).To(typeof(EmailApiService));
            kernel.Bind(typeof(IOpSerialService)).To(typeof(OpSerialService));
            kernel.Bind(typeof(IBInvoiceService)).To(typeof(BInvoiceService));
            kernel.Bind(typeof(IDMTReportService)).To(typeof(DMTReportService));
            kernel.Bind(typeof(IProductService)).To(typeof(ProductService));

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