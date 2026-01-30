using System;
using UnityEngine;

/* ScriptableObject란?
 * Unity에서 제공하는 데이터 클래스 템플릿.
 * 기본적인 게임 데이터 뿐만 아니라 유니티에서 사용하는 Asset들의 참조도
 * 데이터 단위로 묶을 수 있어서 편리함.
 *
 * 게임 데이터를 활용하는데에 매우 유용하지만
 * 대량의 데이터를 게임에 추가하기에는 적절하지는 못하다. 왜나면 불편해서...
 * 게임사들은 CSV나 TSV, JSON 등의 데이터 타입들을 많이 사용함.
 *
 * ScriptableObject는 에디터에서 직접 객체를 생성해야합니다.
 */ 
// [CreateAssetMenu(fileName = "사용할 파일 이름", menuName = "메뉴의 접근 경로", order = {정렬 순서})]
[CreateAssetMenu(fileName = "Game Item", menuName = "SAO/GameItemSAO", order = 1)]

public class Item : ScriptableObject
{
    public int Key;
    public string Name; // 이름
    public string Description; // 설명
    public Sprite Icon; // 아이템 그림
    public int Price;  // 가격
    
    
    
}
