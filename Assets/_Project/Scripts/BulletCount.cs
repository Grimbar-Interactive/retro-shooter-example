using System.Collections.Generic;
using GI.UnityToolkit.Variables;
using UnityEngine;

public class BulletCount : MonoBehaviour
{
    [SerializeField] private IntVariable ammoCount;
    [SerializeField] private List<MeshRenderer> renderers;

    private void OnEnable()
    {
        ammoCount.AddListener(OnAmmoCountUpdated);
    }

    private void OnDisable()
    {
        ammoCount.RemoveListener(OnAmmoCountUpdated);
    }

    private void OnAmmoCountUpdated()
    {
        for (var i = 0; i < renderers.Count; i++)
        {
            renderers[i].enabled = i < ammoCount;
        }
    }
}
