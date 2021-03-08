using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeechDetector : MonoBehaviour
{
    private AudioSource _audioSource;
    private float[] _clipSampleData = new float[1024];
    private bool _isSpeaking = false;
    private float _lastSpeechDetectTime;
    private bool _temporarilyStoppedSpeaking = false;

    public double minimumLevel = 0.0005;
    public double timeRequiredForSilence = 0.5;

    public NetworkEvents networkEvents;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = this.gameObject.AddComponent<AudioSource>();

        _audioSource.clip = Microphone.Start(null, true, 3599, 16000);
        while (!(Microphone.GetPosition(null) > 0)) { }
        _audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            NetworkEvent networkEvent = new NetworkEvent();
            networkEvent.EventName = "AvatarStartTalking";
            networkEvent.EventData = "";
            networkEvents.PostEventMessage(networkEvent);
        }
        else if (Input.GetKeyDown("a"))
        {
            NetworkEvent networkEvent = new NetworkEvent();
            networkEvent.EventName = "AvatarStopTalking";
            networkEvent.EventData = "";
            networkEvents.PostEventMessage(networkEvent);
        }

        _audioSource.GetSpectrumData(_clipSampleData, 0, FFTWindow.Rectangular);
        float currentAverageVolume = _clipSampleData.Average();
        bool aboveThreshold = currentAverageVolume >= minimumLevel;

        //Debug.Log("Current level : " + currentAverageVolume);

        if (aboveThreshold)
        {
            _temporarilyStoppedSpeaking = false;

            if (!_isSpeaking)
            {
                _isSpeaking = true;

                NetworkEvent networkEvent = new NetworkEvent();
                networkEvent.EventName = "AvatarStartTalking";
                networkEvent.EventData = "";
                networkEvents.PostEventMessage(networkEvent);
            }
        }
        else // below speech threshold
        {
            if (_isSpeaking)
            {
                if (!_temporarilyStoppedSpeaking)
                {
                    _lastSpeechDetectTime = Time.fixedTime;
                    _temporarilyStoppedSpeaking = true;
                }
                else if (Time.fixedTime - _lastSpeechDetectTime > timeRequiredForSilence)
                {
                    _isSpeaking = false;
                    _temporarilyStoppedSpeaking = false;

                    NetworkEvent networkEvent = new NetworkEvent();
                    networkEvent.EventName = "AvatarStopTalking";
                    networkEvent.EventData = "";
                    networkEvents.PostEventMessage(networkEvent);
                }
            }
        }
    }
}
