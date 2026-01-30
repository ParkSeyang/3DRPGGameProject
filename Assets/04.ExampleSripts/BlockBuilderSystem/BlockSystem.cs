using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using UnityEngine;

public class BlockSystem : MonoBehaviour
{
    public class Callbacks
    {
        public Action<BlockEvent> OnPlaceBlock;
        public Action<BlockEvent> OnRemoveBlock;
    }

    public class BlockEvent
    {
        public Block Block { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public EventType eventType { get; set; }
    }

    public enum EventType
    {
        PlaceBlock,
        RemoveBlock
    }

    public static BlockSystem Instance { get; private set; }
    
    // 1. 배치가능한 Block 종류의 목록
    // 2. 블록을 선택하고 변경하는 기능
    // 3. 블록을 배치하는 기능
    // 4. 블록을 회전하는 기능
    // 5. 블록을 제거하는 기능
    // 6. 블록을 프리뷰(배치하기 전과 제거하기 전에 미리 볼수있는)하는 기능
    // 7. 월드에 배치된 블록을 저장하는 기능
    // 8. 월드에 배치된 블록을 불러오는 기능

    public Callbacks Events = new Callbacks(); 
    
    [Header("Prefabs")]
    public Block[] blockPrefabs;

    private Block previewBlock;
    private int cursor = -1;
    private Vector3Int prevPreviewPos = Vector3Int.zero;
    private GridSystem gridSystem;
    private List<Block> blocks = new List<Block>();
    private Quaternion currentRotation = Quaternion.identity;
    
    private void Awake()
    {
        Instance = this;
        gridSystem = new GridSystem(Vector3.one);
    }

    private void Start()
    {
        ChangeBlockPrefabCursor(0);
        // LoadBlocksFromCsv();
    }

    private void Update()
    {
        UpdatePreviewBlock();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeBlockPrefabCursor(cursor-1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeBlockPrefabCursor(cursor+1);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SaveBlocksToCsv();
        }
        
    }

    public void ChangeBlockPrefabCursor(int index)
    {
        int previousCursor = cursor;
        cursor = index;
        cursor = Mathf.Clamp(cursor, 0, blockPrefabs.Length - 1);
        if(previousCursor != cursor) CreatePreviewBlock();
    }

    private void CreatePreviewBlock()
    {
        if (previewBlock != null) Destroy(previewBlock.gameObject);
        previewBlock = Instantiate(blockPrefabs[cursor]);
        previewBlock.GetComponentInChildren<Collider>().enabled = false;
        UpdatePreviewBlock();
    }
    
