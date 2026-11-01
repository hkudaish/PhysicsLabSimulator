using System;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace VirtualStem.Phase0
{
    // Diagnostic screen only. Never used as an experiment or an automatic RTL approval.
    public sealed class CompatibilityProbeView : MonoBehaviour
    {
        private UIDocument document;
        private PanelSettings panel;
        private FontAsset fontAsset;
        private FontAsset fallbackFontAsset;
        private Label status;
        private bool arabic = true;
        private bool sqlitePassed;

        private void Start()
        {
            panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.scaleMode = PanelScaleMode.ConstantPixelSize;
            panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("Phase0/ProbeTheme");
            if (panel.themeStyleSheet == null) throw new InvalidOperationException("Probe theme is missing.");
            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panel;
            var font = Resources.Load<Font>("Phase0/Tajawal-Regular");
            if (font == null) throw new InvalidOperationException("Bundled Tajawal font is missing.");
            fontAsset = FontAsset.CreateFontAsset(font);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            var fallbackFont = Resources.Load<Font>("Phase0/DejaVuSans");
            if (fallbackFont == null) throw new InvalidOperationException("Bundled scientific-symbol fallback font is missing.");
            fallbackFontAsset = FontAsset.CreateFontAsset(fallbackFont);
            fallbackFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.fallbackFontAssetTable.Add(fallbackFontAsset);
            Draw();
        }

        private void Draw()
        {
            var root = document.rootVisualElement;
            root.Clear();
            root.languageDirection = arabic ? LanguageDirection.RTL : LanguageDirection.LTR;
            root.style.unityTextGenerator = TextGeneratorType.Advanced;
            root.style.unityFontDefinition = FontDefinition.FromSDFFont(fontAsset);
            root.style.backgroundColor = new Color(0.06f, 0.09f, 0.14f);
            root.style.color = Color.white;
            root.style.paddingTop = root.style.paddingBottom = 24;
            root.style.paddingLeft = root.style.paddingRight = 24;
            root.style.fontSize = 22;
            var content = new ScrollView();
            content.style.flexGrow = 1;
            root.Add(content);
            AddLabel(content, arabic ? "مختبر العلوم الافتراضي — فحص التوافق" : "Virtual STEM — compatibility probe");
            AddLabel(content, arabic ? "هذه شاشة فحص. صحة عرض النص تحتاج مراجعة بشرية." :
                "Diagnostic only. Text rendering requires human review.");
            AddLabel(content, "RTL-01: مختبر العلوم الافتراضي");
            AddLabel(content, "RTL-02: التركيز \u2066C = 0.5 mol/L\u2069");
            AddLabel(content, "RTL-03: H₂O   NaCl   I = V/R   −0.009 A", true);
            var field = new TextField(arabic ? "RTL-04: نص قابل للتحرير" : "RTL-04: editable text")
            {
                value = "محلول 0.5 mol/L",
                name = "arabic-edit-field"
            };
            content.Add(field);
            AddLabel(content, arabic ? "قيمة القياس" : "Measurement");
            var measurement = new TextField("I (A)") { value = "0.009" };
            measurement.languageDirection = LanguageDirection.LTR;
            content.Add(measurement);
            var actions = new VisualElement();
            actions.style.flexDirection = arabic ? FlexDirection.RowReverse : FlexDirection.Row;
            actions.style.flexWrap = Wrap.Wrap;
            content.Add(actions);
            AddButton(actions, "العربية / English", () => { arabic = !arabic; Draw(); });
            foreach (float scale in new[] { 1f, 1.25f, 1.5f })
            {
                float selected = scale;
                AddButton(actions, (scale * 100).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "%",
                    () => panel.scale = selected);
            }
            AddButton(actions, arabic ? "تشغيل فحص SQLite" : "Run SQLite probe", RunSqlite);
            AddButton(actions, arabic ? "حفظ سجل البيئة" : "Save environment record", ExportEnvironment);
            status = AddLabel(content, arabic ? "لم يتم تنفيذ فحص Unity بعد. استخدم Tab للتنقل." :
                "Unity probe not run. Use Tab to navigate.");
            if (sqlitePassed) status.text = "SQLite probe passed in this process; Arabic review is still pending.";
        }

        private static Label AddLabel(VisualElement root, string text, bool ltr = false)
        {
            var label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 16;
            if (ltr) label.languageDirection = LanguageDirection.LTR;
            root.Add(label);
            return label;
        }

        private static void AddButton(VisualElement root, string label, Action action)
        {
            var button = new Button(action) { text = label, focusable = true };
            button.style.marginRight = 12;
            button.style.marginBottom = 12;
            root.Add(button);
        }

        private void RunSqlite()
        {
            try
            {
                var schema = Resources.Load<TextAsset>("Phase0/local.sql");
                if (schema == null) throw new InvalidOperationException("Probe schema resource is missing.");
                var folder = Path.Combine(Application.persistentDataPath, "Phase0Compatibility");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, "probe-" + Guid.NewGuid().ToString("N") + ".db");
                var result = NativeSqliteProbe.RunFresh(path, schema.text);
#if ENABLE_IL2CPP
                result.unityIl2cppVerified = true;
#endif
                File.WriteAllText(Path.Combine(folder, "sqlite-result-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json"),
                    JsonUtility.ToJson(result, true));
                sqlitePassed = true;
                status.text = "SQLite probe passed: " + result.provider + " " + result.sqliteVersion +
                    ". Human RTL review and abrupt-exit player recovery still required.";
            }
            catch (Exception error)
            {
                sqlitePassed = false;
                status.text = "SQLite probe failed: " + error.GetType().Name + ": " + error.Message;
                Debug.LogException(error);
            }
        }

        private void ExportEnvironment()
        {
            var folder = Path.Combine(Application.persistentDataPath, "Phase0Compatibility");
            Directory.CreateDirectory(folder);
            var result = new EnvironmentRecord
            {
                unityVersion = Application.unityVersion,
                operatingSystem = SystemInfo.operatingSystem,
                processor = SystemInfo.processorType,
                memoryMB = SystemInfo.systemMemorySize,
                graphics = SystemInfo.graphicsDeviceName,
                sqlitePassedInThisSession = sqlitePassed,
                arabicVisualReview = "pending human review",
                schoolHardwareApproved = false
            };
            string path = Path.Combine(folder, "environment.json");
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
            status.text = "Environment record: " + path + ". No hardware or Arabic approval inferred.";
        }

        [Serializable]
        private sealed class EnvironmentRecord
        {
            public string unityVersion;
            public string operatingSystem;
            public string processor;
            public int memoryMB;
            public string graphics;
            public bool sqlitePassedInThisSession;
            public string arabicVisualReview;
            public bool schoolHardwareApproved;
        }

        private void OnDestroy()
        {
            if (document != null) document.panelSettings = null;
            if (fontAsset != null) Destroy(fontAsset);
            if (fallbackFontAsset != null) Destroy(fallbackFontAsset);
            if (panel != null) Destroy(panel);
        }
    }
}
