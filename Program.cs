using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

class Program
{
    private const string ClientsDirectoryName = "Clients";
    private static string _selectedClientPath = string.Empty;
    
    static async Task Main(string[] args)
    {
        Console.CursorVisible = false;
        EnsureClientsDirectoryExists();

        bool running = true;
        while (running)
        {
            if (string.IsNullOrEmpty(_selectedClientPath))
            {
                await ShowClientSelectionMenu();
            }
            else
            {
                await ShowFunctionMenu();
            }
        }
    }
    
    static async Task ShowClientSelectionMenu()
    {
        DisplayHeader();
        AnsiConsole.MarkupLine("[red]请使用上下箭头选择选项，按 Enter 确认[/]");
        var options = new[] { "选择客户端文件夹", "整合JSON文件", "JAVA天空转基岩天空", "WY服务器杀en", "退出" };
        var selectedOption = DisplayMenu(options);
        switch (selectedOption)
        {
            case "选择客户端文件夹":
                _selectedClientPath = await SelectClientFolder();
                if (!string.IsNullOrEmpty(_selectedClientPath))
                {
                    AnsiConsole.MarkupLine("[LightSteelBlue1]已选择客户端文件夹：[/] " + _selectedClientPath);
                    AnsiConsole.MarkupLine("[yellow]按任意键继续...[/]");
                    Console.ReadKey();
                }
                break;
            case "整合JSON文件":
                await MergeJsonFiles();
                break;
            case "JAVA天空转基岩天空":
                await ConvertJavaSkyToBedrockSky();
                break;
            case "WY服务器杀en":
                await RunCacheCleanupTool();
                break;
            case "退出":
                Environment.Exit(0);
                break;
        }
    }
    
static async Task RunCacheCleanupTool()
{
    DisplayHeader();
    AnsiConsole.MarkupLine("[CornflowerBlue]by.Ranye/SHGFZZ 2025.2.12[/]");

    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    string packCachePath = Path.Combine(appDataPath, "MinecraftPE_Netease", "packcache");

    if (!Directory.Exists(packCachePath))
    {
        AnsiConsole.MarkupLine("[red]没有服务器缓存，请进入一次服务器后再使用[/]");
        AnsiConsole.MarkupLine("[yellow]按任意键返回主菜单...[/]");
        Console.ReadKey();
        return;
    }

    AnsiConsole.MarkupLine($"[LightSteelBlue1]当前脚本所在目录: {Directory.GetCurrentDirectory()}[/]");
    AnsiConsole.MarkupLine("[LightSteelBlue1]开始删除缓存文件...[/]");

    string[] filesToDelete = new[]
    {
        "player.animation_controllers.json", "player.entity.json", "player.render_controllers.json",
        "player_firstperson.animation.json", "player.animation.json", "bow.player.json", "golden_sword.player.json",
        "netherite_sword.player.json", "stone_sword.player.json", "diamond_sword.player.json", "wooden_sword.player.json",
        "attachables diamond_axe.json", "3d_items.json", "weapon_anxinglian_axe.animation.json",
        "weapon_battle_axe.animation.json", "weapon_blood_sword.animation.json", "weapon_double_end_sword.animation.json",
        "weapon_crystal_sword.animation.json", "weapon_deadwood_battle_axe.animation.json", "weapon_bone_sword.animation.json",
        "weapon_energy_sword.animation.json", "weapon_gouzhuangti_axe.animation.json", "weapon_holy_axe.animation.json",
        "weapon_lengguangshengjian_sword.json", "weapon_night_sword.animation.json", "weapon_mace_axe.animation.json",
        "weapon_lengguangzhanfu_axe.json", "weapon_lengguangzhandao_sword.json", "weapon_pink_heart.animation_axe.animation.json",
        "weapon_non_attack_sword.animation.json", "weapon_spear_sword.animation.json", "weapon_shengjian_sword.json",
        "weapon_xuanlingchi_sword.json", "weapon_xiedi_sword.animation.json", "weapon_wars_hammer_axe.geo.animation.json",
        "weapon_storm_sword.animation.json", "weapon_storm_hammer_axe.animation.json", "weapon_spikes_mace_axe.animation.json",
        "sword.json", "caidai.particle.json", "ec_hit.particle.json", "ghost.particle.json", "sweep.particle.json", "sound_definitions.json"
    };

    foreach (var file in filesToDelete)
    {
        AnsiConsole.MarkupLine($"[yellow]正在检查文件: {file}[/]");
        var matchingFiles = Directory.GetFiles(packCachePath, file, SearchOption.AllDirectories);
        foreach (var filePath in matchingFiles)
        {
            AnsiConsole.MarkupLine($"[yellow]正在删除文件: {filePath}[/]");
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]删除失败: {ex.Message}[/]");
            }
        }
    }

    AnsiConsole.MarkupLine("[LightSteelBlue1]删除完成 感谢使用[/]");
    AnsiConsole.MarkupLine("[yellow]按任意键返回主菜单...[/]");
    Console.ReadKey();
}

    static async Task ConvertJavaSkyToBedrockSky()
    {
        DisplayHeader();
        AnsiConsole.MarkupLine("[red]请选择天空图片文件[/]");

        string cubemapFilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "CubemapFiles");
        if (!Directory.Exists(cubemapFilesDirectory))
        {
            Directory.CreateDirectory(cubemapFilesDirectory);
        }

        var options = new[] { "从 CubemapFiles 文件夹选择", "手动输入图片路径" };
        var selectedOption = DisplayMenu(options);

        string selectedFilePath;

        if (selectedOption == "从 CubemapFiles 文件夹选择")
        {
            var cubemapFiles = Directory.GetFiles(cubemapFilesDirectory, "*.png").Select(Path.GetFileName).ToArray();
            if (cubemapFiles.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]未找到任何天空图片文件！请将图片放入 CubemapFiles 文件夹。[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }

            var selectedFile = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("请选择要转换的天空图片")
                    .PageSize(10)
                    .AddChoices(cubemapFiles));

            selectedFilePath = Path.Combine(cubemapFilesDirectory, selectedFile);
        }
        else
        {
            selectedFilePath = AnsiConsole.Ask<string>("请输入图片的完整路径：");
            if (!File.Exists(selectedFilePath))
            {
                AnsiConsole.MarkupLine("[red]文件路径无效或文件不存在！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }
        }

        using (var image = Image.Load(selectedFilePath))
        {
            int width = image.Width / 3;  // 每块宽度
            int height = image.Height / 2; // 每块高度

            // 创建以当前时间命名的输出文件夹
            string outputFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "CubemapOutput", outputFolderName);
            string environmentDirectory = Path.Combine(outputDirectory, "environment");
            string overworldCubemapDirectory = Path.Combine(environmentDirectory, "overworld_cubemap");

            Directory.CreateDirectory(overworldCubemapDirectory);

            var cropRegions = new[]
            {
            (x: width, y: height, name: "cubemap_0.png"),
            (x: 2 * width, y: height, name: "cubemap_1.png"),
            (x: 2 * width, y: 0, name: "cubemap_2.png"),
            (x: 0, y: height, name: "cubemap_3.png"),
            (x: width, y: 0, name: "cubemap_4.png"),
            (x: 0, y: 0, name: "cubemap_5.png")
        };

            // 直接裁剪并保存
            foreach (var region in cropRegions)
            {
                try
                {
                    using (var crop = image.Clone(ctx => ctx.Crop(new Rectangle(region.x, region.y, width, height))))
                    {
                        var savePath = Path.Combine(overworldCubemapDirectory, region.name);
                        AnsiConsole.MarkupLine($"[yellow]正在保存到: {savePath}[/]");
                        crop.Save(savePath);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]保存失败: {ex.Message}[/]");
                }
            }

            AnsiConsole.MarkupLine($"[LightSteelBlue1]天空图片已保存到文件夹：{overworldCubemapDirectory}[/]");
        }

        AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
        Console.ReadKey();
    }
    static async Task MergeJsonFiles()
    {
        DisplayHeader();
        AnsiConsole.MarkupLine("[red]请选择 JSON 文件整合方式[/]");

        var options = new[] { "生成 JSON 文件夹并选择文件", "输入 JSON 文件路径", "返回上一级" };
        var selectedOption = DisplayMenu(options);

        if (selectedOption == "返回上一级")
        {
            return;
        }

        string jsonFilePath1 = string.Empty;
        string jsonFilePath2 = string.Empty;

        if (selectedOption == "生成 JSON 文件夹并选择文件")
        {
            string jsonDirectory = Path.Combine(Directory.GetCurrentDirectory(), "JsonFiles");
            if (!Directory.Exists(jsonDirectory))
            {
                Directory.CreateDirectory(jsonDirectory);
            }

            AnsiConsole.MarkupLine($"[LightSteelBlue1]请将 JSON 文件放入以下文件夹：[/] {jsonDirectory}");
            AnsiConsole.MarkupLine("[yellow]按任意键继续...[/]");
            Console.ReadKey();

            var jsonFiles = Directory.GetFiles(jsonDirectory, "*.json").Select(Path.GetFileName).ToArray();
            if (jsonFiles.Length < 2)
            {
                AnsiConsole.MarkupLine("[red]需要至少两个 JSON 文件进行整合！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }

            // 使用多选菜单选择文件
            var selectedFiles = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("请选择要合并的 JSON 文件")
                    .PageSize(10)
                    .Required()
                    .MoreChoicesText("[grey](使用上下箭头选择，空格键选中，回车键确认)[/]")
                    .InstructionsText("[grey](按空格键选择/取消选择，按回车键确认)[/]")
                    .AddChoices(jsonFiles));

            if (selectedFiles.Count < 2)
            {
                AnsiConsole.MarkupLine("[red]需要至少两个 JSON 文件进行整合！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }

            // 获取文件路径
            var filePaths = selectedFiles.Select(file => Path.Combine(jsonDirectory, file)).ToArray();

            // 比较文件行数
            var fileLineCounts = filePaths.Select(file => File.ReadAllLines(file).Length).ToArray();

            // 确保行数多的文件作为第二个文件
            if (fileLineCounts[0] > fileLineCounts[1])
            {
                jsonFilePath1 = filePaths[1];
                jsonFilePath2 = filePaths[0];
            }
            else
            {
                jsonFilePath1 = filePaths[0];
                jsonFilePath2 = filePaths[1];
            }
        }
        else if (selectedOption == "输入 JSON 文件路径")
        {
            jsonFilePath1 = AnsiConsole.Ask<string>("请输入第一个 JSON 文件的完整路径：");
            if (!File.Exists(jsonFilePath1))
            {
                AnsiConsole.MarkupLine("[red]第一个文件路径无效或文件不存在！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }

            jsonFilePath2 = AnsiConsole.Ask<string>("请输入第二个 JSON 文件的完整路径：");
            if (!File.Exists(jsonFilePath2))
            {
                AnsiConsole.MarkupLine("[red]第二个文件路径无效或文件不存在！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
                Console.ReadKey();
                return;
            }

            // 比较文件行数
            var file1LineCount = File.ReadAllLines(jsonFilePath1).Length;
            var file2LineCount = File.ReadAllLines(jsonFilePath2).Length;

            // 确保行数多的文件作为第二个文件
            if (file1LineCount > file2LineCount)
            {
                var temp = jsonFilePath1;
                jsonFilePath1 = jsonFilePath2;
                jsonFilePath2 = temp;
            }
        }

        // 读取并整合 JSON 文件
        var json1 = JObject.Parse(await File.ReadAllTextAsync(jsonFilePath1));
        var json2 = JObject.Parse(await File.ReadAllTextAsync(jsonFilePath2));

        json1.Merge(json2, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        // 输出整合后的 JSON 文件
        string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "JsonOutput");
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string outputFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
        string outputFilePath = Path.Combine(outputDirectory, outputFileName);

        await File.WriteAllTextAsync(outputFilePath, json1.ToString(Formatting.Indented));

// 复制到剪贴板
        await TextCopy.ClipboardService.SetTextAsync(json1.ToString(Formatting.Indented));

        AnsiConsole.MarkupLine($"[LightSteelBlue1]整合后的 JSON 文件已保存到：[/] {outputFilePath}");
        AnsiConsole.MarkupLine("[LightSteelBlue1]内容已复制到剪贴板！[/]");
        AnsiConsole.MarkupLine("[yellow]按任意键返回...[/]");
        Console.ReadKey();
    }

    static async Task ShowFunctionMenu()
    {
        DisplayHeader();
        AnsiConsole.MarkupLine($"[bold yellow]当前选择的客户端文件夹：{_selectedClientPath}[/]");
        AnsiConsole.MarkupLine("[red]请使用上下箭头选择选项，按 Enter 确认[/]");

        var options = new[]
        {
            "修改防砍触发",
            "人物信息显示配置修改",
            "添加光影效果",
            "返回主界面"
        };

        var selectedOption = DisplayMenu(options);

        switch (selectedOption)
        {
            case "修改防砍触发":
                await ModifyAntiCutTrigger();
                break;
            case "人物信息显示配置修改":
                await ModifyPlayerInfoDisplay();
                break;
            case "添加光影效果":
                AnsiConsole.MarkupLine("[bold LightSteelBlue1]已添加光影效果！[/]");
                break;
            case "返回主界面":
                _selectedClientPath = string.Empty;
                return;
        }

        AnsiConsole.MarkupLine("[yellow]按任意键返回功能菜单...[/]");
        Console.ReadKey();
    }

    static async Task ModifyPlayerInfoDisplay()
    {
        while (true)
        {
            DisplayHeader();
            AnsiConsole.MarkupLine("[red]请使用上下箭头选择选项，按 Enter 确认[/]");

            var options = new[]
            {
                "修改颜色",
                "玩家信息显示本体开关",
                "绑定实体坐标开关",
                "显示边框开关",
                "显示血条开关",
                "启用nitro样式的bloom泛光",
                "选择小人模型或玩家头像",
                "设置距离动画触发方式",
                "返回上一级"
            };

            var selectedOption = DisplayMenu(options);

            if (selectedOption == "返回上一级")
            {
                return;
            }

            switch (selectedOption)
            {
                case "修改颜色":
                    await ModifyColor();
                    break;
                case "玩家信息显示本体开关":
                    await TogglePlayerInfoDisplay();
                    break;
                case "绑定实体坐标开关":
                    await ToggleWorldCoordBinding();
                    break;
                case "显示边框开关":
                    await ToggleBorderDisplay();
                    break;
                case "显示血条开关":
                    await ToggleHealthBarDisplay();
                    break;
                case "启用nitro样式的bloom泛光":
                    await ToggleNitroBloom();
                    break;
                case "选择小人模型或玩家头像":
                    await ToggleModelOrAvatar();
                    break;
                case "设置距离动画触发方式":
                    await ConfigureDistanceAnimationTrigger();
                    break;
            }

            AnsiConsole.MarkupLine("[yellow]按任意键返回配置菜单...[/]");
            Console.ReadKey();
        }
    }

    static async Task ModifyColor()
    {
        var openWebsite = AnsiConsole.Ask<string>("是否要打开颜色值网站？(y/n，默认y)：");
        if (string.IsNullOrEmpty(openWebsite) || openWebsite.ToLower() == "y")
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://www.rapidtables.org/zh-CN/web/color/RGB_Color.html") { CreateNoWindow = true });
        }

        AnsiConsole.MarkupLine("[yellow]请输入血条左端颜色 (R,G,B)：[/]");
        var leftColors = new int[3];
        for (int i = 0; i < 3; i++)
        {
            leftColors[i] = AnsiConsole.Ask<int>($"请输入左端第 {i + 1} 个颜色值（0-255）：");
            if (leftColors[i] < 0 || leftColors[i] > 255)
            {
                AnsiConsole.MarkupLine("[red]输入无效，请确保值在 0-255 之间！[/]");
                return;
            }
        }

        AnsiConsole.MarkupLine("[yellow]请输入血条右端颜色 (R,G,B)：[/]");
        var rightColors = new int[3];
        for (int i = 0; i < 3; i++)
        {
            rightColors[i] = AnsiConsole.Ask<int>($"请输入右端第 {i + 1} 个颜色值（0-255）：");
            if (rightColors[i] < 0 || rightColors[i] > 255)
            {
                AnsiConsole.MarkupLine("[red]输入无效，请确保值在 0-255 之间！[/]");
                return;
            }
        }

        var playerEntityFiles = Directory.GetFiles(_selectedClientPath, "player.entity.json", SearchOption.AllDirectories);
        if (playerEntityFiles.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]未找到 player.entity.json 文件！[/]");
            return;
        }

        var fullPath = playerEntityFiles.FirstOrDefault();
        if (string.IsNullOrEmpty(fullPath))
        {
            AnsiConsole.MarkupLine("[red]未找到有效的文件路径！[/]");
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(fullPath);
        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_s_r_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{leftColors[0]};");

        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_s_g_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{leftColors[1]};");

        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_s_b_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{leftColors[2]};");

        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_e_r_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{rightColors[0]};");

        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_e_g_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{rightColors[1]};");

        jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            @"(variable.lonel_health_color_e_b_wjxx *= *)\d+;",
            match => $"{match.Groups[1].Value}{rightColors[2]};");

        await File.WriteAllTextAsync(fullPath, jsonContent);
        AnsiConsole.MarkupLine("[LightSteelBlue1]血条颜色已成功更新！[/]");
    }

    static async Task UpdatePlayerInfoSetting(string variable, string newValue)
    {
        var playerEntityFiles = Directory.GetFiles(_selectedClientPath, "player.entity.json", SearchOption.AllDirectories);

        if (playerEntityFiles.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]未找到 player.entity.json 文件！[/]");
            return;
        }

        var fullPath = playerEntityFiles.FirstOrDefault();
        if (string.IsNullOrEmpty(fullPath))
        {
            AnsiConsole.MarkupLine("[red]未找到有效的文件路径！[/]");
            return;
        }

        var jsonContent = await File.ReadAllTextAsync(fullPath);
        var modifiedContent = System.Text.RegularExpressions.Regex.Replace(jsonContent,
            $@"({variable} *= *)(\d+);",
            match => $"{match.Groups[1].Value}{newValue};");

        if (jsonContent != modifiedContent)
        {
            await File.WriteAllTextAsync(fullPath, modifiedContent);
            AnsiConsole.MarkupLine($"[LightSteelBlue1]{variable} 设置已更新为 {newValue}！[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]没有任何更改！[/]");
        }
    }

    static async Task TogglePlayerInfoDisplay()
    {
        var options = new[] { "开", "关" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "开" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_zjpd_toggle_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ToggleWorldCoordBinding()
    {
        var options = new[] { "开", "关" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "开" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_pos_to_entity_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ToggleBorderDisplay()
    {
        var options = new[] { "开", "关" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "开" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_bg_bk_display_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ToggleHealthBarDisplay()
    {
        var options = new[] { "开", "关" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "开" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_health_display_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ToggleNitroBloom()
    {
        var options = new[] { "开", "关" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "开" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_health_nitro_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ToggleModelOrAvatar()
    {
        var options = new[] { "小人模型", "玩家头像" };
        var selectedOption = DisplayMenu(options);
        var value = selectedOption == "小人模型" ? "1.0" : "0.0";
        await UpdatePlayerInfoSetting("variable.lonel_humanoid_head_wjxx", value);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ConfigureDistanceAnimationTrigger()
    {
        var options = new[] { "0.0 (靠近触发)", "1.0 (瞄准触发)", "2.0 (受击触发)" };
        var selectedOption = DisplayMenu(options);
        await UpdatePlayerInfoSetting("variable.lonel_dt_anim_trigger_type_wjxx", selectedOption.Split(' ')[0]);
        AnsiConsole.MarkupLine($"[yellow]当前状态为: {selectedOption}！[/]");
    }

    static async Task ModifyAntiCutTrigger()
    {
        while (true)
        {
            var playerEntityFiles = Directory.GetFiles(_selectedClientPath, "player.entity.json", SearchOption.AllDirectories);

            if (playerEntityFiles.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]未找到 player.entity.json 文件！[/]");
                return;
            }

            var fileOptions = playerEntityFiles
                .Select(f => $"{Path.GetFileName(f)} ({f})")
                .Concat(new[] { "返回上一级" })
                .ToArray();

            var selectedFile = DisplayMenu(fileOptions);

            if (selectedFile == "返回上一级")
            {
                return;
            }

            var fullPath = playerEntityFiles.FirstOrDefault(f => selectedFile.Contains(f));

            if (string.IsNullOrEmpty(fullPath))
            {
                AnsiConsole.MarkupLine("[red]未找到选择的文件！[/]");
                AnsiConsole.MarkupLine("[yellow]按任意键返回功能菜单...[/]");
                Console.ReadKey();
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(fullPath);
            var formattedJson = JToken.Parse(jsonContent).ToString(Formatting.Indented);
            await File.WriteAllTextAsync(fullPath, formattedJson);

            var defenseLine = FindDefenseLine(formattedJson);

            if (string.IsNullOrEmpty(defenseLine))
            {
                AnsiConsole.MarkupLine("[red]未找到防砍触发代码！[/]");
                return;
            }

            var options = new[]
            {
                "疾跑防砍",
                "潜行防砍",
                "点击防砍",
                "受伤防砍",
                "返回上一级"
            };
            var selectedOption = DisplayMenu(options);

            if (selectedOption == "返回上一级")
            {
                continue;
            }

            string newCondition = "v.ifattack"; // 默认条件
            switch (selectedOption)
            {
                case "疾跑防砍":
                    newCondition = "v.ifsprint";
                    break;
                case "潜行防砍":
                    newCondition = "v.ifsneak";
                    break;
                case "点击防砍":
                    newCondition = "v.ifattack";
                    break;
                case "受伤防砍":
                    newCondition = "v.ifhurted";
                    break;
            }

            // 替换防砍行中的条件
            var newDefenseLine = defenseLine.Replace("v.ifmove", newCondition)
                                             .Replace("v.ifsprint", newCondition)
                                             .Replace("v.ifsneak", newCondition)
                                             .Replace("v.ifattack", newCondition)
                                             .Replace("v.ifhurted", newCondition);

            var newJsonContent = formattedJson.Replace(defenseLine, newDefenseLine);
            await File.WriteAllTextAsync(fullPath, newJsonContent);
            AnsiConsole.MarkupLine($"[yellow]正在处理文件: {fullPath}[/]");
            AnsiConsole.MarkupLine($"[yellow]找到的防砍行: {defenseLine}[/]");
            AnsiConsole.MarkupLine($"[yellow]修改后的行: {newDefenseLine}[/]");
            return;
        }
    }

    static string FindDefenseLine(string jsonContent)
    {
        var lines = jsonContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        return lines.FirstOrDefault(line =>
            line.Contains("v.defense = v.ifmove || v.eat_honey;") ||
            line.Contains("v.defense = v.ifsprint || v.eat_honey;") ||
            line.Contains("v.defense = v.ifsneak || v.eat_honey;") ||
            line.Contains("v.defense = v.ifattack || v.eat_honey;") ||
            line.Contains("v.defense = v.ifhurted || v.eat_honey;"));
    }

    static void DisplayHeader()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("Samse-Tools")
                .Centered()
                .Color(Spectre.Console.Color.Aquamarine3));

        AnsiConsole.MarkupLine("[CornflowerBlue]请将客户端文件夹放在Clients 目录下 Json文件放JsonFiles目录下  要转换的天空图片放在CubemapFiles目录下[/]");
        AnsiConsole.MarkupLine("[SteelBlue1_1]当前程序版本：3.2.0 完全免费[/]");
        AnsiConsole.MarkupLine("[LightSteelBlue]作者:Ranye/SHGFZZ[/]");

        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
    }

    static void EnsureClientsDirectoryExists()
    {
        string clientsDirectory = Path.Combine(Directory.GetCurrentDirectory(), ClientsDirectoryName);
        if (!Directory.Exists(clientsDirectory))
        {
            Directory.CreateDirectory(clientsDirectory);
        }
    }

    static async Task<string> SelectClientFolder()
    {
        string clientsDirectory = Path.Combine(Directory.GetCurrentDirectory(), ClientsDirectoryName);

        if (!Directory.Exists(clientsDirectory))
        {
            Directory.CreateDirectory(clientsDirectory);
        }

        var clientFolders = Directory.GetDirectories(clientsDirectory)
                                     .Select(Path.GetFileName)
                                     .Distinct()
                                     .ToArray();

        if (clientFolders.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]未找到任何客户端文件夹！[/]");
            AnsiConsole.MarkupLine("[yellow]按任意键返回主菜单...[/]");
            Console.ReadKey();
            return string.Empty;
        }

        var prompt = new SelectionPrompt<string>()
            .Title("请选择一个客户端文件夹")
            .PageSize(10)
            .HighlightStyle(new Style(foreground: Spectre.Console.Color.LightSteelBlue1))
            .AddChoices(clientFolders);

        return Path.Combine(clientsDirectory, AnsiConsole.Prompt(prompt));
    }

    static string DisplayMenu(string[] options)
    {
        options = options.Distinct().ToArray();
        var prompt = new SelectionPrompt<string>()
            .PageSize(10)
            .HighlightStyle(new Style(foreground: Spectre.Console.Color.LightSteelBlue1))
            .AddChoices(options);

        return AnsiConsole.Prompt(prompt);
    }
}