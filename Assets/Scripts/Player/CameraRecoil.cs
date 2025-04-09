using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;    

    // Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    private PlayerCharacterController playerCharacterController;

    private void Awake()
    {
        playerCharacterController = GetComponentInParent<PlayerCharacterController>();
        if (playerCharacterController == null)
        {
            Debug.LogError("CameraRecoil: PlayerCharacterController not found in parent.");
        }
    }

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
