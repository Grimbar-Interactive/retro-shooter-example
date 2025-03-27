using GI.UnityToolkit.Utilities;
using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask shootableLayers;
    [SerializeField] private UnityEvent onExit;
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            onExit.Invoke();
            return;
        }
        
        if (Input.GetMouseButtonDown(0) == false) return;
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, shootableLayers)) return;

        this.Log($"Was shot: {hit.transform.name}");
        
        var target = hit.transform.GetComponent<Target>();
        if (target == null) return;
        target.ScoreHit(hit);
    }
}
