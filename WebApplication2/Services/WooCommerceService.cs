using Microsoft.Extensions.Options;
using System.Globalization;
using WebApplication2.Models;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace WebApplication2.Services;


public interface IWooCommerceService
{
    Task<IList<Product>> GetProductsAsync();
}

public class WooCommerceService : IWooCommerceService
{
    // https://github.com/XiaoFaye/WooCommerce.NET
    private readonly RestAPI _api;

    public WooCommerceService(IOptions<WooCommerceSettings> options)
    {
        string url = options.Value?.ApiUrl ?? throw new ArgumentException(nameof(options.Value.ApiUrl));
        string key = options.Value?.ApiKey ?? throw new ArgumentException(nameof(options.Value.ApiKey));
        string secret = options.Value?.ApiSecret ?? throw new ArgumentException(nameof(options.Value.ApiSecret));
        _api = new RestAPI(url, key, secret);
    }

    public async Task<IList<Product>> GetProductsAsync()
    {
        WCObject wc = new WCObject(_api);

        //Use below code for WCObject only if you would like to have different CultureInfo
        // WCObject wc = new WCObject(_api, CultureInfo.GetCultureInfo("en-US"));

        //Get all products
        return await wc.Product.GetAll();
    }
}
