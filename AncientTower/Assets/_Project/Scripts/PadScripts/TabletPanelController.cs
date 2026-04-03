using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 平板组件控制器
/// 点击功能按钮 -> 显示详情页 + 隐藏五个按钮
/// 点击Home键 -> 隐藏详情页 + 显示五个按钮
/// </summary>
public class TabletPanelController : MonoBehaviour
{
    [Header("界面面板")]
    [SerializeField] private GameObject notebookPanel;      // 笔记本界面
    [SerializeField] private GameObject documentPanel;     // 文档界面
    [SerializeField] private GameObject wxPanel;           // 微信界面
    [SerializeField] private GameObject callingPanel;      // 通话界面
    [SerializeField] private GameObject handLetterPanel;   // 手写信界面
    
    [Header("按钮图标")]
    [SerializeField] private Toggle noteIconToggle;        // 笔记本按钮
    [SerializeField] private Toggle inforIconToggle;       // 文档按钮
    [SerializeField] private Toggle wxIconToggle;          // 微信按钮
    [SerializeField] private Toggle callingIconToggle;     // 通话按钮
    [SerializeField] private Toggle handLetterIconToggle;  // 手写信按钮
    
    [Header("主界面组件")]
    [SerializeField] private GameObject iconPanel;         // 按钮容器（包含五个按钮的父对象）
    [SerializeField] private Toggle homeToggle;            // Home键
    
    // 当前显示的详情页
    private GameObject currentDetailPanel;
    
    void Start()
    {
        // 初始化：显示五个按钮，隐藏所有详情页
        InitializeUI();
        
        // 绑定按钮事件
        BindButtonEvents();
        
        // 绑定Home键事件
        if (homeToggle != null)
        {
            homeToggle.onValueChanged.AddListener(OnHomeClick);
            // 初始设置Home键为未选中状态
            homeToggle.isOn = false;
        }
        else
        {
            Debug.LogError("Home键未绑定！");
        }
    }
    
    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 显示五个按钮
        ShowFiveButtons(true);
        
        // 隐藏所有详情页
        HideAllDetailPanels();
        
        // 重置所有Toggle状态
        ResetAllToggles();
        
