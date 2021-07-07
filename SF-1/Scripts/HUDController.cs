
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[SerializeField]
public class HUDController : UdonSharpBehaviour
{
    [SerializeField] private EngineController EngineControl;
    [SerializeField] private Material SmokeColorIndicator;
    [SerializeField] private Text HUDText_G;
    [SerializeField] private Text HUDText_mach;
    [SerializeField] private Text HUDText_altitude;
    [SerializeField] private Text HUDText_knotstarget;
    [SerializeField] private Text HUDText_knots;
    [SerializeField] private Text HUDText_knotsairspeed;
    [SerializeField] private Text HUDText_angleofattack;
    [SerializeField] private Text HUDText_AAM_ammo;
    [SerializeField] private Text HUDText_AGM_ammo;
    [SerializeField] private Text HUDText_Bomb_ammo;
    [SerializeField] private GameObject HudCrosshairGun;
    [SerializeField] private GameObject HudCrosshair;
    [SerializeField] private GameObject HudHold;
    [SerializeField] private GameObject HudLimit;
    [SerializeField] private GameObject HudAB;
    [SerializeField] private Transform DownIndicator;
    [SerializeField] private Transform ElevationIndicator;
    [SerializeField] private Transform HeadingIndicator;
    [SerializeField] private Transform VelocityIndicator;
    [SerializeField] private float BulletSpeed = 1050;
    [SerializeField] private Transform PitchRoll;
    [SerializeField] private Transform Yaw;
    [SerializeField] private Transform LStickDisplayHighlighter;
    [SerializeField] private Transform RStickDisplayHighlighter;
    [SerializeField] private GameObject AtGScreen;
    [SerializeField] private GameObject LStick_funcon1;
    [SerializeField] private GameObject LStick_funcon2;
    [SerializeField] private GameObject LStick_funcon4;
    [SerializeField] private GameObject LStick_funcon6;
    [SerializeField] private GameObject LStick_funcon7;
    [SerializeField] private GameObject LStick_funcon8;
    [SerializeField] private GameObject RStick_funcon3;
    [SerializeField] private GameObject RStick_funcon5;
    [SerializeField] private GameObject RStick_funcon6;
    [SerializeField] private GameObject RStick_funcon7;
    [SerializeField] private GameObject RStick_funcon8;
    private Animator PlaneAnimator;
    public float distance_from_head = 1.333f;
    private float maxGs = 0f;
    private Vector3 InputsZeroPos;
    private Vector3 startingpos;
    private float check = 0;
    [System.NonSerializedAttribute] public float MenuSoundCheckLast = 0;
    private int showvel;
    const float InputSquareSize = 0.0284317f;//size of the square on the HUD that shows inputs
    [System.NonSerializedAttribute] public Vector3 GUN_TargetDirOld;
    [System.NonSerializedAttribute] public float GUN_TargetSpeedLerper;
    [System.NonSerializedAttribute] public Vector3 RelativeTargetVelLastFrame;
    private Vector3 TargetDir = Vector3.zero;
    private Vector3 TargetSpeed;
    private bool HasAAMTargets = false;
    private float FullFuelDivider;
    private float FullGunAmmoDivider;
    private Vector3 RelativeTargetVel;
    private Vector3 AAMCurrentTargetPositionLastFrame;
    private Transform VehicleTransform;
    private EffectsController EffectsControl;
    private Camera AtGCam;
    float debuglerper;
    float debugcurrentframe;
    private float VTOLDefaultValue;
    private int FUEL_STRING = Animator.StringToHash("fuel");
    private int GUNAMMO_STRING = Animator.StringToHash("gunammo");
    VRCPlayerApi localPlayer;
    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
        Assert(SmokeColorIndicator != null, "Start: SmokeColorIndicator != null");
        Assert(HUDText_G != null, "Start: HUDText_G != null");
        Assert(HUDText_mach != null, "Start: HUDText_mach != null");
        Assert(HUDText_altitude != null, "Start: HUDText_altitude != null");
        Assert(HUDText_knotstarget != null, "Start: HUDText_knotstarget != null");
        Assert(HUDText_knots != null, "Start: HUDText_knots != null");
        Assert(HUDText_knotsairspeed != null, "Start: HUDText_knotsairspeed != null");
        Assert(HUDText_angleofattack != null, "Start: HUDText_angleofattack != null");
        Assert(HudCrosshairGun != null, "Start: HudCrosshairGun != null");
        Assert(HudCrosshair != null, "Start: HudCrosshair != null");
        Assert(HudHold != null, "Start: HudHold != null");
        Assert(HudLimit != null, "Start: HudLimit != null");
        Assert(HudAB != null, "Start: HudAB != null");
        Assert(DownIndicator != null, "Start: DownIndicator != null");
        Assert(ElevationIndicator != null, "Start: ElevationIndicator != null");
        Assert(HeadingIndicator != null, "Start: HeadingIndicator != null");
        Assert(VelocityIndicator != null, "Start: VelocityIndicator != null");
        Assert(LStickDisplayHighlighter != null, "Start: LStickDisplayHighlighter != null");
        Assert(RStickDisplayHighlighter != null, "Start: RStickDisplayHighlighter != null");
        Assert(PitchRoll != null, "Start: PitchRoll != null");
        Assert(Yaw != null, "Start: Yaw != null");
        Assert(AtGScreen != null, "Start: AGMScreen != null");
        Assert(LStick_funcon1 != null, "Start: LStick_funcon1 != null");
        Assert(LStick_funcon2 != null, "Start: LStick_funcon2 != null");
        Assert(LStick_funcon4 != null, "Start: LStick_funcon4 != null");
        Assert(LStick_funcon6 != null, "Start: LStick_funcon6 != null");
        Assert(LStick_funcon7 != null, "Start: LStick_funcon7 != null");
        Assert(LStick_funcon8 != null, "Start: LStick_funcon8 != null");
        Assert(RStick_funcon3 != null, "Start: LStick_funcon3 != null");
        Assert(RStick_funcon5 != null, "Start: LStick_funcon5 != null");
        Assert(RStick_funcon6 != null, "Start: LStick_funcon6 != null");
        Assert(RStick_funcon7 != null, "Start: LStick_funcon7 != null");
        Assert(RStick_funcon8 != null, "Start: LStick_funcon8 != null");

