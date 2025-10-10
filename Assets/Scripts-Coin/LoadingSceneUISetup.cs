using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 加载界面UI设置示例脚本
/// 这个脚本展示了如何在Unity编辑器中设置加载界面的UI元素
/// </summary>
public class LoadingSceneUISetup : MonoBehaviour
{
    [Header("=== 加载界面UI设置指南 ===")]
    [Space(10)]
    
    [Header("1. 创建Canvas")]
    [Tooltip("确保场景中有一个Canvas，设置为Screen Space - Overlay")]
    public Canvas mainCanvas;
    
    [Header("2. 背景设置")]
    [Tooltip("拖入背景图片，可以是星空、太空等")]
    public Image backgroundImage;
    
    [Header("3. 飞船设置")]
    [Tooltip("拖入您的飞船图片或对象")]
    public Transform spaceship;
    [Tooltip("飞船起始位置（空的GameObject）")]
    public Transform spaceshipStartPoint;
    
    [Header("4. 星球设置")]
    [Tooltip("拖入您的星球图片或对象")]
    public Transform planet;
    [Tooltip("飞船目标位置（通常是星球的位置）")]
    public Transform spaceshipEndPoint;
    
    [Header("5. UI元素")]
    [Tooltip("进度条 - 使用UI -> Slider")]
    public Slider progressBar;
    [Tooltip("进度百分比文本 - 使用TextMeshPro")]
    public TextMeshProUGUI progressText;
    [Tooltip("加载提示文本 - 使用TextMeshPro")]
    public TextMeshProUGUI loadingTipText;
    [Tooltip("光年显示文本 - 使用TextMeshPro")]
    public TextMeshProUGUI lightYearText;
    
    [Header("6. 可选装饰元素")]
    [Tooltip("星星粒子系统")]
    public ParticleSystem starsParticles;
    [Tooltip("其他装饰元素")]
    public GameObject[] decorativeElements;
    
    [Header("=== 设置步骤 ===")]
    [TextArea(10, 15)]
    public string setupInstructions = @"
设置步骤：

1. 创建新场景，命名为 'LoadingScene'

2. 添加Canvas：
   - 创建UI -> Canvas
   - 设置Render Mode为 Screen Space - Overlay

3. 添加背景：
   - 在Canvas下创建UI -> Image，命名为 'Background'
   - 拖入您的星空背景图片
   - 设置Anchor为全屏拉伸

4. 添加星球：
   - 在Canvas下创建UI -> Image，命名为 'Planet'
   - 拖入您的星球图片
   - 调整位置到合适的位置（通常在右侧）

5. 添加飞船：
   - 在Canvas下创建UI -> Image，命名为 'Spaceship'
   - 拖入您的飞船图片
   - 调整大小

6. 创建位置锚点：
   - 创建空的GameObject，命名为 'StartPoint'，放在飞船起始位置
   - 创建空的GameObject，命名为 'EndPoint'，放在星球附近

7. 添加进度条：
   - 创建UI -> Slider，命名为 'ProgressBar'
   - 设置Min Value: 0, Max Value: 1
   - 调整样式和颜色

8. 添加文本：
   - 创建TextMeshPro - Text (UI)，命名为 'ProgressText'
   - 创建TextMeshPro - Text (UI)，命名为 'LoadingTipText'
   - 创建TextMeshPro - Text (UI)，命名为 'LightYearText'

9. 添加LoadingManager脚本：
   - 创建空的GameObject，命名为 'LoadingManager'
   - 添加LoadingManager脚本
   - 在Inspector中拖入所有UI元素和位置锚点

10. 可选添加粒子系统：
    - 创建Particle System，设置为星星效果
    - 调整参数让星星在背景中闪烁

11. 保存场景为 'LoadingScene'

12. 在Build Settings中添加LoadingScene

完成后，您的游戏就会在场景切换时显示漂亮的飞船飞向星球的加载界面！
    ";
    
    void Start()
    {
        // 这个脚本只是用于展示设置说明，实际上不需要任何逻辑
        Debug.Log("LoadingSceneUISetup: 请参考Inspector中的设置指南来创建加载界面");
    }

    [ContextMenu("自动设置基础UI布局")]
    void AutoSetupBasicLayout()
    {
        // 这个方法可以帮助自动创建基础的UI布局
        if (mainCanvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        Debug.Log("请手动添加UI元素并配置LoadingManager脚本");
    }
}