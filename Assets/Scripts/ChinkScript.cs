using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class ChinkScript : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float magnitude;
    [SerializeField] float audioMaxPitch = 1f;
    [SerializeField] float audioMaxVolume = 0.15f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void OnCollisionEnter2D(Collision2D collision)
    {
        magnitude = rb.velocity.magnitude;
        

        if(collision.transform.tag == "Wall")
        {
            audioSource.volume = magnitude / 48f;
            audioSource.pitch = (magnitude / 64f) + 0.7f;

            if (audioSource.volume > audioMaxVolume)
            {
                audioSource.volume = audioMaxVolume;
            }
            if(audioSource.pitch > audioMaxPitch)
            {
                audioSource.pitch = audioMaxPitch;
                float randomOffset = Random.Range(audioSource.pitch - 0.1f, audioSource.pitch + 0.1f);
                audioSource.pitch += randomOffset;
            }

            audioSource.PlayOneShot(audioSource.clip);

        }
    }
}
