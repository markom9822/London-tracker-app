using UnityEngine;

/// <summary>
/// Class to handle resizing and positioning of a <see cref="BoxCollider"/> component to match the rect size and center of the <see cref="RectTransform"/> component.
/// Used to make sure that <see cref="BoxCollider"/> components needed for input are always sized and positioned correctly.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider))]
public class BoxColliderResizer : MonoBehaviour
{
   private RectTransform m_RectTransform;
   private BoxCollider m_BoxCollider;

   [SerializeField, Tooltip("The padding to apply to the X size of the collider.")]
   private float m_XPadding;
       
   [SerializeField, Tooltip("The padding to apply to the Y size of the collider.")]
   private float m_YPadding;

   [SerializeField, Tooltip("The offset of the box collider X and Y center values from the center of the rect transform.")]
   private Vector2 m_CenterOffset;

   private const float THRESHOLD = 0.1f;

   private void Start()
   {
      m_RectTransform = GetComponent<RectTransform>();
      m_BoxCollider = GetComponent<BoxCollider>();

      UpdateBoxColliderSize();
   }

   private void Update()
   {
      //Call in update to allow for dynamic resizing of UI elements.
      UpdateBoxColliderSize();
      UpdateBoxColliderCenter();
   }

   private void UpdateBoxColliderSize()
   {
      Vector2 rectTransformSize = m_RectTransform.rect.size;
      Vector3 boxColliderSize = m_BoxCollider.size;

      // There seemed to be an issue where some of the colliders had a negative Z size which was causing issues.
      // So make sure Z size is not negative. [JB, 02 Jun 2023]
      if (boxColliderSize.z < 0.0f)
      {
         boxColliderSize.z = 0.0f;
      }
          
      if (Mathf.Abs(rectTransformSize.x + m_XPadding - boxColliderSize.x) > THRESHOLD)
      {
         boxColliderSize.x = rectTransformSize.x + m_XPadding;
         m_BoxCollider.size = boxColliderSize;
      }
      if (Mathf.Abs(rectTransformSize.y + m_YPadding - boxColliderSize.y) > THRESHOLD)
      {
         boxColliderSize.y = rectTransformSize.y + m_YPadding;
         m_BoxCollider.size = boxColliderSize;
      }
   }
       
   private void UpdateBoxColliderCenter()
   {
      Vector2 rectTransformCenter = m_RectTransform.rect.center;
      Vector3 boxColliderCenter = m_BoxCollider.center;
      if (Mathf.Abs(rectTransformCenter.x + m_CenterOffset.x - boxColliderCenter.x) > THRESHOLD)
      {
         boxColliderCenter.x = rectTransformCenter.x + m_CenterOffset.x;
         m_BoxCollider.center = boxColliderCenter;
      }
      if (Mathf.Abs(rectTransformCenter.y + m_CenterOffset.y - boxColliderCenter.y) > THRESHOLD)
      {
         boxColliderCenter.y = rectTransformCenter.y + m_CenterOffset.y;
         m_BoxCollider.center = boxColliderCenter;
      }
   }
}