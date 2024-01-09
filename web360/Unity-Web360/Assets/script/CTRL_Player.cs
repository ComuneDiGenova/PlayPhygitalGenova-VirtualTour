using Cinemachine;
using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public delegate void NextPosition();
public class CTRL_Player : MonoBehaviour
{
    CtrlCoordinate CtrlCoordinate;
    JsonReader jsonReader;
    public Transform sferaA;
    public Transform sferaB;
    public bool applyKappaPhi;
    public Button playStop;
    public float slideDuration = 1; // Durata in secondi dell'interpolazione
    public float secondiStepMin = 1f;
    public float secondiStepMax = 10f;
    public float distanceMaxStep = 20f;
    public static int posizioneAttuale = 0;
    public static int posizioneProssima = 0;
    //public Material material;

    float omega1, phi1, kappa1, omega2, phi2, kappa2;
    //float roll1,roll2, pitch1,pitch2, yaw1, yaw2;

    Material material1, material2;

    [SerializeField] private Sprite play;
    [SerializeField] private Sprite pausa;

    public static event NextPosition OnNextPosition;
    public static event NextPosition OnStart;
    public GETPointOfInterest getPointOfInterest;
    public GETShops getShops;
    public float activationDistance = 30f;

    [Header("Debug")]
    public bool startPlayer = false;
    public bool pausePlayer = false;
    public float time = 0, stepTime = 0;
    public bool canJump = true;
    public bool setMapPosition = true;
    PanoPoint corrente, prossima;
    float fadeElapsedTime = 0;
    bool stopJump = false;

    [SerializeField] private GameObject freccia;
    private new Camera camera;
    [SerializeField] private GameObject virtualCamera;

    public void DisbleSetMapPosition()
    {
        Debug.Log("Disable Map Position");
        setMapPosition = false;
    }

    private void Awake()
    {
        CtrlCoordinate = GameObject.Find("CTRL_Coordinate").GetComponent<CtrlCoordinate>();
        jsonReader = GameObject.Find("CTRL_Coordinate").GetComponent<JsonReader>();
        CtrlCoordinate.OnPercorso += inizioApplicazione;
        material1 = sferaA.GetComponent<Renderer>().material;
        material2 = sferaB.GetComponent<Renderer>().material;
        material2.renderQueue = material1.renderQueue + 1;
        material1.SetFloat("_Alpha", 1);
        material2.SetFloat("_Alpha", 0);
        camera = Camera.main;
    }

    private void Start()
    {
        //virtualCamera.AddCinemachineComponent<CinemachineHardLookAt>();
        //virtualCamera.LookAt =CtrlCoordinate.PuntiList[1].transform;
        //virtualCamera.AddCinemachineComponent<CinemachinePOV>();
        //virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.val
    }

    private void inizioApplicazione()
    {
        StartCoroutine(AspettoPOI());
    }

