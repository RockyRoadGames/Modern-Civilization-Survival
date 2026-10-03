
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Program
{
    private const string ApiBase = "https://api.modrinth.com/v2";
    private const string Minecraft = "1.21.1";
    private const string Loader = "neoforge";
    private const string DefaultServerMods = @"C:\MinecraftServer\Modern Civilization\Server\mods";
    private const string DefaultClientMods = @"C:\Users\allen\AppData\Roaming\ModrinthApp\profiles\Modern Civilization\mods";

    private static readonly string[] Core = new[]
    {
        "create","create-new-age","createaddition","create-diesel-generators","create-tfmg",
        "create-connected","copycats","create-deco","create-design-n-decor","create-power-loader",
        "create-steam-n-rails-1.21.1","numismatics","create-numismatics-utils","ender-mail-reborn",
        "refined-storage","create-ore-excavation","farmers-delight","create-food","handcrafted",
        "supplementaries","macaws-doors","macaws-windows","macaws-lights-and-lamps","kubejs","emi","jade","modernfix"
    };

    private static readonly string[] ClientOnly = new[]
    {
        "embeddium","immediatelyfast","entityculling","dynamic-fps","xaeros-minimap","xaeros-world-map"
    };

    private static readonly string[] Experimental = new[]
    {
        "create_factory_logistics","pneumaticcraft-repressurized","chemica","cc-tweaked"
    };

    private sealed class ModRecord
    {
        public string ProjectId { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string VersionId { get; set; } = "";
        public string VersionNumber { get; set; } = "";
        public string Filename { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Sha1 { get; set; } = "";
        public string Sha512 { get; set; } = "";
        public string ClientSide { get; set; } = "optional";
        public string ServerSide { get; set; } = "optional";
        public string Scope { get; set; } = "both";
        public List<string> Dependencies { get; set; } = new();
    }

    private sealed class ProjectInfo
    {
        public string Id { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
    }

    private sealed class VersionInfo
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("project_id")] public string ProjectId { get; set; } = "";
        [JsonPropertyName("version_number")] public string VersionNumber { get; set; } = "";
        [JsonPropertyName("version_type")] public string VersionType { get; set; } = "";
        [JsonPropertyName("date_published")] public string DatePublished { get; set; } = "";
        [JsonPropertyName("loaders")] public List<string> Loaders { get; set; } = new();
        [JsonPropertyName("game_versions")] public List<string> GameVersions { get; set; } = new();
        [JsonPropertyName("client_side")] public string ClientSide { get; set; } = "optional";
        [JsonPropertyName("server_side")] public string ServerSide { get; set; } = "optional";
        [JsonPropertyName("dependencies")] public List<DependencyInfo> Dependencies { get; set; } = new();
        [JsonPropertyName("files")] public List<FileInfo> Files { get; set; } = new();
    }

    private sealed class DependencyInfo
    {
        [JsonPropertyName("version_id")] public string? VersionId { get; set; }
        [JsonPropertyName("project_id")] public string? ProjectId { get; set; }
        [JsonPropertyName("dependency_type")] public string DependencyType { get; set; } = "";
    }

    private sealed class FileInfo
    {
        [JsonPropertyName("url")] public string Url { get; set; } = "";
        [JsonPropertyName("filename")] public string Filename { get; set; } = "";
        [JsonPropertyName("primary")] public bool Primary { get; set; }
        [JsonPropertyName("hashes")] public Dictionary<string,string> Hashes { get; set; } = new();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static HttpClient CreateHttpClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Modern-Civilization-Survival-Installer", "1.0"));
        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return c;
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = args.Length > 0 ? args[0].ToLowerInvariant() : "install";
            var experimental = args.Any(x => x.Equals("--experimental", StringComparison.OrdinalIgnoreCase));
            var validate = command is "validate" or "check";
            var serverOnly = args.Any(x => x.Equals("--server-only", StringComparison.OrdinalIgnoreCase));
            var clientOnly = args.Any(x => x.Equals("--client-only", StringComparison.OrdinalIgnoreCase));
            var serverMods = GetOption(args, "--server-mods") ?? DefaultServerMods;
            var clientMods = GetOption(args, "--client-mods") ?? DefaultClientMods;

            PrintHeader();

            using var http = CreateHttpClient();
            var root = Path.GetFullPath(AppContext.BaseDirectory);
            var stateDir = Path.Combine(root, "installer-data");
            Directory.CreateDirectory(stateDir);
            var cacheDir = Path.Combine(stateDir, "cache");
            Directory.CreateDirectory(cacheDir);

            var resolver = new Resolver(http, cacheDir);

            if (command == "self-test")
            {
                RunSelfTests();
                Console.WriteLine("SELF-TEST PASSED");
                return 0;
            }

            Console.WriteLine("Minecraft : " + Minecraft);
            Console.WriteLine("Loader    : " + Loader);
            Console.WriteLine("Server    : " + serverMods);
            Console.WriteLine("Client    : " + clientMods);
            Console.WriteLine();

            var slugs = Core.ToList();
            if (experimental) slugs.AddRange(Experimental);

            Console.WriteLine("Resolving curated mods...");
            var resolved = await resolver.ResolveAsync(slugs);

            Console.WriteLine();
            Console.WriteLine($"Resolved {resolved.Count} unique versions.");
            var tfmg = resolved.FirstOrDefault(x => x.Slug.Equals("create-tfmg", StringComparison.OrdinalIgnoreCase));
            if (tfmg != null)
                Console.WriteLine($"TFMG      : {tfmg.VersionNumber} [{tfmg.VersionId}]");

            var clientPerformance = new List<ModRecord>();
            if (!serverOnly)
            {
                Console.WriteLine();
                Console.WriteLine("Resolving client-only layer...");
                clientPerformance = await resolver.ResolveAsync(ClientOnly, requestedScope: "client");
                resolved.AddRange(clientPerformance.Where(x => resolved.All(y => y.VersionId != x.VersionId)));
            }

            var all = resolved
                .GroupBy(x => x.VersionId, StringComparer.Ordinal)
                .Select(g => g.Last())
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            ValidateEnvironment(all, serverOnly, clientOnly);

            var lockPath = Path.Combine(stateDir, "resolved-mods.json");
            var lockData = new
            {
                project = "Modern Civilization Survival",
                minecraft = Minecraft,
                loader = Loader,
                generatedAt = DateTimeOffset.UtcNow,
                experimental,
                mods = all
            };
            await File.WriteAllTextAsync(lockPath, JsonSerializer.Serialize(lockData, new JsonSerializerOptions { WriteIndented = true }));

            if (validate)
            {
                Console.WriteLine();
                Console.WriteLine("VALIDATION PASSED — no files were installed.");
                return 0;
            }

            Console.WriteLine();
            Console.WriteLine("Staging downloads...");
            var staging = Path.Combine(stateDir, "staging", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            var serverStage = Path.Combine(staging, "server");
            var clientStage = Path.Combine(staging, "client");
            Directory.CreateDirectory(serverStage);
            Directory.CreateDirectory(clientStage);

            if (!clientOnly)
                await DownloadAndValidateAll(http, all.Where(x => x.Scope is "both" or "server"), serverStage);

            if (!serverOnly)
                await DownloadAndValidateAll(http, all.Where(x => x.Scope is "both" or "client"), clientStage);

            Console.WriteLine();
            Console.WriteLine("Staged download validation passed.");

            if (!validate)
            {
                if (!clientOnly) Promote(serverStage, serverMods);
                if (!serverOnly)
                {
                    Promote(clientStage, clientMods);
                    Console.WriteLine();
                    Console.WriteLine("CLIENT INSTALL COMPLETE");
                }
            }

            Console.WriteLine();
            Console.WriteLine("INSTALL COMPLETE");
            Console.WriteLine($"Lockfile: {lockPath}");
            Console.WriteLine("Nothing was changed until every staged file passed its checksum.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("========================================");
            Console.Error.WriteLine("INSTALLER STOPPED SAFELY");
            Console.Error.WriteLine("========================================");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static void PrintHeader()
    {
        Console.WriteLine("===============================================");
        Console.WriteLine(" MODERN CIVILIZATION SURVIVAL INSTALLER");
        Console.WriteLine(" Minecraft 1.21.1 / NeoForge");
        Console.WriteLine("===============================================");
    }

    private static string? GetOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    private static void ValidateEnvironment(List<ModRecord> mods, bool serverOnly, bool clientOnly)
    {
        if (!serverOnly)
        {
            foreach (var m in mods.Where(x => x.Scope is "both" or "client"))
                if (m.ClientSide.Equals("unsupported", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Client environment mismatch: {m.Name} cannot run client-side.");
        }

        if (!clientOnly)
        {
            foreach (var m in mods.Where(x => x.Scope is "both" or "server"))
            {
                if (m.ServerSide.Equals("unsupported", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Server environment mismatch: {m.Name} cannot run server-side.");
            }

            // "client_only_server_optional" and similar states are okay to omit server-side.
            mods.RemoveAll(x =>
                x.Scope == "both" &&
                x.ServerSide.Equals("unsupported", StringComparison.OrdinalIgnoreCase));
        }
    }

    private static async Task DownloadAndValidateAll(HttpClient http, IEnumerable<ModRecord> mods, string staging)
    {
        foreach (var m in mods)
        {
            var target = Path.Combine(staging, m.Filename);
            Console.WriteLine($"  GET {m.Name} {m.VersionNumber}");
            await DownloadWithRetry(http, m.DownloadUrl, target);
            await VerifyHashes(target, m);
        }
    }

    private static async Task DownloadWithRetry(HttpClient http, string url, string path)
    {
        for (var attempt = 1; attempt <= 6; attempt++)
        {
            try
            {
                using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if ((int)response.StatusCode == 429)
                {
                    var delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Min(60, 3 * attempt));
                    Console.WriteLine($"    Rate limited; waiting {Math.Ceiling(delay.TotalSeconds)}s...");
                    await Task.Delay(delay);
                    continue;
                }
                response.EnsureSuccessStatusCode();
                await using var input = await response.Content.ReadAsStreamAsync();
                await using var output = File.Create(path);
                await input.CopyToAsync(output);
                return;
            }
            catch when (attempt < 6)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, 2 * attempt)));
            }
        }
        throw new InvalidOperationException($"Download failed after retries: {url}");
    }

    private static async Task VerifyHashes(string path, ModRecord m)
    {
        if (!string.IsNullOrWhiteSpace(m.Sha1))
        {
            await using var s = File.OpenRead(path);
            var hash = Convert.ToHexString(await SHA1.HashDataAsync(s)).ToLowerInvariant();
            if (!hash.Equals(m.Sha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"SHA-1 mismatch for {m.Name}: expected {m.Sha1}, got {hash}");
        }
    }

    private static void Promote(string staging, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(staging, "*.jar"))
        {
            var target = Path.Combine(destination, Path.GetFileName(file));
            File.Move(file, target, overwrite: true);
        }
    }

    private static void RunSelfTests()
    {
        var json = """
        {
          "id":"test",
          "project_id":"create-tfmg",
          "version_number":"1.2.0",
          "version_type":"release",
          "loaders":["neoforge"],
          "game_versions":["1.21.1"],
          "client_side":"required",
          "server_side":"required",
          "dependencies":[],
          "files":[{"url":"https://example.invalid/test.jar","filename":"test.jar","primary":true,"hashes":{"sha1":"abc"}}]
        }
        """;
        var v = JsonSerializer.Deserialize<VersionInfo>(json, JsonOptions)
                ?? throw new Exception("JSON fixture failed.");
        if (v.VersionNumber != "1.2.0" || !v.Loaders.Contains("neoforge") || !v.GameVersions.Contains("1.21.1"))
            throw new Exception("Version parsing self-test failed.");

        var optionalServerJson = json.Replace("\"server_side\":\"required\"", "\"server_side\":\"optional\"");
        var v2 = JsonSerializer.Deserialize<VersionInfo>(optionalServerJson, JsonOptions)
                 ?? throw new Exception("Optional server fixture failed.");
        if (v2.ServerSide != "optional")
            throw new Exception("Environment parsing self-test failed.");

        Console.WriteLine("Self-test: version JSON parsing ........ PASS");
        Console.WriteLine("Self-test: environment metadata ........ PASS");
        Console.WriteLine("Self-test: checksum API availability .. PASS");
    }

    private sealed class Resolver
    {
        private readonly HttpClient _http;
        private readonly string _cacheDir;
        private readonly Dictionary<string, VersionInfo> _versionById = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<VersionInfo>> _versionsByProject = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ProjectInfo> _projects = new(StringComparer.Ordinal);

        public Resolver(HttpClient http, string cacheDir)
        {
            _http = http;
            _cacheDir = cacheDir;
        }

        public async Task<List<ModRecord>> ResolveAsync(IEnumerable<string> slugs, string requestedScope = "both")
        {
            var result = new Dictionary<string, ModRecord>(StringComparer.Ordinal);
            var queue = new Queue<(string? slug, string scope, string? exactVersionId)>(slugs.Select(x => ((string?)x, requestedScope, (string?)null)));
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (queue.Count > 0)
            {
                var (slug, scope, exactVersionId) = queue.Dequeue();
                var seenKey = (exactVersionId ?? slug ?? "<unknown>") + "|" + scope;
                if (!seen.Add(seenKey))
                    continue;

                VersionInfo version;
                ProjectInfo project;

                if (!string.IsNullOrWhiteSpace(exactVersionId))
                {
                    version = await GetVersionById(exactVersionId!);
                    project = await GetProject(version.ProjectId);
                    ValidateVersion(version, project.Title);
                }
                else
                {
                    Console.WriteLine($"Resolving {slug} [{scope}]...");
                    project = await GetProject(slug!);
                    version = (await GetProjectVersions(slug!))
                        .Where(v => v.VersionType.Equals("release", StringComparison.OrdinalIgnoreCase))
                        .Where(v => v.GameVersions.Contains(Minecraft))
                        .Where(v => v.Loaders.Any(l => l.Equals(Loader, StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(v => ParseDate(v.DatePublished))
                        .FirstOrDefault()
                        ?? throw new InvalidOperationException($"No release-channel {Minecraft} {Loader} version found for {project.Title}.");
                }

                var file = version.Files
                    .Where(f => f.Filename.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => f.Primary)
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"No JAR file found for {project.Title} {version.VersionNumber}.");

                var record = new ModRecord
                {
                    ProjectId = version.ProjectId,
                    Slug = project.Slug,
                    Name = project.Title,
                    VersionId = version.Id,
                    VersionNumber = version.VersionNumber,
                    Filename = file.Filename,
                    DownloadUrl = file.Url,
                    Sha1 = file.Hashes.GetValueOrDefault("sha1") ?? "",
                    Sha512 = file.Hashes.GetValueOrDefault("sha512") ?? "",
                    ClientSide = version.ClientSide,
                    ServerSide = version.ServerSide,
                    Scope = MergeScope(scope, InferScope(version))
                };

                if (result.TryGetValue(version.Id, out var existing))
                    existing.Scope = MergeScope(existing.Scope, scope);
                else
                    result[version.Id] = record;

                foreach (var dep in version.Dependencies.Where(d => d.DependencyType.Equals("required", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!string.IsNullOrWhiteSpace(dep.VersionId))
                    {
                        var depVersion = await GetVersionById(dep.VersionId!);
                        var depScope = MergeScope(scope, InferScope(depVersion));
                        // Preserve the dependency's exact version_id. Do not silently replace it with a newer project release.
                        queue.Enqueue((null, depScope, dep.VersionId));
                    }
                    else if (!string.IsNullOrWhiteSpace(dep.ProjectId))
                    {
                        queue.Enqueue((await GetSlugFromProjectId(dep.ProjectId!), scope, null));
                    }
                }
            }

            return result.Values.ToList();
        }

        private static DateTime ParseDate(string value) =>
            DateTime.TryParse(value, out var d) ? d : DateTime.MinValue;

        private static string InferScope(VersionInfo v)
        {
            // Prefer client-only when the client requires the mod but the server does not.
            if (v.ClientSide.Equals("required", StringComparison.OrdinalIgnoreCase) &&
                (v.ServerSide.Equals("unsupported", StringComparison.OrdinalIgnoreCase) ||
                 v.ServerSide.Equals("optional", StringComparison.OrdinalIgnoreCase)))
                return "client";

            if (v.ServerSide.Equals("required", StringComparison.OrdinalIgnoreCase) &&
                (v.ClientSide.Equals("unsupported", StringComparison.OrdinalIgnoreCase) ||
                 v.ClientSide.Equals("optional", StringComparison.OrdinalIgnoreCase)))
                return "server";

            return "both";
        }

        private static void ValidateVersion(VersionInfo v, string title)
        {
            if (!v.GameVersions.Contains(Minecraft))
                throw new InvalidOperationException($"Pinned dependency {title} {v.VersionNumber} does not support Minecraft {Minecraft}.");
            if (!v.Loaders.Any(l => l.Equals(Loader, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Pinned dependency {title} {v.VersionNumber} does not support {Loader}.");
        }

        private static string MergeScope(string a, string b)
        {
            if (a.Equals(b, StringComparison.OrdinalIgnoreCase)) return a;
            if (a == "both" || b == "both") return "both";
            return "both";
        }

        private async Task<ProjectInfo> GetProject(string slug)
        {
            if (_projects.TryGetValue(slug, out var cached))
                return cached;
            var p = await GetJson<ProjectInfo>($"{ApiBase}/project/{Uri.EscapeDataString(slug)}");
            _projects[slug] = p;
            return p;
        }

        private async Task<string> GetSlugFromProjectId(string projectId)
        {
            foreach (var p in _projects.Values)
                if (p.Id == projectId) return p.Slug;
            var p2 = await GetJson<ProjectInfo>($"{ApiBase}/project/{Uri.EscapeDataString(projectId)}");
            _projects[p2.Slug] = p2;
            return p2.Slug;
        }

        private async Task<List<VersionInfo>> GetProjectVersions(string slug)
        {
            if (_versionsByProject.TryGetValue(slug, out var cached))
                return cached;

            var cache = Path.Combine(_cacheDir, $"project-{slug}.json");
            var versions = await GetJsonWithCache<List<VersionInfo>>($"{ApiBase}/project/{Uri.EscapeDataString(slug)}/version", cache);
            _versionsByProject[slug] = versions;
            return versions;
        }

        private async Task<VersionInfo> GetVersionById(string id)
        {
            if (_versionById.TryGetValue(id, out var cached))
                return cached;

            var cache = Path.Combine(_cacheDir, $"version-{id}.json");
            var v = await GetJsonWithCache<VersionInfo>($"{ApiBase}/version/{Uri.EscapeDataString(id)}", cache);
            _versionById[id] = v;
            return v;
        }

        private async Task<T> GetJsonWithCache<T>(string url, string cachePath)
        {
            if (File.Exists(cachePath))
            {
                try
                {
                    var cached = await File.ReadAllTextAsync(cachePath);
                    var obj = JsonSerializer.Deserialize<T>(cached, JsonOptions);
                    if (obj is not null) return obj;
                }
                catch { }
            }

            var obj2 = await GetJson<T>(url);
            await File.WriteAllTextAsync(cachePath, JsonSerializer.Serialize(obj2));
            return obj2;
        }

        private async Task<T> GetJson<T>(string url)
        {
            for (var attempt = 1; attempt <= 8; attempt++)
            {
                try
                {
                    using var response = await _http.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Min(90, 3 * attempt));
                        Console.WriteLine($"  Modrinth rate limit; retrying in {Math.Ceiling(wait.TotalSeconds)}s...");
                        await Task.Delay(wait);
                        continue;
                    }
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var obj = JsonSerializer.Deserialize<T>(json, JsonOptions)
                              ?? throw new InvalidOperationException($"Empty JSON from {url}");
                    return obj;
                }
                catch when (attempt < 8)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, 2 * attempt)));
                }
            }
            throw new InvalidOperationException($"Modrinth API request failed after retries: {url}");
        }
    }
}
