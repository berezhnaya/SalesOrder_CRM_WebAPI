using Autofac;
using Autofac.Integration.WebApi;
using IG.CRM.API.CRM.Repositories;
using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.CRM.Services.IG.Marketing;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Reflection;
using System.Web.Http;

namespace IG.CRM.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var config = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new System.Net.Http.Formatting.RequestHeaderMapping("Accept",
                              "text/html",
                              StringComparison.InvariantCultureIgnoreCase,
                              true,
                              "application/json"));

            //Dependency injection
            var builder = new ContainerBuilder();

            //Web service CRM
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().SingleInstance()
                .UsingConstructor(typeof(CrmConnection))
                .WithParameter(new PositionalParameter(0, new CrmConnection("CrmServiceConnection")));

            //Repositories
            builder.RegisterType<BaseRepository>().As<IBaseRepository>().SingleInstance();
            builder.RegisterType<SalesOrderRepository>().As<ISalesOrderRepository>().SingleInstance();
            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>().SingleInstance();
            builder.RegisterType<FunctionRepository>().As<IFunctionRepository>().SingleInstance();
            builder.RegisterType<AccountRepository>().As<IAccountRepository>().SingleInstance();
            builder.RegisterType<AccountDeliveryAddressRepository>().As<IAccountDeliveryAddressRepository>().SingleInstance();
            builder.RegisterType<CurrencyRepository>().As<ICurrencyRepository>().SingleInstance();
            builder.RegisterType<ItemUomRepository>().As<IItemUomRepository>().SingleInstance();
            builder.RegisterType<ProductRepository>().As<IProductRepository>().SingleInstance();
            builder.RegisterType<SalesOrderDetailRepository>().As<ISalesOrderDetailRepository>().SingleInstance();
            builder.RegisterType<UomRepository>().As<IUomRepository>().SingleInstance();
            builder.RegisterType<FirmRepository>().As<IFirmRepository>().SingleInstance();
            builder.RegisterType<SalesOrderDetailRepository>().As<ISalesOrderDetailRepository>().SingleInstance();
            builder.RegisterType<WorkDayRepository>().As<IWorkDayRepository>().SingleInstance();
            builder.RegisterType<SystemUserRepository>().As<ISystemUserRepository>().SingleInstance();                     
            builder.RegisterType<ProductPriceRepository>().As<IProductPriceRepository>().SingleInstance();
            builder.RegisterType<MinfincourceRepository>().As<IMinfincourceRepository>().SingleInstance();
            builder.RegisterType<ConfigurationRepository>().As<IConfigurationRepository>().SingleInstance();
            builder.RegisterType<CurrencyRepository>().As<ICurrencyRepository>().SingleInstance();
            builder.RegisterType<ExecuteActionRepository>().As<IExecuteActionRepository>().SingleInstance();
            builder.RegisterType<AttributeRepository>().As<IAttributeRepository>().SingleInstance();
            builder.RegisterType<MarketingStockRepository>().As<IMarketingStockRepository>().SingleInstance();
            builder.RegisterType<MarketingStockProductRepository>().As<IMarketingStockProductRepository>().SingleInstance();
            builder.RegisterType<AnnotationRepository>().As<IAnnotationRepository>().SingleInstance();

            //Services
            builder.RegisterType<SalesOrderService>().As<ISalesOrderService>().SingleInstance();
            builder.RegisterType<AccountService>().As<IAccountService>().SingleInstance();
            builder.RegisterType<ItemUomService>().As<IItemUomService>().SingleInstance();
            builder.RegisterType<ProductService>().As<IProductService>().SingleInstance();
            builder.RegisterType<CompanyService>().As<ICompanyService>().SingleInstance();
            builder.RegisterType<MarketingService>().As<IMarketingService>().SingleInstance();

            //Controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}