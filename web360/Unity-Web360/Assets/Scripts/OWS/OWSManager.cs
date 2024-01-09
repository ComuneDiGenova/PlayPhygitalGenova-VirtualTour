using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OWS
{
    public class OWSManager
    {
        public const string geoportalEndpoint = "";

        //lista features
        //
        //esempio
        //



        static async Task<Root> GetFeatures(string featuresCode)
        {
            string url = geoportalEndpoint + $"?service=WFS&version=1.0.0&request=GetFeature&typeName={featuresCode}&outputFormat=json&srsName=CRS:84";
            Debug.Log(url);
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    await Task.Yield();
                }
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(uwr.error);
                    return null;
                }
                else
                {
                    string json = uwr.downloadHandler.text;
                    Debug.Log(json);
                    try
                    {
                        Root obj = JsonUtility.FromJson<Root>(json);
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                        return null;
                    }
                }
            }
        }

        public static async Task<List<NTPoi>> GetOWSFeatures(FeatureType featureType, Action<List<NTPoi>> callBack)
        {
            Debug.Log("NTPOI: " + featureType.ToString());
            string featureName = "SITGEO:";
            switch (featureType)
            {
                case FeatureType.Bagni:
                    featureName += "V_BAGNI";
                    break;
                case FeatureType.BikeSharing:
                    featureName += "V_MOB_PARKS_BIKESHARING";
                    break;
                case FeatureType.Autobus:
                    featureName += "V_MOB_FERMATE_AMT";
                    break;
                default:
                    break;
            }
            var root = await GetFeatures(featureName);
            if (root == null) return null;
            List<NTPoi> ntlist = new List<NTPoi>();
            foreach (var f in root.features)
            {
                var npoi = new NTPoi(f, featureType);
                ntlist.Add(npoi);
            }
            if (ntlist.Count > 0)
            {
                callBack?.Invoke(ntlist);
                return ntlist;
            }
            else
                return null;
        }
    }
}