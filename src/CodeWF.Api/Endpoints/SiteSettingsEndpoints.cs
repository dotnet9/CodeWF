using System.Text.Json;
using System.Text.Json.Nodes;
using CodeWF.Api.Models;
using CodeWF.Api.Options;
using CodeWF.Api.Services;
using Microsoft.Extensions.Options;

namespace CodeWF.Api.Endpoints;

public static class SiteSettingsEndpoints
{
    public static RouteGroupBuilder MapSiteSettingsEndpoints(this RouteGroupBuilder admin)
    {
        admin.MapGet("/site-settings", (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            ContentRepository repository) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            return Results.Ok(repository.GetSiteInfo());
        });

        admin.MapPut("/site-settings", async (
            HttpContext context,
            IOptions<AdminOptions> adminOptions,
            ContentRepository repository,
            IHostEnvironment env,
            SiteSettingsRequest request) =>
        {
            if (!AdminGuard.IsSuperAdmin(context, adminOptions.Value))
            {
                return Results.Problem("Super administrator permission is required.", statusCode: StatusCodes.Status403Forbidden);
            }

            var result = await UpdateSiteSettingsAsync(env.ContentRootPath, request, repository.GetSiteInfo());
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        return admin;
    }

    private static async Task<SiteSettingsResult> UpdateSiteSettingsAsync(string contentRootPath, SiteSettingsRequest request, SiteInfo current)
    {
        var appsettingsPath = Path.Combine(contentRootPath, "appsettings.json");
        if (!File.Exists(appsettingsPath))
        {
            return new SiteSettingsResult(false, $"appsettings.json not found: {appsettingsPath}");
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(await File.ReadAllTextAsync(appsettingsPath));
        }
        catch (Exception ex)
        {
            return new SiteSettingsResult(false, $"Unable to read appsettings.json: {ex.Message}");
        }

        if (root is not JsonObject rootObject)
        {
            return new SiteSettingsResult(false, "appsettings.json is not a JSON object.");
        }

        var site = new JsonObject
        {
            ["AppTitle"] = request.AppTitle ?? current.AppTitle,
            ["Domain"] = request.Domain ?? current.Domain,
            ["Memo"] = request.Memo ?? current.Memo,
            ["Owner"] = request.Owner ?? current.Owner,
            ["OwnerDesc"] = request.OwnerDesc ?? current.OwnerDesc,
            ["Favicon"] = request.Favicon ?? current.Favicon,
            ["LocalAssetsDir"] = request.LocalAssetsDir ?? current.LocalAssetsDir,
            ["AssetBaseUrl"] = request.AssetBaseUrl ?? current.AssetBaseUrl,
            ["RemoteAssetsRepository"] = request.RemoteAssetsRepository ?? current.RemoteAssetsRepository,
            ["StartYear"] = request.StartYear == 0 ? current.StartYear : request.StartYear,
            ["BaiAn"] = request.BaiAn ?? current.BaiAn,
            ["WeChatName"] = request.WeChatName ?? current.WeChatName,
            ["WeChatImg"] = request.WeChatImg ?? current.WeChatImg,
            ["DefaultCulture"] = request.DefaultCulture ?? current.DefaultCulture,
            ["SupportedCultures"] = JsonValue.Create(request.SupportedCultures is { Count: > 0 } ? request.SupportedCultures : current.SupportedCultures.ToList())
        };

        rootObject["Site"] = site;

        try
        {
            await File.WriteAllTextAsync(appsettingsPath, rootObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
            return new SiteSettingsResult(true, "Site settings saved.", new SiteInfo(
                request.AppTitle ?? current.AppTitle,
                request.Domain ?? current.Domain,
                request.Memo ?? current.Memo,
                request.Owner ?? current.Owner,
                request.OwnerDesc ?? current.OwnerDesc,
                request.Favicon ?? current.Favicon,
                request.LocalAssetsDir ?? current.LocalAssetsDir,
                request.AssetBaseUrl ?? current.AssetBaseUrl,
                request.RemoteAssetsRepository ?? current.RemoteAssetsRepository,
                request.StartYear == 0 ? current.StartYear : request.StartYear,
                request.DefaultCulture ?? current.DefaultCulture,
                request.SupportedCultures is { Count: > 0 } ? request.SupportedCultures : current.SupportedCultures.ToList(),
                request.BaiAn ?? current.BaiAn,
                request.WeChatName ?? current.WeChatName,
                request.WeChatImg ?? current.WeChatImg));
        }
        catch (Exception ex)
        {
            return new SiteSettingsResult(false, $"Unable to write appsettings.json: {ex.Message}");
        }
    }
}
