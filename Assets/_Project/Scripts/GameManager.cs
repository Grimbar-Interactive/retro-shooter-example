using GI.UnityToolkit.State;
using GI.UnityToolkit.Variables;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private FloatVariable timeRemaining;
    [SerializeField] private StateManager gameStateManager;
    [SerializeField] private State waitingState;

    private void OnEnable()
    {
        timeRemaining.Default();
    }

    private void Update()
    {
        timeRemaining.SetValue(Mathf.Max(timeRemaining - Time.deltaTime, 0));
        if (timeRemaining > 0) return;
        gameStateManager.SetState(waitingState);
        enabled = false;
    }
}
