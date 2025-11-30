using UnityEngine;
using UnityEngine.InputSystem;

public class ExplosionTester : MonoBehaviour
{
    public LayerMask destrubleLayer;
    InputAction clickAction;
    public float explosionForce = 500f;

    void Start()
    {
        clickAction = InputSystem.actions.FindAction("Click");
        clickAction.performed += ctx => OnClick();
        clickAction.Enable();
    }

    void OnClick()
    {
        Debug.Log("Explosion Tester running click");
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction, Color.orange, 10f);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, destrubleLayer))
        {
            Debug.Log($"Explosion Tester Hit: {hitInfo.collider.gameObject.name}");
            Vector3 explosionPosition = hitInfo.point;
            var parent = hitInfo.collider.gameObject.GetComponentInParent<DestructibleObject>();
            if (parent != null)
            {
                parent.Explode(explosionPosition, explosionForce);
            }
        }
    }

    void Update()
    {
        
    }
}
