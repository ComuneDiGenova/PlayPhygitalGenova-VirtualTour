using UnityEngine;

public class LambertProjection
{
    private float scale;
    private float latitudeOrigin;
    private float longitudeOrigin;
    private float falseEasting;
    private float falseNorthing;
    private float e;
    private float n;
    private float f;
    private float rhoOrigin;
    private float thetaOrigin;

    public LambertProjection(float scale, float latitudeOrigin, float longitudeOrigin, float falseEasting, float falseNorthing)
    {
        this.scale = scale;
        this.latitudeOrigin = latitudeOrigin;
        this.longitudeOrigin = longitudeOrigin;
        this.falseEasting = falseEasting;
        this.falseNorthing = falseNorthing;

        float latitudeOriginRad = Mathf.Deg2Rad * latitudeOrigin;
        float nOrigin = Mathf.Log(Mathf.Cos(latitudeOriginRad) * (1f / Mathf.Sin(latitudeOriginRad)));
        float fOrigin = Mathf.Cos(latitudeOriginRad) * Mathf.Pow(Mathf.Tan(Mathf.PI / 4f + latitudeOriginRad / 2f), nOrigin);
        float rhoOrigin = scale * fOrigin / Mathf.Pow(Mathf.Tan(Mathf.PI / 4f + latitudeOriginRad / 2f), nOrigin);
        float thetaOrigin = scale * nOrigin * (0f - longitudeOrigin);

        this.e = 0.0818191908426f; // Eccentricity
        this.n = nOrigin;
        this.f = fOrigin;
        this.rhoOrigin = rhoOrigin;
        this.thetaOrigin = thetaOrigin;
    }

    public Vector3 Project(float latitude, float longitude)
    {
        float latitudeRad = Mathf.Deg2Rad * latitude;
        float longitudeRad = Mathf.Deg2Rad * longitude;

        float n = Mathf.Log(Mathf.Cos(latitudeRad) * (1f / Mathf.Sin(latitudeRad)));
        float f = Mathf.Cos(latitudeRad) * Mathf.Pow(Mathf.Tan(Mathf.PI / 4f + latitudeRad / 2f), n);
        float rho = scale * f / Mathf.Pow(Mathf.Tan(Mathf.PI / 4f + latitudeRad / 2f), n);
        float theta = scale * n * (0f - longitudeRad);

        float x = falseEasting + rho * Mathf.Sin(theta - thetaOrigin);
        float z = falseNorthing + rhoOrigin - rho * Mathf.Cos(theta - thetaOrigin);

        return new Vector3(x, 0, z);
    }
}
