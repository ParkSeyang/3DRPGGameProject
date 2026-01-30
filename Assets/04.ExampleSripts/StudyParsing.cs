using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StudyParsing : MonoBehaviour
{
    private string rawData = 
        "Aaron Cresswell,32,Defender,West Ham United,1589,England,20,0,1\n" +
        "Aaron Lennon,35,Midfielder,Burnley,1217,England,16,1,1\n" +
        "Aaron Mooy,32,Midfielder,Huddersfield Town,2327,Australia,29,3,1";

    public class Player
    {
        // 축구선수들의 정보
        public string Name { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }
        public string Club { get; set; }
        public int MinutesPlayed { get; set; }
        public string Nation { get; set; }
        public int Appearances { get; set; }
        public int Goal { get; set; }
        public int Assist { get; set; }

        // public float CritRate { get; set; }

        public override string ToString()
        {
            return $"Player Info : {Name}, {Age} ,{Position}, {Club}, {MinutesPlayed}, " +
                   $"{Nation}, {Appearances}, {Goal}, {Assist}";
        }

        public static Player Parse(string data)
        {
            string[] splitRow = data.Split(',');

            Player player = new Player();
            player.Name = splitRow[0];
            player.Age = int.Parse(splitRow[1]);
            player.Position = splitRow[2];
            player.Club = splitRow[3];
            player.MinutesPlayed = int.Parse(splitRow[4]);
            player.Nation = splitRow[5];
            player.Appearances = int.Parse(splitRow[6]);
            player.Goal = int.Parse(splitRow[7]);
            player.Assist = int.Parse(splitRow[8]);

          // const int max = 10000;
          // int value = int.Parse(splitRow[8]);
          // player.CritRate = (float)value / max;
            return player;
        }

    }

    void Start()
    {
        // 원래는 아래처럼 사용했지만, CSV나 TSV, JSON같은 문자열 데이터 타입을사용하게되면
        // 더이상 아래처럼 사용하지 않는다.
        // Player player = new Player();
        // player.Name = "PSY";
        // player.Age = 32;
        
        // rawData를 게행 '\n' 으로 나눠주면 선수 몇명인지 알게 됩니다.
        
        string[] playerRows = rawData.Split('\n');

        for (int i = 0; i < playerRows.Length; i++)
        { 
            // 디버그용 출력
           //  Debug.Log($"player {playerRows[i]}");

           Player player = Player.Parse(playerRows[i]);
           Debug.Log(player.ToString());
           Debug.Log("====================================");
        }

        // Unity에서 사용가능한 파일 경로
        // Application.streamingAssetsPath : Editor에서는 Assets/StreamingAssets/ 경로로 인식을 하고
        // Build된 앱에서는 플랫폼 별로 다른 경로들을 자동으로 설정해줍니다.
        // Window의 경우는 실행프로그램의 경로와 동일한 곳(폴더)으로 자동으로 내장되어 설정
        // Anidroid(중요) apk 파일 내부의 읽기전용 경로로 자동 설정됨
        
        // Path.Combine() : 특정 폴더의 하위 파일의 경로를 결합하여 반환하는 함수
        string path = Path.Combine(Application.streamingAssetsPath, "soccer_players_100.csv");
        
        Debug.Log(path);
        
        StreamReader reader = new StreamReader(path);
        List<Player> Players = new List<Player>();

        // 첫줄에 헤더가 있어서 한줄 읽어서 건너뜀
        reader.ReadLine();
        
        while (reader.EndOfStream == false)
        {
            string line = reader.ReadLine();
            Player player = Player.Parse(line);
            Players.Add(player);
        }

        for (int i = 0; i < Players.Count; i++)
        {
            Debug.Log(Players[i].ToString());
            Debug.Log("====================================");
        }

        //string Text = File.ReadAllText(path);
        //Debug.Log(Text);

    }



}
