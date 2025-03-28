using GI.UnityToolkit.Attributes;
using GI.UnityToolkit.State;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(LightgunController))]
public class GunController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask shootableLayers;
    [SerializeField] private UnityEvent onExit;

    [Header("State")]
    [SerializeField] private StateManager gameState;
    [SerializeField] private State playingState;

    [Header("Ammo")]
    [SerializeField] private bool keepAmmoOnHit = true;
    [SerializeField] private IntVariable ammoVariable;
    
    [ShowNativeProperty, UsedImplicitly] public int Ammo => ammoVariable.Value;
    [ShowNonSerializedField, UsedImplicitly] private Vector2 _aimPosition;
    [ShowNonSerializedField, UsedImplicitly] private bool _firePressed;
    
    private PlayerInput _input;
    private LightgunController _lightgun;
    private InputAction _aim;
    private InputAction _fire;
    private InputAction _reload;
    private InputAction _exit;

    private bool CanFire => ammoVariable > 0 || gameState.CurrentState != playingState;
    private bool CanReload => ammoVariable == 0 && gameState.CurrentState == playingState;
    
    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _aim = _input.actions.FindAction("Aim");
        _fire = _input.actions.FindAction("Fire");
        _reload = _input.actions.FindAction("Reload");
        _exit = _input.actions.FindAction("Exit");

        _lightgun = GetComponent<LightgunController>();
        
        ammoVariable.Default();
    }

    private void Update()
    {
        if (_exit.WasPressedThisFrame())
        {
            onExit?.Invoke();
            return;
        }
        
        _aimPosition = _aim.ReadValue<Vector2>();
        _firePressed = _fire.IsPressed();

        if (_reload.WasPressedThisFrame() && CanReload)
        {
            ammoVariable.Default();
            _lightgun.Reload();
            return;
        }

        if (_fire.WasPressedThisFrame() && CanFire) Fire();
    }

    private void Fire()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, shootableLayers))
        {
            if (gameState.CurrentState == playingState) ammoVariable.Decrement();
            _lightgun.Fire(ammoVariable);
            return;
        }

        var target = hit.transform.GetComponent<Target>();
        if (target == null)
        {
            if (gameState.CurrentState == playingState) ammoVariable.Decrement();
            _lightgun.Fire(ammoVariable);
            return;
        }
        
        if (gameState.CurrentState == playingState && keepAmmoOnHit == false) ammoVariable.Decrement();
        
        target.ScoreHit(hit);
        _lightgun.Fire(ammoVariable);
    }
}
