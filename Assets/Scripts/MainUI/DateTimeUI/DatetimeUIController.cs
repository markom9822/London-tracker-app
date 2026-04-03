using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Updates the UI text with a tactical, military-style timestamp.
/// Format: 31 MAR 26 19:16:02
/// </summary>
public class DatetimeUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_DatetimeText;

    private void Update()
    {
        if (m_DatetimeText != null)
        {
            UpdateTimestamp();
        }
    }

    private void UpdateTimestamp()
    {
        DateTime now = DateTime.Now;
        string tacticalTime = now.ToString("dd MMM yy HH:mm:ss").ToUpper();
        m_DatetimeText.text = tacticalTime;
    }
}