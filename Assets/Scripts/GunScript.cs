using UnityEngine;

public class GunScript : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    private float bulletLifetime = 3f;

    [SerializeField] private AudioClip bulletSoundClip;

    // void Update()
    // {
    //     // if (Input.GetKeyDown(KeyCode.Space))
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    //         bullet.GetComponent<Rigidbody>().linearVelocity = -bulletSpawnPoint.forward * bulletSpeed;

    //         // play bullet sound
    //         SoundFXManager.instance.PlaySoundFXClip(bulletSoundClip, transform);

    //         Destroy(bullet, bulletLifetime); // Destroy after 3 seconds
    //     }
    // }

    public void Shoot(Vector3 targetDirection)
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = -targetDirection * bulletSpeed;

        SoundFXManager.instance.PlaySoundFXClip(bulletSoundClip, transform);
        Destroy(bullet, bulletLifetime);
    }
}