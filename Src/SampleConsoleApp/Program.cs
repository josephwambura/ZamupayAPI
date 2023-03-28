using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZamuPay.API.DTOs;
using ZamuPay.API.Extensions;
using ZamuPay.API.Interfaces;
using ZamuPay.API.Services;
using ZamuPay.API.Utils;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.SetBasePath(Directory.GetCurrentDirectory());
        configHost.AddJsonFile("appsettings.Development.json", optional: true);
        configHost.AddJsonFile("appsettings.json", optional: false);
        configHost.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        // needed to load configuration from appsettings.json
        services.AddOptions();

        // needed to store rate limit counters and ip rules
        services.AddDistributedMemoryCache();

        var webServiceConfiguration = hostContext.Configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);

        services.Configure<BaseUrlConfiguration>(webServiceConfiguration);

        services.AddSingleton<IZamupayService, ZamupayService>();
    })
    .Build();

await TestZamupayAPIAsync(host.Services);

await host.RunAsync();

static async Task TestZamupayAPIAsync(IServiceProvider services)
{
    RouteDTO[]? routes;
    ChannelTypeDTO[]? channelTypes;
    string? token;
    int expiresIn;
    DateTime? requestDateTime;

    using IServiceScope serviceScope = services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var _zamupayService = provider.GetRequiredService<IZamupayService>();

    var auth = await _zamupayService.GetZamupayIdentityServerAuthTokenAsync();

    if (!string.IsNullOrWhiteSpace(auth.Item1?.AccessToken))
    {
        token = auth.Item1.AccessToken;
        expiresIn = auth.Item1.ExpiresIn;
        requestDateTime = DateTime.Now;
        Console.WriteLine(auth.Item1.AccessToken);
    }

    var zamupayRoutes = await _zamupayService.GetZamupayRoutesAsync(10);

    if (zamupayRoutes.Item2 != null)
    {
        Console.WriteLine(zamupayRoutes.Item2.ToString());
    }
    else
    {
        if (zamupayRoutes.Item1 != null)
        {
            routes = zamupayRoutes.Item1.Routes.ToArray();

            Console.WriteLine(routes?.FirstOrDefault()?.RouteIntergration);

            var zamupayRouteChannelTypes = await _zamupayService.GetZamupayRouteChannelTypesAsync(Guid.Parse(zamupayRoutes.Item1.Routes[0].Id), 10);

            if (zamupayRouteChannelTypes.Item2 != null)
            {
                Console.WriteLine(zamupayRouteChannelTypes.Item2.ToString());
            }
            else
            {
                if (zamupayRouteChannelTypes.Item1 != null)
                {
                    channelTypes = zamupayRouteChannelTypes.Item1.ToArray();

                    Console.WriteLine(channelTypes?.FirstOrDefault()?.ChannelDescription);
                }
            }

            var categories = new List<string>();

            foreach (var item in zamupayRoutes.Item1.Routes)
            {
                var zamupayRoute = await _zamupayService.GetZamupayRouteAsync(Guid.Parse(item.Id), 10);

                if (zamupayRoute.Item2 != null)
                {
                    Console.WriteLine(zamupayRoute.Item2.ToString());
                }
                else
                {
                    if (zamupayRoute.Item1 != null)
                    {
                        Console.WriteLine(zamupayRoute.Item1.CategoryDescription);
                    }
                }

                categories.Add(item.Category);
            }

            if (categories.Count > 0)
            {
                var zamupayRouteByCategories = await _zamupayService.GetZamupayRoutesByCategoryAsync(categories[0], 10);

                if (zamupayRouteByCategories.Item2 != null)
                {
                    Console.WriteLine(zamupayRouteByCategories.Item2.ToString());
                }
                else
                {
                    if (zamupayRouteByCategories.Item1 != null)
                    {
                        Console.WriteLine(zamupayRouteByCategories.Item1?.FirstOrDefault()?.RouteIntergration);
                    }
                }
            }

            var billPayment = TransactionExtensions.CreateBillPayment("254700000000", "KE-GOTV", 2, "KE", "254700000000",
                10, "Joseph Githithu", "KES", false, 5, "KPLC Prepaid Bill Payment", "https://en1ezbmu2vsd.x.pipedream.net",
                Guid.NewGuid().ToString(), "174379", false);

            var createBillPaymentResult = await _zamupayService.PostBillPaymentAsync(billPayment);

            if (createBillPaymentResult.Item2 != null)
            {
                Console.WriteLine(createBillPaymentResult.Item2.ToString());
            }
            else
            {
                if (createBillPaymentResult.Item1 != null)
                {
                    var transactionQueryModelDTO = new TransactionQueryModelDTO
                    {
                        Id = createBillPaymentResult.Item1?.Message?.OriginatorConversationId,
                        IdType = PaymentIdTypeEnum.OriginatorConversationId
                    };

                    var billPaymentQueryResult = await _zamupayService.GetBillPaymentAsync(transactionQueryModelDTO);

                    if (billPaymentQueryResult.Item2 != null)
                    {
                        Console.WriteLine(billPaymentQueryResult.Item2.ToString());
                    }
                    else
                    {
                        if (billPaymentQueryResult.Item1 != null)
                        {
                            Console.WriteLine(billPaymentQueryResult.Item1?.SystemConversationId);
                        }
                    }

                    Console.WriteLine(createBillPaymentResult.Item1?.Message?.DueAmount);
                }
            }
        }
    }
}