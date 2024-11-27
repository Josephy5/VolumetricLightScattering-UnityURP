using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//this is the render feature script for the godrays
//usually we would have seperate scripts for each class to make it easier to debug
//but for this effect, due to how complex it is, it is better to have it in one script to have
//easier acess to some of the variables
public class GodrayFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public Shader shader;

        [Range(-1f, 1f)]
        public float overall_scattering = 0.25f;

        [Range(0.01f, 3f)]
        public float min_scattering = 0.8f;

        [Range(0.01f, 3f)]
        public float max_scattering = 3f;

        [Range(0.2f, 10.0f)]
        public float intensity = 4.5f;

        [Range(5, 100)]
        public int steps = 35;

        [Range(1, 10)]
        public int maxDistance = 5;

        public enum DownSample
        {
            off = 1,
            half = 2,
            quarter = 4
        }
        public DownSample downsampling = DownSample.quarter;

        public enum PhaseFunction
        {
            HenyeyGreenstein,
            Schlick
        }
        public PhaseFunction phaseFunction = PhaseFunction.HenyeyGreenstein;

        [Serializable]
        public class GaussianBlur
        {
            [Range(1, 10)]
            public float amount;

            [Range(3, 7)]
            public int samples;
        }
        public GaussianBlur gaussianBlur = new GaussianBlur { amount = 2.5f, samples = 6 };

        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();

    class GodrayPass : ScriptableRenderPass
    {
        public Settings settings;
        private RenderTexture tempTexture1;
        private RenderTexture tempTexture2;

        private string profilerTag;
        private Material godrayMaterial;
        private GodrayVolume GodrayVolume;

        public GodrayPass(string profilerTag, Shader godRayShader)
        {
            this.profilerTag = profilerTag;
            if (godRayShader == null)
            {
                Debug.LogError("No Shader");
                return;
            }
            godrayMaterial = CoreUtils.CreateEngineMaterial(godRayShader);
        }

        private DownSample GetDownSample(float x)
        {
            switch (x)
            {
                default: return DownSample.off;
                case 1: return DownSample.off;
                case 2: return DownSample.half;
                case 4: return DownSample.quarter;
            }
        }
        private PhaseFunction GetPhaseFunction(float x)
        {
            switch (x)
            {
                default: return PhaseFunction.HenyeyGreenstein;
                case 0: return PhaseFunction.HenyeyGreenstein;
                case 1: return PhaseFunction.Schlick;
            }
        }

        //configure the render textures for our effect
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB64;
            cameraTextureDescriptor.msaaSamples = 1;
            cameraTextureDescriptor.width /= (int)settings.downsampling;
            cameraTextureDescriptor.height /= (int)settings.downsampling;

            if (tempTexture1 == null || tempTexture1.width != cameraTextureDescriptor.width || tempTexture1.height != cameraTextureDescriptor.height)
            {
                if (tempTexture1 != null)
                {
                    tempTexture1.Release();
                }
                if (tempTexture2 != null)
                {
                    tempTexture2.Release();
                }

                tempTexture1 = RenderTexture.GetTemporary(cameraTextureDescriptor);
                tempTexture2 = RenderTexture.GetTemporary(cameraTextureDescriptor);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            VolumeStack stack = VolumeManager.instance.stack;
            GodrayVolume = stack.GetComponent<GodrayVolume>();

            if (GodrayVolume.IsActive() == false) 
            {
                return;
            }
            else
            {
                float[] hold = GodrayVolume.getValues();

                settings.overall_scattering = hold[0];
                settings.min_scattering = hold[1];
                settings.max_scattering = hold[2];
                settings.intensity = hold[3];
                settings.steps = (int) hold[4];
                settings.maxDistance = (int) hold[5];
                settings.gaussianBlur.amount = hold[8];
                settings.gaussianBlur.samples = (int) hold[9];
                settings.downsampling = (Settings.DownSample) GetDownSample(hold[6]);
                settings.phaseFunction = (Settings.PhaseFunction) GetPhaseFunction(hold[7]);
            }

            godrayMaterial.SetFloat("_Scattering", settings.overall_scattering);
            godrayMaterial.SetFloat("_SigmaS", settings.min_scattering);
            godrayMaterial.SetFloat("_SigmaT", settings.max_scattering);
            godrayMaterial.SetFloat("_Intensity", settings.intensity);
            godrayMaterial.SetFloat("_Steps", settings.steps);
            godrayMaterial.SetFloat("_MaxDistance", settings.maxDistance);
            godrayMaterial.SetFloat("_Jitter", 0.5f);
            godrayMaterial.SetFloat("_GaussAmount", settings.gaussianBlur.amount);
            godrayMaterial.SetInt("_GaussSamples", settings.gaussianBlur.samples);

            if (settings.phaseFunction == Settings.PhaseFunction.HenyeyGreenstein)
            {
                godrayMaterial.EnableKeyword("_HENYEY_GREENSTEIN");
                godrayMaterial.DisableKeyword("_SCHLICK");
            }
            else
            {
                godrayMaterial.EnableKeyword("_SCHLICK");
                godrayMaterial.DisableKeyword("_HENYEY_GREENSTEIN");
            }

            var src = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            // Raymarch
            cmd.Blit(src, tempTexture1, godrayMaterial, 0);
            // Gaussian Blur X
            cmd.Blit(tempTexture1, tempTexture2, godrayMaterial, 1);

            godrayMaterial.SetInt("_BlendSrc", (int)BlendMode.One);
            godrayMaterial.SetInt("_BlendDst", (int)BlendMode.One);
            
            // Gaussian Blur Y
            cmd.Blit(tempTexture2, src, godrayMaterial, 2);

            context.ExecuteCommandBuffer(cmd);

            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            if (tempTexture1 != null)
            {
                tempTexture1.Release();
            }
            if (tempTexture2 != null)
            {
                tempTexture2.Release();
            }
        }
    }

    GodrayPass pass;

    public override void Create()
    {
        pass = new GodrayPass("Godrays", settings.shader);
        name = "Godrays";
        pass.settings = settings;
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
    private void OnEnable()
    {
        settings.shader = Shader.Find("Hidden/Godray");
    }
    public void OnDisable()
    {
        pass.Dispose();
    }

    public void OnDestroy()
    {
        pass.Dispose();
    }

    public void OnValidate()
    {
        //if min scattering is above, set the max scattering to the same values as min scattering to avoid bugs
        if (settings.max_scattering < settings.min_scattering)
        {
            settings.max_scattering = settings.min_scattering;
        }
    }
}