    private IEnumerator AspettoPOI()
    {
        yield return new WaitUntil(() => GETPointOfInterest.PoiIstanziati);
        StartPlayer();
    }
    public void StartPlayer()
    {
        canJump = false;
        setMapPosition = true;
        posizioneAttuale = 0;
        posizioneProssima = 1;
        corrente = CtrlCoordinate.PanoTour[posizioneAttuale];
        prossima = CtrlCoordinate.PanoTour[posizioneProssima];
        //ruotiamo la camera verso la direzione del percorso

        Vector3 direction = CtrlCoordinate.PuntiList[1].transform.position - camera.transform.position;

        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 rotazioneEuler = rotation.eulerAngles;
        CinemachineVirtualCamera cameraV = virtualCamera.GetComponent<CinemachineVirtualCamera>();
        cameraV.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = rotazioneEuler.x;
        cameraV.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = rotazioneEuler.y;
        //Virtualcamera.SetActive(true);

        EvalTime();

        jsonReader.EvaluateBuffer(corrente, () =>
        {

            Debug.Log("inizia il tour");

            material1.SetTexture("_MainTex", corrente.lowResTexture);
            material2.SetTexture("_MainTex", prossima.lowResTexture);
            material1.SetFloat("_Alpha", 1);
            material2.SetFloat("_Alpha", 0);

            startPlayer = true;
            //EvalRotation();
            //SetRotation();

            //gameObject.transform.position = CtrlCoordinate.PuntiList[posizioneAttuale].transform.position;
            MoveTowardsPosition(0.5f, posizioneAttuale);
            ControlloPOI();
            ControlloSpherePoint();

            //HI RES
            if (corrente.hiResTexure != null)
            {
                material1.SetTexture("_MainTex", corrente.hiResTexure);
            }
            else
            {
                jsonReader.GetImmagine360HiRes(corrente, (hires, cpp) =>
                {
                    material1.SetTexture("_MainTex", hires);
                });
            }
            if (prossima.hiResTexure != null)
            {
                material1.SetTexture("_MainTex", prossima.hiResTexure);
                canJump = true;
            }
            else
            {
                jsonReader.GetImmagine360HiRes(prossima, (hires, cpp) =>
                {
                    material2.SetTexture("_MainTex", hires);
                    canJump = true;
                });
            }
            OnNextPosition?.Invoke();
            OnStart?.Invoke();
            SetRotationA(posizioneAttuale);
            SetRotationB(posizioneProssima);
        });
    }