        PlaneAnimator = EngineControl.VehicleMainObj.GetComponent<Animator>();
        InputsZeroPos = PitchRoll.localPosition;
        VehicleTransform = EngineControl.VehicleMainObj.transform;
        EffectsControl = EngineControl.EffectsControl;
        AtGCam = EngineControl.AtGCam;

        float fuel = EngineControl.Fuel;
        FullFuelDivider = 1f / (fuel > 0 ? fuel : 10000000);

        VTOLDefaultValue = EngineControl.VTOLDefaultValue;

        localPlayer = Networking.LocalPlayer;
    }
    private void OnEnable()
    {
        maxGs = 0f;
    }
    private void LateUpdate()
    {
        float SmoothDeltaTime = Time.smoothDeltaTime;
        //RollPitch Indicator
        PitchRoll.localPosition = InputsZeroPos + (new Vector3(-EngineControl.RotationInputs.z, EngineControl.RotationInputs.x, 0)) * InputSquareSize;

        //Yaw Indicator
        Yaw.localPosition = InputsZeroPos + (new Vector3(EngineControl.RotationInputs.y, 0, 0)) * InputSquareSize;

        //Velocity indicator
        Vector3 tempvel;
        if (EngineControl.CurrentVel.magnitude < 2)
        {
            tempvel = -Vector3.up * 2;//straight down instead of spazzing out when moving very slow
        }
        else
        {
            tempvel = EngineControl.CurrentVel;
        }

        VelocityIndicator.position = transform.position + tempvel;
        VelocityIndicator.localPosition = VelocityIndicator.localPosition.normalized * distance_from_head;
        /////////////////




        //Smoke Color Indicator
        SmokeColorIndicator.color = EngineControl.SmokeColor_Color;

        //Heading indicator
        Vector3 VehicleEuler = EngineControl.VehicleMainObj.transform.rotation.eulerAngles;
        HeadingIndicator.localRotation = Quaternion.Euler(new Vector3(0, -VehicleEuler.y, 0));
        /////////////////

        //Elevation indicator
        ElevationIndicator.rotation = Quaternion.Euler(new Vector3(0, VehicleEuler.y, 0));
        /////////////////

        //Down indicator
        DownIndicator.localRotation = Quaternion.Euler(new Vector3(0, 0, -VehicleEuler.z));
        /////////////////

        //LIMITS indicator
        if (EngineControl.FlightLimitsEnabled)
        {
            HudLimit.SetActive(true);
        }
        else { HudLimit.SetActive(false); }

        //Alt. HOLD indicator
        if (EngineControl.AltHold)
        {
            HudHold.SetActive(true);
        }
        else { HudHold.SetActive(false); }

        //Left Stick Selector
        switch (EngineControl.LStickSelection)
        {
            case -1:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 180);//invisible, backfacing
                break;
            case 0:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 45, 0);
                break;
            case 2:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 90, 0);
                break;
            case 3:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 135, 0);
                break;
            case 4:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 180, 0);
                break;
            case 5:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 225, 0);
                break;
            case 6:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 270, 0);
                break;
            case 7:
                LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 315, 0);
                break;
        }

        //Right Stick Selector
        switch (EngineControl.RStickSelection)
        {
            case -1:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 180);//invisible, backfacing
                break;
            case 0:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 45, 0);
                break;
            case 2:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 90, 0);
                break;
            case 3:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 135, 0);
                break;
            case 4:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 180, 0);
                break;
            case 5:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 225, 0);
                break;
            case 6:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 270, 0);
                break;
            case 7:
                RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 315, 0);
                break;
        }

        //Cruise Control target knots
        if (EngineControl.Cruise)
        {
            HUDText_knotstarget.text = ((EngineControl.SetSpeed) * 1.9438445f).ToString("F0");
        }
        else { HUDText_knotstarget.text = string.Empty; }

        //left stick toggles/functions on?
        if (EngineControl.VTOLAngle != VTOLDefaultValue) { LStick_funcon1.SetActive(true); }
        else { LStick_funcon1.SetActive(false); }

        if (EngineControl.FlightLimitsEnabled) { LStick_funcon2.SetActive(true); }
        else { LStick_funcon2.SetActive(false); }

        if (EngineControl.CatapultStatus == 1) { LStick_funcon4.SetActive(true); }
        else { LStick_funcon4.SetActive(false); }

        if (EngineControl.AltHold) { LStick_funcon6.SetActive(true); }
        else { LStick_funcon6.SetActive(false); }

        if (EffectsControl.CanopyOpen) { LStick_funcon7.SetActive(true); }
        else { LStick_funcon7.SetActive(false); }

        if (EngineControl.Cruise) { LStick_funcon8.SetActive(true); }
        else { LStick_funcon8.SetActive(false); }


        //right stick toggles/functions on?
        if (EngineControl.AGMLocked) { RStick_funcon3.SetActive(true); }
        else { RStick_funcon3.SetActive(false); }

        if (!EffectsControl.GearUp) { RStick_funcon5.SetActive(true); }
        else { RStick_funcon5.SetActive(false); }

        if (EffectsControl.Flaps) { RStick_funcon6.SetActive(true); }
        else { RStick_funcon6.SetActive(false); }

        if (EffectsControl.HookDown) { RStick_funcon7.SetActive(true); }
        else { RStick_funcon7.SetActive(false); }

        if (EffectsControl.Smoking) { RStick_funcon8.SetActive(true); }
        else { RStick_funcon8.SetActive(false); }

        //play menu sound if selection changed since last frame
        float MenuSoundCheck = EngineControl.RStickSelection + EngineControl.LStickSelection;
        if (!EngineControl.SoundControl.MenuSelectNull && MenuSoundCheck != MenuSoundCheckLast)
        {
            EngineControl.SoundControl.MenuSelect.Play();
        }
        MenuSoundCheckLast = MenuSoundCheck;

        //AGMScreen
        if (EngineControl.RStickSelection == 3)
        {
            if (!EngineControl.AGMLocked)
            {
                AtGScreen.SetActive(true);
                AtGCam.gameObject.SetActive(true);
                //if turning camera fast, zoom out
                if (EngineControl.AGMRotDif < 2.5f)
                {
                    RaycastHit camhit;
                    Physics.Raycast(AtGCam.transform.position, AtGCam.transform.forward, out camhit, Mathf.Infinity, 1);
                    if (camhit.point != null)
                    {
                        //dolly zoom //Mathf.Atan(100 <--the 100 is the height of the camera frustrum at the target distance
                        float newzoom = Mathf.Clamp(2.0f * Mathf.Atan(100 * 0.5f / Vector3.Distance(gameObject.transform.position, camhit.point)) * Mathf.Rad2Deg, 1.5f, 90);
                        AtGCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(AtGCam.fieldOfView, newzoom, 1.5f * SmoothDeltaTime), 0.3f, 90);
                    }
                }
                else
                {
                    float newzoom = 80;
                    AtGCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(AtGCam.fieldOfView, newzoom, 3.5f * SmoothDeltaTime), 0.3f, 90); //zooming in is a bit slower than zooming out                       
                }
            }
            else
            {
                AtGScreen.SetActive(true);
                AtGCam.gameObject.SetActive(true);
                AtGCam.transform.LookAt(EngineControl.AGMTarget, EngineControl.VehicleMainObj.transform.up);

                RaycastHit camhit;
                Physics.Raycast(AtGCam.transform.position, AtGCam.transform.forward, out camhit, Mathf.Infinity, 1);
                if (camhit.point != null)
                {
                    //dolly zoom //Mathf.Atan(40 <--the 40 is the height of the camera frustrum at the target distance
                    AtGCam.fieldOfView = Mathf.Max(Mathf.Lerp(AtGCam.fieldOfView, 2.0f * Mathf.Atan(60 * 0.5f / Vector3.Distance(gameObject.transform.position, camhit.point)) * Mathf.Rad2Deg, 5 * SmoothDeltaTime), 0.3f);
                }
            }
        }
        else if (EngineControl.RStickSelection == 4)//bomb selected
        {
            AtGScreen.SetActive(true);
            AtGCam.gameObject.SetActive(true);
            AtGCam.fieldOfView = 60;
            AtGCam.transform.localRotation = Quaternion.Euler(110, 0, 0);
        }
        else
        {
            AtGScreen.SetActive(false);
            AtGCam.gameObject.SetActive(false);
        }



        //updating numbers 3~ times a second
        if (check > .3)//update text
        {
            if (EngineControl.Gs > maxGs) { maxGs = EngineControl.Gs; }
            HUDText_G.text = string.Concat(EngineControl.Gs.ToString("F1"), "\n", maxGs.ToString("F1"));
            HUDText_mach.text = ((EngineControl.Speed) / 343f).ToString("F2");
            HUDText_altitude.text = string.Concat((EngineControl.CurrentVel.y * 60 * 3.28084f).ToString("F0"), "\n", ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f).ToString("F0"));
            HUDText_knots.text = ((EngineControl.Speed) * 1.9438445f).ToString("F0");
            HUDText_knotsairspeed.text = ((EngineControl.AirSpeed) * 1.9438445f).ToString("F0");

            if (EngineControl.Speed < 2)
            {
                HUDText_angleofattack.text = System.String.Empty;
            }
            else
            {
                HUDText_angleofattack.text = EngineControl.AngleOfAttack.ToString("F0");
            }
            check = 0;
        }
        check += SmoothDeltaTime;

        if (EngineControl.HasAAM) HUDText_AAM_ammo.text = EngineControl.NumAAM.ToString("F0");
        else HUDText_AAM_ammo.text = string.Empty;
        if (EngineControl.HasAGM) HUDText_AGM_ammo.text = EngineControl.NumAGM.ToString("F0");
        else HUDText_AGM_ammo.text = string.Empty;
        if (EngineControl.HasBomb) HUDText_Bomb_ammo.text = EngineControl.NumBomb.ToString("F0");
        else HUDText_Bomb_ammo.text = string.Empty;

        PlaneAnimator.SetFloat(FUEL_STRING, EngineControl.Fuel * FullFuelDivider);


        //Replacement for leavebuttons below this point
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Oculus_CrossPlatform_Button4"))
        {
            ExitStation();
        }
    }

    [System.NonSerializedAttribute] public int PilotSeat = -1;
    [System.NonSerializedAttribute] public int MySeat = -1;
    [System.NonSerializedAttribute] public int[] SeatedPlayers;
    [System.NonSerializedAttribute] public VRCStation[] VehicleStations;
    [System.NonSerializedAttribute] public int[] InsidePlayers;
    private bool FindSeatsDone = false;
    public void ExitStation()
    {
        VehicleStations[MySeat].ExitStation(EngineControl.localPlayer);
    }
    public void FindSeats()
    {
        if (FindSeatsDone) { return; }
        VehicleStations = (VRC.SDK3.Components.VRCStation[])EngineControl.VehicleMainObj.GetComponentsInChildren(typeof(VRC.SDK3.Components.VRCStation));
        SeatedPlayers = new int[VehicleStations.Length];
        for (int i = 0; i != SeatedPlayers.Length; i++)
        {
            SeatedPlayers[i] = -1;
        }
        FindSeatsDone = true;
    }


    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogWarning("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}