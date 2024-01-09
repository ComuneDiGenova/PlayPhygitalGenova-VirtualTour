using UnityEngine.Networking;

public class AuthorizationAPI
{
  
    
    public static string baseURL;
    public static string username ;
    public static string password ;

    static string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    public static void AddAuthRequestHeader(UnityWebRequest request)
    {
        string authorization = authenticate(username, password);
        request.SetRequestHeader("AUTHORIZATION", authorization);
    }
    public static void AddAnyCertificateHandler(UnityWebRequest request)
    {
        request.certificateHandler = new AcceptAnyCertificate();
    }
}




/////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////
public class AcceptAnyCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}
/////////////////////////// CERITFICATO NON VALIDO TOGLIERE STRINGA IN SEGUITO /////////////////////////////////////

