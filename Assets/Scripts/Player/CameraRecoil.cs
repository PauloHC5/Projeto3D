using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;    

    // Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    [SerializeField] private Transform playerMeshRecoil;

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
        // Lerp the target rotation to zero
        // This is where the recoil returns to the original position
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // Slerp the current rotation to the target rotation
        // This is where the recoil happens
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        transform.localRotation = Quaternion.Euler(currentRotation); // apply the rotation to the camera
        
        if(playerMeshRecoil) playerMeshRecoil.localRotation = Quaternion.Euler(currentRotation); // apply the rotation to the player mesh
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        // Apply recoil to the target rotation
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
