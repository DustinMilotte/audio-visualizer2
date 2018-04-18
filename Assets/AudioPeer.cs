using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class AudioPeer : MonoBehaviour {
    AudioSource _audioSource;
    private float[] _samplesLeft = new float[512];
    private float[] _samplesRight = new float[512];

    private float[] _freqBand = new float[8];
    private float[] _bandBuffer = new float[8];
    private float[] _bufferDecrease = new float[8];
    private float[] _freqBandsHighest = new float[8];

    // audio band 64
    private float[] _freqBand64 = new float[64];
    private float[] _bandBuffer64 = new float[64];
    private float[] _bufferDecrease64 = new float[64];
    private float[] _freqBandsHighest64 = new float[64];

    [HideInInspector]
    public static float[] _audioBand, _audioBandBuffer;
    [HideInInspector]
    public static float[] _audioBand64, _audioBandBuffer64;

    [HideInInspector]
    public float _Amplitude, _AmplitudeBuffer; //todo remove static
    public float _audioProfile;
    private float _AmplitudeHighest;

    public enum _channel {stereo, left, right};
    public _channel channel = new _channel();

	// Use this for initialization
	void Start () {
        _audioBand = new float[8];
        _audioBandBuffer = new float[8];
        _audioBand64 = new float[64];
        _audioBandBuffer64 = new float[64];

        _audioSource = GetComponent<AudioSource>();
        AudioProfile(_audioProfile);
	}
	
	// Update is called once per frame
	void Update () {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        //MakeFrequencyBands64();
        BandBuffer();
        //BandBuffer64();
        CreateAudioBands();
        //CreateAudioBands64();
        GetAmplitude();
	}

    void AudioProfile(float audioProfile) {
        for(int i = 0; i < 8; i++) {
            _freqBandsHighest[i] = _audioProfile;
        }
    }

    void GetAmplitude(){
        float _CurrentAmplitude = 0;
        float _CurrentAmplitudeBuffer = 0;
        for (int i = 0; i < 8; i++){
            _CurrentAmplitude += _audioBand[i];
            _CurrentAmplitudeBuffer += _audioBandBuffer[i];
        }
        if(_CurrentAmplitude > _AmplitudeHighest){
            _AmplitudeHighest = _CurrentAmplitude;
        }
        _Amplitude = _CurrentAmplitude / _AmplitudeHighest;
        _AmplitudeBuffer = _CurrentAmplitudeBuffer / _AmplitudeHighest;
    }

    void CreateAudioBands() {
        for(int i = 0; i < 8; i++) {
            if(_freqBand[i] > _freqBandsHighest[i]) {
                _freqBandsHighest[i] = _freqBand[i];
            }
            _audioBand[i] = (_freqBand[i] / _freqBandsHighest[i]);
            _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandsHighest[i]);
               
        }
    }

    void CreateAudioBands64() {
        for(int i = 0; i < 64; i++) {
            if(_freqBand64[i] > _freqBandsHighest64[i]) {
                _freqBandsHighest64[i] = _freqBand64[i];
            }
            _audioBand64[i] = (_freqBand64[i] / _freqBandsHighest64[i]);
            _audioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandsHighest64[i]);

        }
    }

    void GetSpectrumAudioSource() {
        _audioSource.GetSpectrumData(_samplesLeft,0,FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1,FFTWindow.Blackman);
    }

    void MakeFrequencyBands() {
        int count = 0;

        for(int i = 0; i < 8; i++) {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if(sampleCount == 7) {
                sampleCount += 2;
            }
            for(int j = 0; j < sampleCount; j++) {
                if(channel == _channel.stereo) {
                    average += _samplesLeft[count] + _samplesRight[count] * (count * 1);
                    count++;
                }
                if(channel == _channel.left) {
                    average += _samplesLeft[count] * (count * 1);
                    count++;
                }
                if(channel == _channel.right) {
                    average += _samplesRight[count] * (count * 1);
                    count++;
                }

            }
            average /= count;
            _freqBand[i] = average * 10;
        }
    }

    void MakeFrequencyBands64() {
        int count = 0;
        int sampleCount = 1;
        int power = 0;


        for(int i = 0; i < 64; i++) {
            float average = 0;
            if(i == 16 || i == 32 || i == 40 || i == 48 || i == 56) {
                power++;
                sampleCount = (int)Mathf.Pow(2, i) * 2;
                if(power == 3) {
                    sampleCount -= 2;
                }
            }
           
            //if(sampleCount == 7) {
            //    sampleCount += 2;
            //}

            for(int j = 0; j < sampleCount; j++) {
                if(channel == _channel.stereo) {
                    average += _samplesLeft[count] + _samplesRight[count] * (count * 1);
                    count++;
                }
                if(channel == _channel.left) {
                    average += _samplesLeft[count] * (count * 1);
                    count++;
                }
                if(channel == _channel.right) {
                    average += _samplesRight[count] * (count * 1);
                    count++;
                }

            }
            average /= count;
            _freqBand64[i] = average * 80;
        }
    }

    void BandBuffer() {
        for(int g = 0; g < 8; g++) {
            if(_freqBand[g] > _bandBuffer[g]) {
                _bandBuffer[g] = _freqBand[g];
                _bufferDecrease[g] = 0.005f;
            }
            if(_freqBand[g] < _bandBuffer[g]) {
                _bandBuffer[g] -= _bufferDecrease[g];
                _bufferDecrease[g] *= 1.2f;
            }
        }
    }

    void BandBuffer64() {
        for(int g = 0; g < 64; g++) {
            if(_freqBand64[g] > _bandBuffer64[g]) {
                _bandBuffer64[g] = _freqBand64[g];
                _bufferDecrease64[g] = 0.005f;
            }
            if(_freqBand64[g] < _bandBuffer64[g]) {
                _bandBuffer64[g] -= _bufferDecrease64[g];
                _bufferDecrease64[g] *= 1.2f;
            }
        }
    }
}