    private void Update()
    {
        if (CtrlCoordinate.PuntiList.Count > posizioneProssima)
        {
            // Calcola la direzione dalla freccia al target.
            Vector3 direction = CtrlCoordinate.PuntiList[posizioneProssima].transform.position - freccia.transform.position;
            direction.y = 0f; // Imposta la componente Y a zero per ignorarla.
                              // Utilizza Quaternion.LookRotation per ruotare la freccia nella direzione desiderata.
            freccia.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        if (pausePlayer) return;

        if (posizioneAttuale < CtrlCoordinate.PuntiList.Count - 1)
        {
            //Debug.Log("non sono in pausa e nn ho finito il percorso");
            if (startPlayer)
            {
                time += Time.deltaTime;
            }
            if (time > stepTime && canJump)
            {
                NextPosition();
            }
        }


    }

    public void PlayPause()
    {
        if (!pausePlayer)
        {
            pausePlayer = true;
            playStop.GetComponent<Image>().sprite = play;
        }
        else
        {
            pausePlayer = false;
            playStop.GetComponent<Image>().sprite = pausa;
        }

    }

    public void PlayPauseShop()
    {
        if (!pausePlayer)
        {
            pausePlayer = true;
            playStop.GetComponent<Image>().sprite = play;
            //canJump = true;
        }
        else
        {

        }
    }

    void EvalTime()
    {
        var dist = (float)GeoCoordinate.Utils.HaversineDistance(corrente.coordinate, prossima.coordinate);
        stepTime = Mathf.Lerp(secondiStepMin, secondiStepMax, dist / distanceMaxStep);
        Debug.Log($"Next Distance: {dist}/{distanceMaxStep}, time: {stepTime}/{secondiStepMax}");
    }

    public void NextPosition()
    {
        if (!canJump) return;
        //Debug.LogWarning("NextPosition" + posizioneAttuale + (CtrlCoordinate.PanoTour.Count - 1));
        canJump = false;
        time = 0;
        //startPlayer = false;

        posizioneAttuale += 1;
        posizioneProssima = posizioneAttuale + 1;
        posizioneProssima = posizioneProssima >= CtrlCoordinate.PanoTour.Count - 1 ? CtrlCoordinate.PanoTour.Count - 1 : posizioneProssima;
        Debug.Log("posizione attuale = " + posizioneAttuale);
        StartCoroutine(InterpolateTrasparency(slideDuration, posizioneAttuale, () =>
        {


            OnNextPosition?.Invoke();

            if (posizioneAttuale == CtrlCoordinate.PanoTour.Count - 1)
            {
                canJump = true;
                Debug.LogWarning("Tour Finito");
                return;
            }

            //gameObject.transform.position = CtrlCoordinate.PuntiList[posizioneAttuale].transform.position;
            corrente = CtrlCoordinate.PanoTour[posizioneAttuale];
            prossima = CtrlCoordinate.PanoTour[posizioneProssima];

            ControlloPOI();
            ControlloSpherePoint();
            jsonReader.EvaluateBuffer(corrente, () =>
            {

                EvalTime();
                SetRotationA(posizioneAttuale);
                SetRotationB(posizioneProssima);
                //startPlayer=true;

                material1.SetFloat("_Alpha", 1);
                material2.SetFloat("_Alpha", 0);

                //corrente
                if (corrente.hiResTexure != null)
                {
                    material1.SetTexture("_MainTex", corrente.hiResTexure);
                    canJump = true;
                }
                else
                {
                    material1.SetTexture("_MainTex", corrente.lowResTexture);
                    jsonReader.GetImmagine360HiRes(corrente, (hires, cpp) =>
                    {
                        if (cpp == corrente)
                        {
                            material1.SetTexture("_MainTex", hires);
                            canJump = true;
                            if (corrente == prossima)
                            {
                                material2.SetTexture("_MainTex", hires);
                                canJump = true;
                            }
                        }
                    });
                }
                //prossima
                if (prossima.hiResTexure != null)
                {
                    material2.SetTexture("_MainTex", prossima.hiResTexure);
                    canJump = true;
                }
                else
                {
                    material2.SetTexture("_MainTex", prossima.lowResTexture);
                    //canJump = true;
                    if (corrente != prossima)
                    {
                        //HI RES
                        jsonReader.GetImmagine360HiRes(prossima, (hires, cpp) =>
                        {
                            if (cpp == prossima)
                            {
                                material2.SetTexture("_MainTex", hires);
                                canJump = true;
                            }
                        });
                    }
                }
            });

        }));

    }

    public async void SetPosition(int index, Action callback)
    {
        if (index == posizioneAttuale)
        {
            return;
        }

        if (!canJump)
        {
            Debug.LogWarning("Interrupt jump");
            //stop current fade
            fadeElapsedTime = 1;
            stopJump = true;
            await Task.Yield();
            await Task.Yield();
            stopJump = false;
            //
        }
        //if(!canJump)return;
        //

        canJump = false;
        setMapPosition = true;
        time = 0;
        startPlayer = false;
        posizioneAttuale = index;
        posizioneProssima = posizioneAttuale + 1;
        posizioneProssima = posizioneProssima >= CtrlCoordinate.PanoTour.Count - 1 ? CtrlCoordinate.PanoTour.Count - 1 : posizioneProssima;
        corrente = CtrlCoordinate.PanoTour[posizioneAttuale];
        prossima = CtrlCoordinate.PanoTour[posizioneProssima];
        Debug.Log("posizione attuale = " + posizioneAttuale);
        //Debug.Log("posizioneAttuale : " + posizioneAttuale + "posizioneProssima" + posizioneProssima);



        jsonReader.EvaluateBuffer(corrente, () =>
        {

            if (stopJump) return;
            material2.SetTexture("_MainTex", corrente.lowResTexture);
            material2.SetFloat("_Alpha", 0);

            SetRotationB(posizioneAttuale);

            StartCoroutine(InterpolateTrasparency(slideDuration, index, () =>
            {
                if (stopJump) return;


                //canJump=true;
                ControlloPOI();
                ControlloSpherePoint();
                OnNextPosition?.Invoke();
                callback?.Invoke();

                EvalTime();
                startPlayer = true;

                material1.SetTexture("_MainTex", corrente.lowResTexture);
                material1.SetFloat("_Alpha", 1);
                material2.SetTexture("_MainTex", prossima.lowResTexture);
                material2.SetFloat("_Alpha", 0);
                SetRotationA(posizioneAttuale);
                SetRotationB(posizioneProssima);

                if (corrente.hiResTexure != null)
                {
                    material1.SetTexture("_MainTex", corrente.hiResTexure);
                    if (corrente == prossima)
                    {
                        material2.SetTexture("_MainTex", corrente.hiResTexure);
                        canJump = true;
                    }
                }
                else
                {
                    jsonReader.GetImmagine360HiRes(corrente, (hires, cpp) =>
                    {
                        if (cpp == corrente)
                        {
                            material1.SetTexture("_MainTex", hires);
                            if (corrente == prossima)
                            {
                                canJump = true;
                                material2.SetTexture("_MainTex", hires);
                            }
                        }
                    });
                }
                if (corrente != prossima)
                {
                    if (prossima.hiResTexure != null)
                    {
                        material2.SetTexture("_MainTex", prossima.hiResTexure);
                        canJump = true;
                    }
                    else
                    {
                        jsonReader.GetImmagine360HiRes(prossima, (hires, cpp) =>
                        {
                            if (cpp == prossima)
                            {
                                material2.SetTexture("_MainTex", hires);
                                canJump = true;
                            }
                        });
                    }
                }
            }));
        });
    }

    private IEnumerator MoveTowardsPosition(float duration, int targetIndex)
    {
        if (stopJump) yield break;

        float elapsedTime = 0f;
        Vector3 startPosition = gameObject.transform.position;
        Vector3 targetPosition = CtrlCoordinate.PuntiList[targetIndex].transform.position;

        while (elapsedTime < duration)
        {
            if (stopJump) yield break;

            float t = elapsedTime / duration;
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Puoi anche applicare rotazioni o scalature qui, se necessario.

            elapsedTime += Time.deltaTime;
            yield return null; // Attendere fino al prossimo frame
        }
    }

    private void ControlloPOI()
    {
        Vector3 posizionePlayer = gameObject.transform.position;
        Debug.Log("Controllo POI");
        foreach (GameObject o in getPointOfInterest.ListaPOI)
        {
            float distance = Vector3.Distance(posizionePlayer, o.transform.position);

            // Controlla se la distanza � inferiore alla distanza di attivazione
            if (distance <= activationDistance)
            {
                // Accendi l'oggetto
                o.SetActive(true);
            }
            else
            {
                // Spegni l'oggetto
                o.SetActive(false);
            }
        }
        foreach (GameObject o in getShops.ListaShop)
        {
            float distance = Vector3.Distance(posizionePlayer, o.transform.position);

            // Controlla se la distanza � inferiore alla distanza di attivazione
            if (distance <= activationDistance)
            {
                // Accendi l'oggetto
                o.SetActive(true);
                o.GetComponent<SizeDistance>().SetSize();
            }
            else
            {
                // Spegni l'oggetto
                o.SetActive(false);
            }
        }
    }

    private void ControlloSpherePoint()
    {
        for (int i = 0; i < CtrlCoordinate.PuntiList.Count; i++)
        {
            if (i < posizioneAttuale - 2 || i > posizioneAttuale + 2)
            {
                CtrlCoordinate.PuntiList[i].SetActive(false);
            }
            else
            {
                CtrlCoordinate.PuntiList[i].SetActive(true);
            }

        }

    }
    /*
        private void EvalRotation(){

            roll1 = ((float)double.Parse(corrente.pointData.Roll_X_deg,CultureInfo.InvariantCulture));
            roll2 = ((float)double.Parse(prossima.pointData.Roll_X_deg,CultureInfo.InvariantCulture));
            pitch1 = ((float)double.Parse(corrente.pointData.Pitch_Y_de, CultureInfo.InvariantCulture));
            pitch2 = ((float)double.Parse(prossima.pointData.Pitch_Y_de, CultureInfo.InvariantCulture));
            yaw1 = ((float)double.Parse(corrente.pointData.Yaw_Z_deg_, CultureInfo.InvariantCulture));
            yaw2 = ((float)double.Parse(prossima.pointData.Yaw_Z_deg_, CultureInfo.InvariantCulture));

            omega1 = ((float)double.Parse(corrente.pointData.Omega_deg_,CultureInfo.InvariantCulture));
            omega2 = ((float)double.Parse(prossima.pointData.Omega_deg_,CultureInfo.InvariantCulture));
            phi1 = ((float)double.Parse(corrente.pointData.Phi_deg_,CultureInfo.InvariantCulture));
            phi2 = ((float)double.Parse(prossima.pointData.Phi_deg_,CultureInfo.InvariantCulture));
            kappa1 = ((float)double.Parse(corrente.pointData.Kappa_deg_,CultureInfo.InvariantCulture));
            kappa2 = ((float)double.Parse(prossima.pointData.Kappa_deg_,CultureInfo.InvariantCulture));
        }
    */
    private void SetRotationA(int index)
    {
        /*
        Omega = Z // Y
        Phi = -X // Z
        Kappa = Y + 90 // X
        Kappa Z
        Phi Y
        Omega X
        */
        var ppoint = CtrlCoordinate.PanoTour[index];
        //roll1 = ((float)double.Parse(ppoint.pointData.Roll_X_deg,CultureInfo.InvariantCulture));
        //pitch1 = ((float)double.Parse(ppoint.pointData.Pitch_Y_de, CultureInfo.InvariantCulture));
        //yaw1 = ((float)double.Parse(ppoint.pointData.Yaw_Z_deg_, CultureInfo.InvariantCulture));
        omega1 = ((float)double.Parse(ppoint.pointData.Omega_deg_, CultureInfo.InvariantCulture));
        phi1 = ((float)double.Parse(ppoint.pointData.Phi_deg_, CultureInfo.InvariantCulture));
        kappa1 = ((float)double.Parse(ppoint.pointData.Kappa_deg_, CultureInfo.InvariantCulture));

        //sembra non servire proprio a un cazzo fare la somma tra i due quaternioni.
        if (applyKappaPhi)
        {
            Debug.Log($"id {ppoint.pointData.Id} kappa1 {kappa1}, omega 1  {omega1} phi1 {phi1}");
            sferaA.rotation = Quaternion.identity;
            sferaB.rotation = Quaternion.identity;

            //ruotiamo la sfera A

            if (ppoint.pointData.Filename.StartsWith("Job") || ppoint.pointData.Filename.StartsWith("03A") || ppoint.pointData.Filename.StartsWith("03B"))
            {
                //Debug.LogWarningFormat("la sfera inizia per 'Job' e quindi nn viene ruotata");
                //sferaA.Rotate(sferaA.up, kappa1, Space.Self);
            }
            else if (ppoint.pointData.Filename.StartsWith("ztl"))
            {
                sferaA.rotation = Quaternion.Euler(-omega1, kappa1, phi1);
            }

            else
            {
                sferaA.rotation = Quaternion.Euler(0, kappa1 + 90, 0);
            }

            //sito 1 reference
            //{
            //    sferaA.Rotate(sferaA.up, kappa1, Space.Self);
            //    sferaA.Rotate(sferaA.forward, phi1, Space.Self);
            //    sferaA.Rotate(sferaA.right, omega1, Space.Self);

            //    sferaB.Rotate(sferaB.up, kappa2, Space.Self);
            //    sferaB.Rotate(sferaB.forward, phi2, Space.Self);
            //    sferaB.Rotate(sferaB.right, omega2, Space.Self);

            //}
            //sito2 reference
            //{
            //    sferaA.Rotate(sferaA.up, kappa1, Space.World);
            //    sferaA.Rotate(sferaA.right, omega1, Space.World);
            //    sferaA.Rotate(sferaA.forward, phi1, Space.World);

            //    sferaB.Rotate(sferaB.up, kappa2, Space.World);
            //    sferaB.Rotate(sferaB.right, omega2, Space.World);
            //    sferaB.Rotate(sferaB.forward, phi2, Space.World);

            //}
            //sito3 reference
            //{
            //    sferaA.Rotate(sferaA.right, omega1, Space.Self);
            //    sferaA.Rotate(sferaA.forward, phi1, Space.Self);
            //    sferaA.Rotate(sferaA.up, kappa1, Space.Self);

            //    sferaB.Rotate(sferaB.right, omega2, Space.Self);
            //    sferaB.Rotate(sferaB.forward, phi2, Space.Self);
            //    sferaB.Rotate(sferaB.up, kappa2, Space.Self);
            //}


            // Calcola l'angolo di offset di yaw per allineare la fotocamera al nord
            //float offsetYaw = 360f - yaw1;

            // Calcola l'angolo di yaw normalizzato nell'intervallo 0 a 360 gradi
            //yaw1 = 360f - Mathf.DeltaAngle(0f, yaw1);
            //Debug.Log("yaw dopo " + yaw1);
            //Quaternion rotazioneAsse = Quaternion.Euler((omega1 * Mathf.Deg2Rad),( phi1 * Mathf.Deg2Rad), (kappa1 * Mathf.Deg2Rad));
            //sferaA.rotation = Quaternion.Euler((0),(kappa1),(0));
            //sferaB.rotation = Quaternion.Euler((0),(kappa2),(0));
            //sferaB.rotation = Quaternion.Euler((omega2 * Mathf.Deg2Rad),(phi2 * Mathf.Deg2Rad), (kappa2 * Mathf.Deg2Rad));
        }

        //else{
        //    sferaA.rotation = Quaternion.Euler(0, omega1+90, 0);
        //    sferaB.rotation = Quaternion.Euler(0, omega2+90 , 0);
        //}
    }

    private void SetRotationB(int index)
    {
        var ppoint = CtrlCoordinate.PanoTour[index];
        //roll2 = ((float)double.Parse(ppoint.pointData.Roll_X_deg,CultureInfo.InvariantCulture));
        //pitch2 = ((float)double.Parse(ppoint.pointData.Pitch_Y_de, CultureInfo.InvariantCulture));
        //yaw2 = ((float)double.Parse(ppoint.pointData.Yaw_Z_deg_, CultureInfo.InvariantCulture));
        omega2 = ((float)double.Parse(ppoint.pointData.Omega_deg_, CultureInfo.InvariantCulture));
        phi2 = ((float)double.Parse(ppoint.pointData.Phi_deg_, CultureInfo.InvariantCulture));
        kappa2 = ((float)double.Parse(ppoint.pointData.Kappa_deg_, CultureInfo.InvariantCulture));

        if (applyKappaPhi)
        {
            Debug.Log($"id {ppoint.pointData.Id} kappa1 {kappa1}, omega 1  {omega1} phi1 {phi1}");
            sferaB.rotation = Quaternion.identity;

            //ruotiamo La sfera B
            if (ppoint.pointData.Filename.StartsWith("Job") || ppoint.pointData.Filename.StartsWith("03A") || ppoint.pointData.Filename.StartsWith("03B"))
            {
                //sferaB.Rotate(sferaB.up, kappa2, Space.World);

            }
            else if (ppoint.pointData.Filename.StartsWith("ztl"))
            {
                sferaB.rotation = Quaternion.Euler(-omega2, kappa2, phi2);
            }
            else
            {
                sferaB.Rotate(sferaB.up, kappa2 + 90, Space.Self);
            }
        }
    }


    private IEnumerator InterpolateTrasparency(float duration, int index, Action onComplete)
    {
        fadeElapsedTime = 0.0f;
        StartCoroutine(MoveTowardsPosition(0.5f, index));
        while (fadeElapsedTime < duration)
        {

            float t = fadeElapsedTime / duration;
            float slideValue = Mathf.Lerp(0, 1, t);
            material2.SetFloat("_Alpha", slideValue);
            fadeElapsedTime += Time.deltaTime;
            yield return null;
        }

        material1.SetFloat("_Alpha", 0);
        material2.SetFloat("_Alpha", 1);
        onComplete?.Invoke();
    }
}
