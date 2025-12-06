using UnityEngine;
using System.Collections;

public class Soldier : MonoBehaviour
{
    // Public variables configurable in the Inspector
    public float speed = 4.0f;
    public float stopDistance = 0.1f;
    public float rotationSpeed = 8.0f;
    
    // Health system variables
    public int maxHealth = 10;
    public int currentHealth;
    public bool isDead = false;
    public float healthDropInterval = 1.0f;
    
    // Private variables for tracking state and components
    private Vector3 targetPosition;
    private Vector3 shootDirection;
    private Animator animator;
    private bool isShooting = false;
    private Coroutine healthDropCoroutine;

    [SerializeField] private HealthbarScript healthbar;

    [SerializeField] private GunScript gun;
    [SerializeField] private AudioClip[] deathSoundClips;
    [SerializeField] private AudioClip[] movementStartCommandSoundClips;
    [SerializeField] private AudioClip[] movementEndCommandSoundClips;

    private bool hasPlayedMovementStartSound = false;
    private bool hasPlayedMovementEndSound = false;
    private bool hasPlayedDeathSound = false;


    // Animation parameter names - using triggers for more reliable state changes
    private const string MOVEMENT_PARAM_NAME = "isRunning";
    private const string FIRING_PARAM_NAME = "isFiring";
    private const string DEAD_PARAM_NAME = "isDead";

    void Start()
    {
        gun = GetComponentInChildren<GunScript>();
        if (gun == null)
        {
            Debug.LogError("GunScript not found on soldier!");
        }

        healthbar.UpdateHealthBar(maxHealth, currentHealth);

        animator = GetComponent<Animator>();
        targetPosition = transform.position;
        shootDirection = transform.forward;
        
        // Initialize health
        currentHealth = maxHealth;
        isDead = false;

        if (animator != null)
        {
            animator.SetBool(MOVEMENT_PARAM_NAME, false); 
            animator.SetBool(FIRING_PARAM_NAME, false);
            animator.SetBool(DEAD_PARAM_NAME, false);
        }
        
        // Start the health drop coroutine
        healthDropCoroutine = StartCoroutine(HealthDropRoutine());
        
        Debug.Log($"Soldier spawned with {currentHealth} health");
    }

