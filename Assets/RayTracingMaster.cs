using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    public Vector2 simulationSize;
    public int particleCount;
    public float drawRadius;

    [Header("Debug")]
    public float debugX1;
    public bool resetScene;

    struct Particle
    {
        public Vector2 pos;
        public Vector2 dir;
        public float mass;
        float pressure;
    };
    Particle[] particles;
    ComputeBuffer particleBuffer;

    public ComputeShader particleShader, distGenShader, drawSphereShader, grabPosShader;

    //private RenderTexture distTex;
    private RenderTexture _target;

    private void Awake()
    {


        if (resetScene == false)
        {
            particles = new Particle[(int)(particleCount)];

            for (int i = 0; i < particleCount; i++)
            {
                Particle particle = new Particle();
                particle.pos = new Vector2(Random.Range(0, simulationSize.y) + (simulationSize.x / 4.0f), Random.Range(0, simulationSize.y));
                //particle.dir = ((simulationSize / 2.0f) - particle.pos).normalized;
                particle.mass = Random.value;
                particles[i] = particle;
            }
        }
        else
        {
            particles = new Particle[3];

            particles[0].pos = new Vector2(80, 45);
            particles[0].dir = new Vector2(0f, 0f);
            particles[0].mass = 1;

            particles[1].pos = new Vector2(20, 55);
            particles[1].dir = new Vector2(0.5f, 0);
            particles[1].mass = 0.1f;

            particles[2].pos = new Vector2(140, 30);
            particles[2].dir = new Vector2(-0.5f, 0);
            particles[2].mass = 0.1f;
        }

        particleBuffer = new ComputeBuffer(particles.Length, 24);
        particleBuffer.SetData(particles);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot(Time.time + ".png");
        }



        if (Input.GetMouseButtonDown(0))
        {
            //particles.

            //drawSphereShader.SetFloat("radius", drawRadius);
            //drawSphereShader.SetVector("mousePos", new Vector2((Input.mousePosition.x / Screen.width) * simulationSize.x, (Input.mousePosition.y / Screen.height) * simulationSize.y));
            //drawSphereShader.SetTexture(0, "mapTex", _target);
            //drawSphereShader.Dispatch(0, Mathf.CeilToInt(simulationSize.x / 8.0f), Mathf.CeilToInt(simulationSize.y / 8.0f), 1);
            //
            //grabPosShader.SetBuffer(0, "particleBuffer", particleBuffer);
            //grabPosShader.SetTexture(0, "mapTex", _target);
            //grabPosShader.Dispatch(0, Mathf.CeilToInt(simulationSize.x / 8.0f), Mathf.CeilToInt(simulationSize.y / 8.0f), 1);

        }
    }

    private void FixedUpdate()
    {
        SetShaderParameters();
        Render();
    }

    private void SetShaderParameters()
    {
        distGenShader.SetBuffer(0, "particleBuffer", particleBuffer);

        particleShader.SetBuffer(0, "particleBuffer", particleBuffer);
        distGenShader.SetFloat("drag", debugX1);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != simulationSize.x || _target.height != simulationSize.y)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();

            // Get a render target for Ray Tracing
            _target = new RenderTexture((int)simulationSize.x, (int)simulationSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.filterMode = FilterMode.Point;
            _target.Create();

            //distTex = new RenderTexture((int)simulationSize.x, (int)simulationSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //distTex.enableRandomWrite = true;
            //distTex.filterMode = FilterMode.Point;
            //distTex.Create();
        }
    }

    private void Render()
    {
        // Make sure we have a current render target
        InitRenderTexture();


        // Set the target and dispatch the compute shader
        distGenShader.SetTexture(0, "mapTex", _target);
        particleShader.SetTexture(0, "mapTex", _target);

        distGenShader.Dispatch(0, Mathf.CeilToInt(particles.Length / 128.0f), 1, 1);
        particleShader.Dispatch(0, Mathf.CeilToInt(particles.Length / 128.0f), 1, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_target, destination);

    }

    void OnApplicationQuit()
    {
        particleBuffer.Dispose();
    }
}