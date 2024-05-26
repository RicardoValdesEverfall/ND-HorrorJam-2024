using CrazyMinnow.SALSA;
using FMODUnity;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class SalsaFmodBridge : MonoBehaviour
{
    [SerializeField]
    private Salsa salsa;

    [SerializeField]
    private float averageModifier = 8f;

    private FMOD.DSP dsp;
    private FMOD.ChannelGroup channelGroup;

    private FMOD.Studio.EventInstance sallyInstance;
    public  EventReference sallyEvent;
    public FMOD.Studio.PLAYBACK_STATE sallyState;

    private float timer = 2f;

    private float[] spectrum;

    private void Awake()
    {
        sallyInstance = RuntimeManager.CreateInstance(sallyEvent);

        sallyInstance.setParameterByName("Day 1 to 5", 1);
        sallyInstance.setParameterByName("DialogPath", 1);
        sallyInstance.setParameterByName("Dialog ID", 2);
        sallyInstance.setParameterByName("DialogSeq", 1);

        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out dsp);

        sallyInstance.start();

        // Create a DSP of DSP_TYPE.FFT
        if (FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out dsp) == FMOD.RESULT.OK)
        {
            dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
            dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, 1024 * 2);
            FMODUnity.RuntimeManager.StudioSystem.flushCommands();

            sallyInstance.getChannelGroup(out channelGroup);
            channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, dsp);
        }
    }

    private bool IsSallyFinished()
    {
        if (sallyState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private void Update()
    {
        sallyInstance.getPlaybackState(out sallyState);

        if (!IsSallyFinished())
        {
            IntPtr unmanagedData;
            uint length;

            if (dsp.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out unmanagedData, out length) == FMOD.RESULT.OK)
            {

                FMOD.DSP_PARAMETER_FFT fftData = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(FMOD.DSP_PARAMETER_FFT));
                if (fftData.numchannels > 0)
                {
                    for (int i = 0; i < fftData.numchannels; ++i)
                    {
                        spectrum = new float[fftData.length];
                    }


                    fftData.getSpectrum(0, ref spectrum);
                    salsa.analysisValue = spectrum.Average() * averageModifier;
                }
            }
        }

        else
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                sallyInstance.start();
                timer = 2f;
            }
        }
    }
}