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

        // Aynı açıklamaya sahip görevleri kendi içinde sayıyoruz (Örn: "{Stress Item}" kaç adet var?)
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var desc in taskDescriptions)
        {
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

            GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
            TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                // Eğer birden fazlaysa yanına sayaç koyuyoruz (Örn: "- {Stress Item} (0/2)")
                if (totalCount > 1)
                {
                    textComp.text = $"- {desc} (0/{totalCount})";
                }
                else
                {
                    textComp.text = $"- {desc}";
                }
            }

            activeTaskUIElements.Add(desc, taskObj);
        }
    }

    // Bir görev çözüldüğünde çağrılır
    public void CompleteTask(string taskDescription)
    {
        if (activeTaskUIElements.ContainsKey(taskDescription))
        {
            // Tamamlanan sayısını 1 artırıyoruz
            taskCurrentCounts[taskDescription]++;
            int total = taskTotalCounts[taskDescription];
            int current = taskCurrentCounts[taskDescription];

            GameObject taskObj = activeTaskUIElements[taskDescription];
            TextMeshProUGUI textComp = taskObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                if (total > 1)
                {
                    // Eğer hepsi bitmediyse sadece sayacı güncelliyoruz (Örn: 1/2)
                    if (current < total)
                    {
                        textComp.text = $"- {taskDescription} ({current}/{total})";
                    }
                    else
                    {
                        // Hepsi bittiyse (Örn: 2/2) üstünü çizip rengini soluklaştırıyoruz
                        textComp.text = $"<s>- {taskDescription} ({current}/{total})</s>";
                        textComp.color = Color.gray;
                    }
                }
                else
                {
                    // Tekli görevse direkt üstünü çiziyoruz
                    textComp.text = $"<s>- {taskDescription}</s>";
                    textComp.color = Color.gray;
                }
            }
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
        taskTotalCounts.Clear();
        taskCurrentCounts.Clear();
    }
}