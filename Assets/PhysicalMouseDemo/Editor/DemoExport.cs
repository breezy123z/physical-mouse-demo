using UnityEditor;

namespace PhysicalMouseDemo.Editor
{
    public static class DemoExport
    {
        [MenuItem("Tools/Physical Mouse Demo/Export Package to Logs")]
        public static void ExportToLogs()
        {
            System.IO.Directory.CreateDirectory("Logs");
            ExportTo("Logs/PhysicalMouseDemo.unitypackage");
            UnityEngine.Debug.Log("Exported Logs/PhysicalMouseDemo.unitypackage");
        }
        [MenuItem("Tools/Physical Mouse Demo/Export Teammate Package")]
        public static void Export()
        {
            string destination = EditorUtility.SaveFilePanel("Export demo", "", "PhysicalMouseDemo", "unitypackage");
            if (!string.IsNullOrEmpty(destination)) ExportTo(destination);
        }
        public static void ExportTo(string destination)
        {
            const string root = "Assets/PhysicalMouseDemo/";
            AssetDatabase.ExportPackage(new[] { root + "Runtime", root + "Hand", root + "Editor/DemoUpgrade.cs", root + "Materials", root + "PhysicalMouseDemo.unity", root + "README.md", root + "Editor/PhysicalMouseDemoBuilder.cs", root + "Editor/DemoChecksMenu.cs", root + "Editor/DemoExport.cs" }, destination, ExportPackageOptions.Recurse);
        }
    }
}
