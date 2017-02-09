using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundfxUI : MonoBehaviour
{
[SerializeField] public AudioClip impact;
[SerializeField] AudioSource audio;

    void PlaySoundfx()
    {
        audio = GetComponent<AudioSource>();
        audio.PlayOneShot(impact, 0.7F);

    }
}