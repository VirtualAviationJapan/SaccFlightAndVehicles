
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EffectsController : UdonSharpBehaviour
{
    [SerializeField] private GameObject VehicleMainObj;
    [SerializeField] private EngineController EngineControl;
    [SerializeField] private TrailRenderer[] Trails;
    [SerializeField] private float TrailGs = 4;
    public Transform FrontWheel;
    [SerializeField] private ParticleSystem SplashParticle;
    [Tooltip("Only play the splash particle if vehicle is faster than this. Meters/s")]
    [SerializeField] private float PlaySplashSpeed = 7;
    private bool SplashNULL;
    private bool TrailsOn;
    private bool HasTrails;
    private bool VehicleMainObjNull = true;
    private bool EngineControlNull = true;
    private bool JoyStickNull = true;
    [System.NonSerializedAttribute] public bool FrontWheelNull = true;
    private bool vapor;
    private float Gs_trail = 1000; //ensures it wont cause effects at first frame
    [System.NonSerializedAttribute] public Animator VehicleAnimator;
    [System.NonSerializedAttribute] public float DoEffects = 6f; //4 seconds before sleep so late joiners see effects if someone is already piloting
    private float brake;
    private float FullHealthDivider;
    private Vector3 OwnerRotationInputs;
    private VRCPlayerApi localPlayer;
    private bool InVR;
    private bool InEditor;
    private int OCCUPIED_STRING = Animator.StringToHash("occupied");
    private int PITCHINPUT_STRING = Animator.StringToHash("pitchinput");
    private int YAWINPUT_STRING = Animator.StringToHash("yawinput");
    private int ROLLINPUT_STRING = Animator.StringToHash("rollinput");
    private int THROTTLE_STRING = Animator.StringToHash("throttle");
    private int ENGINEOUTPUT_STRING = Animator.StringToHash("engineoutput");
    private int VTOLANGLE_STRING = Animator.StringToHash("vtolangle");
    private int HEALTH_STRING = Animator.StringToHash("health");
    private int AOA_STRING = Animator.StringToHash("AoA");
    private int MACH10_STRING = Animator.StringToHash("mach10");
    private int GS_STRING = Animator.StringToHash("Gs");
    private int WEAPON_STRING = Animator.StringToHash("weapon");
    private int BOMB_STRING = Animator.StringToHash("bombs");
    private int AAMS_STRING = Animator.StringToHash("AAMs");
    private int AGMS_STRING = Animator.StringToHash("AGMs");
    private int EXPLODE_STRING = Animator.StringToHash("explode");
    private int MISSILESINCOMING_STRING = Animator.StringToHash("missilesincoming");
    private int LOCALPILOT_STRING = Animator.StringToHash("localpilot");
    private int BULLETHIT_STRING = Animator.StringToHash("bullethit");
    private int ONGROUND_STRING = Animator.StringToHash("onground");
    private int ONWATER_STRING = Animator.StringToHash("onwater");


    private void Start()
    {
        if (VehicleMainObj != null) VehicleMainObjNull = false;
        if (EngineControl != null) EngineControlNull = false;
        if (FrontWheel != null) FrontWheelNull = false;

        FullHealthDivider = 1f / EngineControl.Health;
        HasTrails = Trails.Length > 0;

        VehicleAnimator = VehicleMainObj.GetComponent<Animator>();
        localPlayer = Networking.LocalPlayer;
        if (localPlayer == null)
        {
            InEditor = true;
            VehicleAnimator.SetBool(OCCUPIED_STRING, true);
        }
        SplashNULL = SplashParticle == null;
    }
    private void Update()
    {
        if (DoEffects > 10) { return; }

        //if a long way away just skip effects except large vapor effects
        Effects();
        LargeEffects();
    }
    public void Effects()
    {
        Vector3 RotInputs = EngineControl.RotationInputs;
        float DeltaTime = Time.deltaTime;
        if (EngineControl.IsOwner)
        {
            if (InVR)
            { OwnerRotationInputs = RotInputs; }//vr users use raw input
            else
            { OwnerRotationInputs = Vector3.MoveTowards(OwnerRotationInputs, RotInputs, 7 * DeltaTime); }//desktop users use value movetowards'd to prevent instant movement
            VehicleAnimator.SetFloat(PITCHINPUT_STRING, (OwnerRotationInputs.x * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(YAWINPUT_STRING, (OwnerRotationInputs.y * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(ROLLINPUT_STRING, (OwnerRotationInputs.z * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(THROTTLE_STRING, EngineControl.ThrottleInput);
            VehicleAnimator.SetFloat(ENGINEOUTPUT_STRING, EngineControl.EngineOutput);
        }
        else
        {
            float EngineOutput = EngineControl.EngineOutput;
            VehicleAnimator.SetFloat(PITCHINPUT_STRING, (RotInputs.x * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(YAWINPUT_STRING, (RotInputs.y * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(ROLLINPUT_STRING, (RotInputs.z * 0.5f) + 0.5f);
            VehicleAnimator.SetFloat(THROTTLE_STRING, EngineOutput);//non-owners use value that is similar, but smoothed and would feel bad if the pilot used it himself
            VehicleAnimator.SetFloat(ENGINEOUTPUT_STRING, EngineOutput);
        }
        if (EngineControl.Occupied == true)
        {
            DoEffects = 0f;
            if (!FrontWheelNull)
            {
                if (EngineControl.Taxiing)
                {
                    FrontWheel.localRotation = Quaternion.Euler(new Vector3(0, -RotInputs.y * 80 * (-Mathf.Min((EngineControl.Speed / 10), 1) + 1), 0));
                }
                else FrontWheel.localRotation = Quaternion.identity;
            }
        }
        else { DoEffects += DeltaTime; }


        VehicleAnimator.SetFloat(VTOLANGLE_STRING, EngineControl.VTOLAngle);

        vapor = EngineControl.Speed > 20;// only make vapor when going above "20m/s", prevents vapour appearing when taxiing into a wall or whatever

        VehicleAnimator.SetFloat(HEALTH_STRING, EngineControl.Health * FullHealthDivider);
        VehicleAnimator.SetFloat(AOA_STRING, vapor ? Mathf.Abs(EngineControl.AngleOfAttack * 0.00555555556f /* Divide by 180 */ ) : 0);
    }

    private void LargeEffects()//large effects visible from a long distance
    {
        float DeltaTime = Time.deltaTime;

        if (HasTrails)
        {
            //this is to finetune when wingtrails appear and disappear
            float vertgs = Mathf.Abs(EngineControl.VertGs);
            if (vertgs > Gs_trail) //Gs are increasing
            {
                Gs_trail = Mathf.Lerp(Gs_trail, vertgs, 30f * DeltaTime);//apear fast when pulling Gs
                if (!TrailsOn && Gs_trail > TrailGs)
                {
                    TrailsOn = true;
                    for (int x = 0; x < Trails.Length; x++)
                    { Trails[x].emitting = true; }
                }
            }
            else //Gs are decreasing
            {
                Gs_trail = Mathf.Lerp(Gs_trail, vertgs, 2.7f * DeltaTime);//linger for a bit before cutting off
                if (TrailsOn && Gs_trail < TrailGs)
                {
                    TrailsOn = false;
                    for (int x = 0; x < Trails.Length; x++)
                    { Trails[x].emitting = false; }
                }
            }
        }
        //("mach10", EngineControl.Speed / 343 / 10)
        VehicleAnimator.SetFloat(MACH10_STRING, EngineControl.Speed * 0.000291545189504373f);//should be airspeed but nonlocal players don't have it
        //("Gs", vapor ? EngineControl.Gs / 50 : 0)
        VehicleAnimator.SetFloat(GS_STRING, vapor ? (EngineControl.VertGs * 0.01f) + 0.5f : 0.5f);
    }
    public void EffectsResetStatus()//called from enginecontroller.Explode();
    {
        DoEffects = 6;
        VehicleAnimator.SetInteger(WEAPON_STRING, 4);
        VehicleAnimator.SetFloat(BOMB_STRING, 1);
        VehicleAnimator.SetFloat(AAMS_STRING, 1);
        VehicleAnimator.SetFloat(AGMS_STRING, 1);
        if (!FrontWheelNull) FrontWheel.localRotation = Quaternion.identity;
    }
    public void SFEXT_G_PilotExit()
    {
        VehicleAnimator.SetBool(OCCUPIED_STRING, false);
        VehicleAnimator.SetInteger(MISSILESINCOMING_STRING, 0);
    }
    public void SFEXT_G_PilotEnter()
    {
        DoEffects = 0f;
    }
    public void SFEXT_O_PilotEnter()
    {
        InVR = localPlayer.IsUserInVR();
    }
    public void SFEXT_O_ReAppear()
    {
        DoEffects = 6f; //wake up if was asleep
    }
    public void SFEXT_O_PlaneHit()
    {
        DoEffects = 0f;
        VehicleAnimator.SetTrigger(BULLETHIT_STRING);
    }
    public void SFEXT_G_EnterWater()
    {
        if (EngineControl.Speed > PlaySplashSpeed && !SplashNULL) { SplashParticle.Play(); }
    }
    public void SFEXT_G_TakeOff()
    {
        VehicleAnimator.SetBool(ONGROUND_STRING, false);
        VehicleAnimator.SetBool(ONWATER_STRING, false);
    }
    public void SFEXT_G_TouchDown()
    {
        VehicleAnimator.SetBool(ONGROUND_STRING, true);
    }
    public void SFEXT_G_TouchDownWater()
    {
        VehicleAnimator.SetBool(ONWATER_STRING, true);
    }
    public void SFEXT_G_RespawnButton()
    {
        DoEffects = 6;
    }
    public void SFEXT_G_Explode()//old EffectsExplode()
    {
        VehicleAnimator.SetTrigger(EXPLODE_STRING);
        VehicleAnimator.SetFloat(BOMB_STRING, 1);
        VehicleAnimator.SetFloat(AAMS_STRING, 1);
        VehicleAnimator.SetFloat(AGMS_STRING, 1);
        VehicleAnimator.SetInteger(MISSILESINCOMING_STRING, 0);
        VehicleAnimator.SetInteger(WEAPON_STRING, 0);
        VehicleAnimator.SetFloat(PITCHINPUT_STRING, .5f);
        VehicleAnimator.SetFloat(YAWINPUT_STRING, .5f);
        VehicleAnimator.SetFloat(ROLLINPUT_STRING, .5f);
        VehicleAnimator.SetFloat(THROTTLE_STRING, 0);
        VehicleAnimator.SetFloat(ENGINEOUTPUT_STRING, 0);
        if (!InEditor) { VehicleAnimator.SetBool(OCCUPIED_STRING, false); }
        DoEffects = 0f;//keep awake
        if (!FrontWheelNull) FrontWheel.localRotation = Quaternion.identity;
    }
}