using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioSource _source;

    [SerializeField]
    private AudioClip _break, _dent, _miss, _shoot, _explode;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Break()
    {
        Instance._source.PlayOneShot(Instance._break);
    }
    public static void Dent()
    {
        Instance._source.PlayOneShot(Instance._dent);
    }
    public static void Miss()
    {
        Instance._source.PlayOneShot(Instance._miss);
    }
    public static void Shoot()
    {
        Instance._source.PlayOneShot(Instance._shoot);
    }
    public static void Explode()
    {
        Instance._source.PlayOneShot(Instance._explode);
    }
}
