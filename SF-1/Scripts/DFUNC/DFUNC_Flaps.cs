﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DFUNC_Flaps : UdonSharpBehaviour
{
    [SerializeField] private bool UseLeftTrigger;
    [SerializeField] EngineController EngineControl;
    [SerializeField] private GameObject Dial_Funcon;
    [SerializeField] private bool DefaultFlapsOff = false;
    [SerializeField] private float FlapsDragMulti = 1.4f;
    [SerializeField] private float FlapsLiftMulti = 1.35f;
    [SerializeField] private float FlapsMaxLiftMulti = 1;
    private bool Flaps = true;
    private bool Dial_FunconNULL = true;
    private bool TriggerLastFrame;
    private EffectsController EffectsControl;
    private float StartMaxLift;
    private int FLAPS_STRING = Animator.StringToHash("flaps");
    private Animator VehicleAnimator;
    private bool DragApplied;
    private bool LiftApplied;
    private bool MaxLiftApplied;
    private void Update()
    {
        float Trigger;
        if (UseLeftTrigger)
        { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger"); }
        else
        { Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger"); }
        if (Trigger > 0.75)
        {
            if (!TriggerLastFrame) ToggleFlaps();
            TriggerLastFrame = true;
        }
        else { TriggerLastFrame = false; }

        if (Flaps)
        {
            if (EngineControl.PitchDown)//flaps on, but plane's angle of attack is negative so they have no helpful effect
            {
                if (DragApplied) { EngineControl.ExtraDrag -= FlapsDragMulti; DragApplied = false; }
                if (LiftApplied) { EngineControl.ExtraLift -= FlapsLiftMulti; LiftApplied = false; }
                if (MaxLiftApplied) { EngineControl.MaxLift = StartMaxLift; MaxLiftApplied = false; }
            }
            else//flaps on positive angle of attack, flaps are useful
            {
                if (!DragApplied) { EngineControl.ExtraDrag += FlapsDragMulti; DragApplied = true; }
                if (!LiftApplied) { EngineControl.ExtraLift += FlapsLiftMulti; LiftApplied = true; }
                if (!MaxLiftApplied) { EngineControl.MaxLift *= FlapsMaxLiftMulti; MaxLiftApplied = true; }
            }
        }
    }
    public void DFUNC_Selected()
    {
        gameObject.SetActive(true);
    }
    public void DFUNC_Deselected()
    {
        gameObject.SetActive(false);
        TriggerLastFrame = false;
    }
    public void SFEXT_L_ECStart()
    {
        FlapsDragMulti -= 1f;
        FlapsLiftMulti -= 1f;
        StartMaxLift = EngineControl.MaxLift;
        EffectsControl = EngineControl.EffectsControl;
        Dial_FunconNULL = Dial_Funcon == null;
        VehicleAnimator = EngineControl.VehicleMainObj.GetComponent<Animator>();
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(Flaps);
        if (DefaultFlapsOff) { SetFlapsOff(); }
        else { SetFlapsOn(); }
    }
    public void SFEXT_O_PilotEnter()
    {
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(Flaps);
    }
    public void SFEXT_O_PilotExit()
    {
        gameObject.SetActive(false);
        TriggerLastFrame = false;
    }
    public void SFEXT_O_PassengerEnter()
    {
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(Flaps);
    }
    public void SFEXT_G_Explode()
    {
        if (DefaultFlapsOff)
        { SetFlapsOff(); }
        else
        { SetFlapsOn(); }
    }
    public void SFEXT_O_ReAppear()
    {
        if (DefaultFlapsOff)
        { SetFlapsOff(); }
        else
        { SetFlapsOn(); }
    }
    public void SFEXT_O_RespawnButton()
    {
        if (DefaultFlapsOff)
        { SetFlapsOff(); }
        else
        { SetFlapsOn(); }
    }
    public void KeyboardInput()
    {
        ToggleFlaps();
    }
    public void SetFlapsOff()
    {
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(false);
        Flaps = false;
        VehicleAnimator.SetBool(FLAPS_STRING, false);

        if (DragApplied) { EngineControl.ExtraDrag -= FlapsDragMulti; DragApplied = false; }
        if (LiftApplied) { EngineControl.ExtraLift -= FlapsLiftMulti; LiftApplied = false; }
        if (MaxLiftApplied) { EngineControl.MaxLift = StartMaxLift; MaxLiftApplied = false; }

        if (EngineControl.IsOwner)
        {
            EngineControl.SendEventToExtensions("SFEXT_O_FlapsOff", false);
        }
    }
    public void SetFlapsOn()
    {
        Flaps = true;
        VehicleAnimator.SetBool(FLAPS_STRING, true);
        if (!Dial_FunconNULL) Dial_Funcon.SetActive(true);

        if (!DragApplied) { EngineControl.ExtraDrag += FlapsDragMulti; DragApplied = true; }
        if (!LiftApplied) { EngineControl.ExtraLift += FlapsLiftMulti; LiftApplied = true; }
        if (!MaxLiftApplied) { EngineControl.MaxLift *= FlapsMaxLiftMulti; MaxLiftApplied = true; }

        if (EngineControl.IsOwner)
        {
            EngineControl.SendEventToExtensions("SFEXT_O_FlapsOn", false);
        }
    }

    public void ToggleFlaps()
    {
        if (!Flaps)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetFlapsOn");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetFlapsOff");
        }
    }
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Flaps && !DefaultFlapsOff)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetFlapsOff");
        }

        else if (Flaps && DefaultFlapsOff)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetFlapsOn");
        }
    }
}