    void Update()
    {
        // If dead, ensure death state is maintained and exit early
        if (isDead)
        {
            MaintainDeathState();
            return;
        }

        // 1. Check for RIGHT Mouse Click Input for movement
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                targetPosition = hit.point;
                isShooting = false;
                if (animator != null)
                {
                    animator.SetBool(FIRING_PARAM_NAME, false);

                    if (!hasPlayedMovementStartSound)
                    {
                        SoundFXManager.instance.PlayRandomSoundFXClip(movementStartCommandSoundClips, transform);
                        hasPlayedMovementStartSound = true;
                        hasPlayedMovementEndSound = false;
                    }
                }
            }
        }

        // 2. Check for LEFT Mouse Click Input for firing
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // if (currentHealth <= 0)
            // {
            //     Die();
            // }

            if (Physics.Raycast(ray, out hit))
            {
                StartShooting(hit.point);
                // Vector3 shootTarget = hit.point;
                // shootTarget.y = transform.position.y;
                // shootDirection = (shootTarget - transform.position).normalized;
                
                // isShooting = true;
                
                // if (animator != null)
                // {
                //     animator.SetBool(MOVEMENT_PARAM_NAME, false);
                //     animator.SetBool(FIRING_PARAM_NAME, true);
                // }

                // gun.Shoot(hit.point);
            }
        }

        // Stop firing when left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            StopShooting();
            // isShooting = false;
            // if (animator != null && !isDead)
            // {
            //     animator.SetBool(FIRING_PARAM_NAME, false);
            // }
        }

        // Handle actions if alive
        if (!isDead)
        {
            if (isShooting)
            {
                targetPosition = transform.position;
                RotateTowardsDirection(shootDirection);
            }
            else
            {
                HandleMovement();
            }
        }
    }

    // Ensures death state is maintained and overrides other animations
    private void MaintainDeathState()
    {
        // Force stop all movement and actions
        targetPosition = transform.position;
        isShooting = false;
        
        // Use both bool and trigger to ensure death animation plays
        if (animator != null)
        {
            // Immediately stop all other animations
            animator.SetBool(MOVEMENT_PARAM_NAME, false);
            animator.SetBool(FIRING_PARAM_NAME, false);
            
            // Set death state
            animator.SetBool(DEAD_PARAM_NAME, true);
        }
    }

    // Coroutine to drop health every second
    private IEnumerator HealthDropRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(healthDropInterval);
            
            if (!isDead)
            {
                TakeDamage(1);
            }
        }
    }

    // Public method to take damage
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Debug.Log($"Soldier took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");
        
        healthbar.UpdateHealthBar(maxHealth, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to handle death - more forceful approach
    private void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        isShooting = false;
        
        // Stop all coroutines immediately
        if (healthDropCoroutine != null)
        {
            StopCoroutine(healthDropCoroutine);
        }
        
        // Force immediate state change
        MaintainDeathState();
        
        // Optional: Disable collider or other components
        // GetComponent<Collider>().enabled = false;
        
        Debug.Log("Soldier has died!");

        if (animator != null)
        {
            if(!hasPlayedDeathSound)
            {
                SoundFXManager.instance.PlayRandomSoundFXClip(deathSoundClips, transform);
                hasPlayedDeathSound = true;
            }
            animator.SetBool(MOVEMENT_PARAM_NAME, false);
            animator.SetBool(FIRING_PARAM_NAME, false);
            animator.SetBool(DEAD_PARAM_NAME, false);
        }
        
        // Optional: Start coroutine to disable script after death animation
        StartCoroutine(DisableAfterDeath());
    }

    // Coroutine to completely disable the script after death animation has time to play
    private IEnumerator DisableAfterDeath()
    {
        // Wait for death animation to start playing
        yield return new WaitForSeconds(0.5f);
        
        // Completely disable this script to prevent any further updates
        this.enabled = false;
    }

    // Public method to respawn the soldier
    public void Respawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        
        // Reset state
        targetPosition = transform.position;
        isShooting = false;
        
        // Reset animations - use both reset and set methods
        if (animator != null)
        {
            animator.SetBool(MOVEMENT_PARAM_NAME, false);
            animator.SetBool(FIRING_PARAM_NAME, false);
            animator.SetBool(DEAD_PARAM_NAME, false);
        }
        
        // Restart health drop coroutine
        healthDropCoroutine = StartCoroutine(HealthDropRoutine());
        
        // Optional: Re-enable collider
        // GetComponent<Collider>().enabled = true;
        
        Debug.Log($"Soldier respawned with {currentHealth} health");
    }

    // Public method to get health percentage
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Public method to check if soldier is alive
    public bool IsAlive()
    {
        return !isDead;
    }

    // Separate function to handle rotation
    private void RotateTowardsDirection(Vector3 direction)
    {
        if (direction != Vector3.zero && !isDead)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    // Separate function to handle movement logic
    private void HandleMovement()
    {
        if (isDead) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > stopDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,     
                targetPosition,         
                Time.deltaTime * speed  
            );
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
            
            RotateTowardsDirection(direction);
            
            if (animator != null)
            {
                animator.SetBool(MOVEMENT_PARAM_NAME, true);
            }
        }
        else
        {
            if (animator != null)
            {
                if (!hasPlayedMovementEndSound)
                {
                    SoundFXManager.instance.PlayRandomSoundFXClip(movementEndCommandSoundClips, transform);
                    hasPlayedMovementEndSound = true;
                }
                animator.SetBool(MOVEMENT_PARAM_NAME, false);
                hasPlayedMovementStartSound = false;
            }
        }
    }

    private void StartShooting(Vector3 targetPoint)
    {
        if (isDead) return;
        
        Vector3 shootTarget = targetPoint;
        shootTarget.y = transform.position.y;
        shootDirection = (shootTarget - transform.position).normalized;
        
        isShooting = true;
        
        // Play shooting animation
        if (animator != null)
        {
            animator.SetBool(MOVEMENT_PARAM_NAME, false);
            animator.SetBool(FIRING_PARAM_NAME, true);
        }
        
        // Actually shoot the gun
        StartCoroutine(DelayedShoot(shootDirection));
    }

    private IEnumerator DelayedShoot(Vector3 direction)
    {
        // Wait for the aiming/animation to start
        yield return new WaitForSeconds(0.3f); // Adjust this delay as needed
        
        // Actually shoot the gun after delay
        if (gun != null && !isDead)
        {
            gun.Shoot(-direction);
        }
    }

    // New method to stop shooting
    private void StopShooting()
    {
        isShooting = false;
        if (animator != null && !isDead)
        {
            animator.SetBool(FIRING_PARAM_NAME, false);
        }
    }

    public void SetTargetPosition(Vector3 newPosition)
    {
        if (isDead) return;
        
        targetPosition = newPosition;
        isShooting = false;
        
        // Reset sound flags for new movement
        hasPlayedMovementStartSound = false;
        hasPlayedMovementEndSound = false;
        
        // Play movement start sound
        if (animator != null)
        {
            animator.SetBool(FIRING_PARAM_NAME, false);
            
            if (!hasPlayedMovementStartSound)
            {
                SoundFXManager.instance.PlayRandomSoundFXClip(movementStartCommandSoundClips, transform);
                hasPlayedMovementStartSound = true;
            }
        }
        
        Debug.Log($"Soldier moving to: {newPosition}");
    }
}