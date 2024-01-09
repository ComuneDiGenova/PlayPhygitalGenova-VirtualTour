using System;
using System.Collections.Generic;


namespace OWS
{

    public enum FeatureType { Bagni = 0, BikeSharing = 1, Autobus = 2 }

    /**************** JSON **************************/

    [Serializable]
    public class Root
    {
        public string type;
        public List<Feature> features;
        public int totalFeatures;
        public int numberMatched;
        public int numberReturned;
        public string timeStamp;
        public Crs crs;
    }
    [Serializable]
    public class Crs
    {
        public string type;
        public Properties properties;
    }
    [Serializable]
    public class Feature
    {
        public string type;
        public string id;
        public Geometry geometry;
        public string geometry_name;
        public Properties properties;
    }
    [Serializable]
    public class Geometry
    {
        public string type;
        public List<double> coordinates;
    }

    //SPECIALE PER DESERIALIZZARE TUTTI 3 TIPI
    [Serializable]
    public class Properties
    {
        public string NOTE;
        public string AUTOPUL;
        public string ACCESSIBILE;
        public string DATI;
        public string OPERATIVO;
        public int ID;
        public string FONTE_DATI;
        public string name;
        public string NOME;
        public string LINEA;
        public string CODICE_FERMATA;
        public string NOME_FERMATA;
        public string DETTAGLI;
    }


    /*************************************************************/

}
//NTPOI

[Serializable]
public class NTPoi
{
    public int id;
    public OWS.FeatureType type;
    public GeoCoordinate.Coordinate coordinate;
    public NTPoiDetails details;

    public NTPoi(OWS.Feature owsFeature, OWS.FeatureType owsType)
    {
        id = owsFeature.properties.ID;
        type = owsType;
        coordinate = new GeoCoordinate.Coordinate(owsFeature.geometry.coordinates[1], owsFeature.geometry.coordinates[0]);
        details = new NTPoiDetails(owsFeature.properties);
    }
}

[Serializable]
public class NTPoiDetails
{

    public int id;
    public bool accessibile;


    public NTPoiDetails(OWS.Properties properties)
    {
        id = properties.ID;
        accessibile = properties.ACCESSIBILE == "SI" ? true : false;
    }
}

