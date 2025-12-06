using UnityEngine;

public class Ground : Interactive
{
    [SerializeField] private Soldier soldier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (soldier == null)
            {
                soldier = FindFirstObjectByType<Soldier>();
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public new void Interact()
    {
        // Get the hit position from CameraInteract
        RaycastHit hit = CameraInteract.GetLatestHit();
        
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            Vector3 gazePosition = hit.point;
            Debug.Log($"User is looking at ground position: X={gazePosition.x:F2}, Y={gazePosition.y:F2}, Z={gazePosition.z:F2}");

            // Move soldier to this position
            if (soldier != null && soldier.IsAlive())
            {
                MoveSoldierToPosition(gazePosition);
            }
        }
    }

     private void MoveSoldierToPosition(Vector3 position)
    {
        // You'll need to add a public method to your MoveToMouseClick script
        soldier.SetTargetPosition(position);
    }
}