        currentDetailPanel = null;
    }
    
    /// <summary>
    /// 显示或隐藏五个按钮
    /// </summary>
    private void ShowFiveButtons(bool show)
    {
        if (iconPanel != null)
        {
            iconPanel.SetActive(show);
        }
        else
        {
            // 如果没有容器，单独控制每个按钮
            if (noteIconToggle != null) noteIconToggle.gameObject.SetActive(show);
            if (inforIconToggle != null) inforIconToggle.gameObject.SetActive(show);
            if (wxIconToggle != null) wxIconToggle.gameObject.SetActive(show);
            if (callingIconToggle != null) callingIconToggle.gameObject.SetActive(show);
            if (handLetterIconToggle != null) handLetterIconToggle.gameObject.SetActive(show);
        }
    }
    
    /// <summary>
    /// 隐藏所有详情页
    /// </summary>
    private void HideAllDetailPanels()
    {
        if (notebookPanel != null) notebookPanel.SetActive(false);
        if (documentPanel != null) documentPanel.SetActive(false);
        if (wxPanel != null) wxPanel.SetActive(false);
        if (callingPanel != null) callingPanel.SetActive(false);
        if (handLetterPanel != null) handLetterPanel.SetActive(false);
    }
    
    /// <summary>
    /// 重置所有Toggle的选中状态
    /// </summary>
    private void ResetAllToggles()
    {
        if (noteIconToggle != null) noteIconToggle.isOn = false;
        if (inforIconToggle != null) inforIconToggle.isOn = false;
        if (wxIconToggle != null) wxIconToggle.isOn = false;
        if (callingIconToggle != null) callingIconToggle.isOn = false;
        if (handLetterIconToggle != null) handLetterIconToggle.isOn = false;
    }
    
    /// <summary>
    /// 绑定五个按钮的点击事件
    /// </summary>
    private void BindButtonEvents()
    {
        if (noteIconToggle != null)
            noteIconToggle.onValueChanged.AddListener((isOn) => OnButtonClick(noteIconToggle, notebookPanel, isOn, "笔记本"));
        else
            Debug.LogWarning("笔记本按钮未绑定！");
        
        if (inforIconToggle != null)
            inforIconToggle.onValueChanged.AddListener((isOn) => OnButtonClick(inforIconToggle, documentPanel, isOn, "文档"));
        else
            Debug.LogWarning("文档按钮未绑定！");
        
        if (wxIconToggle != null)
            wxIconToggle.onValueChanged.AddListener((isOn) => OnButtonClick(wxIconToggle, wxPanel, isOn, "微信"));
        else
            Debug.LogWarning("微信按钮未绑定！");
        
        if (callingIconToggle != null)
            callingIconToggle.onValueChanged.AddListener((isOn) => OnButtonClick(callingIconToggle, callingPanel, isOn, "通话"));
        else
            Debug.LogWarning("通话按钮未绑定！");
        
        if (handLetterIconToggle != null)
            handLetterIconToggle.onValueChanged.AddListener((isOn) => OnButtonClick(handLetterIconToggle, handLetterPanel, isOn, "手写信"));
        else
            Debug.LogWarning("手写信按钮未绑定！");
    }
    
    /// <summary>
    /// 按钮点击处理
    /// </summary>
    private void OnButtonClick(Toggle toggle, GameObject panel, bool isOn, string panelName)
    {
        if (isOn)
        {
            // 按钮被选中，显示对应的详情页
            
            // 如果有其他详情页显示，先隐藏
            if (currentDetailPanel != null && currentDetailPanel != panel)
            {
                currentDetailPanel.SetActive(false);
            }
            
            // 显示选中的详情页
            if (panel != null)
            {
                panel.SetActive(true);
                currentDetailPanel = panel;
                Debug.Log($"显示{panelName}详情页");
            }
            else
            {
                Debug.LogError($"{panelName}详情页未绑定！");
                toggle.isOn = false; // 恢复状态
                return;
            }
            
            // 隐藏五个按钮
            ShowFiveButtons(false);
            
            // 更新Home键视觉（可选）
            UpdateHomeVisual(true);
        }
        else
        {
            // 按钮被取消选中，不做处理（因为只有点击Home键才会返回）
            // 防止用户手动取消选中
            if (currentDetailPanel != null)
            {
                // 如果当前有详情页显示，不允许取消选中
                toggle.isOn = true;
            }
        }
    }
    
    /// <summary>
    /// Home键点击处理
    /// </summary>
    private void OnHomeClick(bool isOn)
    {
        if (isOn)
        {
            // Home键被按下
            
            // 隐藏当前显示的详情页
            if (currentDetailPanel != null)
            {
                currentDetailPanel.SetActive(false);
                currentDetailPanel = null;
                Debug.Log("Home键按下，关闭详情页");
            }
            
            // 显示五个按钮
            ShowFiveButtons(true);
            
            // 重置所有按钮的选中状态
            ResetAllToggles();
            
            // 更新Home键视觉
            UpdateHomeVisual(false);
            
            // Home键自动取消选中（保持视觉一致）
            if (homeToggle != null)
            {
                homeToggle.isOn = false;
            }
        }
    }
    
    /// <summary>
    /// 更新Home键的视觉效果
    /// </summary>
    private void UpdateHomeVisual(bool isDetailMode)
    {
        if (homeToggle == null) return;
        
        // 改变Home键颜色来提示当前状态
        var homeImage = homeToggle.GetComponent<Image>();
        if (homeImage != null)
        {
            if (isDetailMode)
            {
                homeImage.color = Color.white;  // 详情模式：亮色
            }
            else
            {
                homeImage.color = new Color(1, 1, 1, 0.6f);  // 主界面模式：半透明
            }
        }
    }
    
    /// <summary>
    /// 公共方法：外部调用返回主界面
    /// </summary>
    public void ReturnToMain()
    {
        if (currentDetailPanel != null)
        {
            currentDetailPanel.SetActive(false);
            currentDetailPanel = null;
        }
        
        ShowFiveButtons(true);
        ResetAllToggles();
        
        if (homeToggle != null)
        {
            homeToggle.isOn = false;
            UpdateHomeVisual(false);
        }
    }
    
    /// <summary>
    /// 公共方法：检查是否在详情模式
    /// </summary>
    public bool IsInDetailMode()
    {
        return currentDetailPanel != null;
    }
    
    /// <summary>
    /// 公共方法：获取当前显示的详情页
    /// </summary>
    public GameObject GetCurrentDetailPanel()
    {
        return currentDetailPanel;
    }
}