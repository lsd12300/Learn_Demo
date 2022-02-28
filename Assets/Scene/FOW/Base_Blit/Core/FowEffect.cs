using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FowEffect : MonoBehaviour
{

    public static FowEffect Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<FowEffect>();
            return instance;
        }
    }

    private static FowEffect instance;

    [SerializeField]
    private Color m_FogColor = Color.black;
    [SerializeField]
    private float m_XSize;
    [SerializeField]
    private float m_ZSize;
    [SerializeField]
    private int m_TexWidth;
    [SerializeField]
    private int m_TexHeight;
    [SerializeField]
    private Vector3 m_CenterPosition;
    [SerializeField]
    private float m_HeightRange;
    /// <summary>
    /// 模糊偏移量
    /// </summary>
    [SerializeField]
    private float m_BlurOffset;
    /// <summary>
    /// 模糊迭代次数
    /// </summary>
    [SerializeField]
    private int m_BlurInteration;

    /// <summary>
    /// 迷雾特效shader
    /// </summary>
    public Shader effectShader;
    /// <summary>
    /// 模糊shader
    /// </summary>
    public Shader blurShader;

    private FogRender m_Renderer;

    private bool m_IsInitialized;

    private Camera m_Camera;
    

    public RenderTexture orthRT;



    void Awake()
    {
        m_IsInitialized = Init();

    }

    void OnDestroy()
    {
        if (m_Renderer != null)
            m_Renderer.Release();
        m_Renderer = null;
        instance = null;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        //Graphics.Blit(src, dst);
        if (!m_IsInitialized)
            Graphics.Blit(src, dst);
        else
        {
            m_Renderer.RenderFogOfWar(m_Camera, orthRT, src, dst);
        }
    }

    private bool Init()
    {
        if (m_XSize <= 0 || m_ZSize <= 0 || m_TexWidth <= 0 || m_TexHeight <= 0)
            return false;
        if (effectShader == null || !effectShader.isSupported)
            return false;
        m_Camera = gameObject.GetComponent<Camera>();
        if (m_Camera == null)
            return false;
        m_Camera.depthTextureMode |= DepthTextureMode.Depth;
        m_Renderer = new FogRender(effectShader, blurShader, null, m_CenterPosition, m_XSize, m_ZSize, m_FogColor, m_BlurOffset, m_BlurInteration);
        return true;
    }
}
