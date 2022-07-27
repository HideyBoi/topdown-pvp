using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

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

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y)
        {
            // 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

            if (x>= minPosition.x && x<=maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                return obligatory ? 2 : 1;
            }

            return 0;
        }

    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public Vector2 offset;

    List<Cell> board;

    public int totalRooms;
    public int currentRoomCount;

    public bool sentMapData = false;
    public bool doneGenerating = false;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        size = Vector2Int.CeilToInt(GameManager.instance.mapSize);
    }

    // Start is called before the first frame update
    public void StartGenerating()
    {
        //cam.transform.position = new Vector3((size.x * offset.x / 2) - size.x, 100, -((size.y * offset.y) / 2) + size.y);
        //cam.orthographicSize = offset.x * size.x / 2;
        MazeGenerator();
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.mapHeader)]
    static void HandleMapHeader(Message msg)
    {
        if (instance == null)
            instance = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
        instance.totalRooms = msg.GetInt();
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.mapData)]
    static void HandleMapData(Message msg)
    {
        if (instance == null)
            instance = GameObject.Find("Generator").GetComponent<DungeonGenerator>();
        instance.currentRoomCount++;

        var newRoom = Instantiate(instance.rooms[msg.GetInt()].room, msg.GetVector3(), Quaternion.identity, instance.transform).GetComponent<RoomBehaviour>();
        newRoom.UpdateRoom(msg.GetBools());

        if (instance.currentRoomCount == instance.totalRooms)
        {
            Message doneGenerating = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.mapDone, shouldAutoRelay: true);
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
                    int randomRoom = -1;
                    List<int> availableRooms = new List<int>();

                    for (int k = 0; k < rooms.Length; k++)
                    {
                        int p = rooms[k].ProbabilityOfSpawning(i, j);

                        if(p == 2)
                        {
                            randomRoom = k;
                            break;
                        } else if (p == 1)
                        {
                            availableRooms.Add(k);
                        }
                    }

                    if(randomRoom == -1)
                    {
                        if (availableRooms.Count > 0)
                        {
                            randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                        }
                        else
                        {
                            randomRoom = 0;
                        }
                    }


                    var newRoom = Instantiate(rooms[randomRoom].room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                    newRoom.UpdateRoom(currentCell.status);
                    newRoom.name += " " + i + "-" + j;

                    generatedRooms.Add(new Room(randomRoom, new Vector3(i * offset.x, 0, -j * offset.y), newRoom));
                }
            }
        }

        NetworkManager inst = NetworkManager.instance;

        Message mapHeader = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.mapHeader, shouldAutoRelay: true);
        mapHeader.AddInt(generatedRooms.Count);
        inst.Client.Send(mapHeader);

        foreach (var genRoom in generatedRooms)
        {
            Message mapData = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.mapData, shouldAutoRelay: true);
            mapData.AddInt(genRoom.room);
            mapData.AddVector3(genRoom.roomPos);
            mapData.AddBools(genRoom.roomObj.currStatus);
            inst.Client.Send(mapData);
        }

        Message doneGenerating = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.mapDone, shouldAutoRelay: true);
        doneGenerating.AddUShort(NetworkManager.instance.Client.Id);
        NetworkManager.instance.MapIsReady(NetworkManager.instance.Client.Id);
        NetworkManager.instance.Client.Send(doneGenerating);

        GameManager.instance.Respawn();
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
