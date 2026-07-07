using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    CharacterController controller;

    [SerializeField]
    float movementSpeed = 4.0f,
          rotationSpeed = 360.0f,
          stepLength    = 2.0f;

    [SerializeField]
    Transform sensor;

#if DEBUG
    [SerializeField, Range(0f, 1f)]
    float sphereCastRadius = 0.2f;

    [SerializeField, Range(0f, 2f)]
    float sphereCastDistance = 1.0f;
#endif

    [SerializeField]
    LayerMask mask;

    [SerializeField]
    Animator animator;

    // anim = animation
    const string anim_openAnim = "Open_Anim",
                 anim_walkAnim = "Walk_Anim";

    [Header("Audio")]
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip[] audioClips;

    // ac = audio clip
    const string ac_prepareToShoot = "prepare_to_shoot",
                 ac_playerReload   = "player_reload",
                 ac_playerShoot    = "player_shoot";


    Vector2 initialPosition;  
    Quaternion initialRotation;

    bool ValidateAction() => Physics.SphereCast(sensor.position,
                                                sphereCastRadius,
                                                sensor.forward,
                                                out RaycastHit _,
                                                sphereCastDistance,
                                                mask);


    public void StartNewGame(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        animator.SetBool(anim_openAnim, true);
    }

    void PlaySoundClip(string clipName)
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].name == clipName)
            {
                audioSource.PlayOneShot(audioClips[i]);
                break;
            }
        }
    }

    public IEnumerator Move(Vector3 direction)
    {
        if (!ValidateAction()) yield break;

        animator.SetBool(anim_walkAnim, true);

        Vector2 movement = transform.position + (transform.forward * stepLength);

        while (Vector3.Distance(transform.position, movement) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                     movement,
                                                     movementSpeed * Time.deltaTime);

            yield return null;
        }

        animator.SetBool(anim_walkAnim, false);
    }

    public void Shoot()
    {
        PlaySoundClip(ac_prepareToShoot);
        

    }
}
