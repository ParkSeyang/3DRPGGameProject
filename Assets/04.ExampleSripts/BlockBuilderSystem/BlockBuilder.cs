using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBuilder : MonoBehaviour
{
    private Camera mainCam;

    // Start is called before the first frame update
    private void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // 블록 배치한다
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //hit.point에 블록을 배치한다.
                BlockSystem.Instance.PlaceBlock(hit.point);
            }
            
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            // 블록 제거한다
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //hit.point에 블록을 배치한다.
                BlockSystem.Instance.RemoveBlock(hit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            BlockSystem.Instance.RotateBlock();
        }
    }
}
