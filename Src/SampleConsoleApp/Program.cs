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
        services.AddHttpClient();

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

    if (!string.IsNullOrWhiteSpace(auth.Items?.AccessToken))
    {
        token = auth.Items.AccessToken;
        expiresIn = auth.Items.ExpiresIn;
        requestDateTime = DateTime.Now;
        Console.WriteLine(auth.Items.AccessToken);
    }

    var zamupayRoutes = await _zamupayService.GetZamupayRoutesAsync(10);

    if (!zamupayRoutes.Succeeded)
    {
        Console.WriteLine(zamupayRoutes!.Errors!.FirstOrDefault()!.Status);
    }
    else
    {
        if (zamupayRoutes.Succeeded)
        {
            routes = zamupayRoutes.Items!.Routes.ToArray();

            Console.WriteLine(routes?.FirstOrDefault()?.RouteIntergration);

            var zamupayRouteChannelTypes = await _zamupayService.GetZamupayRouteChannelTypesAsync(Guid.Parse(routes![0].Id), 10);

            if (!zamupayRouteChannelTypes.Succeeded)
            {
                Console.WriteLine(zamupayRouteChannelTypes!.Errors!.FirstOrDefault()!.Status);
            }
            else
            {
                if (zamupayRouteChannelTypes.Succeeded)
                {
                    channelTypes = zamupayRouteChannelTypes.Items?.ToArray();

                    Console.WriteLine(channelTypes?.FirstOrDefault()?.ChannelDescription);
                }
            }

            var categories = new List<string>();

            foreach (var item in zamupayRoutes.Items!.Routes)
            {
                var zamupayRoute = await _zamupayService.GetZamupayRouteAsync(Guid.Parse(item.Id), 10);

                if (!zamupayRoute.Succeeded)
                {
                    Console.WriteLine(zamupayRoute!.Errors!.FirstOrDefault()!.Status);
                }
                else
                {
                    if (zamupayRoute.Succeeded)
                    {
                        Console.WriteLine(zamupayRoute.Items?.CategoryDescription);
                    }
                }

                categories.Add(item.Category);
            }

            if (categories.Count > 0)
            {
                var zamupayRouteByCategories = await _zamupayService.GetZamupayRoutesByCategoryAsync(categories[0], 10);

                if (!zamupayRouteByCategories.Succeeded)
                {
                    Console.WriteLine(zamupayRouteByCategories!.Errors!.FirstOrDefault()!.Status);
                }
                else
                {
                    if (zamupayRouteByCategories.Succeeded)
                    {
                        Console.WriteLine(zamupayRouteByCategories.Items?.FirstOrDefault()?.RouteIntergration);
                    }
                }
            }

            var billPayment = TransactionExtensions.CreateBillPayment("254700000000", "KE-GOTV", 2, "KE", "254700000000",
                10, "Joseph Githithu", "KES", false, 5, "KPLC Prepaid Bill Payment", "https://en1ezbmu2vsd.x.pipedream.net",
                Guid.NewGuid().ToString(), "174379", false);

            var createBillPaymentResult = await _zamupayService.PostBillPaymentAsync(billPayment);

            if (!createBillPaymentResult.Succeeded)
            {
                Console.WriteLine(createBillPaymentResult.Errors!.FirstOrDefault()!.Status);
            }
            else
            {
                if (createBillPaymentResult.Succeeded)
                {
                    var transactionQueryModelDTO = new TransactionQueryModelDTO
                    {
                        Id = createBillPaymentResult.Items!.Message!.OriginatorConversationId,
                        IdType = PaymentIdTypeEnum.OriginatorConversationId
                    };

                    var billPaymentQueryResult = await _zamupayService.GetBillPaymentAsync(transactionQueryModelDTO);

                    if (!billPaymentQueryResult.Succeeded)
                    {
                        Console.WriteLine(billPaymentQueryResult.Errors!.FirstOrDefault()!.Status);
                    }
                    else
                    {
                        if (billPaymentQueryResult.Succeeded)
                        {
                            Console.WriteLine(billPaymentQueryResult.Items?.SystemConversationId);
                        }
                    }

                    Console.WriteLine(createBillPaymentResult.Items?.Message?.DueAmount);
                }
            }
        }
    }
}