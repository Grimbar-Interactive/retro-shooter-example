using System;
using System.Collections.Generic;
using GI.UnityToolkit.Events;
using GI.UnityToolkit.Variables;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    [Header("Target Movement")]
    [SerializeField] private Vector2 arenaSize;
    [SerializeField] private int minDistanceChange = 3;
    
    [Header("Scoring")]
    [SerializeField] private int defaultScoreValue = 5;
    [SerializeField] private List<ScoreRange> scoreRanges;

    [Header("Audio")]
    [SerializeField] private int highValueThreshold = 50;
    [SerializeField] private AudioEvent highValueHit;
    [SerializeField] private AudioEvent lowValueHit;
    [SerializeField] private AudioSource source;

    [Header("References")]
    [SerializeField] private IntVariable scoreVariable;
    
    public void ScoreHit(RaycastHit hit)
    {
        var hitPoint = hit.point;
        hitPoint.z = 0;
        var position = transform.position;
        position.z = 0;
        var distance = Vector3.Distance(hitPoint, position);

        var addedScore = 0;
        foreach (var range in scoreRanges)
        {
            if (distance > range.Max || distance < range.Min) continue;
            addedScore = range.ScoreValue;
            scoreVariable.SetValue(scoreVariable + range.ScoreValue);
            break;
        }

        if (addedScore == 0)
        {
            addedScore = defaultScoreValue;
            scoreVariable.SetValue(scoreVariable + defaultScoreValue);
        }

        if (addedScore >= highValueThreshold)
        {
            highValueHit.Play(source);
        }
        else
        {
            lowValueHit.Play(source);
        }
        
        SetToRandomPosition();
    }

    [UsedImplicitly]
    public void ResetPosition()
    {
        transform.position = Vector3.zero;
    }

    private void SetToRandomPosition()
    {
        Vector3 newPosition;
        do
        {
            newPosition = new Vector3(Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f),
                Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f), 0f);
        } while (Vector3.Distance(transform.position, newPosition) < minDistanceChange);
        transform.position = newPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, arenaSize);

        float rotation = 10;
        foreach (var range in scoreRanges)
        {
            Gizmos.color = range.DebugColor;
            var direction = (Quaternion.Euler(0f, 0f, rotation) * Vector3.up).normalized;
            Gizmos.DrawLine(direction * range.Min, direction * range.Max);
            rotation += 10f;
        }
    }

    [Serializable]
    private class ScoreRange
    {
        [field: SerializeField] public float Min { get; private set; } = 0;
        [field: SerializeField] public float Max { get; private set; } = 1;
        [field: SerializeField] public int ScoreValue { get; private set; } = 1;
        [field: SerializeField] public Color DebugColor { get; private set; } = Color.white;
    }
}
