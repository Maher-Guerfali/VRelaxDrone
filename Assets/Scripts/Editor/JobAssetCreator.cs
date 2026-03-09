#if UNITY_EDITOR
using DroneDispatcher.Data;
using UnityEditor;
using UnityEngine;

namespace DroneDispatcher.Editor
{
public static class JobAssetCreator
{
    [MenuItem("Tools/Drone Dispatcher/Create Default Jobs")]
    static void CreateDefaultJobs()
    {
        EnsureFolders();

        CreateJob("MedicalSupply", "job_01", "Medical Supply", "Urgent medical delivery", "Hospital", "Clinic");
        CreateJob("PackageDelivery", "job_02", "Package Delivery", "Standard package delivery", "Warehouse", "Residential");
        CreateJob("FoodDelivery", "job_03", "Food Delivery", "Lunch order delivery", "Restaurant", "Office");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[JobAssetCreator] Created 3 job definitions in Resources/Jobs/");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder("Assets/Resources/Jobs"))
            AssetDatabase.CreateFolder("Assets/Resources", "Jobs");
    }

    static void CreateJob(string assetName, string id, string name, string desc, string pickup, string dropoff)
    {
        string path = $"Assets/Resources/Jobs/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<JobDefinition>(path) != null)
        {
            Debug.Log($"[JobAssetCreator] {assetName} already exists, skipping.");
            return;
        }

        var def = ScriptableObject.CreateInstance<JobDefinition>();
        def.jobId = id;
        def.jobName = name;
        def.description = desc;
        def.pickupLocationName = pickup;
        def.dropoffLocationName = dropoff;

        AssetDatabase.CreateAsset(def, path);
    }
}
}
#endif
