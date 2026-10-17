using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TaskUIManager : MonoBehaviour
{
    public static TaskUIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform taskContainer;

    private Dictionary<string, GameObject> activeTaskUIElements = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Yeni gün başladığında aktif görevleri ekrana basar
    public void PopulateTasks(List<string> taskDescriptions)
    {
        // Önce eski görevleri temizle
        ClearTasks();

        foreach (var desc in taskDescriptions)
        {
            GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
            TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                textComp.text = "- " + desc;
            }

            // Sözlükte saklayalım ki görev tamamlandığında bulup silebilelim veya güncelleyebilelim
            if (!activeTaskUIElements.ContainsKey(desc))
            {
                activeTaskUIElements.Add(desc, taskObj);
            }
        }
    }

    // Bir görev tamamlandığında (çözüldüğünde) çağrılır
    public void CompleteTask(string taskDescription)
    {
        if (activeTaskUIElements.ContainsKey(taskDescription))
        {
            GameObject taskObj = activeTaskUIElements[taskDescription];
            TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                // Görevin üzerini çizme efekti (TextMeshPro zengin metin desteği)
                textComp.text = "<s>- " + taskDescription + "</s>";
                textComp.color = Color.gray; // Soluk renk
            }

            // İstersen kısa bir süre sonra listeden tamamen silebilirsin
            // Destroy(taskObj, 1.5f);
        }
    }

    // Listeyi tamamen sıfırlar (Yeni güne geçerken)
    public void ClearTasks()
    {
        foreach (var kvp in activeTaskUIElements)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        activeTaskUIElements.Clear();
    }
}
