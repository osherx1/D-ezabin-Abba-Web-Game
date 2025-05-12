using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class VideoEndLoadScene : MonoBehaviour
{
    private VideoPlayer _vp;

    void Awake()
    {
        _vp = GetComponent<VideoPlayer>();
        // subscribe to the “playback completed” event
        _vp.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // load the scene at index 1
        SceneManager.LoadScene(0);
    }
}