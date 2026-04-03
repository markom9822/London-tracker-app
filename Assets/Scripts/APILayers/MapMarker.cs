using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class MapMarker : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected MeshRenderer m_Renderer;
    [SerializeField] protected Color m_HoverColor = Color.red;
    
    protected Color m_OriginalColor;
    protected bool m_IsHovered = false;

    protected virtual void Awake()
    {
        if (m_Renderer == null) m_Renderer = GetComponentInChildren<MeshRenderer>();
        if (m_Renderer != null) m_OriginalColor = m_Renderer.material.color;
    }

    public virtual void OnHoverEnter()
    {
        m_IsHovered = true;
        if (m_Renderer != null) m_Renderer.material.color = m_HoverColor;
    }

    public virtual void OnHoverExit()
    {
        m_IsHovered = false;
        if (m_Renderer != null) m_Renderer.material.color = m_OriginalColor;
    }

    // Abstract forces subclasses to define what "clicking" does
    public abstract void OnSelect();

    public virtual void Deactivate()
    {
        OnHoverExit();
        gameObject.SetActive(false);
    }
}