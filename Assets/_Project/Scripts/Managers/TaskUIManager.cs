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

    // Görev UI nesneleri ve sayı takipleri için sözlükler
    private Dictionary<string, GameObject> activeTaskUIElements = new Dictionary<string, GameObject>();
    private Dictionary<string, int> taskTotalCounts = new Dictionary<string, int>();
    private Dictionary<string, int> taskCurrentCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Yeni gün başladığında aktif görevleri sayarak ekrana basar
    public void PopulateTasks(List<string> taskDescriptions)
    {
        ClearTasks();

        if (taskDescriptions == null || taskDescriptions.Count == 0)
        {
            Debug.LogWarning("[TaskUIManager] Görev listesi boş geldi!");
            return;
        }

        // Aynı açıklamaya sahip görevleri sayıyoruz
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var desc in taskDescriptions)
        {
            // GÜVENLİK: Boş, null veya geçersiz string'leri atla (Devre dışı kalanlar burada elenir)
            if (string.IsNullOrWhiteSpace(desc))
                continue;

            if (counts.ContainsKey(desc))
                counts[desc]++;
            else
                counts[desc] = 1;
        }

        // Benzersiz görevleri ekrana tek bir satır olarak basıyoruz
        foreach (var pair in counts)
        {
            string desc = pair.Key;
            int totalCount = pair.Value;

            taskTotalCounts[desc] = totalCount;
            taskCurrentCounts[desc] = 0;

            if (taskItemPrefab == null || taskContainer == null)
            {
                Debug.LogError("[TaskUIManager] taskItemPrefab veya taskContainer atanmamış!");
                continue;
            }

            GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
            TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                if (totalCount > 1)
                {
                    textComp.text = $"- {desc} (0/{totalCount})";
                }
                else
                {
                    textComp.text = $"- {desc}";
                }
            }

            activeTaskUIElements[desc] = taskObj;
        }
    }

    // Bir görev çözüldüğünde çağrılır
    public void CompleteTask(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription)) return;

        // GÜVENLİK KONTROLÜ: Eğer bu görev devre dışı bırakıldıysa ve listede yoksa hata vermeden çık!
        if (!activeTaskUIElements.ContainsKey(taskDescription) || !taskTotalCounts.ContainsKey(taskDescription))
        {
            Debug.Log($"[TaskUIManager] '{taskDescription}' aktif görev listesinde yok (Devre dışı bırakılmış olabilir), işlem atlandı.");
            return;
        }

        // Mevcut sayımı güncelle
        if (!taskCurrentCounts.ContainsKey(taskDescription))
        {
            taskCurrentCounts[taskDescription] = 0;
        }

        taskCurrentCounts[taskDescription]++;
        int total = taskTotalCounts[taskDescription];
        int current = taskCurrentCounts[taskDescription];

        GameObject taskObj = activeTaskUIElements[taskDescription];
        if (taskObj == null) return;

        TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

        if (textComp != null)
        {
            if (total > 1)
            {
                if (current < total)
                {
                    textComp.text = $"- {taskDescription} ({current}/{total})";
                }
                else
                {
                    textComp.text = $"<s>- {taskDescription} ({current}/{total})</s>";
                    textComp.color = Color.gray;
                }
            }
            else
            {
                textComp.text = $"<s>- {taskDescription}</s>";
                textComp.color = Color.gray;
            }
        }
    }

    /// <summary>
    /// SummaryPanelManager için sadece gerçekten aktif olan görevleri hesaplar.
    /// </summary>
    public void GetTaskCounts(out int completedCount, out int uncompletedCount)
    {
        completedCount = 0;
        int totalItems = 0;

        foreach (var pair in taskTotalCounts)
        {
            int required = pair.Value;
            int current = taskCurrentCounts.ContainsKey(pair.Key) ? taskCurrentCounts[pair.Key] : 0;
            current = Mathf.Clamp(current, 0, required);
            completedCount += current;
            totalItems += required;
        }

        uncompletedCount = Mathf.Max(0, totalItems - completedCount);
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
        taskTotalCounts.Clear();
        taskCurrentCounts.Clear();
    }
}