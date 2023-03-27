using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;

    public List<Room> generatedRooms = new List<Room>();

    [System.Serializable]
    public class Room
    {
        public int room;
        public Vector3 roomPos;

        public RoomBehaviour roomObj;

        public Room(int room, Vector3 roomPos, RoomBehaviour roomObj)
        {
            this.room = room;
            this.roomPos = roomPos;
            this.roomObj = roomObj;
        }
    }

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    public Vector2Int size;
    public int startPos = 0;
    public GameObject baseRoom;
    public GameObject[] rooms;
    public Vector2 offset;

    List<Cell> board;

    public int totalRooms;
    public int currentRoomCount;

    public bool sentMapData = false;
    public bool doneGenerating = false;

    public bool ShouldGen = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (RulesManager.instance != null)
        {
            size = Vector2Int.CeilToInt(RulesManager.instance.mapSize);
        } else
        {
            size = new Vector2Int(9, 9);
            ShouldGen = true;
        }     
    }

    private void FixedUpdate()
    {
        if (ShouldGen && NetworkManager.instance.Server.IsRunning)
        {
            if (RulesManager.instance != null)
            {
                size = Vector2Int.CeilToInt(RulesManager.instance.mapSize);
            }
            else
            {
                size = new Vector2Int(9, 9);
            }

            Debug.Log("[Dungeon Generator] Starting dungeon generation.");
            ShouldGen = false;
            MazeGenerator(); 
        }
    }

    public void StartGenerating()
    {
        ShouldGen = true;
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.mapHeader)]
    static void HandleMapHeader(Message msg)
    {
        if (instance == null)
            instance = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
        int totalRooms = msg.GetInt();
        instance.totalRooms = totalRooms;

        Debug.Log("[Dungeon Generator] Recieved map header: Total rooms: " + totalRooms);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.mapData)]
    static void HandleMapData(Message msg)
    {
        

        if (instance == null)
            instance = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
        instance.currentRoomCount++;

        Vector3 roomPos = msg.GetVector3();
        int roomId = msg.GetInt();
        bool[] bools = msg.GetBools();
        Quaternion rot = msg.GetQuaternion();

        var spawnedBase = Instantiate(instance.baseRoom, roomPos, Quaternion.identity, instance.transform);
        Instantiate(instance.rooms[roomId], spawnedBase.transform.position, rot, spawnedBase.transform);

        

        var newRoom = spawnedBase.GetComponent<RoomBehaviour>();
        newRoom.UpdateRoom(bools);

        Debug.Log($"[Dungeon Generator] Recieved map data: Position:{roomPos} Room ID:{roomId} State:{bools}");

        if (instance.currentRoomCount == instance.totalRooms)
        {
            Message doneGenerating = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.mapDone);
            doneGenerating.AddUShort(NetworkManager.instance.Client.Id);
            NetworkManager.instance.MapIsReady(NetworkManager.instance.Client.Id);
            NetworkManager.instance.Client.Send(doneGenerating);

            GameManager.instance.Respawn();
        }
    }

    void GenerateDungeon()
    {

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Cell currentCell = board[(i + j * size.x)];
                if (currentCell.visited)
                {
                    int randomRoom = 0;

                    randomRoom = Random.Range(0, rooms.Length);

                    Quaternion rotation = Quaternion.identity;

                    int rand = Random.Range(0, 4);

                    switch(rand)
                    {
                        case 0:
                            rotation = Quaternion.identity;
                            break;
                        case 1:
                            rotation.eulerAngles = new Vector3(0, 90, 0);
                            break;
                        case 2:
                            rotation.eulerAngles = new Vector3(0, 270, 0);
                            break;
                        case 3:
                            rotation.eulerAngles = new Vector3(0, 180, 0);
                            break;
                    }

                    var spawnedBase = Instantiate(baseRoom, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity);
                    Instantiate(rooms[randomRoom], spawnedBase.transform.position, rotation, spawnedBase.transform);

                    var newRoom = spawnedBase.GetComponent<RoomBehaviour>();
                    newRoom.UpdateRoom(currentCell.status);
                    newRoom.name += " " + i + "-" + j;

                    generatedRooms.Add(new Room(randomRoom, new Vector3(i * offset.x, 0, -j * offset.y), newRoom));
                }
            }
        } 

        if (size.x * size.y * 0.85f <= generatedRooms.Count)
        {
            foreach (var room in generatedRooms)
            {
                room.roomObj.BreakHoles();
            }

            if (NetworkManager.instance != null)
            {
                Debug.Log("[Dungeon Generator] Map accepted and broken holes have finished, starting trasmission.");

                NetworkManager inst = NetworkManager.instance;

                Message mapHeader = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.mapHeader);
                mapHeader.AddInt(generatedRooms.Count);
                inst.Client.Send(mapHeader);

                foreach (var genRoom in generatedRooms)
                {
                    Message mapData = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.mapData);
                    mapData.AddVector3(genRoom.roomPos);
                    mapData.AddInt(genRoom.room);
                    mapData.AddBools(genRoom.roomObj.currStatus);
                    mapData.AddQuaternion(genRoom.roomObj.transform.GetChild(3).rotation);
                    inst.Client.Send(mapData);
                }

                Message doneGenerating = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.mapDone);
                doneGenerating.AddUShort(NetworkManager.instance.Client.Id);
                NetworkManager.instance.MapIsReady(NetworkManager.instance.Client.Id);
                NetworkManager.instance.Client.Send(doneGenerating);

                GameManager.instance.Respawn();
            }
        }
        else
        {
            Debug.Log("[Dungeon Generator] Map failed checks, restarting generation. ");

            foreach (var room in generatedRooms)
            {
                Destroy(room.roomObj.gameObject);
            }

            GameManager.instance.ResetDungeonGen();
            Destroy(gameObject);
        }
    }

    void MazeGenerator()
    {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }

        startPos = Random.Range(0, board.Count);

        int currentCell = startPos;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while (k<1000)
        {
            k++;

            board[currentCell].visited = true;

            if(currentCell == board.Count - 1)
            {
                break;
            }

            //Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    //down or right
                    if (newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    //up or left
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }

            }

        }
        GenerateDungeon();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell-size.x)].visited)
        {
            neighbors.Add((cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited)
        {
            neighbors.Add((cell + size.x));
        }

        //check right neighbor
        if ((cell+1) % size.x != 0 && !board[(cell +1)].visited)
        {
            neighbors.Add((cell +1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited)
        {
            neighbors.Add((cell -1));
        }

        return neighbors;
    }
}
