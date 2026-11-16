using UnityEngine;

public static class Sfx
{
    // Sonido normal (interfaz, voz del prota, victoria, derrota...)
    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        var go = new GameObject("Sfx_" + clip.name);
        Object.DontDestroyOnLoad(go);   // no se corta al cambiar de escena

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.spatialBlend = 0f;          // 2D
        src.playOnAwake = false;
        src.Play();

        Object.Destroy(go, clip.length / Mathf.Max(0.1f, Mathf.Abs(pitch)) + 0.1f);
    }

    // Sonido que baja de volumen con la distancia a la cámara (enemigos y jefe)
    public static void PlayNear(AudioClip clip, Vector3 worldPos, float hearRange,
        float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        var cam = Camera.main;
        if (cam == null) { Play(clip, volume, pitch); return; }

        float d = Vector2.Distance(cam.transform.position, worldPos);
        float factor = 1f - Mathf.Clamp01(d / hearRange);
        if (factor <= 0.01f) return;

        Play(clip, volume * factor, pitch);
    }
}