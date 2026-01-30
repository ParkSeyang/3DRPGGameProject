using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class Study_Quest : MonoBehaviour
{
    private const string CATEGORY_PLACE_BLOCK = "PlaceBlock";
    private const string CATEGORY_REMOVE_BLOCK = "RemoveBlock";
    private const string CATEGORY_MOVE =  "Move";
    
    public struct QuestData // 그냥 데이터
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Parameter { get; set; }
    }

    public class Quest
    {
        public QuestData Data { get; set; }
        public int MaxProgress { get; set; }
        public int CurrentProgress { get; set; }
    }

    // 아래처럼 상속구조 디자인을 사용하셔도 좋습니다.

    #region 예시

    public class PlaceQuest : Quest
    {
        
    }

    public class MoveQuest : Quest
    {
        public Vector3 StartPosition { get; set; }
    }

    #endregion
    

    private QuestData[] questList;
    
    private Quest activeQuest;  // 이거는 여러개의 퀘스트일 경우 리스트로 해놔도 됨
    
    [SerializeField] PlayerController2 localPlayer;
    
    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "QuestTable.tsv");
        questList = TSVReader.ReadTable<QuestData>(filePath).ToArray();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuestData rand = questList[Random.Range(0, questList.Length)];
            activeQuest = CreatQuest(rand);
            
        }
    }

    // 퀘스트를 생성하는 함수
    private Quest CreatQuest(QuestData data)
    {
        Quest newQuest = new Quest();
        newQuest.Data = data;
        
        // currentProgress 와 maxProgress는 따로 설정을 해줘야 합니다.
        // 보통의 게임 구조라면 currentProgress는 유저 데이터에 저장이 되겠죠?

        switch (data.Category)
        {
            case CATEGORY_PLACE_BLOCK :
                int.TryParse(data.Parameter.Split('_')[1], out int placeAmount);
                newQuest.MaxProgress = placeAmount;
                BlockSystem.Instance.Events.OnPlaceBlock += UpdatePlaceBlockQuest;
                BlockSystem.Instance.Events.OnRemoveBlock += UpdateRemoveBlockQuest;
                break;
            case CATEGORY_REMOVE_BLOCK :
                int.TryParse(data.Parameter.Split('_')[1], out int removeAmount);
                newQuest.MaxProgress = removeAmount;
                BlockSystem.Instance.Events.OnRemoveBlock += UpdateRemoveBlockQuest;
                break;
            case CATEGORY_MOVE :
                int.TryParse(data.Parameter.Split('_')[1], out int goalDistance);
                newQuest.MaxProgress = goalDistance;
                
                break;
            default:
                break;
        }
        
        
        return newQuest;
    }
    
    // 퀘스트 완료 처리하는 함수
    private void CompleteQuest(Quest quest)
    {
        switch (quest.Data.Category)
        {
            case CATEGORY_PLACE_BLOCK :
                BlockSystem.Instance.Events.OnPlaceBlock -= UpdatePlaceBlockQuest;
                break;
            case CATEGORY_REMOVE_BLOCK :
                BlockSystem.Instance.Events.OnRemoveBlock -= UpdateRemoveBlockQuest;
                break;
            case CATEGORY_MOVE :
              //  localPlayer.OnMove += UpdateMoveQuest;
                break;
            default :
                break;
        }
        
        Debug.Log($"Quest Complete ");


        activeQuest = null;
    }

    #region 실제 퀘스트 진행도를 체크하는 함수들

    // PlaceBlock계열의 퀘스트들 체크
    // 
    private void UpdatePlaceBlockQuest(BlockSystem.BlockEvent blockEvent)
    {
        // 예외처리(필터)
        int prefabIndex = int.Parse(activeQuest.Data.Parameter.Split('_')[0]);
        if (blockEvent.Block.Data.PrefabIndex != prefabIndex)
        {
            return;
        }

        switch (blockEvent.eventType)
        {
            case BlockSystem.EventType.PlaceBlock :
                activeQuest.CurrentProgress++;
                break;
            case BlockSystem.EventType.RemoveBlock :
                activeQuest.CurrentProgress--;
                break;
            default :
                break;
        }
        
        activeQuest.CurrentProgress++;
        Debug.Log($" Quest Updata ! ::: {activeQuest.Data.Name}, {activeQuest.CurrentProgress} / {activeQuest.MaxProgress}");

        if (activeQuest.CurrentProgress >= activeQuest.MaxProgress)
        {
            CompleteQuest(activeQuest);
        }

    }
    
    // RemoveBlock계열의 퀘스트들 체크
    private void UpdateRemoveBlockQuest(BlockSystem.BlockEvent blockEvent)
    {
        // 예외처리(필터)
        int prefabIndex = int.Parse(activeQuest.Data.Parameter.Split('_')[0]);
        if (blockEvent.Block.Data.PrefabIndex != prefabIndex)
        {
            return;
        }

        activeQuest.CurrentProgress++;
        Debug.Log($" Quest Updata ! ::: {activeQuest.Data.Name}, {activeQuest.CurrentProgress} / {activeQuest.MaxProgress}");

        if (activeQuest.CurrentProgress >= activeQuest.MaxProgress)
        {
            CompleteQuest(activeQuest);
        }
    }

    private void UpdateMoveQuest(Vector3 playerPosition)
    {
        
    }

    #endregion
    
    
    
    
}
