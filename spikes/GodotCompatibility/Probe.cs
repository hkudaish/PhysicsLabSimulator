using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VirtualStem.Phase0;

public partial class Probe : Control
{
    private FontFile font = null!;
    private readonly List<string> passed = new();
    private readonly Dictionary<string, object?> observations = new();
    private Label status = null!;
    private LineEdit arabicInput = null!;
    private VBoxContainer body = null!;
    private SubViewport preview = null!;
    private bool arabic = true;

    public override async void _Ready()
    {
        try
        {
            font = ResourceLoader.Load<FontFile>("res://assets/Tajawal-Regular.ttf");
            var fallback = ResourceLoader.Load<FontFile>("res://assets/DejaVuSans.ttf");
            font.AllowSystemFallback = false;
            fallback.AllowSystemFallback = false;
            font.Fallbacks = new Godot.Collections.Array<Font> { fallback };
            var theme = new Theme { DefaultFont = font, DefaultFontSize = 23 };
            Theme = theme;
            BuildUi();
            if (OS.GetCmdlineUserArgs().Contains("--verify"))
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Verify();
                if (OS.GetCmdlineUserArgs().Contains("--capture"))
                {
                    if (DisplayServer.GetName() == "headless")
                        throw new InvalidOperationException("Visual capture requires an actual renderer.");
                    await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    using var previewImage = preview.GetTexture().GetImage();
                    var colors = new HashSet<Color>();
                    for (int x = 0; x < previewImage.GetWidth(); x += 32)
                        for (int y = 0; y < previewImage.GetHeight(); y += 32) colors.Add(previewImage.GetPixel(x, y));
                    observations["preview3DUniqueSampleColors"] = colors.Count;
                    Require(colors.Count > 4, "3D viewport renders lit geometry");
                    string screenshot = OutputPath("godot-windows-ui.png");
                    Error saved = GetViewport().GetTexture().GetImage().SavePng(screenshot);
                    Require(saved == Error.Ok, "rendered viewport screenshot saved");
                }
                SaveReport(true, null);
                GetTree().Quit(0);
            }
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            SaveReport(false, error.ToString());
            GetTree().Quit(1);
        }
    }

    private void BuildUi()
    {
        foreach (Node child in GetChildren()) { RemoveChild(child); child.QueueFree(); }
        LayoutDirection = arabic ? LayoutDirectionEnum.Rtl : LayoutDirectionEnum.Ltr;
        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        foreach (string side in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + side, 28);
        AddChild(margin);
        var scroll = new ScrollContainer();
        margin.AddChild(scroll);
        body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 18);
        scroll.AddChild(body);
        AddLabel(arabic ? "مختبر العلوم الافتراضي — اختبار Godot" : "Virtual STEM — Godot compatibility probe", 32);
        AddLabel(arabic ? "المرحلة صفر: فحص توافق المحرك واللغة والتخزين" : "Phase 0: engine, language and storage diagnostics");
        AddLabel("RTL-01: مختبر العلوم الافتراضي");
        AddLabel("RTL-02: التركيز \u2066C = 0.5 mol/L\u2069");
        var scientific = AddLabel("H₂O    NaCl    I = V/R    −0.009 A    R = 1 kΩ");
        scientific.TextDirection = TextDirection.Ltr;
        scientific.LayoutDirection = LayoutDirectionEnum.Ltr;
        scientific.HorizontalAlignment = HorizontalAlignment.Left;
        AddLabel(arabic ? "تحرير النص العربي — اختبر حركة المؤشر والحذف" : "Arabic editing — inspect caret and deletion");
        arabicInput = new LineEdit { Text = "محلول \u20660.5 mol/L\u2069", CustomMinimumSize = new Vector2(0, 52), TextDirection = TextDirection.Rtl };
        body.AddChild(arabicInput);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        body.AddChild(row);
        AddButton(row, "العربية / English", () => { arabic = !arabic; BuildUi(); });
        foreach (float scale in new[] { 1f, 1.25f, 1.5f })
        {
            float chosen = scale;
            AddButton(row, ((int)(scale * 100)).ToString() + "%", () => GetWindow().ContentScaleFactor = chosen);
        }
        AddButton(row, arabic ? "تشغيل الفحص" : "Run diagnostics", () =>
        {
            try { Verify(); SaveReport(true, null); status.Text = "PASS — " + passed.Count + " checks; human UI review remains pending."; }
            catch (Exception error) { SaveReport(false, error.ToString()); status.Text = error.Message; }
        });
        status = AddLabel(arabic ? "استخدم Tab للتنقل. هذا اختبار توافق وليس تجربة علمية." :
            "Use Tab to navigate. This is a compatibility test, not a science experiment.");
        AddLabel(arabic ? "فحص العرض ثلاثي الأبعاد — أشكال اختبار فقط" : "3D renderer check — diagnostic geometry only");
        var viewportContainer = new SubViewportContainer { Stretch = true, CustomMinimumSize = new Vector2(0, 230) };
        body.AddChild(viewportContainer);
        preview = new SubViewport { Size = new Vector2I(1000, 230), OwnWorld3D = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        viewportContainer.AddChild(preview);
        var camera = new Camera3D { Position = new Vector3(3.6f, 2.2f, 4), Current = true };
        preview.AddChild(camera);
        camera.LookAt(new Vector3(0, 0.5f, 0));
        var light = new DirectionalLight3D { RotationDegrees = new Vector3(-45, -30, 0), LightEnergy = 1.3f };
        preview.AddChild(light);
        preview.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(1, 1, 1) },
            Position = new Vector3(0, 0.6f, 0),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.7f, 0.65f) } });
        preview.AddChild(new MeshInstance3D { Mesh = new PlaneMesh { Size = new Vector2(8, 8) },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.12f, 0.17f, 0.24f) } });
        arabicInput.GrabFocus();
    }

    private Label AddLabel(string text, int size = 23)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Left };
        label.AddThemeFontSizeOverride("font_size", size);
        body.AddChild(label);
        return label;
    }

    private static void AddButton(Container parent, string text, Action action)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(110, 48) };
        button.Pressed += action;
        parent.AddChild(button);
    }

    private void Verify()
    {
        passed.Clear();
        observations.Clear();
        if (OS.GetCmdlineUserArgs().Contains("--expect-export"))
            Require(!OS.HasFeature("editor") && OS.HasFeature("template"), "runs exported runtime without editor");
        Require(typeof(VirtualStem.Science.IScientificSolver<,,>).Assembly.GetName().Name == "Scientific.Core",
            "shared scientific contract assembly loads");
        Require(TextServerManager.GetPrimaryInterface().HasFeature(TextServer.Feature.Shaping), "advanced text shaping available");
        Require(TextServerManager.GetPrimaryInterface().HasFeature(TextServer.Feature.BidiLayout), "bidirectional layout available");
        Require("مختبر العلوم الافتراضي C = 0.5 mol/L H₂O NaCl I = V/R −0.009 A Ω".All(c => font.HasChar(c)),
            "Arabic and scientific glyph coverage with fallback");
        var server = TextServerManager.GetPrimaryInterface();
        using var line = new TextLine { Direction = TextServer.Direction.Rtl };
        Require(line.AddString("مختبر", font, 28, "ar"), "Arabic text accepted by shaper");
        var glyphs = server.ShapedTextGetGlyphs(line.GetRid());
        int[] positions = glyphs.Select(g => (int)g["start"]).ToArray();
        observations["arabicVisualClusterPositions"] = positions;
        Require(positions.Length > 0 && positions.SequenceEqual(positions.OrderByDescending(p => p)), "Arabic visual clusters ordered RTL");
        var nominal = "مختبر".Select(c => server.FontGetGlyphIndex(font.GetRids()[0], 28, c, 0)).ToHashSet();
        Require(glyphs.Any(g => !nominal.Contains((long)g["index"]) && (long)g["index"] != 0),
            "Arabic contextual shaping changes nominal glyphs");
        const string prefix = "التركيز \u2066";
        const string formula = "C = 0.5 mol/L";
        using var mixed = new TextLine { Direction = TextServer.Direction.Rtl };
        Require(mixed.AddString(prefix + formula + "\u2069", font, 28, "ar"), "mixed text accepted by shaper");
        var mixedGlyphs = server.ShapedTextGetGlyphs(mixed.GetRid());
        int[] formulaPositions = mixedGlyphs.Select(g => (int)g["start"]).Where(i => i >= prefix.Length && i < prefix.Length + formula.Length).ToArray();
        observations["formulaVisualClusterPositions"] = formulaPositions;
        Require(formulaPositions.Length >= formula.Length - 2 && formulaPositions.SequenceEqual(formulaPositions.OrderBy(p => p)),
            "isolated scientific formula retains LTR visual order");
        arabicInput.Text = "محلول";
        arabicInput.CaretColumn = 3;
        arabicInput.DeleteText(2, 3);
        Require(arabicInput.Text == "محول", "Arabic logical editing deletes intended character");
        arabicInput.Text = "محلول \u20660.5 mol/L\u2069";
        foreach (float scale in new[] { 1f, 1.25f, 1.5f })
        {
            GetWindow().ContentScaleFactor = scale;
            Require(Math.Abs(GetWindow().ContentScaleFactor - scale) < 0.001, "content scale " + scale);
        }
        GetWindow().ContentScaleFactor = 1;
        observations["nativeSqlite"] = RunSqlite();
        Require(true, "native SQLite probe executes inside Godot C# runtime");
    }

    private NativeSqliteProbe.Result RunSqlite()
    {
        string directory = ProjectSettings.GlobalizePath("user://sqlite-probes");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "probe-" + Guid.NewGuid().ToString("N") + ".db");
        string schema = Godot.FileAccess.GetFileAsString("res://assets/local.sql.txt");
        var result = NativeSqliteProbe.RunFresh(path, schema);
        observations["sqliteDatabase"] = path;
        return result;
    }

    private void Require(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("Check failed: " + name);
        passed.Add(name);
    }

    private static string OutputPath(string filename)
    {
        string? configured = System.Environment.GetEnvironmentVariable("STEM_GODOT_RESULTS");
        string directory = string.IsNullOrEmpty(configured) ? ProjectSettings.GlobalizePath("user://results") : configured;
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, filename);
    }

    private void SaveReport(bool success, string? error)
    {
        var report = new
        {
            success, error, engine = Engine.GetVersionInfo()["string"].AsString(), platform = OS.GetName(),
            displayServer = DisplayServer.GetName(), renderer = RenderingServer.GetCurrentRenderingMethod(),
            exportTemplate = OS.HasFeature("template"),
            passed, observations, humanArabicReview = "pending", schoolHardwareApproved = false,
            scope = "Phase 0 Godot compatibility spike; no playable laboratory or scientific implementation."
        };
        string path = OutputPath("godot-" + OS.GetName().ToLowerInvariant() + "-result.json");
        File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true }));
        GD.Print("Compatibility report: " + path);
    }
}