    private void UpdatePreviewBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPos = gridSystem.WorldToGrid(hit.point);
            if (prevPreviewPos.Equals(gridPos) == false)
            {
                prevPreviewPos = gridPos;
                Vector3 position = gridSystem.GridToWorld(gridPos);
                previewBlock.transform.position = position;
                previewBlock.transform.rotation = currentRotation;
                previewBlock.gameObject.SetActive(true);
                
                //이미 블록이 존재하는지 여부를 체크해야함
                if (IsBlocked()) SetPreviewColor(Color.red);
                else SetPreviewColor(Color.green);
            } 
        }
        else
        {
            previewBlock.gameObject.SetActive(false);
        }
    }

    private bool IsBlocked()
    {
        if(previewBlock == null) return true;

        var previewBounds = BoundsUtil.CalculateRendererBounds(previewBlock.gameObject);
        previewBounds.size *= 0.95f;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].OverLaps(previewBounds))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void SetPreviewColor(Color color)
    {
        //렌더러의 구조가 복잡해지면 추상화 해서 Block에 옮겨주는게 좋다
        var renders = previewBlock.GetComponentsInChildren<Renderer>();
        if(renders == null || renders.Length == 0) return;
        for (int i = 0; i < renders.Length; i++)
        {
            renders[i].material.color = color;
        }
    }

    public bool PlaceBlock(Vector3 position)
    {
        //배치를 했는지 못했는지 여부 체크
        var gridPosition = gridSystem.WorldToGrid(position);

        if (IsBlocked())
        {
            Debug.Log($"Place Block! ::: isSuccess = {false}, {gridPosition}, {position}");
            return false;
        }
        
        var placedBlock = 
            Instantiate(blockPrefabs[cursor], gridPosition, currentRotation);
        
        placedBlock.Place();
        placedBlock.Data.PrefabIndex = cursor;
        
        blocks.Add(placedBlock);
        Debug.Log($"Place Block! ::: isSuccess = {true}, {gridPosition}, {position}");

        BlockEvent blockEvent = new BlockEvent();
        blockEvent.Block = placedBlock;
        blockEvent.Position = placedBlock.transform.position;
        blockEvent.Rotation = placedBlock.transform.rotation;
        
        Events.OnPlaceBlock?.Invoke(blockEvent);
        
        return true;
    }

    private void PlaceBlockForLoad(BlockData blockData)
    {
        //블록 데이터를 통해서 블럭을 생성하는 로직

        var position = blockData.GetPosition();
        var rotation = Quaternion.Euler(0, blockData.AngleY, 0);
        
        var block = Instantiate(blockPrefabs[blockData.PrefabIndex], position, rotation);
        block.Place();
        block.Data = blockData;
        blocks.Add(block);

    }

    public bool RemoveBlock(Vector3 position)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].bounds.Contains(position))
            {
                BlockEvent blockEvent = new BlockEvent();
                blockEvent.Block = blocks[i];
                blockEvent.Position = blocks[i].transform.position;
                blockEvent.Rotation = blocks[i].transform.rotation;
        
                Events.OnRemoveBlock?.Invoke(blockEvent);
                
                Destroy(blocks[i].gameObject);
                blocks.RemoveAt(i);
                Debug.Log($"Remove Block! ::: isSuccess = {false}, {position}");
               
                return true;
            }
        }
        
        return false;
    }

    public void RotateBlock()
    {
        currentRotation *= Quaternion.Euler(Vector3.up * 90f);
        if(previewBlock != null) previewBlock.transform.rotation = currentRotation;
    }

    private void SaveBlocksToCsv()
    {
        //Application.persistentDataPath는 OS에 허락받은 App이 파일을 읽고 쓸수 있는 폴더
        //C:\Users\{사용자명}}\AppData\LocalLow\{companyName}\{productName}
        //해당 경로는 윈도우,안드로이드 둘다 사용가능 (IOS는 모르겠음)
        string path = Path.Combine(Application.persistentDataPath, "Blocks.csv");

        using (StreamWriter sw = new StreamWriter(path))
        using (CsvWriter cw = new CsvWriter(sw, CultureInfo.CurrentCulture))
        {
            List<BlockData> blockDatas = new List<BlockData>();
            for (int i = 0; i < blocks.Count; i++)
            {
                blockDatas.Add(blocks[i].Data);
            }
            cw.WriteRecords(blockDatas);
        }
        
        Debug.unityLogger.Log($"Saved Blocks! ::: {path}");
    }

    private void LoadBlocksFromCsv()
    {
        //Application.persistentDataPath는 OS에 허락받은 App이 파일을 읽고 쓸수 있는 폴더
        //C:\Users\{사용자명}}\AppData\LocalLow\{companyName}\{productName}
        //해당 경로는 윈도우,안드로이드 둘다 사용가능 (IOS는 모르겠음)
        string path = Path.Combine(Application.persistentDataPath, "Blocks.csv");

        using (StreamReader sr = new StreamReader(path))
        using (CsvReader cr = new CsvReader(sr, CultureInfo.CurrentCulture))
        {
            var blockDatas = cr.GetRecords<BlockData>().ToList();
            foreach (var blockData in blockDatas)
            {
                PlaceBlockForLoad(blockData);
            }
          
        }
        
        Debug.unityLogger.Log($"Loaded Blocks! ::: {path}");
    }
    
}
